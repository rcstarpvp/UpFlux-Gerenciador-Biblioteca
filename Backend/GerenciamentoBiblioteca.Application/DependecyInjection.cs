
using GerenciamentoBiblioteca.Application.Mappings;
using GerenciamentoBiblioteca.Application.Services;
using GerenciamentoBiblioteca.Application.Services.Implementations;

using Microsoft.Extensions.DependencyInjection;


namespace GerenciamentoBiblioteca.Application
{
    public static class DependecyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(BookMappingProfile));
            services.AddAutoMapper(typeof(LoanMappingProfile));

            services.AddScoped<IBookService, BookService>();
            services.AddScoped<ILoanService, LoanService>();

            return services;
        }
    }
}
