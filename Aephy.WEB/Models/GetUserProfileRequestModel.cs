using iTextSharp.text.pdf;
using iTextSharp.text;

namespace Aephy.WEB.Models
{
    public class GetUserProfileRequestModel
    {
        public string? UserId { get; set; } = "";
    }

    public class ImageClass
    {
        public int Id { get; set; }

        public string? BlobStorageBaseUrl { get; set; }
        public string? ImagePath { get; set; }
        public string? ImageUrlWithSas { get; set; }

        public string? FreelancerId { get; set; }
        
    }

    public class RoundedCellEvent : IPdfPCellEvent
    {
        private float radius;
        private BaseColor color;
        private float width = 150;
        private float height = 30;

        public RoundedCellEvent(float radius, BaseColor color)
        {
            this.radius = radius;
            this.color = color;

        }

        public void CellLayout(
            PdfPCell cell, Rectangle position, PdfContentByte[] canvases)
        {
            PdfContentByte canvas = canvases[PdfPTable.BACKGROUNDCANVAS];
            canvas.RoundRectangle(
                position.Left, position.Bottom, this.width, this.height, radius);
            canvas.SetColorFill(color);
            canvas.Fill();
        }

    }
}
