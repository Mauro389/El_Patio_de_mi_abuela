using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Entidades;

namespace Datos
{
    public class RolDatos
    {
        private readonly Conexion _conexion;

        public RolDatos(Conexion conexion)
        {
            _conexion = conexion;
        }

        // Listar Roles Activos
        public List<Rol> ListarActivos()
        {
            List<Rol> lista = new List<Rol>();
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_roles_ListarActivos", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        lista.Add(new Rol
                        {
                            Id_rol = Convert.ToInt32(reader["id_rol"]),
                            Nombre_rol = reader["nombre_rol"].ToString(),
                            Descripcion = reader["descripcion"].ToString(),
                            Activo = Convert.ToBoolean(reader["activo"])
                        });
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar roles activos: " + ex.Message);
            }
            return lista;
        }

        // Listar Roles Inactivos
        public List<Rol> ListarInactivos()
        {
            List<Rol> lista = new List<Rol>();
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_roles_ListarInactivos", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        lista.Add(new Rol
                        {
                            Id_rol = Convert.ToInt32(reader["id_rol"]),
                            Nombre_rol = reader["nombre_rol"].ToString(),
                            Descripcion = reader["descripcion"].ToString(),
                            Activo = Convert.ToBoolean(reader["activo"])
                        });
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar roles inactivos: " + ex.Message);
            }
            return lista;
        }

        // Obtener por ID
        public Rol ObtenerPorId(int id)
        {
            Rol rol = null;
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_roles_ObtenerPorId", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_rol", id);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        rol = new Rol
                        {
                            Id_rol = Convert.ToInt32(reader["id_rol"]),
                            Nombre_rol = reader["nombre_rol"].ToString(),
                            Descripcion = reader["descripcion"].ToString(),
                            Activo = Convert.ToBoolean(reader["activo"])
                        };
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener rol por ID: " + ex.Message);
            }
            return rol;
        }

        // Agregar Rol
        public string Agregar(Rol rol, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_roles_Agregar", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@nombre_rol", rol.Nombre_rol);
                    cmd.Parameters.AddWithValue("@descripcion", rol.Descripcion);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    SqlParameter outputParam = new SqlParameter("@nuevo_id", SqlDbType.Int);
                    outputParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outputParam);

                    con.Open();
                    cmd.ExecuteNonQuery();

                    int nuevoId = Convert.ToInt32(outputParam.Value);
                    return nuevoId > 0 ? "Rol creado correctamente con ID: " + nuevoId : "No se pudo crear el rol.";
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al agregar rol: " + ex.Message);
            }
        }

        // Actualizar Rol
        public string Actualizar(Rol rol, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_roles_Actualizar", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@id_rol", rol.Id_rol);
                    cmd.Parameters.AddWithValue("@nombre_rol", rol.Nombre_rol);
                    cmd.Parameters.AddWithValue("@descripcion", rol.Descripcion);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : "Actualización realizada.";
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar rol: " + ex.Message);
            }
        }

        // Eliminar Lógico
        public string EliminarLogico(int id, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_roles_EliminarLogico", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@id_rol", id);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : "Eliminación lógica realizada.";
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar rol: " + ex.Message);
            }
        }

        // Restaurar Rol
        public string Restaurar(int id, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_roles_Restaurar", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@id_rol", id);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : "Restauración realizada.";
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al restaurar rol: " + ex.Message);
            }
        }

        // Buscar por nombre 
        public List<Rol> BuscarPorNombre(string nombre)
        {
            List<Rol> lista = new List<Rol>();
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_roles_BuscarPorNombre", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@nombre", nombre);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        lista.Add(new Rol
                        {
                            Id_rol = Convert.ToInt32(reader["id_rol"]),
                            Nombre_rol = reader["nombre_rol"].ToString(),
                            Descripcion = reader["descripcion"].ToString(),
                            Activo = Convert.ToBoolean(reader["activo"])
                        });
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al buscar rol: " + ex.Message);
            }
            return lista;
        }
    }
}