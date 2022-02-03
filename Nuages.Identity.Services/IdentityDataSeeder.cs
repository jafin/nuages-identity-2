using Nuages.AspNetIdentity;

namespace Nuages.Identity.Services;

public class IdentityDataSeeder : IHostedService
{
    private readonly IServiceProvider _provider;

    public IdentityDataSeeder(IServiceProvider provider)
    {
        _provider = provider;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _provider.CreateScope();

        var email = "admin@example.com";
        var userName = "admin";
        var password = "Nuages123*";
        
        var userManager = scope.ServiceProvider.GetRequiredService<NuagesUserManager>();
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var newUser = new NuagesApplicationUser
            {
                UserName = userName,
                Email = email,
                EmailConfirmed = true,

            };
            await userManager.CreateAsync(newUser, password);
        }
        
        var roleManager = scope.ServiceProvider.GetRequiredService<NuagesRoleManager>();
        
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            var adminRole = new NuagesApplicationRole
            {
                Name = "Admin"
            };
            
            await roleManager.CreateAsync(adminRole);
        }

        var existingUser = await userManager.FindByEmailAsync(email);

        if (!await userManager.IsInRoleAsync(existingUser, "Admin"))
        {
            await userManager.AddToRoleAsync(existingUser, "Admin");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}