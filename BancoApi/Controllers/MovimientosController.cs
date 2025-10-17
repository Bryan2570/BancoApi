using BancoApi.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace BancoApi.Controllers
{
    [Route("api/v1/movements")]
    [ApiController]
    public class MovimientosController : ControllerBase
    {
        private readonly DbBancoPruebaContext _dbBankContext;
        public MovimientosController(DbBancoPruebaContext dbBankContext)
        {
            _dbBankContext = dbBankContext;
        }

        [HttpGet]       
       
        public async Task<IActionResult> ListMovimientos()
        {
            try
            {
                var listMovimientos = await _dbBankContext.Movimientos.ToListAsync();
                return Ok(listMovimientos);
            }
            catch (Exception ex)
            {
               return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocurrió un error al listar los movimientos", error = ex.Message });           
            }            
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetMovimiento(int id)
        {
            try
            {
                var listmovimiento = await _dbBankContext.Movimientos
                .Where(mv => mv.IdCuenta == id)
               .Select(mv => new
               {
                   mv.Fecha,
                   mv.TipoMovimiento,
                   mv.Valor,
                   mv.Saldo              
               })
               .ToListAsync();

                if (listmovimiento == null || !listmovimiento.Any())
                    return NotFound("No existen movimientos para la cuenta indicada");

                return Ok(listmovimiento);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocurrió un error al obtener el movimiento", error = ex.Message });
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> Agregar([FromBody] Movimiento request)
        //{
        //    _dbBankContext.Movimientos.Add(request);
        //    await _dbBankContext.SaveChangesAsync();
        //    return Ok(new
        //    {
        //        message = "Movimiento Creado Correctamente!",
        //        movimiento = request
        //    });
        //}        

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] Movimiento request)
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocurrió un error al actualizar el movimiento", error = ex.Message });
            }
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocurrió un error al intentar eliminar el movimiento", error = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> RegistrarMovimiento([FromBody] Movimiento request)
        {
            try
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
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocurrió un error al registrar el movimiento", error = ex.Message });
            }
        }

        [HttpGet]
        [Route("generatePDF")]
        public async Task<IActionResult> GenerarReporte([FromQuery] int id, [FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
        {
            try
            {
                var cliente = await _dbBankContext.Clientes
                .Include(cl => cl.IdPersonaNavigation)
                .FirstOrDefaultAsync(cl => cl.IdCliente == id);

                if (cliente == null)
                    return NotFound("El cliente no existe.");


                var cuentas = await _dbBankContext.Cuenta
                    .FirstOrDefaultAsync(cl => cl.IdCliente == id);

                var cuentasReporte = new List<object>();

                var movimientos = await _dbBankContext.Movimientos
                    .Where(m => m.IdCuenta == id &&
                                m.Fecha >= fechaInicio &&
                                m.Fecha <= fechaFin)
                    .OrderBy(m => m.Fecha)
                    .ToListAsync();

                decimal saldoInicial = cuentas.SaldoInicial;
                decimal totalCreditos = movimientos
                    .Where(m => m.TipoMovimiento == "DEPOSITO")
                    .Sum(m => m.Valor);

                decimal totalDebitos = movimientos
                    .Where(m => m.TipoMovimiento == "RETIRO")
                    .Sum(m => m.Valor);

                decimal saldoFinal = saldoInicial + totalCreditos - totalDebitos;

                cuentasReporte.Add(new
                {
                    NumeroCuenta = cuentas.NumCuenta,
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

                var resultado = new
                {
                    Cliente = cliente.IdPersonaNavigation.Nombre,
                    Cuentas = cuentasReporte
                };

                // Generar PDF 
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

                // Título
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

                    var movimientosPDF = cuenta.GetType().GetProperty("Movimientos").GetValue(cuenta) as IEnumerable<object>;
                    foreach (var mov in movimientosPDF)
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

                string pdfBase64 = Convert.ToBase64String(ms.ToArray());

                return Ok(new
                {
                    EstadoCuenta = resultado,
                    PdfBase64 = pdfBase64
                });            
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocurrió un error al generar el informe en PDF", error = ex.Message });
            }
        }   

    }
}
