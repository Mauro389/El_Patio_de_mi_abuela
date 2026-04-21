using BCrypt.Net;

namespace Negocio
{
    public static class CryptoHelper
    {
        // Encripta una contraseña plana y devuelve el hash
        public static string EncriptarPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Verifica si una contraseña plana coincide con un hash guardado
        public static bool VerificarPassword(string password, string hashGuardado)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashGuardado);
        }
    }
}