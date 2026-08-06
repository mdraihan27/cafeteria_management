using Frontend.Models;

namespace Frontend.Services;

/// <summary>Admin-only: browsing the list of registered customers.</summary>
public static class CustomerService
{
    public static Task<List<CustomerListItem>> GetCustomersAsync() =>
        ApiClient.GetAsync<List<CustomerListItem>>("/customers");

    public static Task<CustomerListItem> GetCustomerAsync(int customerId) =>
        ApiClient.GetAsync<CustomerListItem>($"/customers/{customerId}");
}
