// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

using Amazon.XRay.Recorder.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nuages.AspNetIdentity.Core;
using Nuages.Identity.UI.AWS;
using Nuages.Web;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Nuages.Identity.UI.Pages.Account.Manage;

[Authorize]
public class EmailModel : PageModel
{
    private readonly NuagesUserManager _userManager;

    public EmailModel(NuagesUserManager userManager)
    {
        _userManager = userManager;
    }

    [TempData] public string Email { get; set; } = string.Empty;

    [TempData] public bool EmailVerified { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            AWSXRayRecorder.Instance.BeginSubsegment();

            var user = await _userManager.FindByIdAsync(User.Sub()!);
            Email = user.Email;
            EmailVerified = user.EmailConfirmed;

            return Page();
        }
        catch (Exception e)
        {
            AWSXRayRecorder.Instance.AddException(e);

            throw;
        }
        finally
        {
            AWSXRayRecorder.Instance.EndSubsegment();
        }
    }
}