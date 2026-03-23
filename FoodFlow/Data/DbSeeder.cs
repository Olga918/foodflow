using FoodFlow.Enums;
using FoodFlow.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodFlow.Data
{
    public static partial class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var scopeFactory = services.GetRequiredService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await db.Database.MigrateAsync();

            var roles = new[]
            {
                UserRole.Client.ToString(),
                UserRole.Cook.ToString(),
                UserRole.Storekeeper.ToString(),
                UserRole.Admin.ToString()
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            const string adminEmail = "admin@foodflow.local";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "FoodFlow Admin",
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(admin, "Admin123!");
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, UserRole.Admin.ToString());
                }
            }

            await EnsureUserWithRoleAsync(
                userManager,
                "cook@foodflow.local",
                "Cook123!",
                "FoodFlow Cook",
                UserRole.Cook.ToString());

            await EnsureUserWithRoleAsync(
                userManager,
                "storekeeper@foodflow.local",
                "Store123!",
                "FoodFlow Storekeeper",
                UserRole.Storekeeper.ToString());

            var pizzaCategoryId = await EnsureCategoryAsync(db, "Pizza", 1);
            var burgerCategoryId = await EnsureCategoryAsync(db, "Burgers", 2);
            var pastaCategoryId = await EnsureCategoryAsync(db, "Pasta", 3);
            var saladsCategoryId = await EnsureCategoryAsync(db, "Salads", 4);
            var drinksCategoryId = await EnsureCategoryAsync(db, "Drinks", 5);
            var dessertsCategoryId = await EnsureCategoryAsync(db, "Desserts", 6);
            var appetizersCategoryId = await EnsureCategoryAsync(db, "Appetizers", 7);
            var seafoodCategoryId = await EnsureCategoryAsync(db, "Seafood", 8);
            var soupsCategoryId = await EnsureCategoryAsync(db, "Soups", 9);

            await EnsureMenuItemAsync(
                db,
                "Margherita",
                "Classic pizza with tomato sauce and mozzarella.",
                pizzaCategoryId,
                8.99m);
            await EnsureMenuItemAsync(
                db,
                "Pepperoni",
                "Tomato sauce, mozzarella and spicy pepperoni.",
                pizzaCategoryId,
                10.49m);
            await EnsureMenuItemAsync(
                db,
                "BBQ Chicken Pizza",
                "Chicken, mozzarella, red onion and BBQ sauce.",
                pizzaCategoryId,
                11.99m);
            await EnsureMenuItemAsync(
                db,
                "Four Cheese Pizza",
                "Mozzarella, parmesan, gorgonzola and cheddar.",
                pizzaCategoryId,
                12.49m);
            await EnsureMenuItemAsync(
                db,
                "Vegetarian Pizza",
                "Tomato sauce, mozzarella, bell pepper, olives and mushrooms.",
                pizzaCategoryId,
                10.99m);
            await EnsureMenuItemAsync(
                db,
                "Diablo Pizza",
                "Spicy salami, jalapeno, red onion and chili flakes.",
                pizzaCategoryId,
                11.79m);

            await EnsureMenuItemAsync(
                db,
                "Cheese Burger",
                "Beef patty, cheddar, tomato and signature sauce.",
                burgerCategoryId,
                7.49m);
            await EnsureMenuItemAsync(
                db,
                "Double Burger",
                "Double beef patty, cheddar and pickles.",
                burgerCategoryId,
                9.29m);
            await EnsureMenuItemAsync(
                db,
                "Chicken Burger",
                "Crispy chicken, iceberg lettuce and garlic sauce.",
                burgerCategoryId,
                8.29m);
            await EnsureMenuItemAsync(
                db,
                "Mushroom Burger",
                "Beef patty, sauteed mushrooms, cheddar and truffle mayo.",
                burgerCategoryId,
                9.89m);

            await EnsureMenuItemAsync(
                db,
                "Carbonara",
                "Cream sauce, bacon and parmesan.",
                pastaCategoryId,
                8.49m);
            await EnsureMenuItemAsync(
                db,
                "Bolognese",
                "Traditional meat sauce with parmesan.",
                pastaCategoryId,
                8.99m);
            await EnsureMenuItemAsync(
                db,
                "Alfredo",
                "Creamy alfredo sauce with chicken and parmesan.",
                pastaCategoryId,
                9.49m);
            await EnsureMenuItemAsync(
                db,
                "Pesto Pasta",
                "Basil pesto, cherry tomato and parmesan cheese.",
                pastaCategoryId,
                8.79m);

            await EnsureMenuItemAsync(
                db,
                "Caesar Salad",
                "Romaine, chicken, croutons and Caesar dressing.",
                saladsCategoryId,
                6.99m);
            await EnsureMenuItemAsync(
                db,
                "Greek Salad",
                "Tomato, cucumber, feta, olives and olive oil.",
                saladsCategoryId,
                6.49m);
            await EnsureMenuItemAsync(
                db,
                "Tuna Salad",
                "Fresh greens, tuna, eggs, cherry tomato and lemon dressing.",
                saladsCategoryId,
                7.29m);

            await EnsureMenuItemAsync(
                db,
                "Cola",
                "0.5L soft drink.",
                drinksCategoryId,
                1.99m);
            await EnsureMenuItemAsync(
                db,
                "Orange Juice",
                "Fresh orange juice 0.3L.",
                drinksCategoryId,
                2.79m);
            await EnsureMenuItemAsync(
                db,
                "Americano",
                "Hot black coffee.",
                drinksCategoryId,
                2.29m);
            await EnsureMenuItemAsync(
                db,
                "Cappuccino",
                "Espresso with steamed milk and foam.",
                drinksCategoryId,
                2.99m);
            await EnsureMenuItemAsync(
                db,
                "Lemonade",
                "House lemonade with fresh mint and lemon.",
                drinksCategoryId,
                2.59m);
            await EnsureMenuItemAsync(
                db,
                "Still Water",
                "0.5L bottled still water.",
                drinksCategoryId,
                1.49m);

            await EnsureMenuItemAsync(
                db,
                "Cheesecake",
                "Classic cheesecake with berry topping.",
                dessertsCategoryId,
                4.99m);
            await EnsureMenuItemAsync(
                db,
                "Chocolate Brownie",
                "Warm brownie with chocolate sauce.",
                dessertsCategoryId,
                4.49m);
            await EnsureMenuItemAsync(
                db,
                "Tiramisu",
                "Classic Italian dessert with mascarpone and coffee.",
                dessertsCategoryId,
                5.29m);
            await EnsureMenuItemAsync(
                db,
                "Apple Pie",
                "Homemade apple pie with cinnamon and vanilla sauce.",
                dessertsCategoryId,
                4.79m);

            await EnsureMenuItemAsync(
                db,
                "Garlic Bread",
                "Baked baguette with garlic butter and herbs.",
                appetizersCategoryId,
                3.29m);
            await EnsureMenuItemAsync(
                db,
                "Chicken Wings",
                "Spicy glazed chicken wings with dip sauce.",
                appetizersCategoryId,
                6.99m);
            await EnsureMenuItemAsync(
                db,
                "Mozzarella Sticks",
                "Fried mozzarella sticks with tomato dip.",
                appetizersCategoryId,
                5.49m);

            await EnsureMenuItemAsync(
                db,
                "Grilled Salmon",
                "Salmon fillet with lemon butter and herbs.",
                seafoodCategoryId,
                14.99m);
            await EnsureMenuItemAsync(
                db,
                "Shrimp Pasta",
                "Pasta with shrimp, garlic and light cream sauce.",
                seafoodCategoryId,
                12.79m);

            await EnsureMenuItemAsync(
                db,
                "Tom Yum",
                "Spicy Thai soup with shrimp and mushrooms.",
                soupsCategoryId,
                7.49m);
            await EnsureMenuItemAsync(
                db,
                "Chicken Noodle Soup",
                "Clear broth with chicken, noodles and vegetables.",
                soupsCategoryId,
                5.99m);

            await SeedCatalogStockAndRecipesAsync(db);
        }

        private static async Task EnsureUserWithRoleAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string password,
            string fullName,
            string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                {
                    return;
                }
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }

        private static async Task<int> EnsureCategoryAsync(ApplicationDbContext db, string name, int sortOrder)
        {
            var category = await db.MenuCategories.FirstOrDefaultAsync(x => x.Name == name);
            if (category is null)
            {
                category = new MenuCategory
                {
                    Name = name,
                    SortOrder = sortOrder
                };

                await db.MenuCategories.AddAsync(category);
                await db.SaveChangesAsync();
                return category.Id;
            }

            if (category.SortOrder != sortOrder)
            {
                category.SortOrder = sortOrder;
                await db.SaveChangesAsync();
            }

            return category.Id;
        }

        private static async Task EnsureMenuItemAsync(
            ApplicationDbContext db,
            string name,
            string description,
            int menuCategoryId,
            decimal price)
        {
            var item = await db.MenuItems.FirstOrDefaultAsync(x => x.Name == name);
            if (item is null)
            {
                await db.MenuItems.AddAsync(new MenuItem
                {
                    Name = name,
                    Description = description,
                    MenuCategoryId = menuCategoryId,
                    Price = price,
                    IsAvailable = true
                });

                await db.SaveChangesAsync();
                return;
            }

            item.Description = description;
            item.MenuCategoryId = menuCategoryId;
            item.Price = price;
            item.IsAvailable = true;
            await db.SaveChangesAsync();
        }
    }
}
