using Learn_Earn.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity (default UI) with roles enabled
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Seed demo data for Sprint 2 (Lessons) if needed
using (var scope = app.Services.CreateScope())
{
    try
    {
        await Learn_Earn.Data.SeedData.EnsureSeedDataAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error seeding demo data");
    }
}

// Seed roles and admin user for local development
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        var roles = new[] { "User", "Profesor", "Admin" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Create admin user if not exists (development convenience)
        var adminEmail = "admin@local";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
        else
        {
            // If admin exists, for development we recreate the admin account to guarantee a valid password hash.
            try
            {
                // delete existing admin user (dev only)
                var del = await userManager.DeleteAsync(admin);
                // create a fresh admin user with known password
                var fresh = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                var created = await userManager.CreateAsync(fresh, "Admin123!");
                if (created.Succeeded)
                {
                    await userManager.AddToRoleAsync(fresh, "Admin");
                }
                    // verify the created admin can be validated by current hasher
                    try
                    {
                        var can = await userManager.CheckPasswordAsync(fresh, "Admin123!");
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                        logger.LogInformation("Admin password check after create: {ok}", can);
                    }
                    catch (Exception ex)
                    {
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning(ex, "Exception verifying admin password after create.");
                    }
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogWarning(ex, "Could not recreate admin user in seed path.");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error seeding roles/admin");
    }
}

app.MapRazorPages();
app.Run();
