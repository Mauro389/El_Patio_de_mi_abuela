namespace Entidades
{
    public class Factura
    {
        public int Id_factura { get; set; }
        public int Id_orden { get; set; }
        public int Id_mesero_atendiente { get; set; }
        public int Id_cajero_cobrador { get; set; }
        public DateTime Fecha_emision { get; set; }
        public string Metodo_pago { get; set; } // Efectivo, Tarjeta, Transferencia
        public decimal Subtotal { get; set; }
        public decimal Monto_impuestos { get; set; }
        public decimal Monto_propina { get; set; } // Voluntario
        public decimal Monto_total { get; set; }
        public string Numero_factura_fiscal { get; set; } // Ej: A01-00001234
        public bool Activo { get; set; }

        // Propiedades auxiliares para lectura
        public string NombreMesero { get; set; }
        public string NombreCajero { get; set; }
        public List<DetalleOrden> DetalleItems { get; set; } // Para mostrar qué se cobró
    }
}