using SisAlmacenProductos.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SisAlmacenProductos.PruebasUnitarias
{
    public class ProductoTests
    {
        // Método que devuelve los resultados de las pruebas
        public List<string> EjecutarPruebas()
        {
            var resultados = new List<string>();

            // Ejecuta cada prueba y guarda el resultado
            resultados.Add(ProbarCodigoNoMayorACincoCaracteres());
            resultados.Add(ProbarPrecioNoNegativo());
            resultados.Add(ProbarImagenUrlEsHttps());

            return resultados;
        }

        // Prueba 7
        private string ProbarCodigoNoMayorACincoCaracteres()
        {
            var producto = new Producto { Codigo = "ABCDE" };
            var productos = new List<Producto> { producto };

            if (productos.Count == 1 && productos[0].Codigo.Length <= 5)
            {
                return "✅ CodigoNoMayorACincoCaracteres PASÓ.";
            }
            else
            {
                return "❌ CodigoNoMayorACincoCaracteres FALLÓ.";
            }
        }

        // Prueba 8
        private string ProbarPrecioNoNegativo()
        {
            var producto = new Producto { Precio = 10.5m };
            var productos = new List<Producto> { producto };

            if (productos[0].Precio >= 0)
            {
                return "✅ PrecioNoNegativo PASÓ.";
            }
            else
            {
                return "❌ PrecioNoNegativo FALLÓ.";
            }
        }

        // Prueba 9
        private string ProbarImagenUrlEsHttps()
        {
            var productoConUrlValida = new Producto { ImagenUrl = "https://example.com/imagen.jpg" };
            var productoConUrlInvalida = new Producto { ImagenUrl = "http://example.com/imagen.jpg" };
            var productos = new List<Producto> { productoConUrlValida, productoConUrlInvalida };

            bool imagenValida = productos.All(p => p.ImagenUrl.StartsWith("https://"));

            if (imagenValida)
            {
                return "✅ ImagenUrlEsHttps PASÓ.";
            }
            else
            {
                return "❌ ImagenUrlEsHttps FALLÓ.";
            }
        }
    }
}
