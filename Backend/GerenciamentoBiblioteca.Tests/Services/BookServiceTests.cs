using AutoMapper;
using FluentAssertions;

using GerenciamentoBiblioteca.Application.DTOs;
using GerenciamentoBiblioteca.Application.Mappings;
using GerenciamentoBiblioteca.Application.Services.Implementations;
using GerenciamentoBiblioteca.Domain.Entities;
using GerenciamentoBiblioteca.Domain.Repositories;

using Moq;
using Xunit;

namespace GerenciamentoBiblioteca.Tests.Services
{
    public class BookServiceTests
    {
        private readonly Mock<IBookRepository> _mockBookRepository;
        private readonly IMapper _mapper;
        private readonly BookService _bookService;

        public BookServiceTests()
        {
            _mockBookRepository = new Mock<IBookRepository>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<BookMappingProfile>();
            });
            _mapper = configuration.CreateMapper();

            _bookService = new BookService(_mockBookRepository.Object, _mapper);
        }        
        
        [Fact]
        public async Task GetByIdAsync_LivroExistente_RetornaBookDto()
        {
            // Arrange
            var bookId = 1;            
            
            var book = new Book
            {
                Id = bookId,
                Title = "Nome de Livro",
                Author = "Nome de Pessoa",
                CreatedAt = DateTime.UtcNow,
                Loans = new List<Loan>()
            };

            _mockBookRepository.Setup(r => r.GetByIdAsync(bookId))
                              .ReturnsAsync(book);

            // Act
            var result = await _bookService.GetByIdAsync(bookId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(bookId);            
            result.Title.Should().Be("Nome de Livro");
            result.Author.Should().Be("Nome de Pessoa");
            result.IsAvailable.Should().BeTrue();
        }        
        
        [Fact]
        public async Task GetByIdAsync_LivroInexistente_RetornaNull()
        {
            // Arrange
            var bookId = 999;
            _mockBookRepository.Setup(r => r.GetByIdAsync(bookId))
                              .ReturnsAsync((Book?)null);

            // Act
            var result = await _bookService.GetByIdAsync(bookId);

            // Assert
            result.Should().BeNull();
        }        
        
        [Fact]
        public async Task GetAllAsync_RetornaTodosOsLivros()
        {
            // Arrange            
            var books = new List<Book>
            {
                new Book { Id = 1, Title = "Nome de Livro", Author = "Nome de Pessoa", CreatedAt = DateTime.UtcNow, Loans = new List<Loan>() },
                new Book { Id = 2, Title = "Nome de Livro", Author = "Nome de Pessoa", CreatedAt = DateTime.UtcNow, Loans = new List<Loan>() }
            };

            _mockBookRepository.Setup(r => r.GetAllAsync())
                              .ReturnsAsync(books);

            // Act
            var result = await _bookService.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(b => b.IsAvailable == true);
        }        
        
        [Fact]
        public async Task CreateAsync_LivroValido_RetornaBookDto()
        {
            // Arrange            
            var createBookDto = new CreateBookDto
            {
                Title = "Nome de Livro",
                Author = "Nome de Pessoa"
            };

            var createdBook = new Book
            {
                Id = 1,
                Title = createBookDto.Title,
                Author = createBookDto.Author,
                CreatedAt = DateTime.UtcNow,
                Loans = new List<Loan>()
            };

            _mockBookRepository.Setup(r => r.AddAsync(It.IsAny<Book>()))
                              .ReturnsAsync(createdBook);

            // Act
            var result = await _bookService.CreateAsync(createBookDto);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);            
            result.Title.Should().Be("Nome de Livro");
            result.Author.Should().Be("Nome de Pessoa");
            result.IsAvailable.Should().BeTrue();
        }        
        
        [Fact]
        public async Task UpdateAsync_LivroExistente_RetornaBookDtoAtualizado()
        {
            // Arrange  
            var bookId = 1;
            var updateBookDto = new UpdateBookDto
            {
                Title = "Nome de Livro",
                Author = "Nome de Pessoa"
            };            var existingBook = new Book
            {
                Id = bookId,
                Title = "Nome de Livro",
                Author = "Nome de Pessoa",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Loans = new List<Loan>()
            };

            var updatedBook = new Book
            {
                Id = bookId,
                Title = updateBookDto.Title,
                Author = updateBookDto.Author,
                CreatedAt = existingBook.CreatedAt,
                UpdatedAt = DateTime.UtcNow,
                Loans = new List<Loan>()
            };

            _mockBookRepository.Setup(r => r.GetByIdAsync(bookId))
                              .ReturnsAsync(existingBook);
            _mockBookRepository.Setup(r => r.UpdateAsync(It.IsAny<Book>()))
                              .ReturnsAsync(updatedBook);

            // Act
            var result = await _bookService.UpdateAsync(bookId, updateBookDto);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(bookId);            
            result.Title.Should().Be("Nome de Livro");
            result.Author.Should().Be("Nome de Pessoa");
        }        
        
        [Fact]
        public async Task UpdateAsync_LivroInexistente_RetornaNull()
        {
            // Arrange            
            var bookId = 999;
            var updateBookDto = new UpdateBookDto
            {
                Title = "Nome de Livro",
                Author = "Nome de Pessoa"
            };

            _mockBookRepository.Setup(r => r.GetByIdAsync(bookId))
                              .ReturnsAsync((Book?)null);

            // Act
            var result = await _bookService.UpdateAsync(bookId, updateBookDto);

            // Assert
            result.Should().BeNull();
        }        
        
        [Fact]
        public async Task DeleteAsync_LivroExistente_RetornaTrue()
        {
            // Arrange
            var bookId = 1;
            _mockBookRepository.Setup(r => r.ExistsAsync(bookId))
                              .ReturnsAsync(true);

            // Act
            var result = await _bookService.DeleteAsync(bookId);

            // Assert
            result.Should().BeTrue();
            _mockBookRepository.Verify(r => r.DeleteAsync(bookId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_LivroInexistente_RetornaFalse()
        {
            // Arrange
            var bookId = 999;
            _mockBookRepository.Setup(r => r.ExistsAsync(bookId))
                              .ReturnsAsync(false);

            // Act
            var result = await _bookService.DeleteAsync(bookId);

            // Assert
            result.Should().BeFalse();
            _mockBookRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }        
        
        [Fact]
        public async Task SearchAsync_RetornaLivrosCorrespondentes()
        {
            // Arrange
            var searchQuery = "livro";
            var books = new List<Book>
            {
                new Book { Id = 1, Title = "Nome de Livro", Author = "Nome de Pessoa", CreatedAt = DateTime.UtcNow, Loans = new List<Loan>() }
            };

            _mockBookRepository.Setup(r => r.SearchAsync(searchQuery))
                              .ReturnsAsync(books);

            // Act
            var result = await _bookService.SearchAsync(searchQuery);

            // Assert
            result.Should().HaveCount(1);
            result.First().Title.Should().Be("Nome de Livro");
        }
    }
}
