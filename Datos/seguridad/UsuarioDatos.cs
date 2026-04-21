using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Entidades;

namespace Datos
{
    public class UsuarioDatos
    {
        private readonly Conexion _conexion;

        public UsuarioDatos(Conexion conexion)
        {
            _conexion = conexion;
        }

        // ==========================================
        // CONSULTAS (READ)
        // ==========================================

        // 1. Obtener por Username 
        public Usuario ObtenerPorUsername(string username)
        {
            Usuario usuario = null;
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_usuarios_ObtenerPorUsername", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@username", username);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        usuario = new Usuario
                        {
                            Id_usuario = Convert.ToInt32(reader["id_usuario"]),
                            Id_rol = Convert.ToInt32(reader["id_rol"]),
                            Nombre_completo = reader["nombre_completo"].ToString(),
                            Username = reader["username"].ToString(),
                            Password_hash = reader["password_hash"].ToString(), 
                            Activo = Convert.ToBoolean(reader["activo"])
                        };
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al buscar usuario por username: " + ex.Message);
            }
            return usuario;
        }

        // 2. Listar Activos
        public List<Usuario> ListarActivos()
        {
            List<Usuario> lista = new List<Usuario>();
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_usuarios_ListarActivos", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        lista.Add(new Usuario
                        {
                            Id_usuario = Convert.ToInt32(reader["id_usuario"]),
                            Id_rol = Convert.ToInt32(reader["id_rol"]),
                            Nombre_completo = reader["nombre_completo"].ToString(),
                            Username = reader["username"].ToString(),
                            Password_hash = "", // Nunca exponer hash en listas
                            Activo = Convert.ToBoolean(reader["activo"])
                        });
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar usuarios activos: " + ex.Message);
            }
            return lista;
        }

        // 3. Listar Inactivos
        public List<Usuario> ListarInactivos()
        {
            List<Usuario> lista = new List<Usuario>();
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_usuarios_ListarInactivos", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        lista.Add(new Usuario
                        {
                            Id_usuario = Convert.ToInt32(reader["id_usuario"]),
                            Id_rol = Convert.ToInt32(reader["id_rol"]),
                            Nombre_completo = reader["nombre_completo"].ToString(),
                            Username = reader["username"].ToString(),
                            Password_hash = "",
                            Activo = Convert.ToBoolean(reader["activo"])
                        });
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar usuarios inactivos: " + ex.Message);
            }
            return lista;
        }

        // 4. Obtener por ID
        public Usuario ObtenerPorId(int id)
        {
            Usuario usuario = null;
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_usuarios_ObtenerPorId", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_usuario", id);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        usuario = new Usuario
                        {
                            Id_usuario = Convert.ToInt32(reader["id_usuario"]),
                            Id_rol = Convert.ToInt32(reader["id_rol"]),
                            Nombre_completo = reader["nombre_completo"].ToString(),
                            Username = reader["username"].ToString(),
                            Password_hash = reader["password_hash"].ToString(),
                            Activo = Convert.ToBoolean(reader["activo"])
                        };
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener usuario por ID: " + ex.Message);
            }
            return usuario;
        }

        // ==========================================
        // OPERACIONES DE ESCRITURA (CREATE, UPDATE, DELETE)
        // ==========================================

        // 5. Agregar
        public string Agregar(Usuario usuario, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_usuarios_Agregar", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@id_rol", usuario.Id_rol);
                    cmd.Parameters.AddWithValue("@nombre_completo", usuario.Nombre_completo);
                    cmd.Parameters.AddWithValue("@username", usuario.Username);
                    cmd.Parameters.AddWithValue("@password_hash", usuario.Password_hash); 
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    SqlParameter outputParam = new SqlParameter("@nuevo_id", SqlDbType.Int);
                    outputParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outputParam);

                    con.Open();
                    cmd.ExecuteNonQuery();

                    int nuevoId = Convert.ToInt32(outputParam.Value);
                    return nuevoId > 0 ? "Usuario creado correctamente con ID: " + nuevoId : "No se pudo crear el usuario.";
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar usuario: " + ex.Message);
            }
        }

        // 6. Actualizar
        public string Actualizar(Usuario usuario, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_usuarios_Actualizar", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@id_usuario", usuario.Id_usuario);
                    cmd.Parameters.AddWithValue("@id_rol", usuario.Id_rol);
                    cmd.Parameters.AddWithValue("@nombre_completo", usuario.Nombre_completo);
                    cmd.Parameters.AddWithValue("@username", usuario.Username);
                    cmd.Parameters.AddWithValue("@password_hash", usuario.Password_hash);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : "Actualización realizada.";
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar usuario: " + ex.Message);
            }
        }

        // 7. Eliminar Lógico
        public string EliminarLogico(int id, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_usuarios_EliminarLogico", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@id_usuario", id);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : "Eliminación lógica realizada.";
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar usuario: " + ex.Message);
            }
        }

        // 8. Restaurar
        public string Restaurar(int id, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_usuarios_Restaurar", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@id_usuario", id);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : "Restauración realizada.";
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al restaurar usuario: " + ex.Message);
            }
        }
    }



}