namespace DATN_70.Models.ViewModels;

public sealed class CheckoutPageViewModel
{
    public CheckoutCustomerBootstrapViewModel Customer { get; set; } = new();
}

public sealed class CheckoutCustomerBootstrapViewModel
{
    public bool IsAuthenticated { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Ward { get; set; } = string.Empty;
    public string SelectedAddressId { get; set; } = string.Empty;
    public List<SavedAddressViewModel> SavedAddresses { get; set; } = [];
}

public sealed class SavedAddressViewModel
{
    public string Id { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string Ward { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string DisplayAddress => string.Join(", ", new[] { Street, Ward, District, Province }.Where(part => !string.IsNullOrWhiteSpace(part)));
}
