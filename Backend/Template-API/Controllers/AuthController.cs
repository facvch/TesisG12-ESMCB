using Application.DataTransferObjects;
using Application.Repositories;
using Core.Application;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Controllers
{
    [ApiController]
    public class AuthController(
        IUsuarioRepository usuarioRepo,
        IRolRepository rolRepo,
        IConfiguration config) : BaseController
    {
        private readonly IUsuarioRepository _usuarioRepo = usuarioRepo;
        private readonly IRolRepository _rolRepo = rolRepo;
        private readonly IConfiguration _config = config;

        /// <summary>
        /// Login - devuelve JWT token
        /// </summary>
        [HttpPost("api/v1/auth/login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var usuario = await _usuarioRepo.GetByNombreUsuarioAsync(request.NombreUsuario);
            if (usuario == null || !usuario.Activo)
                return Unauthorized("Usuario o contraseña incorrectos");

            if (!usuario.VerifyPassword(request.Password))
                return Unauthorized("Usuario o contraseña incorrectos");

            usuario.RegistrarLogin();
            _usuarioRepo.Update(usuario.Id, usuario);

            var token = GenerateJwtToken(usuario);
            var expiry = DateTime.UtcNow.AddHours(8);

            return Ok(new LoginResponseDto
            {
                Token = token,
                Expiracion = expiry,
                Usuario = MapToDto(usuario)
            });
        }

        /// <summary>
        /// Registrar nuevo usuario (solo Admin)
        /// </summary>
        [HttpPost("api/v1/auth/register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var existingByUsername = await _usuarioRepo.GetByNombreUsuarioAsync(request.NombreUsuario);
            if (existingByUsername != null)
                return BadRequest($"Ya existe un usuario con el nombre '{request.NombreUsuario}'");

            var existingByEmail = await _usuarioRepo.GetByEmailAsync(request.Email);
            if (existingByEmail != null)
                return BadRequest($"Ya existe un usuario con el email '{request.Email}'");

            var rol = await _rolRepo.FindOneAsync(request.RolId);
            if (rol == null) return BadRequest($"No existe el rol con Id {request.RolId}");

            var usuario = new Usuario(request.NombreUsuario, request.Email,
                request.NombreCompleto, request.Password, request.RolId);

            if (!usuario.IsValid) return BadRequest(usuario.GetErrors().Select(e => e.ErrorMessage));
            var id = await _usuarioRepo.AddAsync(usuario);
            return Created($"api/v1/auth/usuarios/{id}", MapToDto(usuario));
        }

        /// <summary>
        /// Seed inicial - crea roles por defecto y usuario admin (solo si no hay usuarios)
        /// </summary>
        [HttpPost("api/v1/auth/seed")]
        public async Task<IActionResult> Seed()
        {
            var existingUsers = await _usuarioRepo.GetActivosAsync();
            if (existingUsers.Any())
                return BadRequest("Ya existen usuarios en el sistema. No se puede hacer seed.");

            // Crear roles
            var roles = new[] { "Admin", "Veterinario", "Recepcionista" };
            foreach (var rolName in roles)
            {
                var existing = await _rolRepo.GetByNombreAsync(rolName);
                if (existing == null)
                    await _rolRepo.AddAsync(new Rol(rolName, $"Rol de {rolName}"));
            }

            var adminRol = await _rolRepo.GetByNombreAsync("Admin");

            // Crear usuario admin
            var admin = new Usuario("admin", "admin@veterinaria.com",
                "Administrador del Sistema", "Admin123!", adminRol.Id);
            await _usuarioRepo.AddAsync(admin);

            return Ok(new { Message = "Seed completado", AdminUser = "admin", AdminPass = "Admin123!" });
        }

        /// <summary>
        /// Obtener perfil del usuario autenticado
        /// </summary>
        [HttpGet("api/v1/auth/me")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var usuario = await _usuarioRepo.FindOneAsync(userId);
            if (usuario == null) return NotFound();
            return Ok(MapToDto(usuario));
        }

        /// <summary>
        /// Cambiar contraseña del usuario autenticado
        /// </summary>
        [HttpPut("api/v1/auth/cambiarPassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var usuario = await _usuarioRepo.FindOneAsync(userId);
            if (usuario == null) return NotFound();

            if (!usuario.VerifyPassword(request.PasswordActual))
                return BadRequest("La contraseña actual es incorrecta");

            usuario.SetPassword(request.NuevaPassword);
            _usuarioRepo.Update(usuario.Id, usuario);
            return NoContent();
        }

        // ═══════════════════════════════════════════
        // GESTIÓN DE USUARIOS (Admin)
        // ═══════════════════════════════════════════

        [HttpGet("api/v1/auth/usuarios")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var usuarios = await _usuarioRepo.GetActivosAsync();
            return Ok(usuarios.Select(MapToDto).ToList());
        }

        [HttpGet("api/v1/auth/roles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _rolRepo.GetActivosAsync();
            return Ok(roles.Select(r => new RolDto
            {
                Id = r.Id, Nombre = r.Nombre, Descripcion = r.Descripcion, Activo = r.Activo
            }).ToList());
        }

        [HttpDelete("api/v1/auth/usuarios/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var usuario = await _usuarioRepo.FindOneAsync(id);
            if (usuario == null) return NotFound();
            usuario.Desactivar();
            _usuarioRepo.Update(id, usuario);
            return NoContent();
        }

        // ═══════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════

        private string GenerateJwtToken(Usuario usuario)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "VeterinariaSecretKeyMuyLargaParaDesarrollo2024!@#$"));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id),
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.GivenName, usuario.NombreCompleto),
                new Claim(ClaimTypes.Role, usuario.Rol?.Nombre ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "VeterinariaAPI",
                audience: _config["Jwt:Audience"] ?? "VeterinariaApp",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static UsuarioDto MapToDto(Usuario u) => new()
        {
            Id = u.Id, NombreUsuario = u.NombreUsuario, Email = u.Email,
            NombreCompleto = u.NombreCompleto, RolId = u.RolId,
            RolNombre = u.Rol?.Nombre ?? "", FechaCreacion = u.FechaCreacion,
            UltimoLogin = u.UltimoLogin, Activo = u.Activo
        };
    }

    public class LoginRequest
    {
        public string NombreUsuario { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        public string NombreUsuario { get; set; }
        public string Email { get; set; }
        public string NombreCompleto { get; set; }
        public string Password { get; set; }
        public int RolId { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string PasswordActual { get; set; }
        public string NuevaPassword { get; set; }
    }
}
