using Microsoft.AspNetCore.Mvc;
using WebApp.Model;
using jsreport.AspNetCore;
using jsreport.Types;
using System.Threading.Tasks;
using jsreport.Local;
using jsreport.Binary;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        public IJsReportMVCService JsReportMVCService { get; }
        private readonly IViewRenderService _viewRenderService;

        public HomeController(IJsReportMVCService jsReportMVCService, IViewRenderService viewRenderService)
        {
            JsReportMVCService = jsReportMVCService;
            _viewRenderService = viewRenderService;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ViewInvoice()
        {
            return View("../Report/Invoice", InvoiceModel.Example());
        }

        [MiddlewareFilter(typeof(JsReportPipeline))]
        public IActionResult Invoice()
        {
            HttpContext.JsReportFeature().Recipe(Recipe.PhantomPdf);

            return View(InvoiceModel.Example());
        }

        [MiddlewareFilter(typeof(JsReportPipeline))]
        public IActionResult InvoiceDownload()
        {
            HttpContext.JsReportFeature().Recipe(Recipe.PhantomPdf)
                .OnAfterRender((r) => HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"myReport.pdf\"");

            return View("Invoice", InvoiceModel.Example());
        }

      
        [MiddlewareFilter(typeof(JsReportPipeline))]
        public async Task<IActionResult> InvoicePrint()
        {
            var header = await JsReportMVCService.RenderViewToStringAsync(HttpContext, RouteData, "Header", new { });
            var footer = await JsReportMVCService.RenderViewToStringAsync(HttpContext, RouteData, "Footer", new { });
            HttpContext.JsReportFeature()
                //.DebugLogsToResponse()
                //.Engine(Engine.JsRender)
                .Recipe(Recipe.PhantomPdf)
                .Configure((r) => r.Template.Phantom = new Phantom
                {
                   
                    //Header = header,
                    //Footer = footer,
                    //Format = PhantomFormat.A4,
                    //Margin = "0cm"
                    //WaitForJS = true
                })
                //.Engine(Engine.EJS)
                .OnAfterRender((r) =>
                    HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"myReport.pdf\""
                );
            return View("../Report/Invoice", InvoiceModel.Example());
        }

        public async Task<IActionResult> Localreport()
        {
            var header = await JsReportMVCService.RenderViewToStringAsync(HttpContext, RouteData, "Header", new { });
            var footer = await JsReportMVCService.RenderViewToStringAsync(HttpContext, RouteData, "Footer", new { });
            var body = await JsReportMVCService.RenderViewToStringAsync(HttpContext, RouteData, "Invoice", new { });

            var rs = new LocalReporting().UseBinary(JsReportBinary.GetBinary()).AsUtility().Create();

            var report = await rs.RenderAsync(new RenderRequest()
            {
                Template = new Template()
                {
                    Recipe = Recipe.PhantomPdf,
                    Engine = Engine.None,
                    Content = body
                }
            });

            HttpContext.JsReportFeature()
                //.DebugLogsToResponse()
                .Recipe(Recipe.PhantomPdf)
                .Configure((r) => r.Template.Phantom = new Phantom
                {

                    Header = header,
                    Footer = footer,
                    Format = PhantomFormat.A4,
                    Margin = "0cm",
                    BlockJavaScript = false
                })
                
                .OnAfterRender((r) =>
                    HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"myReport.pdf\""
                );
            return View("../Report/Invoice", InvoiceModel.Example());
        }


        [MiddlewareFilter(typeof(JsReportPipeline))]
        public async Task<IActionResult> InvoiceWithHeader()
        {
           

            var header = await JsReportMVCService.RenderViewToStringAsync(HttpContext, RouteData, "Header", new { });

            HttpContext.JsReportFeature()
                .Recipe(Recipe.PhantomPdf)
                .Configure((r) => r.Template.Phantom = new Phantom { Header = header });

            return View("Invoice", InvoiceModel.Example());
        }

        [MiddlewareFilter(typeof(JsReportPipeline))]
        public IActionResult Items()
        {
            HttpContext.JsReportFeature()                
                .Recipe(Recipe.HtmlToXlsx);

            return View(InvoiceModel.Example());
        }

        [MiddlewareFilter(typeof(JsReportPipeline))]
        public IActionResult ItemsExcelOnline()
        {
            HttpContext.JsReportFeature()
                .Configure(req => req.Options.Preview = true)
                .Recipe(Recipe.HtmlToXlsx);

            return View("Items", InvoiceModel.Example());
        }

        [MiddlewareFilter(typeof(JsReportPipeline))]
        public IActionResult InvoiceDebugLogs()
        {
            HttpContext.JsReportFeature()
                .DebugLogsToResponse()
                .Recipe(Recipe.PhantomPdf);

            return View("Invoice", InvoiceModel.Example());
        }
    }
}
