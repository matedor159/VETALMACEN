using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace TestPruebasUnitarias
{
    // Modelo simplificado
    public class Usuario
    {
        public string Password { get; set; }
    }

    // Lógica de validación
    public static class Validador
    {
        public static bool EsPasswordFuerte(string password)
        {
            if (string.IsNullOrWhiteSpace(password) ||
                password.Length < 8 ||
                !password.Any(char.IsUpper) ||
                !password.Any(char.IsLower) ||
                !password.Any(char.IsDigit) ||
                !password.Any(ch => "!@#$%^&*()-_=+[{]}\\|;:'\",<.>/?".Contains(ch)))
            {
                return false;
            }
            return true;
        }
    }

    // Pruebas unitarias
    [TestClass]
    public class PasswordTests
    {
        [TestMethod]
        public void Password_Valida_Deberia_Ser_Aceptada()
        {
            var usuario = new Usuario { Password = "Abc123$%" };
            bool resultado = Validador.EsPasswordFuerte(usuario.Password);
            Assert.IsTrue(resultado, "La contraseña debería ser Aceptada.");
        }

    }
}
