namespace Fcg.UserLibrary.Application.Features.Response
{
    public class UserLibraryItemResponse
    {
        public Guid GameId { get; set; }
        public string GameName { get; set; }
        public string Description { get; set; }
        public string Genre { get; set; }
        public DateTime AcquiredAt { get; set; }

        public UserLibraryItemResponse()
        {
        }
    }
}
