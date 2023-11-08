using iTextSharp.text.pdf;
using iTextSharp.text;

namespace Aephy.WEB.Admin.Models;

public class InviteUserViewModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmailAddress { get; set; }
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
