using System.ComponentModel.DataAnnotations;

namespace SP22.P04.Tests.Web.Controllers.SaleEventsController;

public class SaleEventProductDto
{
    [Range(0.00, double.MaxValue)]
    public decimal SaleEventPrice { get; set; }
}