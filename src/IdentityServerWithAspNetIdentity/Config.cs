using IdentityServer4.Models;
using IdentityServerWithAspNetIdentity.ConfigOptions;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace IdentityServerWithAspNetIdentity
{
    public class Config
    {
        private static ApiResourcesOptions apiResourceOptions;
        private static ClientOptions clientOptions;
        private static string secret;

        public static void UseAppSettings(IConfigurationRoot configuration)
        {
            apiResourceOptions = new ApiResourcesOptions();
            clientOptions = new ClientOptions();

            configuration.GetSection("apiResources").Bind(apiResourceOptions);
            configuration.GetSection("clients").Bind(clientOptions);
            secret = configuration.GetValue<string>("secret");
        }
        
        // scopes define the resources in your system
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            var result = new List<ApiResource>();

            if (apiResourceOptions != null)
            {
                foreach (var item in apiResourceOptions.Items)
                {
                    result.Add(new ApiResource(item.Name, item.DisplayName, item.ClaimTypes));
                }
            }

            return result;
        }

        // clients want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients()
        {
            var result = new List<Client>();

            if (clientOptions != null)
            {
                foreach (var item in clientOptions.Items)
                {
                    result.Add(new Client
                    {
                        ClientId = item.ClientId,
                        ClientName = item.ClientName,
                        AllowedGrantTypes = GetAllowedGrandTypes(item.AllowedGrandTypes),
                        AllowAccessTokensViaBrowser = true,

                        RedirectUris = { item.RedirectUri },
                        PostLogoutRedirectUris = { item.PostLogoutRedirectUri },
                        AllowedCorsOrigins = { item.AllowedCorsOrigin },
                        AllowedScopes = item.AllowedScopes
                    });
                }
            }

            return result;
        }

        private static IEnumerable<string> GetAllowedGrandTypes(string allowedGrandTypes)
        {
            if (allowedGrandTypes != null)
            {
                switch (allowedGrandTypes.ToLower())
                {
                    case "implicit":
                        return GrantTypes.Implicit;
                    case "hybridandclientcredentials":
                        return GrantTypes.HybridAndClientCredentials;
                    case "clientcredentials":
                        return GrantTypes.ClientCredentials;
                    case "resourceownerpassword":
                        return GrantTypes.ResourceOwnerPassword;
                    default:
                        return new string[] { allowedGrandTypes };
                }
            }

            return null;
        }
    }
}