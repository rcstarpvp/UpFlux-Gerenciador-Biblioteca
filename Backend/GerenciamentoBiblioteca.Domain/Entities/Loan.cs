using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerenciamentoBiblioteca.Domain.Entities
{
    public class Loan : BaseEntity
    {
        public int Id { get; set; }

        public int BookId { get; set; }

        [Required]
        [MaxLength(100)]
        public string User { get; set; }

        public DateTime Borrowed { get; set; } = DateTime.UtcNow;

        public DateTime? Returned { get; set; }

        public Book Book { get; set; }

        public bool IsActive => Returned == null;
    }
}
