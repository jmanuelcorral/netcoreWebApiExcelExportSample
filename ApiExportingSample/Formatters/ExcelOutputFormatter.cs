using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ApiExportingSample.Formatters
{
    public class ExcelOutputFormatter : OutputFormatter
    {
        public ExcelOutputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
        }

        public bool CanWriteType(OutputFormatterCanWriteContext context)
        {
            return typeof(IEnumerable<object>).IsAssignableFrom(context.ObjectType);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var excelStream = CreateExcelFile(context.Object as IEnumerable<object>);

            var response = context.HttpContext.Response;

            response.ContentLength = excelStream.Length;
            return response.Body.WriteAsync(excelStream.ToArray()).AsTask();
        }

        public override void WriteResponseHeaders(OutputFormatterWriteContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var fileName = "demo.xlsx";

            context.HttpContext.Response.Headers["Content-Disposition"] =
                new ContentDispositionHeaderValue("attachment")
                {
                    FileName = fileName + ".xlsx"
                }.ToString();
            context.HttpContext.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        }

        private MemoryStream CreateExcelFile(IEnumerable<object> data)
        {
            var ms = new MemoryStream();

            using (var workbook = new XLWorkbook(XLEventTracking.Disabled))
            {
                var worksheet = workbook.Worksheets.Add("Sample Sheet");

                var props = new List<PropertyInfo>(data.First().GetType().GetProperties());

                var header = worksheet.FirstRow();
                int i = 1;
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                header.Style.Font.Bold = true;
                header.Style.Fill.BackgroundColor = XLColor.Aqua;
                foreach (var prop in props)
                {
                    header.Cell(i).Value = prop.Name;
                    i++;
                }
                worksheet.Cell(2, 1)
                    .InsertData(data);

                worksheet.Workbook.SaveAs(ms);
            }

            return ms;
        }
    }
}