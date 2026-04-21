using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Entidades;

namespace Datos
{
    public class OrdenDatos
    {
        private readonly Conexion _conexion;

        public OrdenDatos(Conexion conexion)
        {
            _conexion = conexion;
        }

        public List<Orden> ListarActivas()
        {
            var lista = new List<Orden>();
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_ordenes_ListarActivas", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Orden
                        {
                            Id_orden = Convert.ToInt32(reader["id_orden"]),
                            Id_mesa = Convert.ToInt32(reader["id_mesa"]),
                            Id_mesero = Convert.ToInt32(reader["id_mesero"]),
                            Fecha_apertura = Convert.ToDateTime(reader["fecha_apertura"]),
                            Estado = reader["estado"].ToString(),
                            Activo = Convert.ToBoolean(reader["activo"])
                        });
                    }
                }
            }
            catch (Exception ex) { throw new Exception("Error al listar activas: " + ex.Message); }
            return lista;
        }

        public List<Orden> ListarHistorial() // Para ver cerradas/inactivas
        {
            var lista = new List<Orden>();
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_ordenes_ListarHistorial", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Orden
                        {
                            Id_orden = Convert.ToInt32(reader["id_orden"]),
                            Id_mesa = Convert.ToInt32(reader["id_mesa"]),
                            Id_mesero = Convert.ToInt32(reader["id_mesero"]),
                            Fecha_apertura = Convert.ToDateTime(reader["fecha_apertura"]),
                            Estado = reader["estado"].ToString(),
                            Activo = Convert.ToBoolean(reader["activo"])
                        });
                    }
                }
            }
            catch (Exception ex) { throw new Exception("Error al listar historial: " + ex.Message); }
            return lista;
        }

        public Orden ObtenerPorId(int id)
        {
            Orden orden = null;
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_ordenes_ObtenerPorId", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_orden", id);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        orden = new Orden
                        {
                            Id_orden = Convert.ToInt32(reader["id_orden"]),
                            Id_mesa = Convert.ToInt32(reader["id_mesa"]),
                            Id_mesero = Convert.ToInt32(reader["id_mesero"]),
                            Fecha_apertura = Convert.ToDateTime(reader["fecha_apertura"]),
                            Estado = reader["estado"].ToString(),
                            Activo = Convert.ToBoolean(reader["activo"])
                        };
                    }
                }
            }
            catch (Exception ex) { throw new Exception("Error al consultar: " + ex.Message); }
            return orden;
        }

        // Búsqueda simple por Mesa o Estado
        public List<Orden> Buscar(int? id_mesa, string estado)
        {
            var lista = new List<Orden>();
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_ordenes_Buscar", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_mesa", (object)id_mesa ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@estado", (object)estado ?? DBNull.Value);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Orden
                        {
                            Id_orden = Convert.ToInt32(reader["id_orden"]),
                            Id_mesa = Convert.ToInt32(reader["id_mesa"]),
                            Id_mesero = Convert.ToInt32(reader["id_mesero"]),
                            Fecha_apertura = Convert.ToDateTime(reader["fecha_apertura"]),
                            Estado = reader["estado"].ToString(),
                            Activo = Convert.ToBoolean(reader["activo"])
                        });
                    }
                }
            }
            catch (Exception ex) { throw new Exception("Error al buscar: " + ex.Message); }
            return lista;
        }

        public string Agregar(Orden orden, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_ordenes_Agregar", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_mesa", orden.Id_mesa);
                    cmd.Parameters.AddWithValue("@id_mesero", orden.Id_mesero);
                    cmd.Parameters.AddWithValue("@estado", orden.Estado); // Default "Abierta"
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    SqlParameter outputParam = new SqlParameter("@nuevo_id", SqlDbType.Int);
                    outputParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outputParam);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "Orden creada correctamente con ID: " + outputParam.Value;
                }
            }
            catch (Exception ex) { throw new Exception("Error al agregar: " + ex.Message); }
        }

        public string ActualizarEstado(int id_orden, string nuevoEstado, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_ordenes_ActualizarEstado", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_orden", id_orden);
                    cmd.Parameters.AddWithValue("@nuevo_estado", nuevoEstado);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    return $"Estado de la orden actualizado a: {nuevoEstado}";
                }
            }
            catch (Exception ex) { throw new Exception("Error al actualizar estado: " + ex.Message); }
        }

        public string EliminarLogico(int id, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_ordenes_EliminarLogico", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_orden", id);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "Orden eliminada lógicamente.";
                }
            }
            catch (Exception ex) { throw new Exception("Error al eliminar: " + ex.Message); }
        }
    }
}