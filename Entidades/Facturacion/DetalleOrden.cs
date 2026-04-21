namespace Entidades
{
    public class DetalleOrden
    {
        public int Id_detalle { get; set; }
        public int Id_orden { get; set; }
        public int Id_producto { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio_al_momento { get; set; } // Precio congelado al momento de pedir
        public string Notas { get; set; } // Ej: "Sin hielo", "Bien cocido"
        public string Estado_item { get; set; } // Pendiente, EnProceso, Listo, Cancelado
        public bool Activo { get; set; }

        // Propiedades auxiliares para mostrar nombres en lugar de IDs
        public string NombreProducto { get; set; }
        public decimal Subtotal => Cantidad * Precio_al_momento;
    }
}