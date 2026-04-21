using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Entidades;

namespace Datos
{
    public class BitacoraDatos
    {
        private readonly Conexion _conexion;

        public BitacoraDatos(Conexion conexion)
        {
            _conexion = conexion;
        }

        // 🔹 MÉTODO AUXILIAR PARA LEER VALORES SEGUROS
        private T LeerValor<T>(SqlDataReader reader, string columna, T valorPorDefecto)
        {
            if (reader[columna] == DBNull.Value)
                return valorPorDefecto;
            return (T)reader[columna];
        }

        public List<Bitacora> ListarTodo()
        {
            var lista = new List<Bitacora>();
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_bitacora_ListarTodo", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Bitacora
                        {
                            Id_bitacora = LeerValor(reader, "id_bitacora", 0),
                            Fecha_evento = LeerValor(reader, "fecha_evento", DateTime.MinValue),
                            Id_usuario = LeerValor(reader, "id_usuario", 0),
                            Nombre_usuario = LeerValor(reader, "nombre_usuario", ""),
                            Accion = LeerValor(reader, "accion", ""),
                            Tabla_afectada = LeerValor(reader, "tabla_afectada", ""),
                            Descripcion = LeerValor(reader, "descripcion", ""),
                            Ip_origen = LeerValor(reader, "ip_origen", ""),
                            Activo = LeerValor(reader, "activo", true)
                        });
                    }
                    reader.Close();
                }
            }
            catch (Exception ex) { throw new Exception("Error al listar bitácora: " + ex.Message); }
            return lista;
        }

        public List<Bitacora> BuscarPorTabla(string tabla)
        {
            var lista = new List<Bitacora>();
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_bitacora_BuscarPorTabla", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@tabla", tabla);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Bitacora
                        {
                            Id_bitacora = LeerValor(reader, "id_bitacora", 0),
                            Fecha_evento = LeerValor(reader, "fecha_evento", DateTime.MinValue),
                            Id_usuario = LeerValor(reader, "id_usuario", 0),
                            Nombre_usuario = LeerValor(reader, "nombre_usuario", ""),
                            Accion = LeerValor(reader, "accion", ""),
                            Tabla_afectada = LeerValor(reader, "tabla_afectada", ""),
                            Descripcion = LeerValor(reader, "descripcion", ""),
                            Ip_origen = LeerValor(reader, "ip_origen", ""),
                            Activo = LeerValor(reader, "activo", true)
                        });
                    }
                    reader.Close();
                }
            }
            catch (Exception ex) { throw new Exception("Error al buscar: " + ex.Message); }
            return lista;
        }

        public string Registrar(Bitacora bitacora)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_bitacora_Registrar", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@id_usuario", bitacora.Id_usuario);

                    // ✅ Para strings que pueden ser null, usar (object)DBNull.Value
                    cmd.Parameters.AddWithValue("@nombre_usuario",
                        string.IsNullOrEmpty(bitacora.Nombre_usuario) ? (object)DBNull.Value : bitacora.Nombre_usuario);

                    cmd.Parameters.AddWithValue("@accion", bitacora.Accion);
                    cmd.Parameters.AddWithValue("@tabla_afectada", bitacora.Tabla_afectada);
                    cmd.Parameters.AddWithValue("@descripcion", bitacora.Descripcion);

                    cmd.Parameters.AddWithValue("@ip_origen",
                        string.IsNullOrEmpty(bitacora.Ip_origen) ? (object)DBNull.Value : bitacora.Ip_origen);

                    // Fecha se pone en SQL con GETDATE()

                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "Evento registrado en bitácora.";
                }
            }
            catch (Exception ex) { throw new Exception("Error al registrar: " + ex.Message); }
        }
    }
}