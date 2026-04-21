using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Api.Controllers;

public class AuthController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;

    public AuthController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDataDto>> Register(RegisterDto dto)
    {
        var existing = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
        if (existing != null)
            return BadRequest("El correo ya está registrado.");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _unitOfWork.Users.Add(user);
        await _unitOfWork.SaveAsync();

        var role = await _unitOfWork.Roles.GetByNameAsync("User");
        await _unitOfWork.UserRoles.AddAsync(new UserRole { UserId = user.Id, RoleId = role!.Id });
        await _unitOfWork.SaveAsync();

        return CreatedAtAction(nameof(Me), new UserDataDto(user.Id, user.FullName, user.Email, "User"));
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDataDto>> Login(LoginDto dto)
    {
        var user = await _unitOfWork.Users.GetByEmailWithRolesAsync(dto.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Credenciales inválidas.");

        var role = user.UserRoles.FirstOrDefault()?.Role?.Name ?? "User";
        var token = GenerateToken(user, role);

        Response.Cookies.Append("token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddMinutes(
                int.Parse(Environment.GetEnvironmentVariable("JWT_DURATION_MINUTES") ?? "120"))
        });

        return Ok(new UserDataDto(user.Id, user.FullName, user.Email, role));
    }

    [HttpPost("logout")]
    public ActionResult Logout()
    {
        Response.Cookies.Delete("token");
        return Ok(new { message = "Sesión cerrada" });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDataDto>> Me()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(userId);
        if (user is null) return Unauthorized();

        var role = user.UserRoles.FirstOrDefault()?.Role?.Name ?? "User";
        return Ok(new UserDataDto(user.Id, user.FullName, user.Email, role));
    }

    private string GenerateToken(User user, string role)
    {
        var key = Environment.GetEnvironmentVariable("JWT_KEY")!;
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
        var minutes = int.Parse(Environment.GetEnvironmentVariable("JWT_DURATION_MINUTES") ?? "120");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, role)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}