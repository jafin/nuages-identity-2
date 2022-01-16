using System.Diagnostics.CodeAnalysis;
using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nuages.Identity.API.Services;
using Nuages.Identity.API.Services.Admin;

namespace Nuages.Identity.API.Endpoints;

[ExcludeFromCodeCoverage]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public class AdminController
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AdminController> _logger;
    private readonly ISetPasswordService _setPasswordService;
  
    private readonly IMFAService _mfaService;

    public AdminController(IWebHostEnvironment env, ILogger<AdminController> logger,
        ISetPasswordService setPasswordService,
       
        IMFAService mfaService)
    {
        _env = env;
        _logger = logger;
        _setPasswordService = setPasswordService;
      
        _mfaService = mfaService;
    }
    
    [HttpDelete("removeMfa")]
    public async Task<bool> RemoveMfaAsync()
    {
        return await Task.FromResult(true);
    }
    
    [HttpPost("setEmail")]
    public async Task<bool> SetEmailAsync()
    {
        return await Task.FromResult(true);
    }
    
    [HttpPost("setUsername")]
    public async Task<bool> SetUserNameAsync()
    {
        return await Task.FromResult(true);
    }
    
    [HttpPost("setPhoneNumber")]
    public async Task<bool> SetPhoneNumberAsync()
    {
        return await Task.FromResult(true);
    }
    
    [HttpPost("setPassword")]
    public async Task<SetPasswordResultModel> SetPasswordAsync([FromBody] SetPasswordModel model)
    {
        try
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.BeginSubsegment("AdminController.SetPasswordAsync");

            return await _setPasswordService.SetPasswordAsync(model);
        }
        catch (Exception e)
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.AddException(e);

            throw;
        }
        finally
        {
            if (!_env.IsDevelopment())
                AWSXRayRecorder.Instance.EndSubsegment();
        }
    }
}