using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Fcg.UserLibrary.Domain.Entities
{
    public class LibraryCustomer
    {
        [BsonRepresentation(BsonType.String)]
        public Guid CustomerId { get; private set; }
        private LibraryCustomer() { }

        public LibraryCustomer(Guid customerId)
        {
            CustomerId = customerId;
        }
    }
}
