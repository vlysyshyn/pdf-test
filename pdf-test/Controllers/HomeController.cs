using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc;
using pdf_test.Models;
using pdf_test.Services;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.ComponentModel;
using System.Diagnostics;

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

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Content().Table(table =>
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

                        foreach(var payment in model.Records)
                        {
                            table.Cell().Text(payment.OriginatingAccountNumber);
                            table.Cell().Text(payment.OriginatingSortCode);
                            table.Cell().Text($"£{payment.Amount}");
                            table.Cell().Text(payment.Currency);
                            table.Cell().Text(payment.PayeeAccountNumber);
                            table.Cell().Text(payment.PayeeSortCode);
                            table.Cell().Text($"{payment.Date.ToString("d")} {payment.Date.ToString("t")}");
                        }
                    });
                });
            }).GeneratePdf();


            return File(pdf, "application/pdf", $"questpdf-{Guid.NewGuid()}.pdf");
        }

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