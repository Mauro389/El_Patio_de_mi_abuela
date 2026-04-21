using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Entidades;

namespace Datos
{
    public class DetalleOrdenDatos
    {
        private readonly Conexion _conexion;

        public DetalleOrdenDatos(Conexion conexion)
        {
            _conexion = conexion;
        }

        public List<DetalleOrden> ObtenerPorOrden(int id_orden)
        {
            var lista = new List<DetalleOrden>();
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_detalleorden_ObtenerPorOrden", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_orden", id_orden);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new DetalleOrden
                        {
                            Id_detalle = Convert.ToInt32(reader["id_detalle"]),
                            Id_orden = Convert.ToInt32(reader["id_orden"]),
                            Id_producto = Convert.ToInt32(reader["id_producto"]),
                            Cantidad = Convert.ToInt32(reader["cantidad"]),
                            Precio_al_momento = Convert.ToDecimal(reader["precio_al_momento"]),
                            Notas = reader["notas"].ToString(),
                            Estado_item = reader["estado_item"].ToString(),
                            Activo = Convert.ToBoolean(reader["activo"]),
                            NombreProducto = reader["nombre_producto"].ToString() 
                        });
                    }
                }
            }
            catch (Exception ex) { throw new Exception("Error al obtener detalle: " + ex.Message); }
            return lista;
        }

        public string Agregar(DetalleOrden detalle, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_detalleorden_Agregar", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_orden", detalle.Id_orden);
                    cmd.Parameters.AddWithValue("@id_producto", detalle.Id_producto);
                    cmd.Parameters.AddWithValue("@cantidad", detalle.Cantidad);
                    cmd.Parameters.AddWithValue("@precio_al_momento", detalle.Precio_al_momento);
                    cmd.Parameters.AddWithValue("@notas", detalle.Notas ?? "");
                    cmd.Parameters.AddWithValue("@estado_item", detalle.Estado_item ?? "Pendiente");
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    SqlParameter outputParam = new SqlParameter("@nuevo_id", SqlDbType.Int);
                    outputParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outputParam);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "Item agregado a la orden.";
                }
            }
            catch (Exception ex) { throw new Exception("Error al agregar item: " + ex.Message); }
        }

        public string ActualizarEstadoItem(int id_detalle, string nuevoEstado, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_detalleorden_ActualizarEstado", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_detalle", id_detalle);
                    cmd.Parameters.AddWithValue("@nuevo_estado", nuevoEstado);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    return $"Estado del item actualizado a: {nuevoEstado}";
                }
            }
            catch (Exception ex) { throw new Exception("Error al actualizar item: " + ex.Message); }
        }

        public string EliminarLogico(int id_detalle, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_detalleorden_EliminarLogico", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_detalle", id_detalle);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "Item eliminado de la orden.";
                }
            }
            catch (Exception ex) { throw new Exception("Error al eliminar item: " + ex.Message); }
        }
    }
}