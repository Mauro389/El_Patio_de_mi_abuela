using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Entidades;

namespace Datos
{
    public class FacturaDatos
    {
        private readonly Conexion _conexion;

        public FacturaDatos(Conexion conexion)
        {
            _conexion = conexion;
        }

        public List<Factura> ListarActivas()
        {
            var lista = new List<Factura>();
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_facturas_ListarActivas", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Factura
                        {
                            Id_factura = Convert.ToInt32(reader["id_factura"]),
                            Id_orden = Convert.ToInt32(reader["id_orden"]),
                            Id_mesero_atendiente = Convert.ToInt32(reader["id_mesero_atendiente"]),
                            Id_cajero_cobrador = Convert.ToInt32(reader["id_cajero_cobrador"]),
                            Fecha_emision = Convert.ToDateTime(reader["fecha_emision"]),
                            Metodo_pago = reader["metodo_pago"].ToString(),
                            Subtotal = Convert.ToDecimal(reader["subtotal"]),
                            Monto_impuestos = Convert.ToDecimal(reader["monto_impuestos"]),
                            Monto_propina = Convert.ToDecimal(reader["monto_propina"]),
                            Monto_total = Convert.ToDecimal(reader["monto_total"]),
                            Numero_factura_fiscal = reader["numero_factura_fiscal"].ToString(),
                            Activo = Convert.ToBoolean(reader["activo"])
                        });
                    }
                }
            }
            catch (Exception ex) { throw new Exception("Error al listar facturas: " + ex.Message); }
            return lista;
        }

        public Factura ObtenerPorId(int id)
        {
            Factura factura = null;
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    // Obtenemos la cabecera
                    SqlCommand cmd = new SqlCommand("sp_facturas_ObtenerPorId", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_factura", id);
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        factura = new Factura
                        {
                            Id_factura = Convert.ToInt32(reader["id_factura"]),
                            Id_orden = Convert.ToInt32(reader["id_orden"]),
                            Id_mesero_atendiente = Convert.ToInt32(reader["id_mesero_atendiente"]),
                            Id_cajero_cobrador = Convert.ToInt32(reader["id_cajero_cobrador"]),
                            Fecha_emision = Convert.ToDateTime(reader["fecha_emision"]),
                            Metodo_pago = reader["metodo_pago"].ToString(),
                            Subtotal = Convert.ToDecimal(reader["subtotal"]),
                            Monto_impuestos = Convert.ToDecimal(reader["monto_impuestos"]),
                            Monto_propina = Convert.ToDecimal(reader["monto_propina"]),
                            Monto_total = Convert.ToDecimal(reader["monto_total"]),
                            Numero_factura_fiscal = reader["numero_factura_fiscal"].ToString(),
                            Activo = Convert.ToBoolean(reader["activo"]),
                            DetalleItems = new List<DetalleOrden>()
                        };

                        
                        reader.Close();

                        
                        SqlCommand cmdDet = new SqlCommand("sp_detalleorden_ObtenerPorOrden", con);
                        cmdDet.CommandType = CommandType.StoredProcedure;
                        cmdDet.Parameters.AddWithValue("@id_orden", factura.Id_orden);
                        SqlDataReader readerDet = cmdDet.ExecuteReader();

                        while (readerDet.Read())
                        {
                            factura.DetalleItems.Add(new DetalleOrden
                            {
                                Id_detalle = Convert.ToInt32(readerDet["id_detalle"]),
                                Id_producto = Convert.ToInt32(readerDet["id_producto"]),
                                NombreProducto = readerDet["nombre_producto"].ToString(),
                                Cantidad = Convert.ToInt32(readerDet["cantidad"]),
                                Precio_al_momento = Convert.ToDecimal(readerDet["precio_al_momento"]),
                                Notas = readerDet["notas"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex) { throw new Exception("Error al consultar factura: " + ex.Message); }
            return factura;
        }

        // Generar Factura (Calcula totales automáticamente)
        public string Generar(Factura factura, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_facturas_Generar", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@id_orden", factura.Id_orden);
                    cmd.Parameters.AddWithValue("@id_mesero_atendiente", factura.Id_mesero_atendiente);
                    cmd.Parameters.AddWithValue("@id_cajero_cobrador", factura.Id_cajero_cobrador);
                    cmd.Parameters.AddWithValue("@metodo_pago", factura.Metodo_pago);
                    cmd.Parameters.AddWithValue("@monto_propina", factura.Monto_propina); 
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);

                    SqlParameter outputParam = new SqlParameter("@nuevo_id", SqlDbType.Int);
                    outputParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outputParam);

                    SqlParameter numFacturaParam = new SqlParameter("@numero_fiscal", SqlDbType.NVarChar, 20);
                    numFacturaParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(numFacturaParam);

                    con.Open();
                    cmd.ExecuteNonQuery();

                    return $"Factura #{outputParam.Value} generada correctamente. Fiscal: {numFacturaParam.Value}";
                }
            }
            catch (Exception ex) { throw new Exception("Error al generar factura: " + ex.Message); }
        }

        public string Anular(int id, string motivo, string usuarioEjecutor)
        {
            try
            {
                using (SqlConnection con = _conexion.GetConnection())
                {
                    SqlCommand cmd = new SqlCommand("sp_facturas_Anular", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id_factura", id);
                    cmd.Parameters.AddWithValue("@motivo", motivo);
                    cmd.Parameters.AddWithValue("@usuario_ejecutor", usuarioEjecutor);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    return "Factura anulada correctamente.";
                }
            }
            catch (Exception ex) { throw new Exception("Error al anular: " + ex.Message); }
        }
    }
}