using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jsreport.AspNetCore;
using jsreport.Types;
using Microsoft.AspNetCore.Mvc;
using WebApp.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApp
{
    [Route("api/[controller]")]
    public class ReportController : Controller
    {
        private readonly IViewRenderService _viewRenderService;
        public IJsReportMVCService JsReportMVCService { get; }
        public ReportController(IViewRenderService viewRenderService, IJsReportMVCService jsReportMVCService)
        {
            _viewRenderService = viewRenderService;
            JsReportMVCService = jsReportMVCService;
        }

        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("{ActionName}")]
        [MiddlewareFilter(typeof(JsReportPipeline))]
        public async Task<IActionResult> Print()
        {
            var header = await JsReportMVCService.RenderViewToStringAsync(HttpContext, RouteData, "Header", new { });
            var footer = await JsReportMVCService.RenderViewToStringAsync(HttpContext, RouteData, "Footer", new { });
            HttpContext.JsReportFeature()
                //.DebugLogsToResponse()
                .Recipe(Recipe.PhantomPdf)
                .Configure((r) => r.Template.Phantom = new Phantom {
                    Header = header,
                    Footer = footer,
                    Format = PhantomFormat.A4,
                    Margin = "0cm",
                    BlockJavaScript = false
                    

                    //Orientation = PhantomOrientation.Portrait
                    //WaitForJS = true

                    //JavascriptDelay = 1000

                })
                .OnAfterRender((r) => 
                    HttpContext.Response.Headers["Content-Disposition"] = "attachment; filename=\"myReport.pdf\""
                );

            //var result = await _viewRenderService.RenderToStringAsync("Home/Invoice", InvoiceModel.Example());

            //byte[] array = Encoding.ASCII.GetBytes(result);
            // Stream stream = new MemoryStream(array);

            // var response = FileStreamResult(array, "application/octet-stream"); // FileStreamResult

            return View("Invoice", InvoiceModel.Example());
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        public IActionResult Invoice()
        {
            return View(InvoiceModel.Example());
        }
    }
}
