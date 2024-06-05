﻿using Duende.IdentityServer.Models;

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
           },

           new Client
           {
               ClientId = "nextApp",
               ClientName = "nextApp",
               ClientSecrets = {new Secret("secret".Sha256())},
               AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
               RequirePkce = false,
               RedirectUris = {"http://localhost:3000/api/auth/callback/id-server"},
               AllowOfflineAccess = true,
               AllowedScopes = {"openid", "profile", "auctionApp"},
               AccessTokenLifetime = 3600 * 24 * 30
           }
        };
}
