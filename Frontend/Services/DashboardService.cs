using Frontend.Models;

namespace Frontend.Services;

public static class DashboardService
{
    public static Task<AdminDashboard> GetAdminDashboardAsync() =>
        ApiClient.GetAsync<AdminDashboard>("/dashboard/admin");

    public static Task<CustomerDashboard> GetCustomerDashboardAsync() =>
        ApiClient.GetAsync<CustomerDashboard>("/dashboard/customer");
}
