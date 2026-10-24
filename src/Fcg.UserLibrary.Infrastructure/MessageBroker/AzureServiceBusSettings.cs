using System.ComponentModel.DataAnnotations;

namespace Fcg.UserLibrary.Infrastructure.MessageBroker
{
    internal class AzureServiceBusSettings
    {
        public const string SectionName = "AzureServiceBus";
        [Required]
        public string ConnectionString { get; set; } = string.Empty;
        [Required(ErrorMessage = "UserLibraryIntegrationQueue é obrigatório")]
        public string UserLibraryIntegrationQueue { get; set; } = string.Empty;
        [Required(ErrorMessage = "DeactiveUserIntegrationQueue é obrigatório")]
        public string DeactiveUserIntegrationQueue { get; set; } = string.Empty;
        [Required(ErrorMessage = "UserLibraryGameCreatedIntegrationQueue é obrigatório")]
        public string UserLibraryGameCreatedIntegrationQueue { get; set; } = string.Empty;
        [Required(ErrorMessage = "ReactiveUserIntegrationQueue é obrigatório")]
        public string ReactiveUserIntegrationQueue { get; set; } = string.Empty;
    }
}
