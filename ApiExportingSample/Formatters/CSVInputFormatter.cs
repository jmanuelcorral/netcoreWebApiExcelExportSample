using DocumentFormat.OpenXml.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ApiExportingSample.Formatters
{
    public class CSVInputFormatter : TextInputFormatter
    {
        public CSVInputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("text/csv"));

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(
            InputFormatterContext context, Encoding effectiveEncoding)
        {
            var httpContext = context.HttpContext;
            var serviceProvider = httpContext.RequestServices;
            var listOfTemperatures = new List<WeatherForecast>();
            var logger = serviceProvider.GetRequiredService<ILogger<CSVInputFormatter>>();

            using var reader = new StreamReader(httpContext.Request.Body, effectiveEncoding);
            string nameLine = null;

            try
            {
                while (!reader.EndOfStream)
                {
                    nameLine = await ReadLineAsync(reader, context, logger);
                    var temperature = TransformInDto(nameLine);
                }

                return await InputFormatterResult.SuccessAsync(listOfTemperatures);
            }
            catch
            {
                logger.LogError("Read failed: nameLine = {nameLine}", nameLine);
                return await InputFormatterResult.FailureAsync();
            }
        }

        protected override bool CanReadType(Type type)
        {
            return type == typeof(WeatherForecast);
        }

        private static async Task<string> ReadLineAsync(
            StreamReader reader,
            InputFormatterContext context,
            ILogger logger)
        {
            var line = await reader.ReadLineAsync();
            return line;
        }

        private static WeatherForecast TransformInDto(string line)
        {
            string delimiter = ",";
            var values = line.Split(delimiter);
            return new WeatherForecast
            {
                Date = DateTime.Parse(values[0]),
                Summary = values[1],
                TemperatureC = int.Parse(values[2]),
            };
        }
    }
}