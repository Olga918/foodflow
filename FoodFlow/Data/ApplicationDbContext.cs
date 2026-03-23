using FoodFlow.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FoodFlow.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<MenuCategory> MenuCategories => Set<MenuCategory>();
        public DbSet<MenuItem> MenuItems => Set<MenuItem>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
        public DbSet<PurchaseList> PurchaseLists => Set<PurchaseList>();
        public DbSet<PurchaseListLine> PurchaseListLines => Set<PurchaseListLine>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<MenuItem>()
                .Property(x => x.Price)
                .HasPrecision(10, 2);

            builder.Entity<Product>()
                .Property(x => x.QuantityInStock)
                .HasPrecision(12, 3);

            builder.Entity<Product>()
                .Property(x => x.ReorderLevel)
                .HasPrecision(12, 3);

            builder.Entity<StockTransaction>()
                .Property(x => x.Quantity)
                .HasPrecision(12, 3);

            builder.Entity<RecipeIngredient>()
                .Property(x => x.AmountPerDish)
                .HasPrecision(12, 3);

            builder.Entity<Order>()
                .Property(x => x.TotalAmount)
                .HasPrecision(10, 2);

            builder.Entity<OrderItem>()
                .Property(x => x.UnitPrice)
                .HasPrecision(10, 2);

            builder.Entity<Order>()
                .HasMany(x => x.Items)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PurchaseListLine>()
                .Property(x => x.SuggestedQuantity)
                .HasPrecision(12, 3);

            builder.Entity<PurchaseListLine>()
                .Property(x => x.ReceivedQuantity)
                .HasPrecision(12, 3);

            builder.Entity<PurchaseList>()
                .HasMany(x => x.Lines)
                .WithOne(x => x.PurchaseList)
                .HasForeignKey(x => x.PurchaseListId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
