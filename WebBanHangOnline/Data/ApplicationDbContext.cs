using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebBanHangOnline.Models;

namespace WebBanHangOnline.Data
{

    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Khai báo các DbSet tương ứng với các bảng trong cơ sở dữ liệu
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); 

            builder.Entity<User>().ToTable("Users");
            builder.Entity<IdentityRole<int>>().ToTable("Roles");
            builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

            builder.Entity<Product>()
                   .Property(p => p.Price)
                   .HasColumnType("decimal(18, 2)");

            builder.Entity<OrderDetail>()
                   .Property(p => p.Price)
                   .HasColumnType("decimal(18, 2)");

            builder.Entity<Order>()
                   .Property(p => p.TotalAmount)
                   .HasColumnType("decimal(18, 2)");
        }
    }
}
