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
            var listCuentas = await _dbBankContext.Cuenta.ToListAsync();
            return Ok(listCuentas);
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCuenta(int id)
        {
            var cuenta = await _dbBankContext.Cuenta
                .FirstOrDefaultAsync(c => c.IdCuenta == id);
            if (cuenta == null)
                return NotFound("No existe la Cuenta");
            return Ok(cuenta);
        }

        [HttpPost]
        public async Task<IActionResult> Agregar([FromBody] Cuentum request)
        {
            _dbBankContext.Cuenta.Add(request);
            await _dbBankContext.SaveChangesAsync();
            return Ok(new
            {
                message = "Cuenta Creada Correctamente!",
                cuenta = request
            });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] Cuentum request)
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

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
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
    }
}
