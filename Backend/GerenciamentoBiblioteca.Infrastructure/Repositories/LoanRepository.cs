using GerenciamentoBiblioteca.Domain.Entities;
using GerenciamentoBiblioteca.Domain.Infrastructure;
using GerenciamentoBiblioteca.Domain.Repositories;
using Microsoft.EntityFrameworkCore;


namespace GerenciamentoBiblioteca.Infrastructure.Repositories
{
    public class LoanRepository : ILoanRepository
    {
        private readonly LibraryDbContext _context;

        public LoanRepository(LibraryDbContext context)
        {
            _context = context;
        }

        public async Task<Loan?> GetByIdAsync(int id)
        {
            return await _context.Loans
                .Include(l => l.Book)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<Loan>> GetAllAsync()
        {
            return await _context.Loans
                .Include(l => l.Book)
                .OrderByDescending(l => l.Borrowed)
                .ToListAsync();
        }

        public async Task<IEnumerable<Loan>> GetByBookIdAsync(int bookId)
        {
            return await _context.Loans
                .Include(l => l.Book)
                .Where(l => l.BookId == bookId)
                .OrderByDescending(l => l.Borrowed)
                .ToListAsync();
        }

        public async Task<IEnumerable<Loan>> GetActiveLoansAsync()
        {
            return await _context.Loans
                .Include(l => l.Book)
                .Where(l => l.Returned == null)
                .OrderByDescending(l => l.Borrowed)
                .ToListAsync();
        }

        public async Task<Loan> AddAsync(Loan loan)
        {
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(loan.Id) ?? loan;
        }

        public async Task<Loan> UpdateAsync(Loan loan)
        {
            _context.Loans.Update(loan);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(loan.Id) ?? loan;
        }

        public async Task DeleteAsync(int id)
        {
            var loan = await _context.Loans.FindAsync(id);
            if (loan != null)
            {
                _context.Loans.Remove(loan);
                await _context.SaveChangesAsync();
            }
        }
    }
}
