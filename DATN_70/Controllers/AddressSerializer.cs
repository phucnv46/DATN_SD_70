namespace DATN_70.Controllers;

public static class AddressSerializer
{
    private const string Separator = "||";

    public static string Pack(string ward, string street)
    {
        return $"{ward}{Separator}{street}";
    }

    public static string ExtractWard(string? packedWard)
    {
        if (string.IsNullOrWhiteSpace(packedWard)) return string.Empty;
        var parts = packedWard.Split(Separator, StringSplitOptions.None);
        return parts[0];
    }

    public static string ExtractStreet(string? packedWard)
    {
        if (string.IsNullOrWhiteSpace(packedWard)) return string.Empty;
        var parts = packedWard.Split(Separator, StringSplitOptions.None);
        return parts.Length > 1 ? parts[1] : string.Empty;
    }
}