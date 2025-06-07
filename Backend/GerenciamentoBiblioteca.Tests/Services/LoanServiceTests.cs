using AutoMapper;
using FluentAssertions;
using GerenciamentoBiblioteca.Application.DTOs;
using GerenciamentoBiblioteca.Application.Mappings;
using GerenciamentoBiblioteca.Application.Services.Implementations;
using GerenciamentoBiblioteca.Domain.Entities;
using GerenciamentoBiblioteca.Domain.Repositories;
using Moq;

namespace GerenciamentoBiblioteca.Tests.Services
{
    public class LoanServiceTests
    {
        private readonly Mock<ILoanRepository> _mockLoanRepository;
        private readonly Mock<IBookRepository> _mockBookRepository;
        private readonly IMapper _mapper;
        private readonly LoanService _loanService;

        public LoanServiceTests()
        {
            _mockLoanRepository = new Mock<ILoanRepository>();
            _mockBookRepository = new Mock<IBookRepository>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<LoanMappingProfile>();
                cfg.AddProfile<BookMappingProfile>();
            });
            _mapper = configuration.CreateMapper();

            _loanService = new LoanService(_mockLoanRepository.Object, _mockBookRepository.Object, _mapper);
        }        
        
        [Fact]
        public async Task GetByIdAsync_EmprestimoExistente_RetornaLoanDto()
        {
            // Arrange
            var loanId = 1;
            var book = new Book
            {
                Id = 1,
                Title = "Nome de Livro",
                Author = "Nome de Pessoa",
                CreatedAt = DateTime.UtcNow,
                Loans = new List<Loan>()
            };

            var loan = new Loan
            {
                Id = loanId,
                BookId = 1,
                User = "Nome de Pessoa",
                Borrowed = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Book = book
            };

            _mockLoanRepository.Setup(r => r.GetByIdAsync(loanId))
                              .ReturnsAsync(loan);

            // Act
            var result = await _loanService.GetByIdAsync(loanId);

            // Assert            result.Should().NotBeNull();
            result!.Id.Should().Be(loanId);
            result.User.Should().Be("Nome de Pessoa");
            result.BookTitle.Should().Be("Nome de Livro");
            result.BookAuthor.Should().Be("Nome de Pessoa");
            result.IsActive.Should().BeTrue();
        }        
        
        [Fact]
        public async Task GetByIdAsync_EmprestimoInexistente_RetornaNull()
        {
            // Arrange
            var loanId = 999;
            _mockLoanRepository.Setup(r => r.GetByIdAsync(loanId))
                              .ReturnsAsync((Loan?)null);

            // Act
            var result = await _loanService.GetByIdAsync(loanId);

            // Assert
            result.Should().BeNull();
        }        
        
        [Fact]
        public async Task CreateAsync_EmprestimoValido_RetornaLoanDto()
        {
            // Arrange
            var createLoanDto = new CreateLoanDto
            {
                BookId = 1,
                User = "Nome de Pessoa"
            };

            var book = new Book
            {
                Id = 1,
                Title = "Nome de Livro",
                Author = "Nome de Pessoa",
                CreatedAt = DateTime.UtcNow,
                Loans = new List<Loan>()
            };

            var createdLoan = new Loan
            {
                Id = 1,
                BookId = createLoanDto.BookId,
                User = createLoanDto.User,
                Borrowed = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                Book = book
            };

            _mockBookRepository.Setup(r => r.GetByIdAsync(createLoanDto.BookId))
                              .ReturnsAsync(book);
            _mockLoanRepository.Setup(r => r.AddAsync(It.IsAny<Loan>()))
                              .ReturnsAsync(createdLoan);

            // Act
            var result = await _loanService.CreateAsync(createLoanDto);

            // Assert            
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.User.Should().Be("Nome de Pessoa");
            result.BookId.Should().Be(1);
            result.IsActive.Should().BeTrue();
        }        
        
        [Fact]
        public async Task CreateAsync_LivroNaoEncontrado_LancaArgumentException()
        {
            // Arrange
            var createLoanDto = new CreateLoanDto
            {
                BookId = 999,
                User = "Nome de Pessoa"
            };

            _mockBookRepository.Setup(r => r.GetByIdAsync(createLoanDto.BookId))
                              .ReturnsAsync((Book?)null);

            // Act
            var act = async () => await _loanService.CreateAsync(createLoanDto);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                     .WithMessage("Book not found");
        }        
        
        [Fact]
        public async Task CreateAsync_LivroIndisponivel_LancaInvalidOperationException()
        {
            // Arrange            
            var createLoanDto = new CreateLoanDto
            {
                BookId = 1,
                User = "Nome de Pessoa"
            };

            var unavailableBook = new Book
            {
                Id = 1,
                Title = "Nome de Livro",
                Author = "Nome de Pessoa",
                CreatedAt = DateTime.UtcNow,
                Loans = new List<Loan>
                {
                    new Loan { Id = 1, BookId = 1, User = "Nome de Pessoa", Borrowed = DateTime.UtcNow.AddDays(-1) }
                }
            };

            _mockBookRepository.Setup(r => r.GetByIdAsync(createLoanDto.BookId))
                              .ReturnsAsync(unavailableBook);

            // Act
            var act = async () => await _loanService.CreateAsync(createLoanDto);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("Book is not available for loan");
        }        
        
        [Fact]
        public async Task ReturnBookAsync_EmprestimoAtivoExistente_RetornaLoanDtoAtualizado()
        {
            // Arrange
            var loanId = 1;            
            var book = new Book
            {
                Id = 1,
                Title = "Nome de Livro",
                Author = "Nome de Pessoa",
                CreatedAt = DateTime.UtcNow,
                Loans = new List<Loan>()
            };

            var activeLoan = new Loan
            {
                Id = loanId,
                BookId = 1,
                User = "Nome de Pessoa",
                Borrowed = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Book = book
            };

            var returnedLoan = new Loan
            {
                Id = loanId,
                BookId = 1,
                User = "Nome de Pessoa",
                Borrowed = DateTime.UtcNow.AddDays(-1),
                Returned = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow,
                Book = book
            };

            _mockLoanRepository.Setup(r => r.GetByIdAsync(loanId))
                              .ReturnsAsync(activeLoan);
            _mockLoanRepository.Setup(r => r.UpdateAsync(It.IsAny<Loan>()))
                              .ReturnsAsync(returnedLoan);

            // Act
            var result = await _loanService.ReturnBookAsync(loanId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(loanId);
            result.IsActive.Should().BeFalse();
            result.Returned.Should().NotBeNull();
        } 

        [Fact]
        public async Task ReturnBookAsync_EmprestimoInexistente_RetornaNull()
        {
            // Arrange
            var loanId = 999;
            _mockLoanRepository.Setup(r => r.GetByIdAsync(loanId))
                              .ReturnsAsync((Loan?)null);

            // Act
            var result = await _loanService.ReturnBookAsync(loanId);

            // Assert
            result.Should().BeNull();
        }        
        
        [Fact]
        public async Task ReturnBookAsync_EmprestimoJaDevolvido_LancaInvalidOperationException()
        {
            // Arrange
            var loanId = 1;
            var book = new Book
            {
                Id = 1,                
                Title = "Nome de Livro",
                Author = "Nome de Pessoa",
                CreatedAt = DateTime.UtcNow,
                Loans = new List<Loan>()
            };

            var returnedLoan = new Loan
            {
                Id = loanId,
                BookId = 1,
                User = "Nome de Pessoa",
                Borrowed = DateTime.UtcNow.AddDays(-2),
                Returned = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                Book = book
            };

            _mockLoanRepository.Setup(r => r.GetByIdAsync(loanId))
                              .ReturnsAsync(returnedLoan);

            // Act
            var act = async () => await _loanService.ReturnBookAsync(loanId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("Book has already been returned");
        }        
        
        [Fact]
        public async Task GetActiveLoansAsync_RetornaApenasEmprestimosAtivos()
        {
            // Arrange            
            var book1 = new Book { Id = 1, Title = "Nome de Livro", Author = "Nome de Pessoa", CreatedAt = DateTime.UtcNow, Loans = new List<Loan>() };
            var book2 = new Book { Id = 2, Title = "Nome de Livro", Author = "Nome de Pessoa", CreatedAt = DateTime.UtcNow, Loans = new List<Loan>() };

            var activeLoans = new List<Loan>
            {               
                new Loan { Id = 1, BookId = 1, User = "Nome de Pessoa", Borrowed = DateTime.UtcNow.AddDays(-1), CreatedAt = DateTime.UtcNow.AddDays(-1), Book = book1 },
                new Loan { Id = 2, BookId = 2, User = "Nome de Pessoa", Borrowed = DateTime.UtcNow.AddDays(-2), CreatedAt = DateTime.UtcNow.AddDays(-2), Book = book2 }
            };

            _mockLoanRepository.Setup(r => r.GetActiveLoansAsync())
                              .ReturnsAsync(activeLoans);

            // Act
            var result = await _loanService.GetActiveLoansAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().OnlyContain(l => l.IsActive == true);
        }        
        
        [Fact]
        public async Task DeleteAsync_EmprestimoExistente_RetornaTrue()
        {
            // Arrange
            var loanId = 1;
            var loan = new Loan { Id = loanId, BookId = 1, User = "Nome de Pessoa", Borrowed = DateTime.UtcNow, CreatedAt = DateTime.UtcNow };

            _mockLoanRepository.Setup(r => r.GetByIdAsync(loanId))
                              .ReturnsAsync(loan);

            // Act
            var result = await _loanService.DeleteAsync(loanId);

            // Assert
            result.Should().BeTrue();
            _mockLoanRepository.Verify(r => r.DeleteAsync(loanId), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_EmprestimoInexistente_RetornaFalse()
        {
            // Arrange
            var loanId = 999;
            _mockLoanRepository.Setup(r => r.GetByIdAsync(loanId))
                              .ReturnsAsync((Loan?)null);

            // Act
            var result = await _loanService.DeleteAsync(loanId);

            // Assert
            result.Should().BeFalse();
            _mockLoanRepository.Verify(r => r.DeleteAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
