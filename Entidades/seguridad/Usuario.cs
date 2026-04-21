namespace Entidades
{
    public class Usuario
    {
        public int Id_usuario { get; set; }
        public int Id_rol { get; set; }
        public string Nombre_completo { get; set; }
        public string Username { get; set; }

        // Este campo se usa internamente para guardar el hash en BD
        public string Password_hash { get; set; }

        // Este campo es para recibir la contraseña normal desde el Controller
        public string Password { get; set; }

        public bool Activo { get; set; }
    }
}