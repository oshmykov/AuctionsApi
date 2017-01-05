namespace IdentityServerWithAspNetIdentity.ConfigOptions
{
    public class ApiResourcesOptions
    {
        public ApiResourceOptions[] Items { get; set; }
    }

    public class ApiResourceOptions
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string[] ClaimTypes { get; set; }
    }

    public class ClientOptions
    {
        public ClientOption[] Items { get; set; }
    }

    public class ClientOption
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public bool AllowAccessTokensViaBrowser { get; set; }
        public string RedirectUri { get; set; }
        public string PostLogoutRedirectUri { get; set; }
        public string AllowedCorsOrigin { get; set; }
        public string AllowedGrandTypes { get; set; }
        public string[] AllowedScopes { get; set; }
    }
}
