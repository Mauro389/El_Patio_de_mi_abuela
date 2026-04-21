using System;
using System.Collections.Generic;
using Entidades;
using Datos;

namespace Negocio
{
    public class ProductoNegocio
    {
        private readonly ProductoDatos _productoDatos;

        public ProductoNegocio(ProductoDatos productoDatos)
        {
            _productoDatos = productoDatos;
        }

        private string ValidarDatos(Producto producto, bool esActualizacion = false)
        {
            if (producto == null) return "El objeto Producto no puede ser nulo.";
            if (string.IsNullOrWhiteSpace(producto.Nombre)) return "El nombre del producto es obligatorio.";
            if (producto.Id_categoria <= 0) return "La categoría es obligatoria.";
            if (producto.Precio_unitario < 0) return "El precio no puede ser negativo.";
            if (producto.Stock_actual < 0) return "El stock actual no puede ser negativo.";
            if (producto.Stock_minimo < 0) return "El stock mínimo no puede ser negativo.";
            return string.Empty;
        }

        public List<Producto> ListarActivos()
        {
            try { return _productoDatos.ListarActivos() ?? new List<Producto>(); }
            catch (Exception ex) { throw new Exception("Error negocio: " + ex.Message); }
        }

        public List<Producto> ListarInactivos()
        {
            try { return _productoDatos.ListarInactivos() ?? new List<Producto>(); }
            catch (Exception ex) { throw new Exception("Error negocio: " + ex.Message); }
        }

        public Producto ObtenerPorId(int id)
        {
            if (id <= 0) throw new ArgumentException("ID inválido.");
            var prod = _productoDatos.ObtenerPorId(id);
            if (prod == null) throw new Exception("Producto no encontrado.");
            return prod;
        }

        public List<Producto> Buscar(string nombre, int? id, int? id_categoria)
        {
            try { return _productoDatos.Buscar(nombre, id, id_categoria) ?? new List<Producto>(); }
            catch (Exception ex) { throw new Exception("Error al buscar: " + ex.Message); }
        }

        public string Agregar(Producto producto, string usuarioEjecutor)
        {
            string error = ValidarDatos(producto);
            if (!string.IsNullOrEmpty(error)) return $"Error validación: {error}";
            try { return _productoDatos.Agregar(producto, usuarioEjecutor); }
            catch (Exception ex) { throw new Exception("Error al crear: " + ex.Message); }
        }

        public string Actualizar(Producto producto, string usuarioEjecutor)
        {
            string error = ValidarDatos(producto, true);
            if (!string.IsNullOrEmpty(error)) return $"Error validación: {error}";
            if (producto.Id_producto <= 0) return "ID de producto inválido.";
            try { return _productoDatos.Actualizar(producto, usuarioEjecutor); }
            catch (Exception ex) { throw new Exception("Error al actualizar: " + ex.Message); }
        }

        public string EliminarLogico(int id, string usuarioEjecutor)
        {
            if (id <= 0) return "ID inválido.";
            try { return _productoDatos.EliminarLogico(id, usuarioEjecutor); }
            catch (Exception ex) { throw new Exception("Error al eliminar: " + ex.Message); }
        }

        public string Restaurar(int id, string usuarioEjecutor)
        {
            if (id <= 0) return "ID inválido.";
            try { return _productoDatos.Restaurar(id, usuarioEjecutor); }
            catch (Exception ex) { throw new Exception("Error al restaurar: " + ex.Message); }
        }
    }
}