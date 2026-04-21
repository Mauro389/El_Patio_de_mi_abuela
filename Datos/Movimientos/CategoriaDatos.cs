using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;
using Entidades;

namespace Datos
{
    public class CategoriaDatos
    {
        private readonly Conexion _conexion;

        public CategoriaDatos(Conexion conexion)
        {
            _conexion = conexion;
        }

        // --- LECTURA ---

        public List<Categoria> ListarActivos()
        {
            var lista = new List<Categoria>();
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_categorias_ListarActivos", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Categoria
                        {
                            Id_categoria = Convert.ToInt32(reader["id_categoria"]),
                            Nombre = reader["nombre"].ToString(),
                            Activo = Convert.ToBoolean(reader["activo"])
                        });
                    }
                }
            }
            catch (Exception ex) { throw new Exception("Error al listar activos: " + ex.Message); }
            return lista;
        }

        public List<Categoria> ListarInactivos()
        {
            var lista = new List<Categoria>();
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_categorias_ListarInactivos", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Categoria
                        {
                            Id_categoria = Convert.ToInt32(reader["id_categoria"]),
                            Nombre = reader["nombre"].ToString(),
                            Activo = Convert.ToBoolean(reader["activo"])
                        });
                    }
                }
            }
            catch (Exception ex) { throw new Exception("Error al listar inactivos: " + ex.Message); }
            return lista;
        }

        public Categoria ObtenerPorId(int id)
        {
            Categoria categoria = null;
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_categorias_ObtenerPorId", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_categoria", id);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        categoria = new Categoria
                        {
                            Id_categoria = Convert.ToInt32(reader["id_categoria"]),
                            Nombre = reader["nombre"].ToString(),
                            Activo = Convert.ToBoolean(reader["activo"])
                        };
                    }
                }
            }
            catch (Exception ex) { throw new Exception("Error al consultar: " + ex.Message); }
            return categoria;
        }

        // --- BÚSQUEDA (ID, NOMBRE O AMBOS) ---
        public List<Categoria> Buscar(string nombre, int? id)
        {
            var lista = new List<Categoria>();
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_categorias_Buscar", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@nombre", (object)nombre ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", (object)id ?? DBNull.Value);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Categoria
                        {
                            Id_categoria = Convert.ToInt32(reader["id_categoria"]),
                            Nombre = reader["nombre"].ToString(),
                            Activo = Convert.ToBoolean(reader["activo"])
                        });
                    }
                }
            }
            catch (Exception ex) { throw new Exception("Error al buscar: " + ex.Message); }
            return lista;
        }

        // --- ESCRITURA 

        public string Agregar(Categoria categoria, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_categorias_Agregar", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@nombre", categoria.Nombre);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    SqlParameter outputParam = new SqlParameter("@nuevo_id", SqlDbType.Int);
                    outputParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outputParam);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "Categoría creada correctamente con ID: " + outputParam.Value;
                }
            }
            catch (Exception ex) { throw new Exception("Error al agregar: " + ex.Message); }
        }

        public string Actualizar(Categoria categoria, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_categorias_Actualizar", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_categoria", categoria.Id_categoria);
                    cmd.Parameters.AddWithValue("@nombre", categoria.Nombre);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "Categoría actualizada correctamente.";
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
                    SqlCommand cmd = new SqlCommand("sp_categorias_EliminarLogico", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_categoria", id);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "Categoría eliminada lógicamente.";
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
                    SqlCommand cmd = new SqlCommand("sp_categorias_Restaurar", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_categoria", id);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "Categoría restaurada correctamente.";
                }
            }
            catch (Exception ex) { throw new Exception("Error al restaurar: " + ex.Message); }
        }
    }
}