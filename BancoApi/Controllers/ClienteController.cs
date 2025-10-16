using Azure.Core;
using BancoApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BancoApi.Controllers
{
    [Route("api/v1/client")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly DbBancoPruebaContext _dbBankContext;
        public ClienteController(DbBancoPruebaContext dbBankContext)
        {
            _dbBankContext = dbBankContext;
        }    

        [HttpGet]     
        public async Task<IActionResult> ListClients()
        {
            try
            {
                var listClient = await _dbBankContext.Clientes
                .Include(c => c.IdPersonaNavigation)
                .ToListAsync();
                return Ok(listClient);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocurrió un error al listar los Clientes", error = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetClient(int id)
        {
            try
            {
                var client = await _dbBankContext.Clientes
                .Include(c => c.IdPersonaNavigation)
                .FirstOrDefaultAsync(c => c.IdCliente == id);
                if (client == null)
                    return NotFound("No existe el Cliente");
                return Ok(client);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocurrió un error al listar el Cliente", error = ex.Message });
            }            
        }

        [HttpPost]
        public async Task<IActionResult> Agregar([FromBody] Cliente requets)
        {
            try
            {
                if (requets.IdPersonaNavigation != null)
                {
                    _dbBankContext.Personas.Add(requets.IdPersonaNavigation);
                    await _dbBankContext.SaveChangesAsync();
                    requets.IdPersona = requets.IdPersonaNavigation.IdPersona;
                }

                _dbBankContext.Clientes.Add(requets);
                await _dbBankContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Cliente agregado correctamente",
                    cliente = requets
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocurrió un error al crear el Cliente", error = ex.Message });
            }            
        }

        [HttpPut("{id:int}")]        
        public async Task<IActionResult> Actualizar(int id, [FromBody] Cliente request)
        {
            try
            {
                if (id != request.IdCliente)
                    return BadRequest("No existe el Cliente");

                var clientUpdate = await _dbBankContext.Clientes
                    .Include(c => c.IdPersonaNavigation)
                    .FirstOrDefaultAsync(c => c.IdCliente == id);
                if (clientUpdate == null)
                    return NotFound("No existe el Cliente");

                clientUpdate.Contrasena = request.Contrasena;
                clientUpdate.Estado = request.Estado;
                clientUpdate.IdPersona = request.IdPersona;

                if (request.IdPersonaNavigation != null)
                {
                    if (clientUpdate.IdPersonaNavigation == null)
                    {
                        clientUpdate.IdPersonaNavigation = new Persona();
                    }
                    clientUpdate.IdPersonaNavigation.Nombre = request.IdPersonaNavigation.Nombre;
                    clientUpdate.IdPersonaNavigation.Genero = request.IdPersonaNavigation.Genero;
                    clientUpdate.IdPersonaNavigation.Edad = request.IdPersonaNavigation.Edad;
                    clientUpdate.IdPersonaNavigation.Identificacion = request.IdPersonaNavigation.Identificacion;
                    clientUpdate.IdPersonaNavigation.Direccion = request.IdPersonaNavigation.Direccion;
                    clientUpdate.IdPersonaNavigation.Telefono = request.IdPersonaNavigation.Telefono;
                    _dbBankContext.Personas.Update(clientUpdate.IdPersonaNavigation);
                }
                await _dbBankContext.SaveChangesAsync();
                return Ok(clientUpdate);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocurrió un error al actualiza el Clientes", error = ex.Message });
            }            
        }

        [HttpDelete("{id:int}")]       
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var clientDelete = await _dbBankContext.Clientes.FindAsync(id);
                if (clientDelete == null)
                    return NotFound("No existe el Cliente");
                _dbBankContext.Clientes.Remove(clientDelete);
                await _dbBankContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Ocurrió un error al eliminar el Cliente", error = ex.Message });
            }                 
        }        
    }
}
