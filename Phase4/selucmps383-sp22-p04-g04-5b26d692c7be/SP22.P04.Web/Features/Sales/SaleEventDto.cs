using System.ComponentModel.DataAnnotations;

namespace SP22.P04.Web.Features.Sales;

public class SaleEventDto
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    public DateTimeOffset StartUtc { get; set; }
    public DateTimeOffset EndUtc { get; set; }
}