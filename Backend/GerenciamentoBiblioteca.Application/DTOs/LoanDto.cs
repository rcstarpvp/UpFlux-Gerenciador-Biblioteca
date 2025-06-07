using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerenciamentoBiblioteca.Application.DTOs
{
    public class LoanDto
    {
        public int Id { get; set; }
        public int BookId { get; set; }

        [Required]
        [MaxLength(100)]
        public string User { get; set; }

        public DateTime Borrowed { get; set; }
        public DateTime? Returned { get; set; }
        public bool IsActive { get; set; }

        public string BookTitle { get; set; }
        public string BookAuthor { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateLoanDto
    {
        [Required]
        public int BookId { get; set; }

        [Required]
        [MaxLength(100)]
        public string User { get; set; }
    }

    public class UpdateLoanDto
    {
        [Required]
        [MaxLength(100)]
        public string User { get; set; }

        public DateTime? Returned { get; set; }
    }
}
