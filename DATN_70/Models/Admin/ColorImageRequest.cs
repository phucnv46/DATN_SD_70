using Microsoft.AspNetCore.Http;

public class ColorImageRequest
{
    public string? MauID { get; set; }
    public IFormFile? FileAnh { get; set; }
}