using GerenciamentoBiblioteca.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerenciamentoBiblioteca.Infrastructure.Data.Configuration
{
    public class LoanConfiguration : IEntityTypeConfiguration<Loan>
    {
        public void Configure(EntityTypeBuilder<Loan> builder)
        {
            builder.HasKey(l => l.Id);

            builder.Property(l => l.Id)
                .ValueGeneratedOnAdd();

            builder.Property(l => l.User)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(l => l.Borrowed)
                .IsRequired();

            builder.Property(l => l.Returned)
                .IsRequired(false);

            builder.Property(l => l.CreatedAt)
                .IsRequired();

            builder.Property(l => l.UpdatedAt)
                .IsRequired(false);

            builder.Property(l => l.BookId)
                .IsRequired();

            builder.HasOne(l => l.Book)
                .WithMany(b => b.Loans)
                .HasForeignKey(l => l.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(l => l.BookId);
            builder.HasIndex(l => l.User);
            builder.HasIndex(l => l.Returned);
        }
    }
}
