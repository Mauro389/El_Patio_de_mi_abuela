using System;
using System.Collections.Generic;
using Entidades;
using Datos;

namespace Negocio
{
    public class DetalleOrdenNegocio
    {
        private readonly DetalleOrdenDatos _detalleDatos;

        public DetalleOrdenNegocio(DetalleOrdenDatos detalleDatos)
        {
            _detalleDatos = detalleDatos;
        }

        public List<DetalleOrden> ObtenerPorOrden(int id_orden)
        {
            if (id_orden <= 0) throw new ArgumentException("ID de orden inválido.");
            try { return _detalleDatos.ObtenerPorOrden(id_orden) ?? new List<DetalleOrden>(); }
            catch (Exception ex) { throw new Exception("Error negocio: " + ex.Message); }
        }

        public string Agregar(DetalleOrden detalle, string usuarioEjecutor)
        {
            if (detalle.Id_orden <= 0 || detalle.Id_producto <= 0 || detalle.Cantidad <= 0)
                return "Datos incompletos: Orden, Producto y Cantidad son obligatorios.";

            // Validación simple de inventario si es inventariable 
            try { return _detalleDatos.Agregar(detalle, usuarioEjecutor); }
            catch (Exception ex) { throw new Exception("Error al crear: " + ex.Message); }
        }

        public string ActualizarEstadoItem(int id_detalle, string nuevoEstado, string usuarioEjecutor)
        {
            if (id_detalle <= 0) return "ID de detalle inválido.";
            if (string.IsNullOrWhiteSpace(nuevoEstado)) return "Estado obligatorio.";
            try { return _detalleDatos.ActualizarEstadoItem(id_detalle, nuevoEstado, usuarioEjecutor); }
            catch (Exception ex) { throw new Exception("Error al actualizar: " + ex.Message); }
        }

        public string EliminarLogico(int id_detalle, string usuarioEjecutor)
        {
            if (id_detalle <= 0) return "ID inválido.";
            try { return _detalleDatos.EliminarLogico(id_detalle, usuarioEjecutor); }
            catch (Exception ex) { throw new Exception("Error al eliminar: " + ex.Message); }
        }
    }
}