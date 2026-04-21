using Application.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Api.Profiles;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Category, CategoryDto>();
        CreateMap<CategoryCreateDto, Category>();
        CreateMap<CategoryUpdateDto, Category>();

        CreateMap<Expense, ExpenseDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : string.Empty))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User != null ? s.User.FullName : string.Empty));

        CreateMap<ExpenseCreateDto, Expense>();
        CreateMap<ExpenseUpdateDto, Expense>();
    }
} 