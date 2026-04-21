using System;
using System.Collections.Generic;
using Entidades;
using Datos;

namespace Negocio
{
    public class BitacoraNegocio
    {
        private readonly BitacoraDatos _bitacoraDatos;

        public BitacoraNegocio(BitacoraDatos bitacoraDatos)
        {
            _bitacoraDatos = bitacoraDatos;
        }

        public List<Bitacora> ListarTodo()
        {
            try { return _bitacoraDatos.ListarTodo() ?? new List<Bitacora>(); }
            catch (Exception ex) { throw new Exception("Error negocio: " + ex.Message); }
        }

        public List<Bitacora> BuscarPorTabla(string tabla)
        {
            if (string.IsNullOrWhiteSpace(tabla)) throw new ArgumentException("Tabla obligatoria.");
            try { return _bitacoraDatos.BuscarPorTabla(tabla) ?? new List<Bitacora>(); }
            catch (Exception ex) { throw new Exception("Error negocio: " + ex.Message); }
        }

        public string Registrar(Bitacora bitacora)
        {
            if (bitacora.Id_usuario <= 0) return "Usuario obligatorio.";
            if (string.IsNullOrWhiteSpace(bitacora.Accion)) return "Acción obligatoria.";

            try { return _bitacoraDatos.Registrar(bitacora); }
            catch (Exception ex) { throw new Exception("Error al registrar: " + ex.Message); }
        }
    }
}