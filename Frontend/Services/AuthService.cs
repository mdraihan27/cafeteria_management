using Frontend.Models;

namespace Frontend.Services;

/// <summary>Login, registration, and account management for both Admins and Customers.</summary>
public static class AuthService
{
    // --- Admin -------------------------------------------------------
    public static Task<TokenResponse> AdminLoginAsync(string username, string password) =>
        ApiClient.PostAsync<TokenResponse>("/auth/admin/login", new AdminLoginRequest { Username = username, Password = password });

    public static Task<MessageResponse> AdminLogoutAsync() =>
        ApiClient.PostAsync<MessageResponse>("/auth/admin/logout");

    public static Task<AdminProfile> GetAdminProfileAsync() =>
        ApiClient.GetAsync<AdminProfile>("/auth/admin/profile");

    public static Task<AdminProfile> UpdateAdminProfileAsync(AdminProfileUpdateRequest request) =>
        ApiClient.PutAsync<AdminProfile>("/auth/admin/profile", request);

    public static Task<MessageResponse> ChangeAdminPasswordAsync(string currentPassword, string newPassword) =>
        ApiClient.PostAsync<MessageResponse>("/auth/admin/change-password",
            new ChangePasswordRequest { CurrentPassword = currentPassword, NewPassword = newPassword });

    // --- Customer ------------------------------------------------------
    public static Task<TokenResponse> CustomerRegisterAsync(CustomerRegisterRequest request) =>
        ApiClient.PostAsync<TokenResponse>("/auth/customer/register", request);

    public static Task<TokenResponse> CustomerLoginAsync(string email, string password) =>
        ApiClient.PostAsync<TokenResponse>("/auth/customer/login", new CustomerLoginRequest { Email = email, Password = password });

    public static Task<MessageResponse> CustomerLogoutAsync() =>
        ApiClient.PostAsync<MessageResponse>("/auth/customer/logout");

    public static Task<CustomerProfile> GetCustomerProfileAsync() =>
        ApiClient.GetAsync<CustomerProfile>("/auth/customer/profile");

    public static Task<CustomerProfile> UpdateCustomerProfileAsync(CustomerProfileUpdateRequest request) =>
        ApiClient.PutAsync<CustomerProfile>("/auth/customer/profile", request);

    public static Task<MessageResponse> ChangeCustomerPasswordAsync(string currentPassword, string newPassword) =>
        ApiClient.PostAsync<MessageResponse>("/auth/customer/change-password",
            new ChangePasswordRequest { CurrentPassword = currentPassword, NewPassword = newPassword });

    public static Task<MessageResponse> ForgotPasswordAsync(string email, string phone, string newPassword) =>
        ApiClient.PostAsync<MessageResponse>("/auth/customer/forgot-password",
            new ForgotPasswordRequest { Email = email, Phone = phone, NewPassword = newPassword });
}
