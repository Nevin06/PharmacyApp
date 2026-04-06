using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Models;
using PharmacyApp.Services;

namespace PharmacyApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly PharmacyJsonStore _store;

    public SalesController(PharmacyJsonStore store)
    {
        _store = store;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Sale>> GetAll()
    {
        return Ok(_store.GetSales());
    }

    [HttpPost]
    public ActionResult<Sale> Create([FromBody] CreateSaleDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var sale = _store.RecordSale(dto, out var error);
        if (sale == null)
            return BadRequest(new { message = error });
        return CreatedAtAction(nameof(GetAll), new { id = sale.Id }, sale);
    }
}
