using ApiRestaurante2.Servicios;
using Entidades;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ApiRestaurante2.Servicios
{
    public class FacturaPdfService
    {
        public Task<byte[]> GenerarFacturaAsync(Factura factura)
        {
            // 1. Configurar Licencia Community de QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;

            // 2. Obtener la ruta del logo ajustada a tu carpeta assets/icons/logo_patio.png
            var rutaLogo = ObtenerRutaLogo();

            // 3. Generación del documento PDF
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(80, 200, Unit.Millimetre);
                    page.Margin(2, Unit.Millimetre);

                    // HEADER
                    page.Header().Column(header =>
                    {
                        // Se renderiza el logo solo si la imagen existe en el disco
                        if (!string.IsNullOrEmpty(rutaLogo) && File.Exists(rutaLogo))
                        {
                            header.Item()
                                .AlignCenter()
                                .Width(115)
                                .Height(115)
                                .Image(rutaLogo)
                                .FitArea();
                        }

                        header.Item()
                            .PaddingTop(1)
                            .Text("EL PATIO DE MI ABUELA")
                            .FontSize(9)
                            .Bold()
                            .AlignCenter();
                    });

                    // CONTENIDO
                    page.Content().Column(column =>
                    {
                        column.Item().PaddingTop(1).Row(row =>
                        {
                            row.RelativeItem().Text($"#{factura.Numero_factura_fiscal}").FontSize(6).Bold();
                            row.ConstantItem(45).Text($"{factura.Fecha_emision:dd/MM/yy HH:mm}").FontSize(5).AlignRight();
                        });

                        column.Item().Text($"Mesa:{factura.Id_orden} | Mesero:{factura.Id_mesero_atendiente}").FontSize(5).AlignCenter();

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

                            if (factura.Detalle_items != null)
                            {
                                foreach (var item in factura.Detalle_items)
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

                        column.Item().PaddingTop(0.5f).Text($"{factura.Metodo_pago} - ¡Gracias!").FontSize(4).AlignCenter();
                    });
                });
            });

            byte[] pdfBytes = document.GeneratePdf();
            return Task.FromResult(pdfBytes);
        }

        // MÉTODO DE BÚSQUEDA DE LOGO CON LA RUTA EXACTA DEl PROYECTO
        private string ObtenerRutaLogo()
        {
            var rutaBase = AppDomain.CurrentDomain.BaseDirectory;

            // 1. Ruta exacta en wwwroot/assets/icons/logo_patio.png (Servidor Azure / Publicado)
            var rutaOficial = Path.Combine(rutaBase, "wwwroot", "assets", "icons", "logo_patio.png");
            if (File.Exists(rutaOficial)) return rutaOficial;

            // 2. Ruta alternativa en assets/icons/logo_patio.png (Entorno local / Desarrollo)
            var rutaAlternativa = Path.Combine(rutaBase, "assets", "icons", "logo_patio.png");
            if (File.Exists(rutaAlternativa)) return rutaAlternativa;

            return string.Empty;
        }
    }
}