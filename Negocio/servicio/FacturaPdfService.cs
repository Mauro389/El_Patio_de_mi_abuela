using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Entidades;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace ApiRestaurante2.Servicios
{
    public class FacturaPdfService
    {
        public byte[] GenerarFactura(Factura factura)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(80, 200, Unit.Millimetre);
                    page.Margin(2, Unit.Millimetre);

                    // (Tamaño Razonable - 55mm)
                    page.Header()
                        .Column(header =>
                        {
                            // Logo del restaurante 
                            header.Item()
                                .AlignCenter()
                                .Width(115)        // ✅ Aumentado115
                                .Height(115)       // ✅ Aumentado 115
                                .Image(ObtenerRutaLogo())
                                .FitArea();

                            // Nombre del restaurante
                            header.Item()
                                .PaddingTop(1)
                                .Text("EL PATIO DE MI ABUELA")
                                .FontSize(9)
                                .Bold()
                                .AlignCenter();
                        });

                    page.Content()
                        .Column(column =>
                        {
                            // Datos en 1 línea
                            column.Item().PaddingTop(1).Row(row =>
                            {
                                row.RelativeItem().Text($"#{factura.Numero_factura_fiscal}").FontSize(6).Bold();
                                row.ConstantItem(45).Text($"{factura.Fecha_emision:dd/MM/yy HH:mm}").FontSize(5).AlignRight();
                            });

                            column.Item().Text($"Mesa:{factura.Id_orden} | Mesero:{factura.Id_mesero_atendiente}").FontSize(5).AlignCenter();

                            // Separador
                            column.Item().PaddingVertical(0.3f).Text("--------------------------------").FontSize(3).AlignCenter();

                            // Tabla de Items
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.ConstantColumn(18);
                                    columns.ConstantColumn(25);
                                    columns.ConstantColumn(28);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Prod").FontSize(5).Bold();
                                    header.Cell().Element(CellStyle).Text("Cant").FontSize(5).Bold().AlignRight();
                                    header.Cell().Element(CellStyle).Text("P.U.").FontSize(5).Bold().AlignRight();
                                    header.Cell().Element(CellStyle).Text("Total").FontSize(5).Bold().AlignRight();
                                });

                                if (factura.DetalleItems != null)
                                {
                                    foreach (var item in factura.DetalleItems)
                                    {
                                        var subtotal = item.Cantidad * item.Precio_al_momento;
                                        var nombre = item.NombreProducto ?? "Item";
                                        if (nombre.Length > 10) nombre = nombre.Substring(0, 10) + "..";

                                        table.Cell().Element(CellStyle).Text(nombre).FontSize(5);
                                        table.Cell().Element(CellStyle).Text($"{item.Cantidad}").FontSize(5).AlignRight();
                                        table.Cell().Element(CellStyle).Text($"{item.Precio_al_momento:C}").FontSize(5).AlignRight();
                                        table.Cell().Element(CellStyle).Text($"{subtotal:C}").FontSize(5).AlignRight();
                                    }
                                }

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.PaddingVertical(0.3f);
                                }
                            });

                            // Separador
                            column.Item().PaddingVertical(0.3f).Text("--------------------------------").FontSize(3).AlignCenter();

                            // Totales
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Subtotal:").FontSize(5);
                                row.ConstantItem(45).Text($"{factura.Subtotal:C}").FontSize(5).AlignRight();
                            });

                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Imp:").FontSize(5);
                                row.ConstantItem(45).Text($"{factura.Monto_impuestos:C}").FontSize(5).AlignRight();
                            });

                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Propina:").FontSize(5);
                                row.ConstantItem(45).Text($"{factura.Monto_propina:C}").FontSize(5).AlignRight();
                            });

                            column.Item().PaddingTop(0.3f).Row(row =>
                            {
                                row.RelativeItem().Text("TOTAL:").FontSize(7).Bold();
                                row.ConstantItem(45).Text($"{factura.Monto_total:C}").FontSize(8).Bold().AlignRight();
                            });

                            // Pie mínimo
                            column.Item().PaddingTop(0.5f).Text($"{factura.Metodo_pago} - ¡Gracias!").FontSize(4).AlignCenter();
                        });

                    // ❌ SIN FOOTER da error 
                });
            });

            return document.GeneratePdf();
        }

        // MÉTODO PARA OBTENER LA RUTA DEL LOGO
        private string ObtenerRutaLogo()
        {
            var rutaBase = AppDomain.CurrentDomain.BaseDirectory;
            var rutaLogo = Path.Combine(rutaBase, "wwwroot", "Imagenes", "logo.png");

            if (File.Exists(rutaLogo))
            {
                return rutaLogo;
            }

            var rutaAlternativa = Path.Combine(rutaBase, "Imagenes", "logo.png");
            if (File.Exists(rutaAlternativa))
            {
                return rutaAlternativa;
            }

            return string.Empty;
        }
    }
}