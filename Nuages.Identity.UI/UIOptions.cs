// ReSharper disable InconsistentNaming

using MongoDB.Driver.Core.Configuration;
using Nuages.Web.Recaptcha;

namespace Nuages.Identity.UI;

public class UIOptions
{
    public string SupportEmail { get; set; } = string.Empty;
    public string SupportUrl { get; set; } = string.Empty;
    public bool ShowRegistration { get; set; } = false;
    public string RegistrationUrl { get; set; } = string.Empty;
    public string LogosUrl { get; set; } = string.Empty;
}

public static class UIConfigExtension
{
    public static void AddUI(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<UIOptions>(configuration.GetSection("Nuages:UI"));
        
        services.AddGoogleRecaptcha(configuration);
    }
}