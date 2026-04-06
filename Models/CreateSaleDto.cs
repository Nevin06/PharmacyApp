using System.ComponentModel.DataAnnotations;

namespace PharmacyApp.Models;

public class CreateSaleDto
{
    [Required]
    public int MedicineId { get; set; }

    [Range(1, int.MaxValue)]
    public int QuantitySold { get; set; }
}
