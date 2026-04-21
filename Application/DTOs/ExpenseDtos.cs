namespace Application.DTOs;

public class ExpenseDto
{
    public int Id { get; set; }
    public string Concept { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
}

public record ExpenseCreateDto(
    string Concept,
    decimal Amount,
    DateTime ExpenseDate,
    int CategoryId);

public record ExpenseUpdateDto(
    string Concept,
    decimal Amount,
    DateTime ExpenseDate,
    int CategoryId);