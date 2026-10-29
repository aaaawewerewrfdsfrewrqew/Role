using RoleAuth.Api.Dtos;

namespace RoleAuth.Api.Services;

public interface IAccountService
{
    Task<ProfileResponse> GetProfileAsync(int userId, CancellationToken ct = default);
    Task<ProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken ct = default);
    Task ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken ct = default);
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default);
    Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
    Task RequestEmailChangeAsync(int userId, ChangeEmailRequest request, CancellationToken ct = default);
    Task ConfirmEmailChangeAsync(ConfirmEmailChangeRequest request, CancellationToken ct = default);
}
