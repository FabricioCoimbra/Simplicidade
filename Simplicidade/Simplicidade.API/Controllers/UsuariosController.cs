using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Simplicidade.API.Data;
using Simplicidade.API.Services;
using Simplicidade.Compartilhada.Model;
using Simplicidade.Compartilhada.Model.ViewModel;

namespace Simplicidade.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        //TODO remover dependência direta do contexto de dados ...
        private readonly APIDataContext _context;

        public UsuariosController(APIDataContext context)
        {
            _context = context;
        }

        // POST: api/Login
        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]// único método que permite acessar sem autorização... 
        public async Task<IActionResult> Login(
                   [FromServices] APIDataContext context,
                   [FromBody] Usuario usuario)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                //TODO gravar usuário e senha criptografados, e retornar essa criptografia para comparação
                var user = await context.Usuario.FirstOrDefaultAsync(
                    x => x.Username.ToLower() == usuario.Username.ToLower() && x.Password == usuario.Password);

                if (user == null)
                    return StatusCode(404, "Usuário ou senha inválidos");

                var token = TokenService.GenerateToken(usuario);
                usuario.Password = "";//não retornar senha descriptografada
                // mais uma dependência 
                return Ok(new UsuarioViewModel
                {
                    Usuario = usuario,
                    Token = token
                });
            }
            catch
            {
                return StatusCode(500, "Falha na autenticação");
            }
        }

        // POST: api/Usuarios
        [HttpPost]
        [AllowAnonymous] //TODO apenas para testes permitir qualquer usuário criar um usuário
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            _context.Usuario.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUsuario", new { id = usuario.Id }, usuario);
        }

        // GET: api/Usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuario()
        {
            return await _context.Usuario.ToListAsync();
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _context.Usuario.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            return usuario;
        }

        // PUT: api/Usuarios/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return BadRequest();
            }

            _context.Entry(usuario).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsuarioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Usuarios/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Usuario>> DeleteUsuario(int id)
        {
            //TODO deletar pode não ser uma boa ideia para usuários.... 
            // verificar se será implementada uma flag ou permitir apenas se não houver histórico de login ... 
            var usuario = await _context.Usuario.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.Usuario.Remove(usuario);
            await _context.SaveChangesAsync();

            return usuario;
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuario.Any(e => e.Id == id);
        }
    }
}