using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GerenciamentoBiblioteca.Tests
{    public class SimpleTests
    {        [Fact]
        public void Teste_Simples_Deve_Passar()
        {
            // Arrange
            var expected = 2;

            // Act
            var actual = 1 + 1;

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
