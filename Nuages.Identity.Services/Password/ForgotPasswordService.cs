
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

using Nuages.AspNetIdentity.Core;
using Nuages.Identity.Services.Email;
using Nuages.Web;

namespace Nuages.Identity.Services.Password;

public class ForgotPasswordService : IForgotPasswordService
{
    private readonly NuagesUserManager _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMessageService _messageService;
    private readonly IRuntimeConfiguration _runtimeConfiguration;
    private readonly NuagesIdentityOptions _options;

    public ForgotPasswordService(NuagesUserManager userManager, IHttpContextAccessor httpContextAccessor, IMessageService messageService, 
        IOptions<NuagesIdentityOptions> options, IRuntimeConfiguration runtimeConfiguration)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _messageService = messageService;
        _runtimeConfiguration = runtimeConfiguration;

        _options = options.Value;
    }
    
    public async Task<ForgotPasswordResultModel> StartForgotPassword(ForgotPasswordModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
        {
            return new ForgotPasswordResultModel
            {
                Success = true // Fake success
            };
        }
       
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var url = $"{_options.Authority}/Account/ResetPassword?code={code}";

        _messageService.SendEmailUsingTemplate(model.Email, "Password_Reset", new Dictionary<string, string>
        {
            { "Link", url }
        });

        await _httpContextAccessor.HttpContext!.SignInAsync(NuagesIdentityConstants.ResetPasswordScheme, StorePasswordResetEmailInfo(user.Id, user.Email));
        
        return new ForgotPasswordResultModel
        {

            Url = _runtimeConfiguration.IsTest ? url : null,
            Code = _runtimeConfiguration.IsTest ? code : null,

            Success = true // Fake success
        };
    }
    
    private static ClaimsPrincipal StorePasswordResetEmailInfo(string userId, string email)
    {
        var identity = new ClaimsIdentity("PasswordReset");
        identity.AddClaim(new Claim(ClaimTypes.Name, userId));
        identity.AddClaim(new Claim(ClaimTypes.Email, email));
        return new ClaimsPrincipal(identity);
    }
}

public interface IForgotPasswordService
{
    Task<ForgotPasswordResultModel> StartForgotPassword(ForgotPasswordModel model);
}

// ReSharper disable once ClassNeverInstantiated.Global
public class ForgotPasswordModel
{
    public string Email { get; set; } = string.Empty;
}

public class ForgotPasswordResultModel
{
    public bool Success { get; set; }
    public string? Message { get; set; }
        
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; set; }
}
