using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Fcg.UserLibrary.Infrastructure.Persistence
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options ) : base( options )  
        {            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {  
            base.OnModelCreating(modelBuilder);
            modelBuilder.AddInboxStateEntity();
            modelBuilder.AddOutboxMessageEntity();
            modelBuilder.AddOutboxStateEntity();
        }
    }
}
