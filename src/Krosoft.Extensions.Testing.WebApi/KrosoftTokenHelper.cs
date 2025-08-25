using Krosoft.Extensions.Core.Models;

namespace Krosoft.Extensions.Testing.WebApi;

public static class KrosoftTokenHelper
{
    public static KrosoftToken Defaut => new KrosoftToken
    {
        Id = "Claim_Id", 
        Name = "Claim_Name",
        Email = "Claim_Email",
        RoleId = "Claim_RoleId", 
        LangueId = "Claim_LangueId",
        LangueCode = "Claim_LangueCode"
    };
}