using Duende.IdentityServer.Models;

namespace IdentityServer;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(), // using openId give use two token one is access tokena and other is ID token
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("auctionApp", "Auction app full access"),
        };

    public static IEnumerable<Client> Clients =>
        new Client[]
        {
           new Client
           {
               ClientId = "postman",
               ClientName = "Postman",
               AllowedScopes = {"openid", "profile", "auctionApp"},
               RedirectUris = {"https://www.getpostname.com/oauth2/callback"},
               ClientSecrets = new [] {new Secret("NotASecret".Sha256())},
               AllowedGrantTypes = {GrantType.ResourceOwnerPassword}
           }
        };
}
