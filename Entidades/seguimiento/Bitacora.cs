namespace Entidades
{
    public class Bitacora
    {
        public int Id_bitacora { get; set; }
        public DateTime Fecha_evento { get; set; }
        public int Id_usuario { get; set; }
        public string Nombre_usuario { get; set; }
        public string Accion { get; set; } 
        public string Tabla_afectada { get; set; } 
        public string Descripcion { get; set; } 
        public string Ip_origen { get; set; }
        public bool Activo { get; set; }
    }
}