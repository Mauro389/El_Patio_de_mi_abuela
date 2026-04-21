using System;
using System.Collections.Generic;
using Entidades;
using Datos;

namespace Negocio
{
    public class OrdenNegocio
    {
        private readonly OrdenDatos _ordenDatos;

        public OrdenNegocio(OrdenDatos ordenDatos)
        {
            _ordenDatos = ordenDatos;
        }

        public List<Orden> ListarActivas()
        {
            try { return _ordenDatos.ListarActivas() ?? new List<Orden>(); }
            catch (Exception ex) { throw new Exception("Error negocio: " + ex.Message); }
        }

        public List<Orden> ListarHistorial()
        {
            try { return _ordenDatos.ListarHistorial() ?? new List<Orden>(); }
            catch (Exception ex) { throw new Exception("Error negocio: " + ex.Message); }
        }

        public Orden ObtenerPorId(int id)
        {
            if (id <= 0) throw new ArgumentException("ID inválido.");
            var ord = _ordenDatos.ObtenerPorId(id);
            if (ord == null) throw new Exception("Orden no encontrada.");
            return ord;
        }

        public List<Orden> Buscar(int? id_mesa, string estado)
        {
            try { return _ordenDatos.Buscar(id_mesa, estado) ?? new List<Orden>(); }
            catch (Exception ex) { throw new Exception("Error al buscar: " + ex.Message); }
        }

        public string Agregar(Orden orden, string usuarioEjecutor)
        {
            if (orden.Id_mesa <= 0 || orden.Id_mesero <= 0) return "Mesa y Mesero son obligatorios.";
            if (string.IsNullOrWhiteSpace(orden.Estado)) orden.Estado = "Abierta";

            try { return _ordenDatos.Agregar(orden, usuarioEjecutor); }
            catch (Exception ex) { throw new Exception("Error al crear: " + ex.Message); }
        }

        public string ActualizarEstado(int id_orden, string nuevoEstado, string usuarioEjecutor)
        {
            if (id_orden <= 0) return "ID de orden inválido.";
            if (string.IsNullOrWhiteSpace(nuevoEstado)) return "El estado es obligatorio.";

            try { return _ordenDatos.ActualizarEstado(id_orden, nuevoEstado, usuarioEjecutor); }
            catch (Exception ex) { throw new Exception("Error al actualizar: " + ex.Message); }
        }

        public string EliminarLogico(int id, string usuarioEjecutor)
        {
            if (id <= 0) return "ID inválido.";
            try { return _ordenDatos.EliminarLogico(id, usuarioEjecutor); }
            catch (Exception ex) { throw new Exception("Error al eliminar: " + ex.Message); }
        }
    }
}