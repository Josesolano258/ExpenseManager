namespace Application.DTOs;

public record RegisterDto(string FullName, string Email, string Password);
public record LoginDto(string Email, string Password);
public record UserDataDto(int Id, string FullName, string Email, string Role);