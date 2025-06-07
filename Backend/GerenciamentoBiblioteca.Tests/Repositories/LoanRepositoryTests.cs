using FluentAssertions;
using Xunit;
using GerenciamentoBiblioteca.Domain.Entities;
using GerenciamentoBiblioteca.Domain.Infrastructure;
using GerenciamentoBiblioteca.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using GerenciamentoBiblioteca.Infrastructure.Repositories;
using Xunit;

namespace GerenciamentoBiblioteca.Tests.Repositories
{
    public class LoanRepositoryTests : IDisposable
    {
        private readonly LibraryDbContext _context;
        private readonly LoanRepository _repository;

        public LoanRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<LibraryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new LibraryDbContext(options);
            _repository = new LoanRepository(_context);
        }        
        
        [Fact]       
        public async Task AddAsync_EmprestimoValido_AdicionaEmprestimoNoBancoDeDados()
        {
            // Arrange
            var book = new Book
            {
                Title = "Nome de Livro",
                Author = "Nome de Pessoa"
            };            
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            var loan = new Loan
            {
                BookId = book.Id,
                User = "Nome de Pessoa",
                Borrowed = DateTime.UtcNow
            };

            // Act
            var result = await _repository.AddAsync(loan);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().BeGreaterThan(0);            
            result.User.Should().Be("Nome de Pessoa");
            result.BookId.Should().Be(book.Id);
            result.Borrowed.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            result.Book.Should().NotBeNull();
            result.Book!.Title.Should().Be("Nome de Livro");

            var loanInDb = await _context.Loans.FindAsync(result.Id);
            loanInDb.Should().NotBeNull();
        }        

        [Fact]        
        public async Task GetByIdAsync_EmprestimoExistente_RetornaEmprestimoComLivro()
        {
            // Arrange
            var book = new Book
            {
                Title = "Nome de Livro",
                Author = "Nome de Pessoa"
            };
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            var loan = new Loan
            {
                BookId = book.Id,
                User = "Usuário de Teste",
                Borrowed = DateTime.UtcNow
            };
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(loan.Id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(loan.Id);
            result.User.Should().Be("Usuário de Teste");
            result.BookId.Should().Be(book.Id);
            result.Book.Should().NotBeNull();
            result.Book!.Title.Should().Be("Nome de Livro");
            result.Book.Author.Should().Be("Nome de Pessoa");
        }

        [Fact]
        public async Task GetByIdAsync_EmprestimoInexistente_RetornaNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }        
        
        [Fact]
        public async Task GetAllAsync_RetornaTodosOsEmprestimosComLivros()
        {
            // Arrange
            var book1 = new Book { Title = "Livro 1", Author = "Autor 1" };
            var book2 = new Book { Title = "Livro 2", Author = "Autor 2" };
            _context.Books.AddRange(book1, book2);
            await _context.SaveChangesAsync();

            var loans = new List<Loan>
            {
                new Loan { BookId = book1.Id, User = "Usuário 1", Borrowed = DateTime.UtcNow.AddDays(-2) },
                new Loan { BookId = book2.Id, User = "Usuário 2", Borrowed = DateTime.UtcNow.AddDays(-1) }
            };
            _context.Loans.AddRange(loans);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeInDescendingOrder(l => l.Borrowed);
            result.All(l => l.Book != null).Should().BeTrue();
            result.First().User.Should().Be("Usuário 2"); // Most recent loan first
        }        
        
        [Fact]
        public async Task GetByBookIdAsync_RetornaEmprestimosParaLivroEspecifico()
        {
            // Arrange
            var book1 = new Book { Title = "Livro 1", Author = "Autor 1" };
            var book2 = new Book { Title = "Livro 2", Author = "Autor 2" };
            _context.Books.AddRange(book1, book2);
            await _context.SaveChangesAsync();

            var loans = new List<Loan>
            {
                new Loan { BookId = book1.Id, User = "Usuário 1", Borrowed = DateTime.UtcNow.AddDays(-3) },
                new Loan { BookId = book1.Id, User = "Usuário 2", Borrowed = DateTime.UtcNow.AddDays(-1) },
                new Loan { BookId = book2.Id, User = "Usuário 3", Borrowed = DateTime.UtcNow.AddDays(-2) }
            };
            _context.Loans.AddRange(loans);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByBookIdAsync(book1.Id);

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(l => l.BookId == book1.Id);
            result.Should().BeInDescendingOrder(l => l.Borrowed);
            result.All(l => l.Book != null).Should().BeTrue();
            result.First().User.Should().Be("Usuário 2"); // Most recent loan first
        }        
        
        [Fact]
        public async Task GetActiveLoansAsync_RetornaApenasEmprestimosSemDataDevolucao()
        {
            // Arrange
            var book1 = new Book { Title = "Livro 1", Author = "Autor 1" };
            var book2 = new Book { Title = "Livro 2", Author = "Autor 2" };
            var book3 = new Book { Title = "Livro 3", Author = "Autor 3" };
            _context.Books.AddRange(book1, book2, book3);
            await _context.SaveChangesAsync();

            var loans = new List<Loan>
            {
                new Loan { BookId = book1.Id, User = "Usuário 1", Borrowed = DateTime.UtcNow.AddDays(-3) }, // Active
                new Loan { BookId = book2.Id, User = "Usuário 2", Borrowed = DateTime.UtcNow.AddDays(-2), Returned = DateTime.UtcNow.AddDays(-1) }, // Returned
                new Loan { BookId = book3.Id, User = "Usuário 3", Borrowed = DateTime.UtcNow.AddDays(-1) } // Active
            };
            _context.Loans.AddRange(loans);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetActiveLoansAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(l => l.Returned == null);
            result.Should().BeInDescendingOrder(l => l.Borrowed);
            result.All(l => l.Book != null).Should().BeTrue();
            result.First().User.Should().Be("Usuário 3"); // Most recent active loan first
        }        
        
        [Fact]
        public async Task UpdateAsync_EmprestimoExistente_AtualizaEmprestimo()
        {
            // Arrange
            var book = new Book
            {
                Title = "Livro de Teste",
                Author = "Autor de Teste"
            };
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            var loan = new Loan
            {
                BookId = book.Id,
                User = "Usuário Original",
                Borrowed = DateTime.UtcNow.AddDays(-1)
            };
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            // Act
            loan.User = "Usuário Atualizado";
            loan.Returned = DateTime.UtcNow;
            loan.UpdatedAt = DateTime.UtcNow;

            var result = await _repository.UpdateAsync(loan);

            // Assert
            result.Should().NotBeNull();
            result.User.Should().Be("Usuário Atualizado");
            result.Returned.Should().NotBeNull();
            result.UpdatedAt.Should().NotBeNull();
            result.Book.Should().NotBeNull();

            var loanInDb = await _context.Loans.FindAsync(loan.Id);
            loanInDb!.User.Should().Be("Usuário Atualizado");
            loanInDb.Returned.Should().NotBeNull();
        }        
        
        [Fact]
        public async Task DeleteAsync_EmprestimoExistente_RemoveEmprestimo()
        {
            // Arrange
            var book = new Book
            {
                Title = "Livro de Teste",
                Author = "Autor de Teste"
            };
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            var loan = new Loan
            {
                BookId = book.Id,
                User = "Usuário para Deletar",
                Borrowed = DateTime.UtcNow
            };
            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();
            var loanId = loan.Id;

            // Act
            await _repository.DeleteAsync(loanId);

            // Assert
            var loanInDb = await _context.Loans.FindAsync(loanId);
            loanInDb.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAsync_EmprestimoInexistente_NaoLancaExcecao()
        {
            // Act & Assert
            await _repository.DeleteAsync(999);

            // Should not throw any exception
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
