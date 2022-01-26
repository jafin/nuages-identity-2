using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Web.Exceptions;
// ReSharper disable InconsistentNaming

namespace Nuages.Identity.Services.Login;

public class LoginService : ILoginService
{
    private readonly NuagesUserManager _userManager;
    private readonly NuagesSignInManager _signInManager;
    private readonly IStringLocalizer _stringLocalizer;

    public LoginService(NuagesUserManager userManager, NuagesSignInManager signInManager, IStringLocalizer stringLocalizer)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _stringLocalizer = stringLocalizer;
    }
    
    public async Task<LoginResultModel> LoginAsync(LoginModel model)
    {
        var user = await _userManager.FindAsync(model.UserNameOrEmail);
        if (user == null)
        {
            return new LoginResultModel
            {
                Result = SignInResult.Failed,
                Message = _stringLocalizer["errorMessage:userNameOrPasswordInvalid"]
            };
        }
        
        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password,
            true);

        if (result.Succeeded)
        {
            result = await _signInManager.CustomSignInOrTwoFactorAsync(user, model.RememberMe);
        }
        
        if (result == SignInResult.Success)
        {
            return new LoginResultModel
            {
                Success = true
            };
        }

        return new LoginResultModel
        {
            Result = result,
            Message = GetMessage(user.LastFailedLoginReason),
            Success = false,
            Reason = user.LastFailedLoginReason
        };
    }

    public async Task<LoginResultModel> Login2FAAsync(Login2FAModel model)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new NotFoundException("UserNotFound");
        }

        var authenticatorCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, model.RememberMe, model.RememberMachine);

        if (result == SignInResult.Success)
        {
            return new LoginResultModel
            {
                Success = true
            };
        }

        user.LastFailedLoginReason = FailedLoginReason.FailedMfa;
        await _userManager.UpdateAsync(user);
        
        return new LoginResultModel
        {
            Result = result,
            Message = GetMessage(user.LastFailedLoginReason),
            Success = false,
            Reason = user.LastFailedLoginReason
        };
    }
    
    public async Task<LoginResultModel> LoginRecoveryCodeAsync(LoginRecoveryCodeModel model)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new NotFoundException("UserNotFound");
        }

        var recoveryCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

        if (result == SignInResult.Success)
        {
            return new LoginResultModel
            {
                Success = true
            };
        }

        user.LastFailedLoginReason = FailedLoginReason.FailedRecoveryCode;
        await _userManager.UpdateAsync(user);

        return new LoginResultModel
        {
            Result = result,
            Message = GetMessage(user.LastFailedLoginReason),
            Success = false,
            Reason = user.LastFailedLoginReason
        };
    }
    
    public async Task<LoginResultModel> LoginSMSAsync(LoginSMSModel model)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            throw new NotFoundException("UserNotFound");
        }

        var code = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result = await _signInManager.TwoFactorSignInAsync("Phone", code, false, false);

        if (result == SignInResult.Success)
        {
            return new LoginResultModel
            {
                Success = true
            };
        }

        user.LastFailedLoginReason = FailedLoginReason.FailedSms;
        await _userManager.UpdateAsync(user);

        return new LoginResultModel
        {
            Result = result,
            Message = GetMessage(user.LastFailedLoginReason),
            Success = false,
            Reason = user.LastFailedLoginReason
        };
    }

    
    public string? GetMessage(FailedLoginReason? failedLoginReason)
    {
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (failedLoginReason == null)
            return null;
        
        return _stringLocalizer[GetMessageKey(failedLoginReason)];
    }
    public static string GetMessageKey(FailedLoginReason? failedLoginReason)
    {
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (failedLoginReason)
        {
            case FailedLoginReason.LockedOut:
            case FailedLoginReason.NotWithinDateRange:
            case FailedLoginReason.EmailNotConfirmed:
            case FailedLoginReason.AccountNotConfirmed:
            {
                return $"errorMessage:no_access:{failedLoginReason}";
            }
            case FailedLoginReason.UserNameOrPasswordInvalid:
            case FailedLoginReason.RecaptchaError:
            {
                return $"errorMessage:{failedLoginReason}";
            }
            
            // case FailedLoginReason.PasswordMustBeChanged:
            // case FailedLoginReason.PhoneNotConfirmed:
            // case FailedLoginReason.PasswordExpired:
            // {
            //     throw new NotSupportedException("ValueNotSupportedHere");
            // }
            // case null:
            // {
            //     return "";
            // }
            default:
            {
                return "errorMessage.no_access.error";
            }
        }
    }
}



public interface ILoginService
{
    Task<LoginResultModel> LoginAsync(LoginModel model);
    Task<LoginResultModel> Login2FAAsync(Login2FAModel model);
    Task<LoginResultModel> LoginRecoveryCodeAsync(LoginRecoveryCodeModel model);
    Task<LoginResultModel> LoginSMSAsync(LoginSMSModel model);
}

// ReSharper disable once ClassNeverInstantiated.Global
public class LoginModel
{
    public string UserNameOrEmail { get; set; } = null!;

    public string Password { get; set; } = null!;

    public bool RememberMe { get; set; }

    public string? RecaptchaToken { get; set; } = null!;
}

public class Login2FAModel
{
    public string Code { get; set; } = null!;

    public bool RememberMe { get; set; }
    public bool RememberMachine { get; set; }
    
    public string? RecaptchaToken { get; set; } = null!;
}

public class LoginRecoveryCodeModel
{
    public string Code { get; set; } = null!;
    
    public string? RecaptchaToken { get; set; } = null!;
}

public class LoginSMSModel
{
    public string Code { get; set; } = null!;
    
    public string? RecaptchaToken { get; set; } = null!;
}

public class LoginResultModel
{
    public bool Success { get; set; }
    public SignInResult Result { get; set; } = null!;
    public string? Message { get; set; }
    public FailedLoginReason? Reason { get; set; }
}