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


        [HttpPost]
        [Route("RegistrarMovimiento")]
        public async Task<IActionResult> RegistrarMovimiento([FromBody] Movimiento request)
        {
            if (request == null)
                return BadRequest("Request inválido.");

            if (request.IdCuenta <= 0)
                return BadRequest("IdCuenta inválido.");

            if (request.Valor <= 0)
                return BadRequest("El valor debe ser mayor que cero.");

            if (string.IsNullOrWhiteSpace(request.TipoMovimiento))
                return BadRequest("TipoMovimiento es requerido (RETIRO o DEPOSITO).");

            var tipo = request.TipoMovimiento.Trim().ToUpperInvariant();

            if (tipo != "RETIRO" && tipo != "DEPOSITO")
                return BadRequest("TipoMovimiento inválido. Debe ser 'RETIRO' o 'DEPOSITO'.");

            // Buscar cuenta
            var cuenta = await _dbBankContext.Cuenta
                .FirstOrDefaultAsync(c => c.IdCuenta == request.IdCuenta);

            if (cuenta == null)
                return NotFound("La cuenta no existe");

            decimal saldoActual = cuenta.SaldoInicial;

            if (tipo == "RETIRO")
            {
                const decimal LIMITE_DIARIO = 1000m;

                var inicioDia = DateTime.Today;
                var finDia = inicioDia.AddDays(1);

                var totalRetirosHoy = await _dbBankContext.Movimientos
                    .Where(m => m.IdCuenta == cuenta.IdCuenta &&
                                m.TipoMovimiento.ToUpper() == "RETIRO" &&
                                m.Fecha >= inicioDia && m.Fecha < finDia)
                    .SumAsync(m => m.Valor);

                decimal retiroSolicitado = request.Valor;

                if (totalRetirosHoy + retiroSolicitado > LIMITE_DIARIO)
                    return BadRequest("Cupo diario Excedido");

                if (saldoActual < retiroSolicitado)
                    return BadRequest("Saldo no disponible");

                saldoActual -= retiroSolicitado;
            }
            else if (tipo == "DEPOSITO")
            {
                saldoActual += request.Valor;
            }

            // Actualizar cuenta y movimiento
            request.Fecha = DateTime.Now;
            request.Saldo = saldoActual;

            cuenta.SaldoInicial = saldoActual;

            _dbBankContext.Movimientos.Add(request);
            await _dbBankContext.SaveChangesAsync();

            return Ok(new
            {
                message = "Movimiento registrado correctamente",
                saldoActual = saldoActual,
                movimiento = request
            });
        }





    }
}
