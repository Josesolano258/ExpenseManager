using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Authorize]
public class CategoriesController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoriesController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> Get()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<CategoryDto>>(categories));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> Get(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category is null) return NotFound();
        return Ok(_mapper.Map<CategoryDto>(category));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Post(CategoryCreateDto dto)
    {
        var exists = await _unitOfWork.Categories.ExistsByNameAsync(dto.Name);
        if (exists) return BadRequest("La categoría ya existe.");

        var category = _mapper.Map<Category>(dto);
        _unitOfWork.Categories.Add(category);
        await _unitOfWork.SaveAsync();

        return CreatedAtAction(nameof(Get), new { id = category.Id }, _mapper.Map<CategoryDto>(category));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> Put(int id, CategoryUpdateDto dto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category is null) return NotFound();

        _mapper.Map(dto, category);
        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveAsync();

        return Ok(_mapper.Map<CategoryDto>(category));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category is null) return NotFound();

        _unitOfWork.Categories.Remove(category);
        await _unitOfWork.SaveAsync();

        return NoContent();
    }
}