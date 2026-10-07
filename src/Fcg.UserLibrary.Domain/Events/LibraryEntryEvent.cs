using MediatR;

namespace Fcg.UserLibrary.Domain.Events
{
    public class LibraryEntryEvent : INotification
    {
        public Guid UserId { get; set; }

        public LibraryEntryEvent(Guid userId)
        {
            UserId = userId;
        }
    }
}
