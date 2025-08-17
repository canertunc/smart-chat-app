using Microsoft.EntityFrameworkCore;
using RagBasedChatbot.Models;
namespace RagBasedChatbot.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasNoKey();
            modelBuilder.Entity<ChatMessage>()
                .HasKey(cm => cm.MessageId);

            base.OnModelCreating(modelBuilder);
            
        }
    }
}