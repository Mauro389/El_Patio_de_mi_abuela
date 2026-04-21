using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Datos
{
    public class Conexion
    {
        private readonly IConfiguration _configuration;

        public Conexion(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public SqlConnection GetConnection()
        {
            
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            return new SqlConnection(connectionString);
        }
    }
}