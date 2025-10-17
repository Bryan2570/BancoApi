using BancoApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BancoApi.Controllers
{
    [Route("api/v1/account")]
    [ApiController]
    public class CuentaController : ControllerBase
    {
        private readonly DbBancoPruebaContext _dbBankContext;

        public CuentaController(DbBancoPruebaContext dbBankContext)
        {
            _dbBankContext = dbBankContext;
        }

        [HttpGet]       
        public async Task<IActionResult> ListCuentas()
        {
            try
            {
                var listCuentas = await _dbBankContext.Cuenta.ToListAsync();
                return Ok(listCuentas);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocurrió un error al listar las Cuentas", error = ex.Message });
            }            
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCuenta(int id)
        {
            try
            {
                var listCuentas = await _dbBankContext.Cuenta                
           .Where(ct => ct.IdCliente == id)
           .Select(ct => new
           {  
               ct.NumCuenta,
               ct.TipoCuenta,
               ct.SaldoInicial,
               ct.Estado
           })
           .ToListAsync();

                if (listCuentas == null || !listCuentas.Any())
                    return NotFound("No existen cuentas para el cliente indicado");

                return Ok(listCuentas);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocurrió un error al listar la Cuenta", error = ex.Message });
            }            
        }

       
        [HttpPost]
        public async Task<IActionResult> Agregar([FromBody] Cuentum request)
        {
            try
            {
                _dbBankContext.Cuenta.Add(request);
                await _dbBankContext.SaveChangesAsync();
                return Ok(new
                {
                    message = "Cuenta Creada Correctamente!",
                    cuenta = request
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocurrió un error al crear la Cuenta", error = ex.Message });
            }           
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] Cuentum request)
        {
            try
            {
                var cuenta = await _dbBankContext.Cuenta
                .FirstOrDefaultAsync(c => c.IdCuenta == id);
                if (cuenta == null)
                    return NotFound("No existe la Cuenta");
                cuenta.NumCuenta = request.NumCuenta;
                cuenta.Estado = request.Estado;
                cuenta.SaldoInicial = request.SaldoInicial;
                cuenta.TipoCuenta = request.TipoCuenta;
                await _dbBankContext.SaveChangesAsync();
                return Ok(new
                {
                    message = "Cuenta Actualizada Correctamente!",
                    cuenta = cuenta
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocurrió un error al actualizar la Cuenta", error = ex.Message });
            }            
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var cuenta = await _dbBankContext.Cuenta
                .FirstOrDefaultAsync(c => c.IdCuenta == id);
                if (cuenta == null)
                    return NotFound("No existe la Cuenta");
                _dbBankContext.Cuenta.Remove(cuenta);
                await _dbBankContext.SaveChangesAsync();
                return Ok(new
                {
                    message = "Cuenta Eliminada Correctamente!",
                    cuenta = cuenta
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocurrió un error al eliminar la Cuenta", error = ex.Message });
            }            
        }
    }
}
