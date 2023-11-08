using Aephy.WEB.Admin.Models;
using Aephy.WEB.Provider;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System.Net.Mime;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace Aephy.WEB.Admin.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly string _rootPath;
        private readonly IConfiguration _configuration;
        private readonly IApiRepository _apiRepository;

        public InvoiceController(IWebHostEnvironment hostEnvironment, IConfiguration configuration, IApiRepository apiRepository)
        {
            _rootPath = hostEnvironment.WebRootPath;
            _configuration = configuration;
            _apiRepository = apiRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        //GetActiveProjectInvoices
        [HttpGet]
        public async Task<string> GetClientInvoices()
        {
            var projectData = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetClientInvoiceslist", HttpMethod.Get);
            return projectData;

        }

        //GetInvoiceTranscationTypeDetails
        [HttpPost]
        public async Task<string> GetInvoiceTranscationTypeDetails([FromBody] SolutionFundModel model)
        {
            if (model != null)
            {
                try
                {
                    var data = await _apiRepository.MakeApiCallAsync("api/Client/GetInvoiceTranscationTypeDetails", HttpMethod.Post, model);
                    return data;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return "";
        }

        //GetClientInvoiceDetails
        [HttpPost]
        public async Task<string> GetClientInvoiceDetails([FromBody] SolutionFundModel model)
        {
            var invoiceData = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetInvoiceDetails", HttpMethod.Post, model);
            return invoiceData;

        }

        public async Task<IActionResult> DownloadInvoice(int InvoiceId,string ClientId)
        {
            SolutionFundModel solutionFund = new SolutionFundModel();
            solutionFund.InvoiceId = InvoiceId;
            solutionFund.UserId = ClientId;
            var invoiceDetails = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetInvoiceDetails", HttpMethod.Post, solutionFund);
            dynamic datas = JObject.Parse(invoiceDetails);
            var Details = datas.Result;
            var FundDetails = datas.Result.FundType;

            float cellHeight = 100f;
            iTextSharp.text.Document document = new iTextSharp.text.Document();
            var memoryStream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();
            PdfPTable centertable = new PdfPTable(1);
            PdfPCell centerCell = new PdfPCell(new Phrase(" "));
            centerCell.HorizontalAlignment = Element.ALIGN_CENTER;
            centerCell.Padding = 7.5f;

            IPdfPCellEvent roundedRectangle = new RoundedCellEvent(5, new BaseColor(System.Drawing.Color.Gray)); // Specify corner radius and fill color
            centerCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
            centerCell.PaddingBottom = 5f;
            centertable.DefaultCell.Border = 0;
            centertable.AddCell(centerCell);


            PdfPTable righttable = new PdfPTable(1);
            PdfPCell rightCell = new PdfPCell();
            rightCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            rightCell.Border = iTextSharp.text.Rectangle.NO_BORDER;
            righttable.DefaultCell.Border = 0;
            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance("https://aephyweb.azurewebsites.net/assets/img/ephylink_no_background_blue.png");


            logo.ScaleAbsolute(70f, 70f);
            rightCell.PaddingLeft = 80f;
            rightCell.PaddingBottom = -75f;
            rightCell.PaddingTop = 0f;
            rightCell.PaddingRight = 0f;

            logo.SetAbsolutePosition(iTextSharp.text.PageSize.A4.Rotate().Width - 0, 20);
            rightCell.AddElement(logo);


            righttable.AddCell(rightCell);
            AddClientTables(document, writer, Details);
            document.Add(new Paragraph("\r\n  "));
            AddTwoContentTables(document, writer, Details);
            document.Add(new Paragraph("\r\n  "));
            AddSingleTableWithCustomColumnWidths(document, writer, Details, FundDetails);
            document.NewPage();
            document.NewPage();
            document.Close();
            Response.ContentType = MediaTypeNames.Application.Pdf;
            var contentDisposition = new ContentDisposition
            {
                FileName = "Inv-" + Details.InvoiceNumber + ".pdf",
                Inline = false
            };
            Response.Headers.Add("Content-Disposition", contentDisposition.ToString());
            return File(memoryStream.ToArray(), Response.ContentType);
        }


        private void AddClientTables(iTextSharp.text.Document doc, PdfWriter writer, dynamic data)
        {
            //dynamic datas = JObject.Parse(data);
            //var Details = datas.Result;

            BaseFont baseF14 = BaseFont.CreateFont();

            // Master Table: Contains two side-by-side tables
            PdfPTable masterTable = new PdfPTable(2);
            masterTable.WidthPercentage = 100;
            masterTable.SetWidths(new float[] { 68f, 30f });
            masterTable.DefaultCell.Border = 3;

            // Left Table: Display the logo
            PdfPTable leftTable = new PdfPTable(1);
            leftTable.HorizontalAlignment = Element.ALIGN_LEFT;
            float[] widths = new float[] { 40f };

            leftTable.SetWidths(widths);
            iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance("https://aephyweb.azurewebsites.net/assets/img/ephylink_no_background_blue.png");
            img.ScaleToFit(150f, 150f);
            PdfPCell logoCell = new PdfPCell(img);
            logoCell.HorizontalAlignment = Element.ALIGN_LEFT;
            logoCell.Border = PdfPCell.NO_BORDER;
            leftTable.AddCell(logoCell);

            // Right Table: Labels and Text Fields
            PdfPTable rightTable = new PdfPTable(2);
            // rightTable.WidthPercentage = 30;
            float[] widths1 = new float[] { 0f, 100f };
            rightTable.SetWidths(widths1);
            rightTable.HorizontalAlignment = Element.ALIGN_RIGHT;

            // Set the font for the entire table
            rightTable.DefaultCell.Phrase = new Phrase() { Font = FontFactory.GetFont(FontFactory.HELVETICA, 8) };

            // Create label cells and set font styles
            PdfPCell labelCell1 = new PdfPCell(new Phrase("               Invoice                   ", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));
            labelCell1.BackgroundColor = new BaseColor(0xD9, 0xD9, 0xD9); // Set background color
            labelCell1.VerticalAlignment = Element.ALIGN_MIDDLE; // Center the text vertically
            labelCell1.Colspan = 5;
            labelCell1.FixedHeight = 23; ;

            //labelCell1.Colspan = 2;
            //labelCell1.PaddingLeft = 40;


            PdfPCell labelCell6 = new PdfPCell(new Phrase("Invoice  # " + data.InvoiceNumber, FontFactory.GetFont(FontFactory.HELVETICA, 8)));
            PdfPCell labelCell2 = new PdfPCell(new Phrase("Date " + data.InvoiceDate.ToString("dd MMMM yyyy"), FontFactory.GetFont(FontFactory.HELVETICA, 8)));
            PdfPCell labelCell3 = new PdfPCell(new Phrase("Due Date " + data.InvoiceDate.ToString("dd MMMM yyyy"), FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8))); // Bold font for "Due Date" label
            PdfPCell labelCell4 = new PdfPCell(new Phrase("Total Amount " + data.PreferredCurrency + data.TotalAmount, FontFactory.GetFont(FontFactory.HELVETICA, 8)));
            PdfPCell labelCell5 = new PdfPCell(new Phrase("Total Due " + data.TotalAmount, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8)));

            // Create textField cells
            PdfPCell textFieldCell1 = new PdfPCell(); // You need to add a text field here
            PdfPCell textFieldCell2 = new PdfPCell(); // You need to add a text field here
            PdfPCell textFieldCell3 = new PdfPCell(); // You need to add a text field here
            PdfPCell textFieldCell4 = new PdfPCell(); // You need to add a text field here
            PdfPCell textFieldCell5 = new PdfPCell(); // You need to add a text field here
            PdfPCell textFieldCell6 = new PdfPCell(); // You need to add a text field here

            // Set cell properties
            PdfPCell[] cells = { labelCell1, textFieldCell1, labelCell6, textFieldCell6, labelCell2, textFieldCell2, labelCell3, textFieldCell3, labelCell4, textFieldCell4, labelCell5, textFieldCell5 };

            foreach (PdfPCell cell in cells)
            {
                cell.Border = PdfPCell.NO_BORDER;
                rightTable.AddCell(cell);
            }

            // Add the left and right tables to the master table
            PdfPCell leftCell = new PdfPCell(leftTable);
            leftCell.Border = PdfPCell.NO_BORDER;

            PdfPCell rightCell = new PdfPCell(rightTable);
            rightCell.Border = PdfPCell.NO_BORDER;

            masterTable.AddCell(leftCell);
            masterTable.AddCell(rightCell);

            // Add the master table to the document
            doc.Add(masterTable);
        }

        private void AddTwoContentTables(iTextSharp.text.Document doc, PdfWriter writer, dynamic data)
        {
            // Master Table: Contains two side-by-side tables
            PdfPTable masterTable = new PdfPTable(2);
            masterTable.WidthPercentage = 100;

            // Table 1: Content 1
            PdfPTable table1 = new PdfPTable(1);
            table1.WidthPercentage = 100;
            table1.HorizontalAlignment = Element.ALIGN_LEFT;

            // Content 1
            string content1 = "From\nEphylink\nDimitrios Vamvakas\nEllispontou 39, Patra, 26226\nAchaia, Greece\nVAT ID: 148366653";

            // Split content1 into lines
            string[] content1Lines = content1.Split('\n');

            foreach (string line in content1Lines)
            {
                iTextSharp.text.Font font = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                if (line == "From")
                {
                    font.Color = new BaseColor(195, 120, 19);
                    font.Size = 14;


                }
                if (line == "Ephylink")
                {
                    PdfPCell spaceCell1 = new PdfPCell();
                    spaceCell1.Border = 0;
                    spaceCell1.FixedHeight = 6f;
                    table1.AddCell(spaceCell1);
                    font.Size = 9;
                    font.SetStyle(iTextSharp.text.Font.BOLD);

                }

                Chunk chunk = new Chunk(line, font);
                PdfPCell contentCell1 = new PdfPCell(new Phrase(chunk));
                contentCell1.Border = PdfPCell.NO_BORDER;
                table1.AddCell(contentCell1);

                PdfPCell spaceCell2 = new PdfPCell();
                spaceCell2.Border = 0;
                spaceCell2.FixedHeight = 3f;
                table1.AddCell(spaceCell2);


            }

            // Table 2: Content 2
            PdfPTable table2 = new PdfPTable(1);
            table2.WidthPercentage = 100;
            table2.HorizontalAlignment = Element.ALIGN_LEFT;

            // Content 2
            var taxDetails = data.TaxType.ToString();
            string content2 = string.Empty;
            if (taxDetails != null && taxDetails != "" && taxDetails != "null")
            {
                content2 = "Bill to\n" + data.ClientFullName + "\nName \nAddress: " + data.ClientAddress + "\n" + data.TaxType.ToString() + " ID: " + data.TaxId.ToString();
            }
            else
            {
                content2 = "Bill to\n" + data.ClientFullName + "\nName \nAddress: " + data.ClientAddress + "\n";
            }


            // Split content2 into lines
            string[] content2Lines = content2.Split('\n');

            foreach (string line in content2Lines)
            {
                iTextSharp.text.Font font = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                if (line == "Bill to")
                {
                    font.Color = new BaseColor(195, 120, 19);
                    font.Size = 14;
                }
                if (line == "Client A")
                {
                    PdfPCell spaceCell3 = new PdfPCell();
                    spaceCell3.Border = 0;
                    spaceCell3.FixedHeight = 6f;
                    table2.AddCell(spaceCell3);
                    font.SetStyle(iTextSharp.text.Font.BOLD);
                    font.Size = 9;
                }
                Chunk chunk = new Chunk(line, font);
                PdfPCell contentCell2 = new PdfPCell(new Phrase(chunk));
                contentCell2.Border = PdfPCell.NO_BORDER;
                table2.AddCell(contentCell2);

                PdfPCell spaceCell4 = new PdfPCell();
                spaceCell4.Border = 0;
                spaceCell4.FixedHeight = 3f;
                table2.AddCell(spaceCell4);
            }

            // Add the left and right tables to the master table
            PdfPCell leftCell = new PdfPCell(table1);
            leftCell.Border = PdfPCell.NO_BORDER;

            PdfPCell rightCell = new PdfPCell(table2);
            rightCell.Border = PdfPCell.NO_BORDER;

            masterTable.AddCell(leftCell);
            masterTable.AddCell(rightCell);

            // Add the master table to the document
            doc.Add(masterTable);
        }

        private void AddSingleTableWithCustomColumnWidths(iTextSharp.text.Document doc, PdfWriter writer, dynamic data, dynamic fundType)
        {
            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 80f, 20f }); // Set column widths to 70% and 30%
            table.DefaultCell.Border = PdfPCell.BOX;
            table.DefaultCell.Phrase = new Phrase() { Font = FontFactory.GetFont(FontFactory.HELVETICA, 8) };

            // Cell 1: Description
            PdfPCell cell1 = new PdfPCell(new Phrase("DESCRIPTION", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8)));
            cell1.Border = PdfPCell.BOX;
            cell1.PaddingBottom = 10;
            cell1.BackgroundColor = new BaseColor(0xD9, 0xD9, 0xD9); // Background color #D9D9D9
            table.AddCell(cell1);

            // Cell 2: Amount
            PdfPCell cell2 = new PdfPCell(new Phrase("AMOUNT", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8)));
            cell2.Border = PdfPCell.BOX;
            cell2.PaddingBottom = 10;
            cell2.BackgroundColor = new BaseColor(0xD9, 0xD9, 0xD9); // Background color #D9D9D9
            table.AddCell(cell2);

            // Row 1: Invoice Description
            //PdfPCell invoiceDescCell = new PdfPCell(new Phrase("Invoice for Milestone 1: \"Title of Milestone\"", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            //invoiceDescCell.Border = PdfPCell.BOX;
            //table.AddCell(invoiceDescCell);

            foreach (var datas in data.InvoicelistDetails)
            {
                PdfPCell invoiceDescCell = new PdfPCell(new Phrase(datas.Description.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 8)));
                invoiceDescCell.Border = PdfPCell.BOX;
                table.AddCell(invoiceDescCell);

                PdfPCell amount1Cell = new PdfPCell(new Phrase(datas.Amount.ToString(), FontFactory.GetFont(FontFactory.HELVETICA, 9)));
                amount1Cell.Border = PdfPCell.BOX;
                amount1Cell.PaddingBottom = 10;
                table.AddCell(amount1Cell);
            }


            // Add the table to the document
            doc.Add(table);
        }

        public async Task<IActionResult> DownloadCreditMemo()   //int ContractId
        {
            /*SolutionFundModel solutionFund = new SolutionFundModel();
            solutionFund.ContractId = ContractId;
            var invoiceDetails = await _apiRepository.MakeApiCallAsync("api/Freelancer/GetInvoiceDetails", HttpMethod.Post, solutionFund);
            dynamic datas = JObject.Parse(invoiceDetails);
            var Details = datas.Result;*/

            float cellHeight = 100f;
            iTextSharp.text.Document document = new iTextSharp.text.Document();
            var memoryStream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();
            AddClientTables1(document, writer);
            document.Add(new Paragraph("\r\n  "));
            AddTwoContentTables1(document, writer);
            document.Add(new Paragraph("\r\n  "));
            AddSingleTableWithCustomColumnWidths1(document, writer);
            document.NewPage();
            document.NewPage();
            document.Close();
            Response.ContentType = MediaTypeNames.Application.Pdf;
            var contentDisposition = new ContentDisposition
            {
                FileName = "CN-00129.pdf",
                Inline = false
            };
            Response.Headers.Add("Content-Disposition", contentDisposition.ToString());
            return File(memoryStream.ToArray(), Response.ContentType);
        }

        private void AddClientTables1(iTextSharp.text.Document doc, PdfWriter writer)
        {
            BaseFont baseF14 = BaseFont.CreateFont();

            // Master Table: Contains two side-by-side tables
            PdfPTable masterTable = new PdfPTable(2);
            masterTable.WidthPercentage = 100;
            masterTable.SetWidths(new float[] { 68f, 30f });
            masterTable.DefaultCell.Border = 3;

            // Left Table: Display the logo
            PdfPTable leftTable = new PdfPTable(1);
            // leftTable.WidthPercentage = 80;
            leftTable.HorizontalAlignment = Element.ALIGN_LEFT;
            float[] widths = new float[] { 40f };



            leftTable.SetWidths(widths);
            iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance("https://aephyweb.azurewebsites.net/assets/img/ephylink_no_background_blue.png");
            img.ScaleToFit(150f, 150f);
            PdfPCell logoCell = new PdfPCell(img);
            logoCell.HorizontalAlignment = Element.ALIGN_LEFT;
            logoCell.Border = PdfPCell.NO_BORDER;
            leftTable.AddCell(logoCell);

            // Right Table: Labels and Text Fields
            PdfPTable rightTable = new PdfPTable(2);
            // rightTable.WidthPercentage = 30;
            float[] widths1 = new float[] { 0f, 100f };
            rightTable.SetWidths(widths1);
            rightTable.HorizontalAlignment = Element.ALIGN_RIGHT;

            //rightTable.SpacingBefore = 30;

            // Set the font for the entire table
            rightTable.DefaultCell.Phrase = new Phrase() { Font = FontFactory.GetFont(FontFactory.HELVETICA, 8) };

            // Create label cells and set font styles
            PdfPCell labelCell1 = new PdfPCell(new Phrase("           CREDIT MEMO                   ", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11)));
            labelCell1.BackgroundColor = new BaseColor(0xD9, 0xD9, 0xD9); // Set background color
            labelCell1.VerticalAlignment = Element.ALIGN_MIDDLE; // Center the text vertically
            labelCell1.Colspan = 5;

            labelCell1.FixedHeight = 23; ;

            //labelCell1.Colspan = 2;
            //labelCell1.PaddingLeft = 40;


            PdfPCell labelCell6 = new PdfPCell(new Phrase("Invoice", FontFactory.GetFont(FontFactory.HELVETICA, 9)));


            PdfPCell labelCell2 = new PdfPCell(new Phrase("Date", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            //labelCell2.PaddingLeft = 10;
            PdfPCell labelCell3 = new PdfPCell(new Phrase("Due Date", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9))); // Bold font for "Due Date" label
            //labelCell3.PaddingLeft = 10;
            PdfPCell labelCell4 = new PdfPCell(new Phrase("Total Amount", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            //labelCell4.PaddingLeft = 10;
            PdfPCell labelCell5 = new PdfPCell(new Phrase("Total Due ", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9)));
            //labelCell5.PaddingLeft = 10;

            // Create textField cells
            PdfPCell textFieldCell1 = new PdfPCell();
            PdfPCell textFieldCell2 = new PdfPCell();
            PdfPCell textFieldCell3 = new PdfPCell();
            PdfPCell textFieldCell4 = new PdfPCell();
            PdfPCell textFieldCell5 = new PdfPCell();
            PdfPCell textFieldCell6 = new PdfPCell();

            // Set cell properties
            PdfPCell[] cells = { labelCell1, textFieldCell1, labelCell6, textFieldCell6, labelCell2, textFieldCell2, labelCell3, textFieldCell3, labelCell4, textFieldCell4, labelCell5, textFieldCell5 };

            foreach (PdfPCell cell in cells)
            {
                cell.Border = PdfPCell.NO_BORDER;
                rightTable.AddCell(cell);
            }

            // Add the left and right tables to the master table
            PdfPCell leftCell = new PdfPCell(leftTable);
            leftCell.Border = PdfPCell.NO_BORDER;

            PdfPCell rightCell = new PdfPCell(rightTable);
            rightCell.Border = PdfPCell.NO_BORDER;

            masterTable.AddCell(leftCell);
            masterTable.AddCell(rightCell);

            // Add the master table to the document
            doc.Add(masterTable);
        }

        private void AddTwoContentTables1(iTextSharp.text.Document doc, PdfWriter writer)
        {
            // Master Table: Contains two side-by-side tables
            PdfPTable masterTable = new PdfPTable(2);
            masterTable.WidthPercentage = 100;

            // Table 1: Content 1
            PdfPTable table1 = new PdfPTable(1);
            table1.WidthPercentage = 100;
            table1.HorizontalAlignment = Element.ALIGN_LEFT;

            // Content 1
            string content1 = "From\nEphylink\nDimitrios Vamvakas\nEllispontou 39, Patra, 26226\nAchaia, Greece\nVAT ID: 148366653";

            // Split content1 into lines
            string[] content1Lines = content1.Split('\n');

            foreach (string line in content1Lines)
            {
                iTextSharp.text.Font font = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                if (line == "From")
                {
                    font.Color = new BaseColor(195, 120, 19);
                    font.Size = 14;
                }
                if (line == "Ephylink")
                {

                    font.Size = 9;
                    font.SetStyle(iTextSharp.text.Font.BOLD);
                }

                Chunk chunk = new Chunk(line, font);
                PdfPCell contentCell1 = new PdfPCell(new Phrase(chunk));
                contentCell1.Border = PdfPCell.NO_BORDER;
                table1.AddCell(contentCell1);
            }

            // Table 2: Content 2
            PdfPTable table2 = new PdfPTable(1);
            table2.WidthPercentage = 100;
            table2.HorizontalAlignment = Element.ALIGN_LEFT;

            // Content 2
            string content2 = "Bill to\nClient 1 International Client\nName \nAddress:\nVAT ID: (If Applicable )\nTax ID: (other)";

            // Split content2 into lines
            string[] content2Lines = content2.Split('\n');

            foreach (string line in content2Lines)
            {
                iTextSharp.text.Font font = FontFactory.GetFont(FontFactory.HELVETICA, 9);
                if (line == "Bill to")
                {
                    font.Color = new BaseColor(195, 120, 19);
                    font.Size = 14;
                }
                if (line == "Client 1 International Client")
                {
                    font.SetStyle(iTextSharp.text.Font.BOLD);
                    font.Size = 9;
                }
                Chunk chunk = new Chunk(line, font);
                PdfPCell contentCell2 = new PdfPCell(new Phrase(chunk));
                contentCell2.Border = PdfPCell.NO_BORDER;
                table2.AddCell(contentCell2);
            }

            // Add the left and right tables to the master table
            PdfPCell leftCell = new PdfPCell(table1);
            leftCell.Border = PdfPCell.NO_BORDER;

            PdfPCell rightCell = new PdfPCell(table2);
            rightCell.Border = PdfPCell.NO_BORDER;

            masterTable.AddCell(leftCell);
            masterTable.AddCell(rightCell);

            // Add the master table to the document
            doc.Add(masterTable);
        }
        private void AddSingleTableWithCustomColumnWidths1(iTextSharp.text.Document doc, PdfWriter writer)
        {
            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.SetWidths(new float[] { 80f, 20f }); // Set column widths to 70% and 30%
            table.DefaultCell.Border = PdfPCell.BOX;
            table.DefaultCell.Phrase = new Phrase() { Font = FontFactory.GetFont(FontFactory.HELVETICA, 9) };

            // Cell 1: Description
            PdfPCell cell1 = new PdfPCell(new Phrase("DESCRIPTION", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9)));
            cell1.Border = PdfPCell.BOX;
            cell1.PaddingBottom = 10;
            cell1.BackgroundColor = new BaseColor(0xD9, 0xD9, 0xD9); // Background color #D9D9D9
            table.AddCell(cell1);

            // Cell 2: Amount
            PdfPCell cell2 = new PdfPCell(new Phrase("AMOUNT", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9)));
            cell2.Border = PdfPCell.BOX;
            cell2.PaddingBottom = 10;
            cell2.BackgroundColor = new BaseColor(0xD9, 0xD9, 0xD9); // Background color #D9D9D9
            table.AddCell(cell2);

            // Row 1: Invoice Description
            PdfPCell invoiceDescCell = new PdfPCell(new Phrase("Paid from escrow for \"Title of Milestone\"", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            invoiceDescCell.Border = PdfPCell.BOX;
            table.AddCell(invoiceDescCell);

            PdfPCell amount1Cell = new PdfPCell(new Phrase("-1,198.92", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            amount1Cell.Border = PdfPCell.BOX;
            amount1Cell.PaddingBottom = 10;
            table.AddCell(amount1Cell);

            // Row 2: VAT
            PdfPCell vatDescCell = new PdfPCell(new Phrase("VAT (0%)", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            vatDescCell.Border = PdfPCell.BOX;
            vatDescCell.PaddingBottom = 10;
            table.AddCell(vatDescCell);

            PdfPCell amount2Cell = new PdfPCell(new Phrase(" ", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            amount2Cell.Border = PdfPCell.BOX;
            amount2Cell.PaddingBottom = 10;
            table.AddCell(amount2Cell);

            // Row 3: Total Amount
            PdfPCell totalDescCell = new PdfPCell(new Phrase("Total amount", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            totalDescCell.Border = PdfPCell.BOX;
            totalDescCell.PaddingBottom = 10;
            table.AddCell(totalDescCell);

            PdfPCell totalAmountCell = new PdfPCell(new Phrase("€ -1,198.92", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            totalAmountCell.Border = PdfPCell.BOX;
            totalAmountCell.PaddingBottom = 10;
            table.AddCell(totalAmountCell);

            // Add the table to the document
            doc.Add(table);
        }
    }
}
