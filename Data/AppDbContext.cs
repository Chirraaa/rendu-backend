using KanbanApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace KanbanApp.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Kanban> Kanbans { get; set; }
        public DbSet<KanbanMember> KanbanMembers { get; set; }
        public DbSet<Column> Columns { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketMovementHistory> TicketMovementHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User -> unique email
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            // User -> many RefreshTokens
            modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            // Kanban -> CreatedBy User
            modelBuilder.Entity<Kanban>()
            .HasOne(k => k.CreatedBy)
            .WithMany()
            .HasForeignKey(k => k.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

            // KanbanMember -> Kanban
            modelBuilder.Entity<KanbanMember>()
            .HasOne(km => km.User)
            .WithMany(u => u.KanbanMemberships)
            .HasForeignKey(km => km.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            // Kanban -> many Columns
            modelBuilder.Entity<Column>()
            .HasOne(c => c.Kanban)
            .WithMany(k => k.Columns)
            .HasForeignKey(c => c.KanbanId)
            .OnDelete(DeleteBehavior.Cascade);

            // Column -> many Tickets
            modelBuilder.Entity<Ticket>()
            .HasOne(t => t.Column)
            .WithMany(c => c.Tickets)
            .HasForeignKey(t => t.ColumnId)
            .OnDelete(DeleteBehavior.Cascade);

            // Ticket -> AssignedTo User
            modelBuilder.Entity<Ticket>()
            .HasOne(t => t.AssignedTo)
            .WithMany()
            .HasForeignKey(t => t.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

            // TicketMovementHistory -> Ticket
            modelBuilder.Entity<TicketMovementHistory>()
            .HasOne(h => h.Ticket)
            .WithMany(t => t.MovementHistory)
            .HasForeignKey(h => h.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

            // TicketMovementHistory -> FromColumn / ToColumn (no cascade)
            modelBuilder.Entity<TicketMovementHistory>()
            .HasOne(h => h.FromColumn)
            .WithMany()
            .HasForeignKey(h => h.FromColumnId)
            .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TicketMovementHistory>()
            .HasOne(h => h.ToColumn)
            .WithMany()
            .HasForeignKey(h => h.ToColumnId)
            .OnDelete(DeleteBehavior.SetNull);
        }
    }
}