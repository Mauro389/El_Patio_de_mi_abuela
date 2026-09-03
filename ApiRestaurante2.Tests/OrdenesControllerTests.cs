using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using Moq;
using Xunit;
using Entidades;
using Datos;
using Negocio;
using ApiRestaurante2.Controllers;

namespace ApiRestaurante2.Tests
{
    public class OrdenesControllerTests
    {
        private readonly Mock<OrdenDatos> _mockDatos;
        private readonly OrdenNegocio _negocio;
        private readonly OrdenesController _controller;

        public OrdenesControllerTests()
        {
            // 1. Preparamos la cadena de dependencias (igual que en la prueba anterior)
            var mockConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var mockConexion = new Mock<Conexion>(mockConfig.Object);
            _mockDatos = new Mock<OrdenDatos>(mockConexion.Object);

            // 2. Instanciamos el Negocio con los datos falsos
            _negocio = new OrdenNegocio(_mockDatos.Object);

            // 3. Instanciamos el Controller
            _controller = new OrdenesController(_negocio);
        }

        // ==========================================
        // PRUEBA 1: Mesero puede crear orden (Éxito)
        // ==========================================
        [Fact]
        public void Post_MeseroCreaOrden_Retorna200OK()
        {
            //  ARRANGE
            var ordenEntrante = new Orden { Id_mesa = 5, Id_mesero = 10 };

            // Simulamos que el usuario es un MESERO (Rol 2)
            SimularUsuarioEnController(
                username: "mesero_juan",
                idUsuario: 10,
                idRol: 2 // Rol Mesero
            );

            // Mockeamos que la capa de negocio responde OK
            _mockDatos.Setup(x => x.Agregar(It.IsAny<Orden>(), It.IsAny<string>()))
                      .Returns("Orden creada correctamente con ID: 99");

            // 🟡 ACT
            var resultado = _controller.Post(ordenEntrante);

            // 🔴 ASSERT
            var okResult = resultado.Should().BeOfType<OkObjectResult>().Subject;
            var respuesta = okResult.Value;

            // Verificamos que el JSON tenga "exito: true"
            respuesta.GetType().GetProperty("exito").GetValue(respuesta).Should().Be(true);
        }

        // ==========================================
        // PRUEBA 2: Cocina NO puede crear orden (Prohibido)
        // ==========================================
        [Fact]
        public void Post_CocinaIntentaCrearOrden_Retorna403Forbidden()
        {
            // 🟢 ARRANGE
            var ordenEntrante = new Orden { Id_mesa = 5, Id_mesero = 10 };

            // Simulamos que el usuario es COCINA (Rol 4)
            SimularUsuarioEnController(
                username: "cocinero_pepe",
                idUsuario: 20,
                idRol: 4 // Rol Cocina
            );

            // 🟡 ACT
            var resultado = _controller.Post(ordenEntrante);

            // 🔴 ASSERT
            // Debe ser un StatusCode 403 (ObjectResult)
            var statusResult = resultado.Should().BeOfType<ObjectResult>().Subject;
            statusResult.StatusCode.Should().Be(403);

            var respuesta = statusResult.Value;
            var mensaje = respuesta.GetType().GetProperty("mensaje").GetValue(respuesta).ToString();

            mensaje.Should().Contain("Acceso Denegado");
        }

        // ==========================================
        //  MÉTODO AUXILIAR (NO BORRAR)
        // Sirve para "loguear" al usuario falso en la prueba
        // ==========================================
        private void SimularUsuarioEnController(string username, int idUsuario, int idRol)
        {
            var claims = new List<Claim>
            {
                new Claim("Username", username),
                new Claim("IdUsuario", idUsuario.ToString()),
                new Claim("role", idRol.ToString())
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }
    }
}