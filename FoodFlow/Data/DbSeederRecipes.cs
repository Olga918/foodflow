using FoodFlow.Enums;
using FoodFlow.Models;
using Microsoft.EntityFrameworkCore;

namespace FoodFlow.Data
{
    public static partial class DbSeeder
    {
        private static async Task<int> EnsureProductWithIncoming(
            ApplicationDbContext db,
            string name,
            string unit,
            decimal reorder,
            decimal initialQty)
        {
            var existing = await db.Products.FirstOrDefaultAsync(x => x.Name == name);
            if (existing is not null)
            {
                return existing.Id;
            }

            var p = new Product
            {
                Name = name,
                Unit = unit,
                ReorderLevel = reorder,
                QuantityInStock = initialQty
            };

            await db.Products.AddAsync(p);
            await db.SaveChangesAsync();

            await db.StockTransactions.AddAsync(new StockTransaction
            {
                ProductId = p.Id,
                Type = StockTransactionType.Incoming,
                Quantity = initialQty,
                Comment = "Seed inventory",
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();

            return p.Id;
        }

        private static async Task EnsureRecipeRow(
            ApplicationDbContext db,
            string dishName,
            string productName,
            decimal amountPerDish)
        {
            var dish = await db.MenuItems.FirstOrDefaultAsync(x => x.Name == dishName);
            var product = await db.Products.FirstOrDefaultAsync(x => x.Name == productName);
            if (dish is null || product is null)
            {
                return;
            }

            var exists = await db.RecipeIngredients.AnyAsync(x =>
                x.MenuItemId == dish.Id && x.ProductId == product.Id);
            if (exists)
            {
                return;
            }

            db.RecipeIngredients.Add(new RecipeIngredient
            {
                MenuItemId = dish.Id,
                ProductId = product.Id,
                AmountPerDish = amountPerDish
            });
            await db.SaveChangesAsync();
        }

        private static async Task SeedCatalogStockAndRecipesAsync(ApplicationDbContext db)
        {
            const decimal q = 200_000m;

            await EnsureProductWithIncoming(db, "Pizza dough", "g", 5000, q);
            await EnsureProductWithIncoming(db, "Tomato sauce", "ml", 2000, q);
            await EnsureProductWithIncoming(db, "Mozzarella", "g", 3000, q);
            await EnsureProductWithIncoming(db, "Pepperoni", "g", 2000, q);
            await EnsureProductWithIncoming(db, "Chicken fillet", "g", 5000, q);
            await EnsureProductWithIncoming(db, "BBQ sauce", "ml", 1500, q);
            await EnsureProductWithIncoming(db, "Cheese blend", "g", 4000, q);
            await EnsureProductWithIncoming(db, "Bell pepper mix", "g", 2000, q);
            await EnsureProductWithIncoming(db, "Mushrooms", "g", 2000, q);
            await EnsureProductWithIncoming(db, "Spicy salami", "g", 1500, q);
            await EnsureProductWithIncoming(db, "Jalapeno", "g", 500, q);
            await EnsureProductWithIncoming(db, "Burger bun", "pcs", 500, 10_000);
            await EnsureProductWithIncoming(db, "Beef patty", "pcs", 500, 10_000);
            await EnsureProductWithIncoming(db, "Fresh tomato", "g", 2000, q);
            await EnsureProductWithIncoming(db, "Burger sauce", "ml", 1500, q);
            await EnsureProductWithIncoming(db, "Pickles", "g", 1000, q);
            await EnsureProductWithIncoming(db, "Crispy chicken patty", "pcs", 300, 5000);
            await EnsureProductWithIncoming(db, "Iceberg lettuce", "g", 2000, q);
            await EnsureProductWithIncoming(db, "Garlic sauce", "ml", 1500, q);
            await EnsureProductWithIncoming(db, "Sauteed mushrooms", "g", 1500, q);
            await EnsureProductWithIncoming(db, "Truffle mayo", "ml", 800, q);
            await EnsureProductWithIncoming(db, "Pasta dry", "g", 5000, q);
            await EnsureProductWithIncoming(db, "Bacon", "g", 2000, q);
            await EnsureProductWithIncoming(db, "Parmesan", "g", 2000, q);
            await EnsureProductWithIncoming(db, "Bolognese sauce", "ml", 3000, q);
            await EnsureProductWithIncoming(db, "Cream", "ml", 5000, q);
            await EnsureProductWithIncoming(db, "Pesto sauce", "g", 1500, q);
            await EnsureProductWithIncoming(db, "Cherry tomato", "g", 1500, q);
            await EnsureProductWithIncoming(db, "Croutons", "g", 1000, q);
            await EnsureProductWithIncoming(db, "Tuna", "g", 2000, q);
            await EnsureProductWithIncoming(db, "Lemon dressing", "ml", 1500, q);
            await EnsureProductWithIncoming(db, "Cucumber", "g", 2000, q);
            await EnsureProductWithIncoming(db, "Feta", "g", 1500, q);
            await EnsureProductWithIncoming(db, "Olives", "g", 1000, q);
            await EnsureProductWithIncoming(db, "Olive oil", "ml", 2000, q);
            await EnsureProductWithIncoming(db, "Soft drink syrup", "ml", 5000, q);
            await EnsureProductWithIncoming(db, "Orange juice mix", "ml", 3000, q);
            await EnsureProductWithIncoming(db, "Coffee beans", "g", 2000, q);
            await EnsureProductWithIncoming(db, "Milk", "ml", 8000, q);
            await EnsureProductWithIncoming(db, "Lemonade mix", "ml", 2000, q);
            await EnsureProductWithIncoming(db, "Bottled water", "ml", 10000, q);
            await EnsureProductWithIncoming(db, "Cheesecake mix", "g", 1500, q);
            await EnsureProductWithIncoming(db, "Brownie mix", "g", 1500, q);
            await EnsureProductWithIncoming(db, "Tiramisu mix", "g", 1500, q);
            await EnsureProductWithIncoming(db, "Apple pie mix", "g", 1500, q);
            await EnsureProductWithIncoming(db, "Baguette", "pcs", 300, 5000);
            await EnsureProductWithIncoming(db, "Garlic butter", "g", 1500, q);
            await EnsureProductWithIncoming(db, "Chicken wings raw", "g", 4000, q);
            await EnsureProductWithIncoming(db, "Wing glaze", "ml", 1000, q);
            await EnsureProductWithIncoming(db, "Frozen mozzarella sticks", "pcs", 500, 8000);
            await EnsureProductWithIncoming(db, "Tomato dip", "ml", 1500, q);
            await EnsureProductWithIncoming(db, "Salmon fillet", "pcs", 200, 3000);
            await EnsureProductWithIncoming(db, "Lemon butter", "g", 800, q);
            await EnsureProductWithIncoming(db, "Shrimp", "g", 3000, q);
            await EnsureProductWithIncoming(db, "Tom yum base", "ml", 2000, q);
            await EnsureProductWithIncoming(db, "Chicken broth", "ml", 5000, q);
            await EnsureProductWithIncoming(db, "Egg noodles", "g", 2000, q);
            await EnsureProductWithIncoming(db, "Soup vegetables", "g", 2000, q);
            await EnsureProductWithIncoming(db, "Caesar dressing", "ml", 1500, q);
            await EnsureProductWithIncoming(db, "Romaine lettuce", "g", 3000, q);
            await EnsureProductWithIncoming(db, "Egg", "pcs", 200, 5000);

            // Pizzas
            await EnsureRecipeRow(db, "Margherita", "Pizza dough", 220);
            await EnsureRecipeRow(db, "Margherita", "Tomato sauce", 50);
            await EnsureRecipeRow(db, "Margherita", "Mozzarella", 180);

            await EnsureRecipeRow(db, "Pepperoni", "Pizza dough", 220);
            await EnsureRecipeRow(db, "Pepperoni", "Tomato sauce", 45);
            await EnsureRecipeRow(db, "Pepperoni", "Mozzarella", 120);
            await EnsureRecipeRow(db, "Pepperoni", "Pepperoni", 45);

            await EnsureRecipeRow(db, "BBQ Chicken Pizza", "Pizza dough", 220);
            await EnsureRecipeRow(db, "BBQ Chicken Pizza", "BBQ sauce", 40);
            await EnsureRecipeRow(db, "BBQ Chicken Pizza", "Mozzarella", 90);
            await EnsureRecipeRow(db, "BBQ Chicken Pizza", "Chicken fillet", 100);

            await EnsureRecipeRow(db, "Four Cheese Pizza", "Pizza dough", 220);
            await EnsureRecipeRow(db, "Four Cheese Pizza", "Tomato sauce", 35);
            await EnsureRecipeRow(db, "Four Cheese Pizza", "Cheese blend", 220);

            await EnsureRecipeRow(db, "Vegetarian Pizza", "Pizza dough", 220);
            await EnsureRecipeRow(db, "Vegetarian Pizza", "Tomato sauce", 50);
            await EnsureRecipeRow(db, "Vegetarian Pizza", "Mozzarella", 130);
            await EnsureRecipeRow(db, "Vegetarian Pizza", "Bell pepper mix", 40);
            await EnsureRecipeRow(db, "Vegetarian Pizza", "Olives", 25);
            await EnsureRecipeRow(db, "Vegetarian Pizza", "Mushrooms", 35);

            await EnsureRecipeRow(db, "Diablo Pizza", "Pizza dough", 220);
            await EnsureRecipeRow(db, "Diablo Pizza", "Tomato sauce", 40);
            await EnsureRecipeRow(db, "Diablo Pizza", "Mozzarella", 110);
            await EnsureRecipeRow(db, "Diablo Pizza", "Spicy salami", 50);
            await EnsureRecipeRow(db, "Diablo Pizza", "Jalapeno", 15);

            // Burgers
            await EnsureRecipeRow(db, "Cheese Burger", "Burger bun", 1);
            await EnsureRecipeRow(db, "Cheese Burger", "Beef patty", 1);
            await EnsureRecipeRow(db, "Cheese Burger", "Mozzarella", 40);
            await EnsureRecipeRow(db, "Cheese Burger", "Fresh tomato", 30);
            await EnsureRecipeRow(db, "Cheese Burger", "Burger sauce", 15);

            await EnsureRecipeRow(db, "Double Burger", "Burger bun", 1);
            await EnsureRecipeRow(db, "Double Burger", "Beef patty", 2);
            await EnsureRecipeRow(db, "Double Burger", "Mozzarella", 60);
            await EnsureRecipeRow(db, "Double Burger", "Pickles", 20);

            await EnsureRecipeRow(db, "Chicken Burger", "Burger bun", 1);
            await EnsureRecipeRow(db, "Chicken Burger", "Crispy chicken patty", 1);
            await EnsureRecipeRow(db, "Chicken Burger", "Iceberg lettuce", 40);
            await EnsureRecipeRow(db, "Chicken Burger", "Garlic sauce", 20);

            await EnsureRecipeRow(db, "Mushroom Burger", "Burger bun", 1);
            await EnsureRecipeRow(db, "Mushroom Burger", "Beef patty", 1);
            await EnsureRecipeRow(db, "Mushroom Burger", "Sauteed mushrooms", 50);
            await EnsureRecipeRow(db, "Mushroom Burger", "Mozzarella", 40);
            await EnsureRecipeRow(db, "Mushroom Burger", "Truffle mayo", 25);

            // Pasta
            await EnsureRecipeRow(db, "Carbonara", "Pasta dry", 150);
            await EnsureRecipeRow(db, "Carbonara", "Cream", 80);
            await EnsureRecipeRow(db, "Carbonara", "Bacon", 45);
            await EnsureRecipeRow(db, "Carbonara", "Parmesan", 25);

            await EnsureRecipeRow(db, "Bolognese", "Pasta dry", 150);
            await EnsureRecipeRow(db, "Bolognese", "Bolognese sauce", 180);
            await EnsureRecipeRow(db, "Bolognese", "Parmesan", 20);

            await EnsureRecipeRow(db, "Alfredo", "Pasta dry", 140);
            await EnsureRecipeRow(db, "Alfredo", "Cream", 120);
            await EnsureRecipeRow(db, "Alfredo", "Chicken fillet", 80);
            await EnsureRecipeRow(db, "Alfredo", "Parmesan", 25);

            await EnsureRecipeRow(db, "Pesto Pasta", "Pasta dry", 140);
            await EnsureRecipeRow(db, "Pesto Pasta", "Pesto sauce", 40);
            await EnsureRecipeRow(db, "Pesto Pasta", "Cherry tomato", 50);
            await EnsureRecipeRow(db, "Pesto Pasta", "Parmesan", 20);

            // Salads
            await EnsureRecipeRow(db, "Caesar Salad", "Romaine lettuce", 180);
            await EnsureRecipeRow(db, "Caesar Salad", "Chicken fillet", 90);
            await EnsureRecipeRow(db, "Caesar Salad", "Croutons", 25);
            await EnsureRecipeRow(db, "Caesar Salad", "Caesar dressing", 45);

            await EnsureRecipeRow(db, "Greek Salad", "Fresh tomato", 80);
            await EnsureRecipeRow(db, "Greek Salad", "Cucumber", 60);
            await EnsureRecipeRow(db, "Greek Salad", "Feta", 55);
            await EnsureRecipeRow(db, "Greek Salad", "Olives", 30);
            await EnsureRecipeRow(db, "Greek Salad", "Olive oil", 25);

            await EnsureRecipeRow(db, "Tuna Salad", "Romaine lettuce", 150);
            await EnsureRecipeRow(db, "Tuna Salad", "Tuna", 80);
            await EnsureRecipeRow(db, "Tuna Salad", "Egg", 1);
            await EnsureRecipeRow(db, "Tuna Salad", "Cherry tomato", 40);
            await EnsureRecipeRow(db, "Tuna Salad", "Lemon dressing", 30);

            // Drinks
            await EnsureRecipeRow(db, "Cola", "Soft drink syrup", 40);
            await EnsureRecipeRow(db, "Cola", "Bottled water", 460);

            await EnsureRecipeRow(db, "Orange Juice", "Orange juice mix", 300);

            await EnsureRecipeRow(db, "Americano", "Coffee beans", 18);
            await EnsureRecipeRow(db, "Americano", "Bottled water", 200);

            await EnsureRecipeRow(db, "Cappuccino", "Coffee beans", 18);
            await EnsureRecipeRow(db, "Cappuccino", "Milk", 180);

            await EnsureRecipeRow(db, "Lemonade", "Lemonade mix", 50);
            await EnsureRecipeRow(db, "Lemonade", "Bottled water", 350);

            await EnsureRecipeRow(db, "Still Water", "Bottled water", 500);

            // Desserts
            await EnsureRecipeRow(db, "Cheesecake", "Cheesecake mix", 120);
            await EnsureRecipeRow(db, "Chocolate Brownie", "Brownie mix", 100);
            await EnsureRecipeRow(db, "Tiramisu", "Tiramisu mix", 110);
            await EnsureRecipeRow(db, "Apple Pie", "Apple pie mix", 130);

            // Appetizers
            await EnsureRecipeRow(db, "Garlic Bread", "Baguette", 0.5m);
            await EnsureRecipeRow(db, "Garlic Bread", "Garlic butter", 35);

            await EnsureRecipeRow(db, "Chicken Wings", "Chicken wings raw", 220);
            await EnsureRecipeRow(db, "Chicken Wings", "Wing glaze", 35);

            await EnsureRecipeRow(db, "Mozzarella Sticks", "Frozen mozzarella sticks", 6);
            await EnsureRecipeRow(db, "Mozzarella Sticks", "Tomato dip", 40);

            // Seafood
            await EnsureRecipeRow(db, "Grilled Salmon", "Salmon fillet", 1);
            await EnsureRecipeRow(db, "Grilled Salmon", "Lemon butter", 25);
            await EnsureRecipeRow(db, "Grilled Salmon", "Garlic butter", 10);

            await EnsureRecipeRow(db, "Shrimp Pasta", "Pasta dry", 130);
            await EnsureRecipeRow(db, "Shrimp Pasta", "Shrimp", 100);
            await EnsureRecipeRow(db, "Shrimp Pasta", "Cream", 70);
            await EnsureRecipeRow(db, "Shrimp Pasta", "Garlic sauce", 15);

            // Soups
            await EnsureRecipeRow(db, "Tom Yum", "Tom yum base", 250);
            await EnsureRecipeRow(db, "Tom Yum", "Shrimp", 50);
            await EnsureRecipeRow(db, "Tom Yum", "Mushrooms", 30);

            await EnsureRecipeRow(db, "Chicken Noodle Soup", "Chicken broth", 350);
            await EnsureRecipeRow(db, "Chicken Noodle Soup", "Egg noodles", 80);
            await EnsureRecipeRow(db, "Chicken Noodle Soup", "Chicken fillet", 90);
            await EnsureRecipeRow(db, "Chicken Noodle Soup", "Soup vegetables", 40);

            await BootstrapKitchenLineFromWarehouseIfEmptyAsync(db, portionsPerDish: 8);
        }

        /// <summary>
        /// If no dish has line stock yet (fresh DB or new feature), prepare a small batch per recipe
        /// so the menu is orderable and stock matches reality (write-off like Line prep).
        /// </summary>
        private static async Task BootstrapKitchenLineFromWarehouseIfEmptyAsync(
            ApplicationDbContext db,
            int portionsPerDish)
        {
            if (portionsPerDish < 1)
            {
                return;
            }

            var alreadyBootstrapped = await db.StockTransactions.AnyAsync(x =>
                x.Comment != null && x.Comment.Contains("Opening line prep (seed)"));
            if (alreadyBootstrapped)
            {
                return;
            }

            var kitchenAlreadyInUse = await db.MenuItems.AnyAsync(x => x.KitchenPortions > 0)
                || await db.StockTransactions.AnyAsync(x =>
                    x.Comment != null && x.Comment.Contains("Kitchen prep:"));
            if (kitchenAlreadyInUse)
            {
                return;
            }

            var dishes = await db.MenuItems.ToListAsync();
            var now = DateTime.UtcNow;

            foreach (var dish in dishes)
            {
                var recipe = await db.RecipeIngredients
                    .Where(x => x.MenuItemId == dish.Id)
                    .ToListAsync();
                if (!recipe.Any())
                {
                    continue;
                }

                var requiredByProduct = new Dictionary<int, decimal>();
                foreach (var row in recipe)
                {
                    var need = row.AmountPerDish * portionsPerDish;
                    if (requiredByProduct.TryGetValue(row.ProductId, out var existing))
                    {
                        requiredByProduct[row.ProductId] = existing + need;
                    }
                    else
                    {
                        requiredByProduct[row.ProductId] = need;
                    }
                }

                var productIds = requiredByProduct.Keys.ToList();
                var products = await db.Products.Where(x => productIds.Contains(x.Id)).ToListAsync();
                var ok = true;
                foreach (var (productId, need) in requiredByProduct)
                {
                    var p = products.FirstOrDefault(x => x.Id == productId);
                    if (p is null || p.QuantityInStock < need)
                    {
                        ok = false;
                        break;
                    }
                }

                if (!ok)
                {
                    continue;
                }

                foreach (var product in products)
                {
                    var need = requiredByProduct[product.Id];
                    if (need <= 0)
                    {
                        continue;
                    }

                    product.QuantityInStock -= need;
                    await db.StockTransactions.AddAsync(new StockTransaction
                    {
                        ProductId = product.Id,
                        Type = StockTransactionType.WriteOff,
                        Quantity = need,
                        Comment = $"Opening line prep (seed): {dish.Name} ×{portionsPerDish}",
                        CreatedAt = now
                    });
                }

                dish.KitchenPortions += portionsPerDish;
            }

            await db.SaveChangesAsync();
        }
    }
}
