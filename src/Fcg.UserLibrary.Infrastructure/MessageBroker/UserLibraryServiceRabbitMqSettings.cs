using Fcg.Core.WebApi.MessageBroker;
using System.ComponentModel.DataAnnotations;

namespace Fcg.UserLibrary.Infrastructure.MessageBroker
{
    public class UserLibraryServiceRabbitMqSettings : RabbitMqConnectionSettings
    {
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
