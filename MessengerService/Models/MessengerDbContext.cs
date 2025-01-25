using Microsoft.EntityFrameworkCore;

namespace MessengerService.Models
{
    public class MessengerDbContext : DbContext
    {
        public MessengerDbContext(DbContextOptions<MessengerDbContext> options)
            : base(options)
        {
        }

        public DbSet<Message> Messages { get; set; } = null!;
    }
}
