using System.Text.Json;
using PharmacyApp.Models;

namespace PharmacyApp.Services;

public class PharmacyJsonStore
{
    private readonly string _medicinesPath;
    private readonly string _salesPath;
    private readonly object _lock = new();
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public PharmacyJsonStore(IWebHostEnvironment env)
    {
        var dataDir = Path.Combine(env.ContentRootPath, "Data");
        Directory.CreateDirectory(dataDir);
        _medicinesPath = Path.Combine(dataDir, "medicines.json");
        _salesPath = Path.Combine(dataDir, "sales.json");
        EnsureSeedFiles();
    }

    private void EnsureSeedFiles()
    {
        if (!File.Exists(_medicinesPath))
            File.WriteAllText(_medicinesPath, "[]");
        if (!File.Exists(_salesPath))
            File.WriteAllText(_salesPath, "[]");
    }

    public List<Medicine> GetMedicines(string? search = null)
    {
        lock (_lock)
        {
            var list = ReadList<Medicine>(_medicinesPath);
            if (string.IsNullOrWhiteSpace(search))
                return list.OrderBy(m => m.FullName).ToList();
            var q = search.Trim();
            return list
                .Where(m => m.FullName.Contains(q, StringComparison.OrdinalIgnoreCase))
                .OrderBy(m => m.FullName)
                .ToList();
        }
    }

    public Medicine? GetMedicineById(int id)
    {
        lock (_lock)
        {
            return ReadList<Medicine>(_medicinesPath).FirstOrDefault(m => m.Id == id);
        }
    }

    public Medicine AddMedicine(CreateMedicineDto dto)
    {
        lock (_lock)
        {
            var list = ReadList<Medicine>(_medicinesPath);
            var nextId = list.Count == 0 ? 1 : list.Max(m => m.Id) + 1;
            var medicine = new Medicine
            {
                Id = nextId,
                FullName = dto.FullName.Trim(),
                Notes = dto.Notes?.Trim() ?? string.Empty,
                ExpiryDate = dto.ExpiryDate.Date,
                Quantity = dto.Quantity,
                Price = Math.Round(dto.Price, 2, MidpointRounding.AwayFromZero),
                Brand = dto.Brand.Trim()
            };
            list.Add(medicine);
            WriteList(_medicinesPath, list);
            return medicine;
        }
    }

    public Medicine? UpdateMedicine(int id, CreateMedicineDto dto)
    {
        lock (_lock)
        {
            var list = ReadList<Medicine>(_medicinesPath);
            var index = list.FindIndex(m => m.Id == id);
            if (index < 0) return null;
            list[index] = new Medicine
            {
                Id = id,
                FullName = dto.FullName.Trim(),
                Notes = dto.Notes?.Trim() ?? string.Empty,
                ExpiryDate = dto.ExpiryDate.Date,
                Quantity = dto.Quantity,
                Price = Math.Round(dto.Price, 2, MidpointRounding.AwayFromZero),
                Brand = dto.Brand.Trim()
            };
            WriteList(_medicinesPath, list);
            return list[index];
        }
    }

    public bool DeleteMedicine(int id)
    {
        lock (_lock)
        {
            var list = ReadList<Medicine>(_medicinesPath);
            var removed = list.RemoveAll(m => m.Id == id);
            if (removed == 0) return false;
            WriteList(_medicinesPath, list);
            return true;
        }
    }

    public List<Sale> GetSales()
    {
        lock (_lock)
        {
            return ReadList<Sale>(_salesPath).OrderByDescending(s => s.SaleDate).ThenByDescending(s => s.Id).ToList();
        }
    }

    /// <summary>
    /// Records a sale and decrements stock. Returns null if medicine not found or insufficient quantity.
    /// </summary>
    public Sale? RecordSale(CreateSaleDto dto, out string? error)
    {
        error = null;
        lock (_lock)
        {
            var medicines = ReadList<Medicine>(_medicinesPath);
            var med = medicines.FirstOrDefault(m => m.Id == dto.MedicineId);
            if (med == null)
            {
                error = "Medicine not found.";
                return null;
            }
            if (med.Quantity < dto.QuantitySold)
            {
                error = $"Insufficient stock. Available: {med.Quantity}.";
                return null;
            }

            med.Quantity -= dto.QuantitySold;
            WriteList(_medicinesPath, medicines);

            var sales = ReadList<Sale>(_salesPath);
            var nextSaleId = sales.Count == 0 ? 1 : sales.Max(s => s.Id) + 1;
            var sale = new Sale
            {
                Id = nextSaleId,
                MedicineId = med.Id,
                MedicineName = med.FullName,
                QuantitySold = dto.QuantitySold,
                UnitPrice = med.Price,
                SaleDate = DateTime.UtcNow
            };
            sales.Add(sale);
            WriteList(_salesPath, sales);
            return sale;
        }
    }

    private List<T> ReadList<T>(string path)
    {
        var json = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(json))
            return new List<T>();
        return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
    }

    private void WriteList<T>(string path, List<T> list)
    {
        var json = JsonSerializer.Serialize(list, _jsonOptions);
        File.WriteAllText(path, json);
    }
}
