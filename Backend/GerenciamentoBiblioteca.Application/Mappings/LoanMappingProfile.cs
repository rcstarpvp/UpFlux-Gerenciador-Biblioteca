using AutoMapper;

using GerenciamentoBiblioteca.Application.DTOs;
using GerenciamentoBiblioteca.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerenciamentoBiblioteca.Application.Mappings
{
    public class LoanMappingProfile : Profile
    {
        public LoanMappingProfile()
        {
            CreateMap<Loan, LoanDto>()
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book.Title))
                .ForMember(dest => dest.BookAuthor, opt => opt.MapFrom(src => src.Book.Author));

            CreateMap<CreateLoanDto, Loan>()
                .ForMember(dest => dest.Borrowed, opt => opt.MapFrom(src => DateTime.UtcNow));

            CreateMap<UpdateLoanDto, Loan>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.BookId, opt => opt.Ignore())
                .ForMember(dest => dest.Borrowed, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Book, opt => opt.Ignore());
        }
    }
}
