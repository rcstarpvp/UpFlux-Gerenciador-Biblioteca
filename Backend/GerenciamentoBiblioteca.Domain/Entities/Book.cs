using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerenciamentoBiblioteca.Domain.Entities
{
    public class Book : BaseEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        [MaxLength(100)]
        public string Author { get; set; }

        public List<Loan> Loans { get; set; } = new List<Loan>();

        public bool IsAvailable => !Loans.Any(l => l.Returned == null);
    }
}
