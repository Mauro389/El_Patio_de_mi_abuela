namespace Entidades
{
    public class Producto
    {
        public int Id_producto { get; set; }
        public int Id_categoria { get; set; }
        public string Nombre { get; set; }
        public decimal Precio_unitario { get; set; }
        public bool Es_inventariable { get; set; }
        public int Stock_actual { get; set; }
        public int Stock_minimo { get; set; }
        public bool Activo { get; set; }

        // Propiedad opcional para mostrar el nombre de la categoría en lecturas (no se guarda en BD)
        public string NombreCategoria { get; set; }
    }
}