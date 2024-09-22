using Microsoft.EntityFrameworkCore;

namespace WebAPI.Model
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=DESKTOP-0UIUHS9; Initial Catalog=UserDB; Trusted_Connection=True; MultipleActiveResultSets=True; TrustServerCertificate=true");
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating (ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>()
                .HasKey(u=> new
                {
                u.user_id
            });

            builder.Entity<Role>()
                .HasKey(r => new
                {
                    r.role_id
                });

            builder.Entity<UserRole>()
                .HasKey(ur => new
                {
                    ur.user_id, ur.role_id
                });

            builder.Entity<UserRole>()
                .HasOne(ur => ur.Users)
                .WithMany(user => user.UserRoles)
                .HasForeignKey(u => u.user_id);

            builder.Entity<UserRole>()
                .HasOne(ur => ur.Roles)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(r => r.role_id);

        }


        public DbSet<User> Users { get; set; }
        public DbSet<Role>Roles { get; set; }

        public DbSet<UserRole> UserRoles { get; set; }
    }

}
