using System;
using System.Collections.Generic;
using Entidades;
using Datos;

namespace Negocio
{
    public class FacturaNegocio
    {
        private readonly FacturaDatos _facturaDatos;
        private readonly OrdenDatos _ordenDatos;
        private readonly MesaDatos _mesaDatos;
        private readonly UsuarioDatos _usuarioDatos;

        public FacturaNegocio(FacturaDatos facturaDatos, OrdenDatos ordenDatos, MesaDatos mesaDatos, UsuarioDatos usuarioDatos)
        {
            _facturaDatos = facturaDatos;
            _ordenDatos = ordenDatos;
            _mesaDatos = mesaDatos;
            _usuarioDatos = usuarioDatos;
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

            try
            {
                // Buscar la orden para obtener id_mesa e id_mesero
                var orden = _ordenDatos.ObtenerPorId(factura.Id_orden);
                if (orden == null) return "La orden no existe.";

                // Buscar el número de mesa y guardarlo como NombreMesa en la factura
                var mesa = _mesaDatos.ObtenerPorId(orden.Id_mesa);
                factura.NombreMesa = mesa?.Numero_mesa.ToString() ?? "";

                // Buscar el nombre del mesero y guardarlo como NombreMesero en la factura
                var mesero = _usuarioDatos.ObtenerPorId(orden.Id_mesero);
                factura.NombreMesero = mesero?.Nombre_completo ?? "";

                // Asegurar que el mesero atendiente quede registrado en la factura si no venía
                if (factura.Id_mesero_atendiente == 0)
                    factura.Id_mesero_atendiente = orden.Id_mesero;

                return _facturaDatos.Generar(factura, usuarioEjecutor);
            }
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