namespace Entidades
{
    public class Mesa
    {
        public int Id_mesa { get; set; }
        public int Numero_mesa { get; set; }
        public string Estado { get; set; } // Ej: "Disponible", "Ocupada", "Reservada"
        public bool Activo { get; set; }
    }
}