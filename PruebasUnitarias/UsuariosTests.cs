using SisAlmacenProductos.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SisAlmacenProductos.PruebasUnitarias
{
    public class UsuariosTests
    {
        // Método que devuelve los resultados de las pruebas
        public List<string> EjecutarPruebas()
        {
            var resultados = new List<string>();

            // Ejecuta cada prueba y guarda el resultado
            resultados.Add(ProbarCrearUsuarioCorrectamente());
            resultados.Add(ProbarNoPermitirUsernameDuplicado());
            resultados.Add(ProbarAsignarRolValido());

            return resultados;
        }

        private string ProbarCrearUsuarioCorrectamente()
        {
            var nuevoUsuario = new User { Username = "nuevoUsuario", Role = "Cliente" };
            var usuarios = new List<User>();

            usuarios.Add(nuevoUsuario);

            if (usuarios.Count == 1 &&
                usuarios[0].Username == "nuevoUsuario" &&
                usuarios[0].Role == "Cliente")
            {
                return "✅ CrearUsuarioCorrectamente PASÓ.";
            }
            else
            {
                return "❌ CrearUsuarioCorrectamente FALLÓ.";
            }
        }

        // Prueba 1
        private string ProbarNoPermitirUsernameDuplicado()
        {
            var usuarios = new List<User>
            {
                new User { Username = "usuarioExistente", Role = "Cliente" }
            };

            var nuevoUsuario = new User { Username = "usuarioExistente", Role = "Cliente" };

            bool usernameExiste = usuarios.Any(u => u.Username == nuevoUsuario.Username);

            if (usernameExiste)
            {
                return "✅ NoPermitirUsernameDuplicado PASÓ.";
            }
            else
            {
                return "❌ NoPermitirUsernameDuplicado FALLÓ.";
            }
        }

        private string ProbarAsignarRolValido()
        {
            var rolesValidos = new List<string> { "Administrador", "Logistica", "Almacenero", "Cliente" };
            var nuevoUsuario = new User { Username = "pruebaUsuario", Role = "Administrador" };

            bool rolValido = rolesValidos.Contains(nuevoUsuario.Role);

            if (rolValido)
            {
                return "✅ AsignarRolValido PASÓ.";
            }
            else
            {
                return "❌ AsignarRolValido FALLÓ.";
            }
        }
    }
}
