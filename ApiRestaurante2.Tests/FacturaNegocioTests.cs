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
    public class FacturaNegocioTests
    {
        private readonly Mock<FacturaDatos> _mockFacturaDatos;
        private readonly FacturaNegocio _negocio;

        public FacturaNegocioTests()
        {
            // Mockeamos la cadena de dependencias (igual que en pruebas anteriores)
            var mockConfig = new Mock<IConfiguration>();
            var mockConexion = new Mock<Conexion>(mockConfig.Object);
            _mockFacturaDatos = new Mock<FacturaDatos>(mockConexion.Object);

            _negocio = new FacturaNegocio(_mockFacturaDatos.Object);
        }

        // ==========================================
        // PRUEBA 1: Facturación exitosa (caso feliz)
        // ==========================================
        [Fact]
        public void Generar_ConDatosValidos_RetornaMensajeExitoConNumeroFiscal()
        {
            // 🟢 ARRANGE - Preparar datos válidos
            var factura = new Factura
            {
                Id_orden = 101,
                Id_mesero_atendiente = 12,
                Id_cajero_cobrador = 3,
                Metodo_pago = "Tarjeta",
                Subtotal = 1000m,
                Monto_impuestos = 150m,      // 15% IVA
                Monto_propina = 50m,          // Propina voluntaria
                Monto_total = 1200m           // 1000 + 150 + 50
            };
            string usuario = "cajero_maria";
            string respuestaEsperada = "Factura #555 generada correctamente. Fiscal: A01-0000555";

            // Mockeamos la capa de datos para que retorne el mensaje de éxito
            _mockFacturaDatos
                .Setup(x => x.Generar(It.IsAny<Factura>(), usuario))
                .Returns(respuestaEsperada);

            // 🟡 ACT - Ejecutar el método de negocio
            string resultado = _negocio.Generar(factura, usuario);

            // 🔴 ASSERT - Verificar resultados
            resultado.Should().Be(respuestaEsperada);
            resultado.Should().Contain("Factura #");
            resultado.Should().Contain("Fiscal:");

            // Verificar que se llamó exactamente 1 vez a la capa de datos
            _mockFacturaDatos.Verify(x => x.Generar(It.IsAny<Factura>(), usuario), Times.Once);
        }

        // ==========================================
        // PRUEBA 2: Validación de cálculos de totales
        // ==========================================
        [Fact]
        public void Generar_ValidaQueTotalesSeanConsistentes_YLosPasaALaCapaDatos()
        {
            // 🟢 ARRANGE
            var factura = new Factura
            {
                Id_orden = 202,
                Id_mesero_atendiente = 15,
                Id_cajero_cobrador = 3,
                Metodo_pago = "Efectivo",
                Subtotal = 500m,
                Monto_impuestos = 75m,        // 15% de 500
                Monto_propina = 25m,
                Monto_total = 600m            // 500 + 75 + 25 = 600 ✓
            };
            string usuario = "cajero_luis";

            // Variable para capturar la factura que recibe la capa de datos
            Factura facturaRecibida = null;
            _mockFacturaDatos
                .Setup(x => x.Generar(It.IsAny<Factura>(), usuario))
                .Callback<Factura, string>((fac, usr) => facturaRecibida = fac)
                .Returns("Factura #999 generada correctamente. Fiscal: A01-0000999");

            // 🟡 ACT
            _negocio.Generar(factura, usuario);

            // 🔴 ASSERT - Verificar que los totales se preservaron intactos
            facturaRecibida.Should().NotBeNull();
            facturaRecibida.Subtotal.Should().Be(500m);
            facturaRecibida.Monto_impuestos.Should().Be(75m);
            facturaRecibida.Monto_propina.Should().Be(25m);
            facturaRecibida.Monto_total.Should().Be(600m);

            // Verificar consistencia matemática: Total = Subtotal + Impuestos + Propina
            var totalCalculado = facturaRecibida.Subtotal +
                               facturaRecibida.Monto_impuestos +
                               facturaRecibida.Monto_propina;

            facturaRecibida.Monto_total.Should().Be(totalCalculado,
                "El monto total debe ser la suma de subtotal, impuestos y propina");
        }

        // ==========================================
        // PRUEBA EXTRA: Validaciones de entrada (bonus)
        // ==========================================
        [Fact]
        public void Generar_SinOrdenOMetodoPago_RetornaErrorSinLlamarDatos()
        {
            // 🟢 ARRANGE
            var facturaSinOrden = new Factura { Id_orden = 0, Metodo_pago = "Tarjeta" };
            var facturaSinMetodo = new Factura { Id_orden = 101, Metodo_pago = "" };
            var facturaPropinaNegativa = new Factura { Id_orden = 101, Metodo_pago = "Efectivo", Monto_propina = -10m };
            string usuario = "cajero_ana";

            // 🟡 ACT + 🔴 ASSERT
            _negocio.Generar(facturaSinOrden, usuario).Should().Be("La orden es obligatoria.");
            _negocio.Generar(facturaSinMetodo, usuario).Should().Be("El método de pago es obligatorio.");
            _negocio.Generar(facturaPropinaNegativa, usuario).Should().Be("La propina no puede ser negativa.");

            // Verificar que NUNCA se llamó a la capa de datos (falló en validación)
            _mockFacturaDatos.Verify(x => x.Generar(It.IsAny<Factura>(), It.IsAny<string>()), Times.Never);
        }
    }
}