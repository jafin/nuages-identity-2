using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Nuages.Identity.Services.AspNetIdentity;
using Nuages.Identity.Services.Email;
using Nuages.Identity.Services.Email.Sender;
using Nuages.Identity.Services.Login;
using Nuages.Identity.Services.Login.Passwordless;
using Nuages.Identity.Services.Manage;
using Nuages.Identity.Services.Password;
using Nuages.Identity.Services.Register;

using Nuages.Web.Utilities;

namespace Nuages.Identity.Services;

public static class NuagesIdentityConfigExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IdentityBuilder AddNuagesIdentityServices(this IdentityBuilder builder, IConfiguration configuration,
        Action<NuagesIdentityOptions> configure)
    {
        var services = builder.Services;

        services.Configure<NuagesIdentityOptions>(configuration.GetSection("Nuages:Identity"));


        services.Configure(configure);


        var userType = builder.UserType;
        var totpProvider = typeof(PasswordlessLoginProvider<>).MakeGenericType(userType);
        builder.AddTokenProvider("PasswordlessLoginProvider", totpProvider);

        services.AddScoped<IEmailValidator, EmailValidator>();

        //Anonymous
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IForgotPasswordService, ForgotPasswordService>();
        services.AddScoped<IResetPasswordService, ResetPasswordService>();
        services.AddScoped<ISendEmailConfirmationService, SendEmailConfirmationService>();
        services.AddScoped<IConfirmEmailService, ConfirmEmailService>();
        services.AddScoped<IRegisterService, RegisterService>();
        services.AddScoped<IRegisterExternalLoginService, RegisterExternalLoginService>();
        services.AddScoped<IPasswordlessService, PasswordlessService>();
        services.AddScoped<ISMSSendCodeService, SMSSendCodeService>();

        //Manage
        services.AddScoped<IChangeEmailService, ChangeEmailService>();
        services.AddScoped<IChangeUserNameService, ChangeUserNameService>();
        services.AddScoped<IChangePasswordService, ChangePasswordService>();
        services.AddScoped<IChangePhoneNumberService, ChangePhoneNumberService>();
        services.AddScoped<ISendSMSVerificationCodeService, SendSMSVerificationCodeService>();
        services.AddScoped<ISendEmailChangeConfirmationService, SendEmailChangeConfirmationService>();
        services.AddScoped<IMFAService, MFAService>();
        services.AddScoped<IProfileService, ProfileService>();


        return builder;
    }

    public static AuthenticationBuilder AddNuagesAuthentication(this IServiceCollection services)
    {
        var builder = services.AddAuthentication()
            .AddCookie(NuagesIdentityConstants.EmailNotVerifiedScheme)
            .AddCookie(NuagesIdentityConstants.ResetPasswordScheme)
            .AddCookie(NuagesIdentityConstants.PasswordExpiredScheme);

        return builder;
    }
}