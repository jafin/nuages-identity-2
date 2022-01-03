using AspNetCore.Identity.Mongo.Model;

namespace Nuages.Identity.Services;

public class NuagesApplicationUser : MongoUser<string>
{
    public string? Language { get; set; }
    
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }

    public string? UserAgreementVersion { get; set; }

    public bool MfaEnabled { get; set; }
    
    public DateTime? LastLogin { get; set; }
    public int LoginCount { get; set; }

    public bool UserMustChangePassword { get; set; }

    public string? PasswordHistory { get; set; }
    
    public DateTime CreatedOn { get; set; }
    
    public DateTime? LastPasswordChangedDate { get; set; }
    public bool EnableAutoExpirePassword { get; set; } = true;
    
    public string? Country { get; set; }
    
    public DateTime? EmailDateTime { get; set; }
    public DateTime? PhoneDateTime { get; set; }
}