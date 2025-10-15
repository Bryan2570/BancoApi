using Azure.Core;
using BancoApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BancoApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly DbBancoPruebaContext _dbBankContext;
        public ClienteController(DbBancoPruebaContext dbBankContext)
        {
            _dbBankContext = dbBankContext;
        }

        [HttpGet]
        [Route("Lista")]
        public async Task<IActionResult> ListClients()
        {
            var listClient = await _dbBankContext.Clientes
                .Include(c => c.IdPersonaNavigation)
                .ToListAsync();
            return Ok(listClient);
        }

        [HttpGet]
        [Route("Lista/{id:int}")]
        public async Task<IActionResult> GetClient(int id)
        {
            var client = await _dbBankContext.Clientes
                .Include(c => c.IdPersonaNavigation)    
                .FirstOrDefaultAsync(c => c.IdPersona == id);
            if (client == null)
                return NotFound("No existe el Cliente");
            return Ok(client);
        }

        [HttpPost]
        [Route("Agregar")]

        public async Task<IActionResult> Agregar([FromBody] Cliente requets)
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

        [HttpPut]
        [Route("Actualizar/{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] Cliente request)
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

        [HttpDelete]
        [Route("Eliminar/{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        { 
            var clientDelete = await _dbBankContext.Clientes.FindAsync(id);
            if (clientDelete == null)            
                return NotFound("No existe el Cliente");
                _dbBankContext.Clientes.Remove(clientDelete);
                await _dbBankContext.SaveChangesAsync();
                return Ok();             
        }
        
    }
}
