using System;

namespace SP22.P04.Tests.Web.Dtos;

internal class SaleEventDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset StartUtc { get; set; }
    public DateTimeOffset EndUtc { get; set; }
}