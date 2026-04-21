namespace Entidades
{
    public class Orden
    {
        public int Id_orden { get; set; }
        public int Id_mesa { get; set; }
        public int Id_mesero { get; set; } // Quién abrió la orden
        public DateTime Fecha_apertura { get; set; }
        public string Estado { get; set; } // Abierta, Cerrada, EnProceso, Lista
        public bool Activo { get; set; }

        // Propiedades auxiliares para lectura (no se guardan en BD)
        public string NombreMesa { get; set; }
        public string NombreMesero { get; set; }
    }
}