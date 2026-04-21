namespace Application.DTOs;

public record CategoryDto(int Id, string Name);
public record CategoryCreateDto(string Name);
public record CategoryUpdateDto(string Name);