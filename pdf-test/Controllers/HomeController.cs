using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;
using pdf_test.Models;
using pdf_test.Services;
using PdfSharp.Drawing.Layout;
using PdfSharp.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Diagnostics;
using PdfSharp.Fonts;
using System.Text;
using QuestPDF.Helpers;

namespace pdf_test.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IViewRenderService _viewRenderService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConverter _converter;

        public HomeController(ILogger<HomeController> logger,
            IViewRenderService viewRenderService,
            IWebHostEnvironment webHostEnvironment,
            IConverter converter)
        {
            _logger = logger;
            _viewRenderService = viewRenderService;
            _webHostEnvironment = webHostEnvironment;
            _converter = converter;
        }

        public IActionResult Index()
        {
            var model = GetPdfModel();

            return View(model);
        }

        public ActionResult PdfView()
        {
            var model = GetPdfModel();

            return View("Pdf", model);
        }

        public async Task<ActionResult> Pdf()
        {
            var model = GetPdfModel();
            var html = await _viewRenderService.RenderToStringAsync("Home/Pdf", model);
            var renderer = new ChromePdfRenderer();
            var pdf = renderer.RenderHtmlAsPdf(html);

            return File(pdf.BinaryData, "application/pdf", $"ironpdf-{Guid.NewGuid()}.pdf");
        }

        public async Task<ActionResult> Pdf2()
        {
            var model = GetPdfModel();
            var html = await _viewRenderService.RenderToStringAsync("Home/Pdf", model);

            var document = new HtmlToPdfDocument
            {
                GlobalSettings =
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4
                },
                Objects =
                {
                    new ObjectSettings
                    {
                        HtmlContent = html
                    }
                }
            };

            var list = new List<Task>();
            for (var i = 0; i < 10; i++)
            {
                list.Add(Task.Run(() => _converter.Convert(document)));
            }

            await Task.WhenAll(list);

            var pdfData = _converter.Convert(document);

            return File(pdfData, "application/pdf", $"DinkToPdf-{Guid.NewGuid()}.pdf");
        }

        public async Task<ActionResult> Pdf3()
        {
            var model = GetPdfModel();
            var html = await _viewRenderService.RenderToStringAsync("Home/Pdf", model);
            QuestPDF.Settings.License = LicenseType.Community;

            var webRootPath = _webHostEnvironment.WebRootPath;
            var logoPath = Path.Combine(webRootPath, "images", "cashplus-bank-logo.png");
            var logoBytes = System.IO.File.ReadAllBytes(logoPath);

            var pdf = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    //page.Content().Height(100).Image(logoBytes);
                    page.Content().Column(column =>
                    {
                        column.Spacing(15);
                        column.Item().Background(Colors.Grey.Lighten2).Width(100).Image(logoBytes).FitArea();
                        column.Item().Table(table =>
                        {
                             table.ColumnsDefinition(columns =>
                             {
                                 columns.RelativeColumn();
                                 columns.RelativeColumn();
                                 columns.RelativeColumn();
                                 columns.RelativeColumn();
                                 columns.RelativeColumn();
                                 columns.RelativeColumn();
                                 columns.RelativeColumn();
                             });

                             table.Header(header =>
                             {
                                 header.Cell().Text("Originating AccountNumber");
                                 header.Cell().Text("Originating Sort Code");
                                 header.Cell().Text("Amount");
                                 header.Cell().Text("Currency");
                                 header.Cell().Text("PayeeAccountNumber");
                                 header.Cell().Text("PayeeSortCode");
                                 header.Cell().Text("Date");
                             });

                             foreach (var payment in model.Records)
                             {
                                 table.Cell().Text(payment.OriginatingAccountNumber);
                                 table.Cell().Text(payment.OriginatingSortCode);
                                 table.Cell().Text($"£{payment.Amount}");
                                 table.Cell().Text(payment.Currency);
                                 table.Cell().Text(RandomStr(50)/*payment.PayeeAccountNumber*/);
                                 table.Cell().Text(RandomStr(50)/*payment.PayeeSortCode*/);
                                 table.Cell().Text($"{payment.Date.ToString("d")} {payment.Date.ToString("t")}");
                             }
                        });
                    });
                });
            }).GeneratePdf();


            return File(pdf, "application/pdf", $"questpdf-{Guid.NewGuid()}.pdf");
        }

        public async Task<ActionResult> PdfSharp()
        {
            var webRootPath = _webHostEnvironment.WebRootPath;
            var logoPath = Path.Combine(webRootPath, "images", "cashplus-bank-logo.png");
            var logoBytes = System.IO.File.ReadAllBytes(logoPath);

            GlobalFontSettings.FontResolver = new FileFontResolver();

            var model = GetPdfModel();
            var html = await _viewRenderService.RenderToStringAsync("Home/Pdf", model);
            var document = new PdfSharp.Pdf.PdfDocument();
            /*PdfGenerator.AddPdfPages(data, html, PageSize.A4);
            byte[]? response = null;
            using (MemoryStream ms = new MemoryStream())
            {
                data.Save(ms);
                response = ms.ToArray();
            }*/

            for (int p = 0; p < 1; p++)
            {
                // Page Options
                PdfSharp.Pdf.PdfPage pdfPage = document.AddPage();
                pdfPage.Height = 842;//842
                pdfPage.Width = 590;

                

                // Get an XGraphics object for drawing
                XGraphics graph = XGraphics.FromPdfPage(pdfPage);

                MemoryStream stream = new MemoryStream();
                stream.Write(logoBytes, 0, logoBytes.Length);

                // Text format
                var format = new XStringFormat();
                format.LineAlignment = XLineAlignment.Near;
                format.Alignment = XStringAlignment.Near;
                var tf = new XTextFormatter(graph);

                var fontParagraph = new XFont("Roboto", 8, XFontStyleEx.Regular);
                //var fontParagraph = new XFont("Times New Roman", 8, XFontStyleEx.Regular);

                // Row elements
                int el1Width = 80;
                int el2Width = 380;

                // page structure options
                double lineHeight = 20;
                int marginLeft = 20;
                int marginTop = 100;

                int elHeight = 30;
                int rectHeight = 17;

                int interLineX1 = 2;
                int interLineX2 = 2 * interLineX1;

                int offSetX_1 = el1Width;
                int offSetX_2 = el1Width + el2Width;

                XSolidBrush rectStyle1 = new XSolidBrush(XColors.LightGray);
                XSolidBrush rectStyle2 = new XSolidBrush(XColors.DarkGreen);
                XSolidBrush rectStyle3 = new XSolidBrush(XColors.Red);

                graph.DrawRectangle(rectStyle1, marginLeft, 10, pdfPage.Width - 2 * marginLeft, 40);
                graph.DrawImage(XImage.FromStream(stream), marginLeft, 10);

                for (int i = 0; i < 30; i++)
                {
                    double distY = lineHeight * (i + 1);
                    double distY2 = distY - 2;

                    // header della G
                    if (i == 0)
                    {
                        graph.DrawRectangle(rectStyle2, marginLeft, marginTop, pdfPage.Width - 2 * marginLeft, rectHeight);

                        tf.DrawString("column1", fontParagraph, XBrushes.White,
                                      new XRect(marginLeft, marginTop, el1Width, elHeight), format);

                        tf.DrawString("column2", fontParagraph, XBrushes.White,
                                      new XRect(marginLeft + offSetX_1 + interLineX1, marginTop, el2Width, elHeight), format);

                        tf.DrawString("column3", fontParagraph, XBrushes.White,
                                      new XRect(marginLeft + offSetX_2 + 2 * interLineX2, marginTop, el1Width, elHeight), format);

                        // stampo il primo elemento insieme all'header
                        graph.DrawRectangle(rectStyle1, marginLeft, distY2 + marginTop, el1Width, rectHeight);
                        tf.DrawString("text1", fontParagraph, XBrushes.Black,
                                      new XRect(marginLeft, distY + marginTop, el1Width, elHeight), format);

                        //ELEMENT 2 - BIG 380
                        graph.DrawRectangle(rectStyle1, marginLeft + offSetX_1 + interLineX1, distY2 + marginTop, el2Width, rectHeight);
                        tf.DrawString(
                            "text2",
                            fontParagraph,
                            XBrushes.Black,
                            new XRect(marginLeft + offSetX_1 + interLineX1, distY + marginTop, el2Width, elHeight),
                            format);


                        //ELEMENT 3 - SMALL 80

                        graph.DrawRectangle(rectStyle1, marginLeft + offSetX_2 + interLineX2, distY2 + marginTop, el1Width, rectHeight);
                        tf.DrawString(
                            "text3",
                            fontParagraph,
                            XBrushes.Black,
                            new XRect(marginLeft + offSetX_2 + 2 * interLineX2, distY + marginTop, el1Width, elHeight),
                            format);


                    }
                    else
                    {

                        //if (i % 2 == 1)
                        //{
                        //  graph.DrawRectangle(TextBackgroundBrush, marginLeft, lineY - 2 + marginTop, pdfPage.Width - marginLeft - marginRight, lineHeight - 2);
                        //}

                        //ELEMENT 1 - SMALL 80
                        graph.DrawRectangle(rectStyle1, marginLeft, marginTop + distY2, el1Width, rectHeight);
                        tf.DrawString(
                            RandomStr(50),
                            fontParagraph,
                            XBrushes.Black,
                            new XRect(marginLeft, marginTop + distY, el1Width, elHeight),
                            format);

                        //ELEMENT 2 - BIG 380
                        graph.DrawRectangle(rectStyle1, marginLeft + offSetX_1 + interLineX1, distY2 + marginTop, el2Width, rectHeight);
                        tf.DrawString(
                            "text2",
                            fontParagraph,
                            XBrushes.Black,
                            new XRect(marginLeft + offSetX_1 + interLineX1, marginTop + distY, el2Width, elHeight),
                            format);


                        //ELEMENT 3 - SMALL 80

                        graph.DrawRectangle(rectStyle1, marginLeft + offSetX_2 + interLineX2, distY2 + marginTop, el1Width, rectHeight);
                        tf.DrawString(
                            RandomStr(50),
                            fontParagraph,
                            XBrushes.Black,
                            new XRect(marginLeft + offSetX_2 + 2 * interLineX2, marginTop + distY, el1Width, elHeight),
                            format);
                    }
                }
            }

            //document.Save(filename);

            byte[] bytes = null;
            using (MemoryStream stream = new MemoryStream())
            {
                document.Save(stream, true);
                bytes = stream.ToArray();
            }

            return File(bytes, "application/pdf", $"pdfsharp-{Guid.NewGuid()}.pdf");
        }

        private static string RandomStr(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();

            var stringBuilder = new StringBuilder(length);
            for (var i =0; i < length; i++)
            {
                stringBuilder.Append(chars[random.Next(chars.Length)]);
            }

            return stringBuilder.ToString();
        }

        /*public async Task<ActionResult> PdfSharpHtml()
        {
            GlobalFontSettings.FontResolver = new FileFontResolver();

            var model = GetPdfModel();
            var html = await _viewRenderService.RenderToStringAsync("Home/Pdf", model);
            var document = new PdfSharp.Pdf.PdfDocument();
            PdfGenerator.AddPdfPages(document, html, PageSize.A4);
            byte[]? response = null;
            using (MemoryStream ms = new MemoryStream())
            {
                data.Save(ms);
                response = ms.ToArray();
            }

            return File(response, "application/pdf", $"pdfsharp-html-{Guid.NewGuid()}.pdf");
        }*/

        private BatchPdfModel GetPdfModel()
        {
            var webRootPath = _webHostEnvironment.WebRootPath;
            var logoPath = Path.Combine(webRootPath, "images", "cashplus-bank-logo.png");
            var logoBytes = System.IO.File.ReadAllBytes(logoPath);
            var logoBase64 = Convert.ToBase64String(logoBytes);

            var model = new BatchPdfModel
            {
                Logo = logoBase64
            };

            for (int i = 0; i < 100; i++)
            {
                model.Records.Add(new TestBatchRecord
                {
                    OriginatingAccountNumber = $"account_number_{i}",
                    OriginatingSortCode = $"sort_code_{i}",
                    Amount = 1.ToString("G"),
                    Currency = "GBP",
                    PayeeAccountNumber = $"payee_account_number_{i}",
                    PayeeSortCode = $"payee_sort_code_{i}",
                    Date = DateTime.Now
                });
            }

            return model;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}