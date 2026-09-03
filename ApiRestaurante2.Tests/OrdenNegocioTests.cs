using System;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Configuration;
using Xunit;
using Entidades;
using Datos;
using Negocio;

namespace ApiRestaurante2.Tests
{
    public class OrdenNegocioTests
    {
        private readonly Mock<OrdenDatos> _mockOrdenDatos;
        private readonly OrdenNegocio _negocio;

        public OrdenNegocioTests()
        {
            // 1️⃣ Mockeamos IConfiguration (interfaz de .NET para appsettings.json)
            var mockConfig = new Mock<IConfiguration>();

            // 2️⃣ Mockeamos Conexion pasándole el config mockeado
            var mockConexion = new Mock<Conexion>(mockConfig.Object);

            // 3️⃣ Mockeamos OrdenDatos pasándole la conexion mockeada
            _mockOrdenDatos = new Mock<OrdenDatos>(mockConexion.Object);

            _negocio = new OrdenNegocio(_mockOrdenDatos.Object);
        }

        [Fact]
        public void Agregar_ConDatosValidos_RetornaMensajeExito()
        {
            var orden = new Orden { Id_mesa = 5, Id_mesero = 12, Estado = "Abierta" };
            string respuestaEsperada = "Orden creada correctamente con ID: 101";

            _mockOrdenDatos
                .Setup(x => x.Agregar(It.IsAny<Orden>(), It.IsAny<string>()))
                .Returns(respuestaEsperada);

            string resultado = _negocio.Agregar(orden, "mesero_juan");

            resultado.Should().Be(respuestaEsperada);
            _mockOrdenDatos.Verify(x => x.Agregar(It.IsAny<Orden>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void Agregar_SinMesaOMesero_RetornaError()
        {
            var ordenSinMesa = new Orden { Id_mesa = 0, Id_mesero = 12 };
            var ordenSinMesero = new Orden { Id_mesa = 5, Id_mesero = 0 };
            string usuario = "mesero_ana";

            _negocio.Agregar(ordenSinMesa, usuario).Should().Be("Mesa y Mesero son obligatorios.");
            _negocio.Agregar(ordenSinMesero, usuario).Should().Be("Mesa y Mesero son obligatorios.");

            _mockOrdenDatos.Verify(x => x.Agregar(It.IsAny<Orden>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Agregar_ConEstadoVacio_AsignaEstadoAbierta()
        {
            var orden = new Orden { Id_mesa = 3, Id_mesero = 7, Estado = "" };
            string respuestaMock = "Orden creada correctamente con ID: 205";
            Orden ordenRecibidaPorDatos = null;

            _mockOrdenDatos
                .Setup(x => x.Agregar(It.IsAny<Orden>(), It.IsAny<string>()))
                .Callback<Orden, string>((ord, usr) => ordenRecibidaPorDatos = ord)
                .Returns(respuestaMock);

            _negocio.Agregar(orden, "cajero_luis");

            ordenRecibidaPorDatos.Should().NotBeNull();
            ordenRecibidaPorDatos.Estado.Should().Be("Abierta");
            _mockOrdenDatos.Verify(x => x.Agregar(It.IsAny<Orden>(), It.IsAny<string>()), Times.Once);
        }
    }
}