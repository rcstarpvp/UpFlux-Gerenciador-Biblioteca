using GerenciamentoBiblioteca.Application.DTOs;
using GerenciamentoBiblioteca.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace GerenciamentoBiblioteca.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly ILoanService _loanService;

        public LoansController(ILoanService loanService)
        {
            _loanService = loanService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetAll()
        {
            var loans = await _loanService.GetAllAsync();
            return Ok(loans);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<LoanDto>> GetById(int id)
        {
            var loan = await _loanService.GetByIdAsync(id);
            if (loan == null)
                return NotFound();

            return Ok(loan);
        }
        [HttpGet("book/{bookId}")]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetByBookId(int bookId)
        {
            var loans = await _loanService.GetByBookIdAsync(bookId);
            return Ok(loans);
        }
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetActiveLoans()
        {
            var loans = await _loanService.GetActiveLoansAsync();
            return Ok(loans);
        }
        [HttpPost]
        public async Task<ActionResult<LoanDto>> Create([FromBody] CreateLoanDto createLoanDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var loan = await _loanService.CreateAsync(createLoanDto);
                return CreatedAtAction(nameof(GetById), new { id = loan.Id }, loan);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<LoanDto>> Update(int id, [FromBody] UpdateLoanDto updateLoanDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var loan = await _loanService.UpdateAsync(id, updateLoanDto);
            if (loan == null)
                return NotFound();

            return Ok(loan);
        }
        [HttpPost("{id}/return")]
        public async Task<ActionResult<LoanDto>> ReturnBook(int id)
        {
            try
            {
                var loan = await _loanService.ReturnBookAsync(id);
                if (loan == null)
                    return NotFound();

                return Ok(loan);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var success = await _loanService.DeleteAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
