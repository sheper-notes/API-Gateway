using Ganss.Xss;
using KubeClient.Extensions.KubeConfig.Models;
using System.Text;

namespace API_Gateway
{

    public class XSSMiddleware
    {
        private readonly RequestDelegate _next;

        public XSSMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // enable buffering so that the request can be read by the model binders next
            httpContext.Request.EnableBuffering();

            // leaveOpen: true to leave the stream open after disposing,
            // so it can be read by the model binders
            using (var streamReader = new StreamReader
                  (httpContext.Request.Body, Encoding.UTF8, leaveOpen: true))
            {
                var raw = await streamReader.ReadToEndAsync();
                var sanitiser = new HtmlSanitizer();
                var sanitised = sanitiser.Sanitize(raw);

                if (raw != sanitised)
                {
                    httpContext.Response.StatusCode = 401;
                    await httpContext.Response.WriteAsync("XSS injection detected from middleware.");
                }
            }

            // rewind the stream for the next middleware
            httpContext.Request.Body.Seek(0, SeekOrigin.Begin);
            await _next.Invoke(httpContext);
        }
    }
}
