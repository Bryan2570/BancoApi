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
            var listClient = await _dbBankContext.Clientes.ToListAsync();
            return Ok(listClient);
        }

        [HttpPost]
        [Route("Agregar")]

        public async Task<IActionResult> Agregar([FromBody] Cliente requets)
        {
            await _dbBankContext.Clientes.AddAsync(requets);
            await _dbBankContext.SaveChangesAsync();
            return Ok(requets);
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
