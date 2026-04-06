using Microsoft.AspNetCore.Mvc;
using PharmacyApp.Models;
using PharmacyApp.Services;

namespace PharmacyApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MedicinesController : ControllerBase
{
    private readonly PharmacyJsonStore _store;

    public MedicinesController(PharmacyJsonStore store)
    {
        _store = store;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Medicine>> GetAll([FromQuery] string? search)
    {
        return Ok(_store.GetMedicines(search));
    }

    [HttpGet("{id:int}")]
    public ActionResult<Medicine> GetById(int id)
    {
        var m = _store.GetMedicineById(id);
        if (m == null) return NotFound();
        return Ok(m);
    }

    [HttpPost]
    public ActionResult<Medicine> Create([FromBody] CreateMedicineDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var created = _store.AddMedicine(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Medicine> Update(int id, [FromBody] CreateMedicineDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var updated = _store.UpdateMedicine(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        if (!_store.DeleteMedicine(id)) return NotFound();
        return NoContent();
    }
}
