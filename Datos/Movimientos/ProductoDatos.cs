using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Entidades;

namespace Datos
{
    public class ProductoDatos
    {
        private readonly Conexion _conexion;

        public ProductoDatos(Conexion conexion)
        {
            _conexion = conexion;
        }

        // --- LECTURA ---

        public List<Producto> ListarActivos()
        {
            var lista = new List<Producto>();
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_productos_ListarActivos", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Producto
                        {
                            Id_producto = Convert.ToInt32(reader["id_producto"]),
                            Id_categoria = Convert.ToInt32(reader["id_categoria"]),
                            Nombre = reader["nombre"].ToString(),
                            Precio_unitario = Convert.ToDecimal(reader["precio_unitario"]),
                            Es_inventariable = Convert.ToBoolean(reader["es_inventariable"]),
                            Stock_actual = Convert.ToInt32(reader["stock_actual"]),
                            Stock_minimo = Convert.ToInt32(reader["stock_minimo"]),
                            Activo = Convert.ToBoolean(reader["activo"])
                        });
                    }
                }
            }
            catch (Exception ex) { throw new Exception("Error al listar activos: " + ex.Message); }
            return lista;
        }

        public List<Producto> ListarInactivos()
        {
            var lista = new List<Producto>();
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_productos_ListarInactivos", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Producto
                        {
                            Id_producto = Convert.ToInt32(reader["id_producto"]),
                            Id_categoria = Convert.ToInt32(reader["id_categoria"]),
                            Nombre = reader["nombre"].ToString(),
                            Precio_unitario = Convert.ToDecimal(reader["precio_unitario"]),
                            Es_inventariable = Convert.ToBoolean(reader["es_inventariable"]),
                            Stock_actual = Convert.ToInt32(reader["stock_actual"]),
                            Stock_minimo = Convert.ToInt32(reader["stock_minimo"]),
                            Activo = Convert.ToBoolean(reader["activo"])
                        });
                    }
                }
            }
            catch (Exception ex) { throw new Exception("Error al listar inactivos: " + ex.Message); }
            return lista;
        }

        public Producto ObtenerPorId(int id)
        {
            Producto producto = null;
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_productos_ObtenerPorId", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_producto", id);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        producto = new Producto
                        {
                            Id_producto = Convert.ToInt32(reader["id_producto"]),
                            Id_categoria = Convert.ToInt32(reader["id_categoria"]),
                            Nombre = reader["nombre"].ToString(),
                            Precio_unitario = Convert.ToDecimal(reader["precio_unitario"]),
                            Es_inventariable = Convert.ToBoolean(reader["es_inventariable"]),
                            Stock_actual = Convert.ToInt32(reader["stock_actual"]),
                            Stock_minimo = Convert.ToInt32(reader["stock_minimo"]),
                            Activo = Convert.ToBoolean(reader["activo"])
                        };
                    }
                }
            }
            catch (Exception ex) { throw new Exception("Error al consultar: " + ex.Message); }
            return producto;
        }

        // --- BÚSQUEDA (Nombre, ID, o Categoría) ---
        public List<Producto> Buscar(string nombre, int? id, int? id_categoria)
        {
            var lista = new List<Producto>();
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_productos_Buscar", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@nombre", (object)nombre ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@id", (object)id ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@id_categoria", (object)id_categoria ?? DBNull.Value);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Producto
                        {
                            Id_producto = Convert.ToInt32(reader["id_producto"]),
                            Id_categoria = Convert.ToInt32(reader["id_categoria"]),
                            Nombre = reader["nombre"].ToString(),
                            Precio_unitario = Convert.ToDecimal(reader["precio_unitario"]),
                            Es_inventariable = Convert.ToBoolean(reader["es_inventariable"]),
                            Stock_actual = Convert.ToInt32(reader["stock_actual"]),
                            Stock_minimo = Convert.ToInt32(reader["stock_minimo"]),
                            Activo = Convert.ToBoolean(reader["activo"])
                        });
                    }
                }
            }
            catch (Exception ex) { throw new Exception("Error al buscar: " + ex.Message); }
            return lista;
        }

        // --- ESCRITURA ---

        public string Agregar(Producto producto, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_productos_Agregar", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_categoria", producto.Id_categoria);
                    cmd.Parameters.AddWithValue("@nombre", producto.Nombre);
                    cmd.Parameters.AddWithValue("@precio_unitario", producto.Precio_unitario);
                    cmd.Parameters.AddWithValue("@es_inventariable", producto.Es_inventariable);
                    cmd.Parameters.AddWithValue("@stock_actual", producto.Stock_actual);
                    cmd.Parameters.AddWithValue("@stock_minimo", producto.Stock_minimo);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    SqlParameter outputParam = new SqlParameter("@nuevo_id", SqlDbType.Int);
                    outputParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outputParam);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "Producto creado correctamente con ID: " + outputParam.Value;
                }
            }
            catch (Exception ex) { throw new Exception("Error al agregar: " + ex.Message); }
        }

        public string Actualizar(Producto producto, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_productos_Actualizar", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_producto", producto.Id_producto);
                    cmd.Parameters.AddWithValue("@id_categoria", producto.Id_categoria);
                    cmd.Parameters.AddWithValue("@nombre", producto.Nombre);
                    cmd.Parameters.AddWithValue("@precio_unitario", producto.Precio_unitario);
                    cmd.Parameters.AddWithValue("@es_inventariable", producto.Es_inventariable);
                    cmd.Parameters.AddWithValue("@stock_actual", producto.Stock_actual);
                    cmd.Parameters.AddWithValue("@stock_minimo", producto.Stock_minimo);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "Producto actualizado correctamente.";
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
                    SqlCommand cmd = new SqlCommand("sp_productos_EliminarLogico", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_producto", id);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "Producto eliminado lógicamente.";
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
                    SqlCommand cmd = new SqlCommand("sp_productos_Restaurar", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_producto", id);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "Producto restaurado correctamente.";
                }
            }
            catch (Exception ex) { throw new Exception("Error al restaurar: " + ex.Message); }
        }
    }
}