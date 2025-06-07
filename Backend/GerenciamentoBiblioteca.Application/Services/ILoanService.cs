using GerenciamentoBiblioteca.Application.DTOs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerenciamentoBiblioteca.Application.Services
{
    public interface ILoanService
    {
        Task<LoanDto?> GetByIdAsync(int id);
        Task<IEnumerable<LoanDto>> GetAllAsync();
        Task<IEnumerable<LoanDto>> GetByBookIdAsync(int bookId);
        Task<IEnumerable<LoanDto>> GetActiveLoansAsync();
        Task<LoanDto> CreateAsync(CreateLoanDto createLoanDto);
        Task<LoanDto?> UpdateAsync(int id, UpdateLoanDto updateLoanDto);
        Task<bool> DeleteAsync(int id);
        Task<LoanDto?> ReturnBookAsync(int loanId);
    }
}
