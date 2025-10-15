using BancoApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BancoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovimientosController : ControllerBase
    {
        private readonly DbBancoPruebaContext _dbBankContext;
        public MovimientosController(DbBancoPruebaContext dbBankContext)
        {
            _dbBankContext = dbBankContext;
        }

        [HttpGet]
        [Route("Lista")]
        public async Task<IActionResult> ListMovimientos()
        {
            var listMovimientos = await _dbBankContext.Movimientos.ToListAsync();
            return Ok(listMovimientos);
        }

        [HttpGet]
        [Route("Lista/{id:int}")]
        public async Task<IActionResult> GetMovimiento(int id)
        {
            var movimiento = await _dbBankContext.Movimientos
                .FirstOrDefaultAsync(m => m.IdMovimiento == id);
            if (movimiento == null)
                return NotFound("No existe el Movimiento");
            return Ok(movimiento);
        }

        [HttpPost]
        [Route("Crear")]
        public async Task<IActionResult> Agregar([FromBody] Movimiento request)
        {
            _dbBankContext.Movimientos.Add(request);
            await _dbBankContext.SaveChangesAsync();
            return Ok(new
            {
                message = "Movimiento Creado Correctamente!",
                movimiento = request
            });
        }

        [HttpPut]
        [Route("Actualizar/{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] Movimiento request)
        {
            var movimiento = await _dbBankContext.Movimientos
                .FirstOrDefaultAsync(m => m.IdMovimiento == id);
            if (movimiento == null)
                return NotFound("No existe el Movimiento");
            movimiento.Fecha = request.Fecha;
            movimiento.TipoMovimiento = request.TipoMovimiento;
            movimiento.Valor = request.Valor;
            movimiento.Saldo = request.Saldo;
            movimiento.IdCuenta = request.IdCuenta;
            await _dbBankContext.SaveChangesAsync();
            return Ok(new
            {
                message = "Movimiento Actualizado Correctamente!",
                movimiento = movimiento
            });
        }


        [HttpDelete]
        [Route("Eliminar/{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var movimiento = await _dbBankContext.Movimientos
                .FirstOrDefaultAsync(m => m.IdMovimiento == id);
            if (movimiento == null)
                return NotFound("No existe el Movimiento");
            _dbBankContext.Movimientos.Remove(movimiento);
            await _dbBankContext.SaveChangesAsync();
            return Ok(new
            {
                message = "Movimiento Eliminado Correctamente!",
                movimiento = movimiento
            });
        }

    }
}
