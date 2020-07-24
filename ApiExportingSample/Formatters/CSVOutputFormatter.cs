using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ApiExportingSample.Formatters
{
    public class CsvOutputFormatter : TextOutputFormatter
    {
        public CsvOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        public override async Task WriteResponseBodyAsync(
            OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            var httpContext = context.HttpContext;
            var serviceProvider = httpContext.RequestServices;

            var logger = serviceProvider.GetRequiredService<ILogger<CsvOutputFormatter>>();
            var buffer = new StringBuilder();

            if (context.Object is IEnumerable<WeatherForecast> contacts)
            {
                foreach (var contact in contacts)
                {
                    FormatCSV(buffer, contact, logger);
                }
            }
            else
            {
                FormatCSV(buffer, (WeatherForecast)context.Object, logger);
            }

            await httpContext.Response.WriteAsync(buffer.ToString());
        }

        protected override bool CanWriteType(Type type)
        {
            return typeof(WeatherForecast).IsAssignableFrom(type) ||
                typeof(IEnumerable<WeatherForecast>).IsAssignableFrom(type);
        }

        private static void FormatCSV(
            StringBuilder buffer, WeatherForecast forecast, ILogger logger)
        {
            string delimiter = ",";
            string CSVLine = $"{forecast.Date}{delimiter}{forecast.Summary}{delimiter}{forecast.TemperatureC}{delimiter}{forecast.TemperatureF}";
            buffer.AppendLine(CSVLine);
            logger.LogInformation(CSVLine);
        }
    }
}