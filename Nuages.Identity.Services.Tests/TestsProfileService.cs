using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using Nuages.Identity.Services.Manage;
using Nuages.Web.Exceptions;
using Xunit;

namespace Nuages.Identity.Services.Tests;

public class TestsProfileService
{
    [Fact]
    public async Task ShouldSaveProfileWithSuccess()
    {
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        
        var profileService = new ProfileService(identityStuff.UserManager, new FakeStringLocalizer());

        var res = await profileService.SaveProfile(user.Id, new SaveProfileModel
        {
            FirstName = "FirstName",
            LastName = "LastName"
        });
        
        Assert.True(res.Success);

    }
    
    [Fact]
    public async Task ShouldSaveProfileWithFailure()
    {
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);

        var profileService = new ProfileService(identityStuff.UserManager, new FakeStringLocalizer());

        
        await Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await profileService.SaveProfile("bad_id", new SaveProfileModel
            {
                FirstName = "FirstName",
                LastName = "LastName"
            });
            
        });
        
    }
    
    [Fact]
    public async Task ShouldSaveProfileWithFailureAndErrors()
    {
        var user = MockHelpers.CreateDefaultUser();
        
        var identityStuff = MockHelpers.MockIdentityStuff(user);
        identityStuff.UserStore.Setup(u => u.UpdateAsync(user, It.IsAny<CancellationToken>())).ReturnsAsync( () => IdentityResult.Failed(new IdentityError { Code = "error", Description = "error"}) );

        
        var profileService = new ProfileService(identityStuff.UserManager, new FakeStringLocalizer());

        var res = await profileService.SaveProfile(user.Id, new SaveProfileModel
        {
            FirstName = "FirstName",
            LastName = "LastName"
        });
        
        Assert.False(res.Success);
        Assert.Equal("identity.error", res.Errors.Single());

    }
}