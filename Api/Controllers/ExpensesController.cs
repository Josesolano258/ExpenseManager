using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[Authorize]
public class ExpensesController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ExpensesController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    private int GetCurrentUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin() => User.IsInRole("Admin");

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> Get([FromQuery] int? categoryId)
    {
        var userId = GetCurrentUserId();
        var expenses = await _unitOfWork.Expenses.GetFilteredAsync(categoryId, userId, IsAdmin());
        return Ok(_mapper.Map<IEnumerable<ExpenseDto>>(expenses));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExpenseDto>> Get(int id)
    {
        var expense = await _unitOfWork.Expenses.GetByIdWithRelationsAsync(id);
        if (expense is null) return NotFound();

        if (!IsAdmin() && expense.UserId != GetCurrentUserId())
            return Forbid();

        return Ok(_mapper.Map<ExpenseDto>(expense));
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Post(ExpenseCreateDto dto)
    {
        var expense = new Expense
        {
            Concept = dto.Concept,
            Amount = dto.Amount,
            ExpenseDate = dto.ExpenseDate,
            CategoryId = dto.CategoryId,
            UserId = GetCurrentUserId()
        };

        _unitOfWork.Expenses.Add(expense);
        await _unitOfWork.SaveAsync();

        var created = await _unitOfWork.Expenses.GetByIdWithRelationsAsync(expense.Id);
        return CreatedAtAction(nameof(Get), new { id = expense.Id }, _mapper.Map<ExpenseDto>(created));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ExpenseDto>> Put(int id, ExpenseUpdateDto dto)
    {
        var expense = await _unitOfWork.Expenses.GetByIdWithRelationsAsync(id);
        if (expense is null) return NotFound();

        if (!IsAdmin() && expense.UserId != GetCurrentUserId())
            return Forbid();

        expense.Concept = dto.Concept;
        expense.Amount = dto.Amount;
        expense.ExpenseDate = dto.ExpenseDate;
        expense.CategoryId = dto.CategoryId;

        _unitOfWork.Expenses.Update(expense);
        await _unitOfWork.SaveAsync();

        var updated = await _unitOfWork.Expenses.GetByIdWithRelationsAsync(expense.Id);
        return Ok(_mapper.Map<ExpenseDto>(updated));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var expense = await _unitOfWork.Expenses.GetByIdWithRelationsAsync(id);
        if (expense is null) return NotFound();

        if (!IsAdmin() && expense.UserId != GetCurrentUserId())
            return Forbid();

        _unitOfWork.Expenses.Remove(expense);
        await _unitOfWork.SaveAsync();

        return NoContent();
    }
}