using System.Text;
using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Nuages.AspNetIdentity.Core;
using Nuages.Identity.Services.Manage;
using Nuages.Identity.UI.AWS;
using Nuages.Web;

namespace Nuages.Identity.UI.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ManageController : Controller
{
    private readonly IChangePasswordService _changePasswordService;
    private readonly IChangeUserNameService _changeUserNameService;
    private readonly ILogger<ManageController> _logger;
    private readonly IMFAService _mfaService;
    private readonly IChangePhoneNumberService _phoneNumberService;
    private readonly IProfileService _profileService;
    private readonly ISendEmailChangeConfirmationService _sendEmailChangedConfirmationService;
    private readonly ISendSMSVerificationCodeService _sendSmsVerificationCode;
    private readonly NuagesSignInManager _signInManager;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly NuagesUserManager _userManager;

    public ManageController(IChangePasswordService changePasswordService, NuagesUserManager userManager,
        NuagesSignInManager signInManager,
        ISendEmailChangeConfirmationService sendEmailChangedConfirmationService,
        IChangeUserNameService changeUserNameService,
        IMFAService mfaService, IChangePhoneNumberService phoneNumberService,
        ISendSMSVerificationCodeService sendSmsVerificationCode,
        IProfileService profileService,
        ILogger<ManageController> logger, IStringLocalizer stringLocalizer)
    {
        _changePasswordService = changePasswordService;
        _userManager = userManager;
        _signInManager = signInManager;
        _sendEmailChangedConfirmationService = sendEmailChangedConfirmationService;
        _changeUserNameService = changeUserNameService;
        _mfaService = mfaService;
        _phoneNumberService = phoneNumberService;
        _sendSmsVerificationCode = sendSmsVerificationCode;
        _profileService = profileService;
        _logger = logger;
        _stringLocalizer = stringLocalizer;
    }

    [HttpPost("changePassword")]
    public async Task<ChangePasswordResultModel> ChangePasswordAsync([FromBody] ChangePasswordModel model)
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment();
            AWSXRayRecorder.Instance.AddMetadata("model", model);
            
            _logger.LogInformation(
                $"Initiate ChangePassword : Name = {User.Identity!.Name} {model.CurrentPassword} NewPassword = {model.NewPassword} NewPasswordConfirm = {model.NewPasswordConfirm}");

            var res = await _changePasswordService.ChangePasswordAsync(User.Sub()!, model.CurrentPassword,
                model.NewPassword, model.NewPasswordConfirm);

            _logger.LogInformation($"Login Result : Success = {res.Success} Error = {res.Errors.FirstOrDefault()}");

            if (res.Success)
            {
                var user = await _userManager.FindByIdAsync(User.Sub()!);
                await _signInManager.RefreshSignInAsync(user);
            }

            return res;
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            return new ChangePasswordResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }

    [HttpPost("setPassword")]
    public async Task<ChangePasswordResultModel> SetPasswordAsync([FromBody] ChangePasswordModel model)
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment();
            AWSXRayRecorder.Instance.AddMetadata("model", model);
            
            _logger.LogInformation(
                $"Initiate ChangePassword : Name = {User.Identity!.Name}  NewPassword = {model.NewPassword} NewPasswordConfirm = {model.NewPasswordConfirm}");

            var res = await _changePasswordService.AddPasswordAsync(User.Sub()!, model.NewPassword,
                model.NewPasswordConfirm);

            _logger.LogInformation($"Login Result : Success = {res.Success} Error = {res.Errors.FirstOrDefault()}");

            return res;
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);

            _logger.LogError(e, e.Message);


            return new ChangePasswordResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }

    [HttpPost("sendEmailChange")]
    public async Task<SendEmailChangeResultModel> SendEmailChangeMessageAsync([FromBody] SendEmailChangeModel model)
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment();
            AWSXRayRecorder.Instance.AddMetadata("model", model);
            
            var res = await _sendEmailChangedConfirmationService.SendEmailChangeConfirmation(User.Sub()!, model.Email);

            return res;
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            return new SendEmailChangeResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }

    [HttpPost("changeUsername")]
    [AllowAnonymous]
    public async Task<ChangeUserNameResultModel> ChangeUsernameAsync([FromBody] ChangeUserNameModel model)
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment();
            AWSXRayRecorder.Instance.AddMetadata("model", model);
            
            var res = await _changeUserNameService.ChangeUserNameAsync(User.Sub()!, model.NewUserName);

            if (res.Success)
            {
                var user = await _userManager.FindByIdAsync(User.Sub()!);
                await _signInManager.RefreshSignInAsync(user);
            }

            return res;
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            return new ChangeUserNameResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }

    [HttpDelete("disable2Fa")]
    [AllowAnonymous]
    public async Task<DisableMFAResultModel> Disable2FaAsync()
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment();

            var res = await _mfaService.DisableMFAAsync(User.Sub()!);


            return res;
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            return new DisableMFAResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }

    [HttpPost("enable2FA")]
    [AllowAnonymous]
    public async Task<MFAResultModel> Enable2FaAsync([FromBody] EnableMFAModel model)
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment();
            AWSXRayRecorder.Instance.AddMetadata("model", model);
            
            var res = await _mfaService.EnableMFAAsync(User.Sub()!, model.Code);

            if (res.Success)
            {
                var user = await _userManager.FindByIdAsync(User.Sub()!);
                await _signInManager.RefreshSignInAsync(user);
            }

            return res;
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            return new MFAResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }

    [HttpDelete("removePhone")]
    [AllowAnonymous]
    public async Task<ChangePhoneNumberResultModel> RemovePhoneAsync()
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment();

            var res = await _phoneNumberService.ChangePhoneNumberAsync(User.Sub()!, "", null);

            if (res.Success)
            {
                var user = await _userManager.FindByIdAsync(User.Sub()!);
                await _signInManager.RefreshSignInAsync(user);
            }

            return res;
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            return new ChangePhoneNumberResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }

    [HttpPost("sendPhoneChangeMessage")]
    [AllowAnonymous]
    public async Task<SendSMSVerificationCodeResultModel> SendPhoneChangeVerificationAsync(
        [FromBody] SendSMSVerificationCodeModel model)
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment();
            AWSXRayRecorder.Instance.AddMetadata("model", model);
            
            var res = await _sendSmsVerificationCode.SendCode(User.Sub()!, model.PhoneNumber);

            return res;
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            return new SendSMSVerificationCodeResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }

    [HttpPost("changePhoneNumber")]
    [AllowAnonymous]
    public async Task<ChangePhoneNumberResultModel> ChangePhoneNumberAsync([FromBody] ChangePhoneNumberModel model)
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment();
            AWSXRayRecorder.Instance.AddMetadata("model", model);
            
            var res = await _phoneNumberService.ChangePhoneNumberAsync(User.Sub()!, model.PhoneNumber, model.Token);

            return res;
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            return new ChangePhoneNumberResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }

    [HttpGet("downloadRecoveryCodes")]
    public async Task<ActionResult> DownloadRecoveryCodesAsync()
    {
        try
        {
            var codes = await _mfaService.GetRecoveryCodes(User.Sub()!);

            var recoveryCodesString = string.Join(",", codes);
            return File(Encoding.Default.GetBytes(recoveryCodesString), "application/text", "recoveryCodes.txt");
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            return new EmptyResult();
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }

    [HttpPost("resetRecoveryCodes")]
    public async Task<MFAResultModel> ResetRecoveryCodesAsync()
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment();

            return await _mfaService.ResetRecoveryCodesAsync(User.Sub()!);
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            return new MFAResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }

    [HttpPost("forgetBrowser")]
    public async Task<bool> ForgetBrowserAsync()
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment();

            await _signInManager.ForgetTwoFactorClientAsync();

            return true;
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            return false;
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }

    [HttpPost("saveProfile")]
    public async Task<SaveProfileResultModel> SaveProfileAsync([FromBody] SaveProfileModel model)
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment();
            AWSXRayRecorder.Instance.AddMetadata("model", model);
            
            AWSXRayRecorder.Instance.AddMetadata("model", model);
            
            return await _profileService.SaveProfile(User.Sub()!, model);
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);
            _logger.LogError(e, e.Message);

            return new SaveProfileResultModel
            {
                Success = false,
                Errors = new List<string> { _stringLocalizer["errorMessage:exception"] }
            };
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }
}