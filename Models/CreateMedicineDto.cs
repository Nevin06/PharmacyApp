using System.ComponentModel.DataAnnotations;

namespace PharmacyApp.Models;

public class CreateMedicineDto
{
    [Required, MaxLength(500)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string Notes { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiryDate { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Required, MaxLength(200)]
    public string Brand { get; set; } = string.Empty;
}
