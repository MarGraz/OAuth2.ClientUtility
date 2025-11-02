using System.ComponentModel.DataAnnotations;

namespace Another.OAuth2.ClientUtility.ClientCredentialsFlow.ConsoleAppSample.Configurations
{
    internal sealed class SampleClientSettings
    {
        private const string BaseUrlRequiredMessage = "The protected API base URL must be provided.";

        [Required(ErrorMessage = BaseUrlRequiredMessage)]
        [Url(ErrorMessage = BaseUrlRequiredMessage)]
        public string BaseUrl { get; set; } = string.Empty;
    }
}
