using AutoMapper;
using GerenciamentoBiblioteca.Domain.Entities;
using GerenciamentoBiblioteca.Domain.Repositories;
using GerenciamentoBiblioteca.Application.DTOs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerenciamentoBiblioteca.Application.Services.Implementations
{
    public class LoanService : ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;

        public LoanService(ILoanRepository loanRepository, IBookRepository bookRepository, IMapper mapper)
        {
            _loanRepository = loanRepository;
            _bookRepository = bookRepository;
            _mapper = mapper;
        }

        public async Task<LoanDto?> GetByIdAsync(int id)
        {
            var loan = await _loanRepository.GetByIdAsync(id);
            return loan == null ? null : _mapper.Map<LoanDto>(loan);
        }

        public async Task<IEnumerable<LoanDto>> GetAllAsync()
        {
            var loans = await _loanRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<LoanDto>>(loans);
        }

        public async Task<IEnumerable<LoanDto>> GetByBookIdAsync(int bookId)
        {
            var loans = await _loanRepository.GetByBookIdAsync(bookId);
            return _mapper.Map<IEnumerable<LoanDto>>(loans);
        }

        public async Task<IEnumerable<LoanDto>> GetActiveLoansAsync()
        {
            var loans = await _loanRepository.GetActiveLoansAsync();
            return _mapper.Map<IEnumerable<LoanDto>>(loans);
        }

        public async Task<LoanDto> CreateAsync(CreateLoanDto createLoanDto)
        {
            var book = await _bookRepository.GetByIdAsync(createLoanDto.BookId);
            if (book == null)
                throw new ArgumentException("Book not found");

            if (!book.IsAvailable)
                throw new InvalidOperationException("Book is not available for loan");

            var loan = _mapper.Map<Loan>(createLoanDto);
            var createdLoan = await _loanRepository.AddAsync(loan);

            return _mapper.Map<LoanDto>(createdLoan);
        }

        public async Task<LoanDto?> UpdateAsync(int id, UpdateLoanDto updateLoanDto)
        {
            var existingLoan = await _loanRepository.GetByIdAsync(id);
            if (existingLoan == null)
                return null;

            _mapper.Map(updateLoanDto, existingLoan);
            existingLoan.UpdatedAt = DateTime.UtcNow;

            var updatedLoan = await _loanRepository.UpdateAsync(existingLoan);
            return _mapper.Map<LoanDto>(updatedLoan);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var loan = await _loanRepository.GetByIdAsync(id);
            if (loan == null)
                return false;

            await _loanRepository.DeleteAsync(id);
            return true;
        }

        public async Task<LoanDto?> ReturnBookAsync(int loanId)
        {
            var loan = await _loanRepository.GetByIdAsync(loanId);
            if (loan == null)
                return null;

            if (loan.Returned.HasValue)
                throw new InvalidOperationException("Book has already been returned");

            loan.Returned = DateTime.UtcNow;
            loan.UpdatedAt = DateTime.UtcNow;

            var updatedLoan = await _loanRepository.UpdateAsync(loan);
            return _mapper.Map<LoanDto>(updatedLoan);
        }
    }
}
