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
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;

        public BookService(IBookRepository bookRepository, IMapper mapper)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
        }

        public async Task<BookDto?> GetByIdAsync(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            return book == null ? null : _mapper.Map<BookDto>(book);
        }

        public async Task<IEnumerable<BookDto>> GetAllAsync()
        {
            var books = await _bookRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<BookDto>>(books);
        }

        public async Task<IEnumerable<BookDto>> SearchAsync(string query)
        {
            var books = await _bookRepository.SearchAsync(query);
            return _mapper.Map<IEnumerable<BookDto>>(books);
        }

        public async Task<BookDto> CreateAsync(CreateBookDto createBookDto)
        {
            var book = _mapper.Map<Book>(createBookDto);
            var createdBook = await _bookRepository.AddAsync(book);
            return _mapper.Map<BookDto>(createdBook);
        }

        public async Task<BookDto?> UpdateAsync(int id, UpdateBookDto updateBookDto)
        {
            var existingBook = await _bookRepository.GetByIdAsync(id);
            if (existingBook == null)
                return null;

            _mapper.Map(updateBookDto, existingBook);
            existingBook.UpdatedAt = DateTime.UtcNow;

            var updatedBook = await _bookRepository.UpdateAsync(existingBook);
            return _mapper.Map<BookDto>(updatedBook);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (!await _bookRepository.ExistsAsync(id))
                return false;

            await _bookRepository.DeleteAsync(id);
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _bookRepository.ExistsAsync(id);
        }
    }
}
