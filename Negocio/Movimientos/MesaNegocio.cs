using System;
using System.Collections.Generic;
using Entidades;
using Datos;

namespace Negocio
{
    public class MesaNegocio
    {
        private readonly MesaDatos _mesaDatos;

        public MesaNegocio(MesaDatos mesaDatos)
        {
            _mesaDatos = mesaDatos;
        }

        private string ValidarDatos(Mesa mesa, bool esActualizacion = false)
        {
            if (mesa == null) return "El objeto Mesa no puede ser nulo.";
            if (mesa.Numero_mesa <= 0) return "El número de mesa es obligatorio y debe ser mayor a 0.";
            if (string.IsNullOrWhiteSpace(mesa.Estado)) return "El estado es obligatorio.";
            return string.Empty;
        }

        public List<Mesa> ListarActivos()
        {
            try { return _mesaDatos.ListarActivos() ?? new List<Mesa>(); }
            catch (Exception ex) { throw new Exception("Error negocio: " + ex.Message); }
        }

        public List<Mesa> ListarInactivos()
        {
            try { return _mesaDatos.ListarInactivos() ?? new List<Mesa>(); }
            catch (Exception ex) { throw new Exception("Error negocio: " + ex.Message); }
        }

        public Mesa ObtenerPorId(int id)
        {
            if (id <= 0) throw new ArgumentException("ID inválido.");
            var mesa = _mesaDatos.ObtenerPorId(id);
            if (mesa == null) throw new Exception("Mesa no encontrada.");
            return mesa;
        }

        public List<Mesa> Buscar(int? numero, int? id)
        {
            try { return _mesaDatos.Buscar(numero, id) ?? new List<Mesa>(); }
            catch (Exception ex) { throw new Exception("Error al buscar: " + ex.Message); }
        }

        public string Agregar(Mesa mesa, string usuarioEjecutor)
        {
            string error = ValidarDatos(mesa);
            if (!string.IsNullOrEmpty(error)) return $"Error validación: {error}";
            try { return _mesaDatos.Agregar(mesa, usuarioEjecutor); }
            catch (Exception ex) { throw new Exception("Error al crear: " + ex.Message); }
        }

        public string Actualizar(Mesa mesa, string usuarioEjecutor)
        {
            string error = ValidarDatos(mesa, true);
            if (!string.IsNullOrEmpty(error)) return $"Error validación: {error}";
            if (mesa.Id_mesa <= 0) return "ID de mesa inválido.";
            try { return _mesaDatos.Actualizar(mesa, usuarioEjecutor); }
            catch (Exception ex) { throw new Exception("Error al actualizar: " + ex.Message); }
        }

        public string EliminarLogico(int id, string usuarioEjecutor)
        {
            if (id <= 0) return "ID inválido.";
            try { return _mesaDatos.EliminarLogico(id, usuarioEjecutor); }
            catch (Exception ex) { throw new Exception("Error al eliminar: " + ex.Message); }
        }

        public string Restaurar(int id, string usuarioEjecutor)
        {
            if (id <= 0) return "ID inválido.";
            try { return _mesaDatos.Restaurar(id, usuarioEjecutor); }
            catch (Exception ex) { throw new Exception("Error al restaurar: " + ex.Message); }
        }
    }
}