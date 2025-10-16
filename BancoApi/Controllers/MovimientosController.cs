using BancoApi.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

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


        [HttpGet]
        [Route("EstadoCuenta/{id:int}")]
        public async Task<IActionResult> GenerarReporte([FromQuery] int clienteId, [FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        {
            // 1️⃣ Obtener cliente
            var cliente = await _dbBankContext.Clientes
                .Include(cl => cl.IdPersonaNavigation)
                .FirstOrDefaultAsync(cl => cl.IdCliente == clienteId);

            if (cliente == null)
                return NotFound("El cliente no existe.");

            // 2️⃣ Obtener todas las cuentas del cliente
            var cuentas = await _dbBankContext.Cuenta
                .Where(c => c.IdCliente == clienteId)
                .ToListAsync();

            if (!cuentas.Any())
                return NotFound("El cliente no tiene cuentas asociadas.");

            // 3️⃣ Para cada cuenta, obtener movimientos y calcular totales
            var cuentasReporte = new List<object>();
            foreach (var cuenta in cuentas)
            {
                var movimientos = await _dbBankContext.Movimientos
                    .Where(m => m.IdCuenta == cuenta.IdCuenta &&
                                m.Fecha >= fechaInicio &&
                                m.Fecha <= fechaFin)
                    .OrderBy(m => m.Fecha)
                    .ToListAsync();

                decimal saldoInicial = cuenta.SaldoInicial;
                decimal totalCreditos = movimientos
                    .Where(m => m.TipoMovimiento == "DEPOSITO")
                    .Sum(m => m.Valor);

                decimal totalDebitos = movimientos
                    .Where(m => m.TipoMovimiento == "RETIRO")
                    .Sum(m => m.Valor);

                decimal saldoFinal = saldoInicial + totalCreditos - totalDebitos;

                cuentasReporte.Add(new
                {
                    NumeroCuenta = cuenta.NumCuenta,
                    SaldoInicial = saldoInicial,
                    SaldoFinal = saldoFinal,
                    TotalCreditos = totalCreditos,
                    TotalDebitos = totalDebitos,
                    Movimientos = movimientos.Select(m => new
                    {
                        m.IdMovimiento,
                        m.Fecha,
                        m.TipoMovimiento,
                        m.Valor,
                        m.Saldo
                    }).ToList()
                });
            }

            // 4️⃣ Armar JSON final
            var resultado = new
            {
                Cliente = cliente.IdPersonaNavigation.Nombre,
                Cuentas = cuentasReporte
            };

            // 5️⃣ Generar PDF en memoria
            using var ms = new MemoryStream();
            var document = new iTextSharp.text.Document();
            PdfWriter.GetInstance(document, ms);
            document.Open();

           
            var tituloFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, BaseColor.BLACK);
          
            Paragraph titulo = new Paragraph("INFORME DE MOVIMIENTOS", tituloFont)
            {
                Alignment = Element.ALIGN_CENTER, 
                SpacingAfter = 20f 
            };

            // Agregar título al documento
            document.Add(titulo);


            document.Add(new Paragraph($"Nombre: {resultado.Cliente}"));
            document.Add(new Paragraph($"Fecha Inicial: {fechaInicio:yyyy-MM-dd}"));
            document.Add(new Paragraph($"Fecha Final: {fechaFin:yyyy-MM-dd}"));
            document.Add(new Paragraph(" "));

            foreach (var cuenta in cuentasReporte)
            {
                document.Add(new Paragraph($"Cuenta: {cuenta.GetType().GetProperty("NumeroCuenta").GetValue(cuenta)}"));
                document.Add(new Paragraph($"Saldo Inicial: {cuenta.GetType().GetProperty("SaldoInicial").GetValue(cuenta):C}"));
                document.Add(new Paragraph($"Saldo Final: {cuenta.GetType().GetProperty("SaldoFinal").GetValue(cuenta):C}"));
                document.Add(new Paragraph($"Total Depósitos: {cuenta.GetType().GetProperty("TotalCreditos").GetValue(cuenta):C}"));
                document.Add(new Paragraph($"Total Retiros: {cuenta.GetType().GetProperty("TotalDebitos").GetValue(cuenta):C}"));
                document.Add(new Paragraph(" "));

                PdfPTable table = new PdfPTable(4);
                table.AddCell("Fecha");
                table.AddCell("Tipo");
                table.AddCell("Valor");
                table.AddCell("Saldo");

                var movimientos = cuenta.GetType().GetProperty("Movimientos").GetValue(cuenta) as IEnumerable<object>;
                foreach (var mov in movimientos)
                {
                    var fecha = mov.GetType().GetProperty("Fecha").GetValue(mov);
                    var tipo = mov.GetType().GetProperty("TipoMovimiento").GetValue(mov);
                    var valor = mov.GetType().GetProperty("Valor").GetValue(mov);
                    var saldo = mov.GetType().GetProperty("Saldo").GetValue(mov);

                    table.AddCell(((DateTime)fecha).ToString("yyyy-MM-dd"));
                    table.AddCell(tipo.ToString());
                    table.AddCell(Convert.ToDecimal(valor).ToString("C"));
                    table.AddCell(Convert.ToDecimal(saldo).ToString("C"));
                }

                PdfPTable tableStyle = new PdfPTable(4)
                {
                    WidthPercentage = 100,
                     SpacingAfter = 20f
                };
                tableStyle.SetWidths(new float[] { 2f, 2f, 2f, 2f });

                document.Add(table);
                document.Add(new Paragraph(" "));
            }

            document.Close();

            // 6️⃣ Convertir a Base64
            string pdfBase64 = Convert.ToBase64String(ms.ToArray());

            // 7️⃣ Respuesta final
            return Ok(new
            {
                EstadoCuenta = resultado,
                PdfBase64 = pdfBase64
            });
        }



    }
}
