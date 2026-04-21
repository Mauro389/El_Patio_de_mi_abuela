using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Entidades;

namespace Datos
{
    public class MesaDatos
    {
        private readonly Conexion _conexion;

        public MesaDatos(Conexion conexion)
        {
            _conexion = conexion;
        }

        // --- LECTURA ---

        public List<Mesa> ListarActivos()
        {
            var lista = new List<Mesa>();
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_mesas_ListarActivos", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Mesa
                        {
                            Id_mesa = Convert.ToInt32(reader["id_mesa"]),
                            Numero_mesa = Convert.ToInt32(reader["numero_mesa"]),
                            Estado = reader["estado"].ToString(),
                            Activo = Convert.ToBoolean(reader["activo"])
                        });
                    }
                }
            }
            catch (Exception ex) { throw new Exception("Error al listar activos: " + ex.Message); }
            return lista;
        }

        public List<Mesa> ListarInactivos()
        {
            var lista = new List<Mesa>();
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_mesas_ListarInactivos", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Mesa
                        {
                            Id_mesa = Convert.ToInt32(reader["id_mesa"]),
                            Numero_mesa = Convert.ToInt32(reader["numero_mesa"]),
                            Estado = reader["estado"].ToString(),
                            Activo = Convert.ToBoolean(reader["activo"])
                        });
                    }
                }
            }
            catch (Exception ex) { throw new Exception("Error al listar inactivos: " + ex.Message); }
            return lista;
        }

        public Mesa ObtenerPorId(int id)
        {
            Mesa mesa = null;
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_mesas_ObtenerPorId", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_mesa", id);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        mesa = new Mesa
                        {
                            Id_mesa = Convert.ToInt32(reader["id_mesa"]),
                            Numero_mesa = Convert.ToInt32(reader["numero_mesa"]),
                            Estado = reader["estado"].ToString(),
                            Activo = Convert.ToBoolean(reader["activo"])
                        };
                    }
                }
            }
            catch (Exception ex) { throw new Exception("Error al consultar: " + ex.Message); }
            return mesa;
        }

        // --- BÚSQUEDA (Por Número o ID) ---
        public List<Mesa> Buscar(int? numero, int? id)
        {
            var lista = new List<Mesa>();
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_mesas_Buscar", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@numero", (object)numero ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", (object)id ?? DBNull.Value);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Mesa
                        {
                            Id_mesa = Convert.ToInt32(reader["id_mesa"]),
                            Numero_mesa = Convert.ToInt32(reader["numero_mesa"]),
                            Estado = reader["estado"].ToString(),
                            Activo = Convert.ToBoolean(reader["activo"])
                        });
                    }
                }
            }
            catch (Exception ex) { throw new Exception("Error al buscar: " + ex.Message); }
            return lista;
        }

        // --- ESCRITURA ---

        public string Agregar(Mesa mesa, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_mesas_Agregar", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@numero_mesa", mesa.Numero_mesa);
                    cmd.Parameters.AddWithValue("@estado", mesa.Estado); // Default "Disponible"
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    SqlParameter outputParam = new SqlParameter("@nuevo_id", SqlDbType.Int);
                    outputParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outputParam);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "Mesa creada correctamente con ID: " + outputParam.Value;
                }
            }
            catch (Exception ex) { throw new Exception("Error al agregar: " + ex.Message); }
        }

        public string Actualizar(Mesa mesa, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_mesas_Actualizar", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_mesa", mesa.Id_mesa);
                    cmd.Parameters.AddWithValue("@numero_mesa", mesa.Numero_mesa);
                    cmd.Parameters.AddWithValue("@estado", mesa.Estado);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "Mesa actualizada correctamente.";
                }
            }
            catch (Exception ex) { throw new Exception("Error al actualizar: " + ex.Message); }
        }

        public string EliminarLogico(int id, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_mesas_EliminarLogico", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_mesa", id);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "Mesa eliminada lógicamente.";
                }
            }
            catch (Exception ex) { throw new Exception("Error al eliminar: " + ex.Message); }
        }

        public string Restaurar(int id, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_mesas_Restaurar", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_mesa", id);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "Mesa restaurada correctamente.";
                }
            }
            catch (Exception ex) { throw new Exception("Error al restaurar: " + ex.Message); }
        }
    }
}