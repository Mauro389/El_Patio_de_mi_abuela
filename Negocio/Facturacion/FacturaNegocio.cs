using System;
using System.Collections.Generic;
using Entidades;
using Datos;

namespace Negocio
{
    public class FacturaNegocio
    {
        private readonly FacturaDatos _facturaDatos;

        public FacturaNegocio(FacturaDatos facturaDatos)
        {
            _facturaDatos = facturaDatos;
        }

        public List<Factura> ListarActivas()
        {
            try { return _facturaDatos.ListarActivas() ?? new List<Factura>(); }
            catch (Exception ex) { throw new Exception("Error negocio: " + ex.Message); }
        }

        public Factura ObtenerPorId(int id)
        {
            if (id <= 0) throw new ArgumentException("ID inválido.");
            var fac = _facturaDatos.ObtenerPorId(id);
            if (fac == null) throw new Exception("Factura no encontrada.");
            return fac;
        }

        public string Generar(Factura factura, string usuarioEjecutor)
        {
            if (factura.Id_orden <= 0) return "La orden es obligatoria.";
            if (string.IsNullOrWhiteSpace(factura.Metodo_pago)) return "El método de pago es obligatorio.";
            if (factura.Monto_propina < 0) return "La propina no puede ser negativa.";

      

            try { return _facturaDatos.Generar(factura, usuarioEjecutor); }
            catch (Exception ex) { throw new Exception("Error al generar: " + ex.Message); }
        }

        public string Anular(int id, string motivo, string usuarioEjecutor)
        {
            if (id <= 0) return "ID inválido.";
            if (string.IsNullOrWhiteSpace(motivo)) return "El motivo de anulación es obligatorio.";
            try { return _facturaDatos.Anular(id, motivo, usuarioEjecutor); }
            catch (Exception ex) { throw new Exception("Error al anular: " + ex.Message); }
        }
    }
}