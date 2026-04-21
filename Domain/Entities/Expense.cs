namespace Domain.Entities;

public class Expense : BaseEntity
{
    public string Concept { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpenseDate { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
}