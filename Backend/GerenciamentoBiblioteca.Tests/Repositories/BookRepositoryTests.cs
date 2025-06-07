using FluentAssertions;
using Xunit;
using Microsoft.EntityFrameworkCore;
using GerenciamentoBiblioteca.Domain.Infrastructure;
using GerenciamentoBiblioteca.Infrastructure.Repositories;
using GerenciamentoBiblioteca.Domain.Entities;


namespace GerenciamentoBiblioteca.Tests.Repositories
{
    public class BookRepositoryTests : IDisposable
    {
        private readonly LibraryDbContext _context;
        private readonly BookRepository _repository;

        public BookRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<LibraryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new LibraryDbContext(options);
            _repository = new BookRepository(_context);
        }        
        
        [Fact]        
        public async Task AddAsync_LivroValido_AdicionaLivroNoBancoDeDados()
        {
            // Arrange
            var book = new Book
            {
                Title = "Nome de Livro",
                Author = "Nome de Pessoa"
            };

            // Act
            var result = await _repository.AddAsync(book);

            // Assert
            result.Should().NotBeNull();            
            result.Id.Should().BeGreaterThan(0);
            result.Title.Should().Be("Nome de Livro");
            result.Author.Should().Be("Nome de Pessoa");
            result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

            var bookInDb = await _context.Books.FindAsync(result.Id);
            bookInDb.Should().NotBeNull();
        }        
        
        [Fact]        
        public async Task GetByIdAsync_LivroExistente_RetornaLivroComEmprestimos()
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

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(book.Id);

            // Assert
            result.Should().NotBeNull();            
            result!.Id.Should().Be(book.Id);
            result.Title.Should().Be("Nome de Livro");
            result.Author.Should().Be("Nome de Pessoa");
            result.Loans.Should().HaveCount(1);
            result.Loans.First().User.Should().Be("Nome de Pessoa");
        }        

        [Fact]
        public async Task GetByIdAsync_LivroInexistente_RetornaNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }        
        
        [Fact]
        public async Task GetAllAsync_RetornaTodosOsLivrosComEmprestimos()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Title = "Livro 1", Author = "Autor 1" },
                new Book { Title = "Livro 2", Author = "Autor 2" }
            };

            _context.Books.AddRange(books);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().BeInAscendingOrder(b => b.Title);
            result.All(b => b.Loans != null).Should().BeTrue();
        }        
        
        [Fact]        
        public async Task SearchAsync_TituloCorrespondente_RetornaLivrosCorrespondentes()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Title = "Nome de Livro", Author = "Nome de Pessoa" },
                new Book { Title = "Nome de Livr", Author = "Nome de Pessoa" },
                new Book { Title = "Nome de Liv", Author = "Nome de Pessoa" }
            };

            _context.Books.AddRange(books);
            await _context.SaveChangesAsync();            
            
            // Act
            var result = await _repository.SearchAsync("livro");

            // Assert
            result.Should().HaveCount(1);
            result.First().Title.Should().Be("Nome de Livro");
        }        
        
        [Fact]        
        public async Task SearchAsync_AutorCorrespondente_RetornaLivrosCorrespondentes()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { Title = "Nome de Livro", Author = "Nome de Pessoa" },
                new Book { Title = "Nome de Livro", Author = "Nome de Pe" },
                new Book { Title = "Nome de Livro", Author = "Nome de Pes" }
            };

            _context.Books.AddRange(books);
            await _context.SaveChangesAsync();            
            
            // Act
            var result = await _repository.SearchAsync("pessoa");

            // Assert
            result.Should().HaveCount(1);
            result.First().Author.Should().Be("Nome de Pessoa");
        }        
        
        [Fact]
        public async Task UpdateAsync_LivroExistente_AtualizaLivro()
        {
            // Arrange            
            var book = new Book
            {
                Title = "Nome de Livro",
                Author = "Nome de Pessoa"
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();            
            
            // Act
            book.Title = "Nome de Livro";
            book.Author = "Nome de Pessoa";
            book.UpdatedAt = DateTime.UtcNow;

            var result = await _repository.UpdateAsync(book);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("Nome de Livro");
            result.Author.Should().Be("Nome de Pessoa");
            result.UpdatedAt.Should().NotBeNull();            
            var bookInDb = await _context.Books.FindAsync(book.Id);
            bookInDb!.Title.Should().Be("Nome de Livro");
            bookInDb.Author.Should().Be("Nome de Pessoa");
        }        
        
        [Fact]
        public async Task DeleteAsync_LivroExistente_RemoveLivro()
        {
            // Arrange            
            
            var book = new Book
            {
                Title = "Nome de Livro",
                Author = "Nome de Pessoa"
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            var bookId = book.Id;

            // Act
            await _repository.DeleteAsync(bookId);

            // Assert
            var bookInDb = await _context.Books.FindAsync(bookId);
            bookInDb.Should().BeNull();
        }        
        
        [Fact]
        public async Task ExistsAsync_LivroExistente_RetornaTrue()
        {
            // Arrange
            var book = new Book
            {
                Title = "Nome de Livro",
                Author = "Nome de Pessoa"
            };

            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.ExistsAsync(book.Id);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_LivroInexistente_RetornaFalse()
        {
            // Act
            var result = await _repository.ExistsAsync(999);

            // Assert
            result.Should().BeFalse();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
