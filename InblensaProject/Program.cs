using InblensaProject.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;

// Creacion del constructor de la aplicacion
var builder = WebApplication.CreateBuilder(args);

// Configuracion de la base de datos
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(1); // Expira la cookie después de un minuto de inactividad
        options.SlidingExpiration = true; // Reinicia el tiempo de expiración cada vez que el usuario interactúa con la aplicacion
        options.AccessDeniedPath = "/Identity/Account/AccessDenied"; // Ruta a la que se redirige si el usuario no tiene permisos para acceder a una pagina
        options.LoginPath = "/Identity/Account/Login"; // Ruta a la que se redirige si se necesita autenticacion
        options.LogoutPath = "/Home/Logout"; // Ruta a la que se redirige cuando se cierra la sesión

        options.Cookie.HttpOnly = true; // Indica si la cookie solo debe ser accesible a través de HTTP
    });


// Configuracion de Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true; // Habilita la confirmacion de correo electronico
})
.AddRoles<IdentityRole>() // Agrega roles
.AddEntityFrameworkStores<ApplicationDbContext>() // Almacena entidades de Identity en la base de datos
.AddDefaultTokenProviders() // Agrega proveedores de tokens predeterminados
.AddDefaultUI() // Agrega UI de Identity
.AddTokenProvider<PhoneNumberTokenProvider<IdentityUser>>("CustomPhone"); // Agrega un proveedor de tokens personalizado

builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedEmail = true; // Requiere correo electronico confirmado para iniciar sesion
    // Configuracion de politicas de contrasenas
    options.Password.RequireDigit = true; // Requiere al menos un digito
    options.Password.RequireLowercase = true; // Requiere al menos una letra minuscula
    options.Password.RequireUppercase = true; // Requiere al menos una letra mayuscula
    options.Password.RequireNonAlphanumeric = true; // Requiere al menos un caracter especial
    options.Password.RequiredLength = 8; // Longitud minima de la contrasena
});


// Autenticacion con Google
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = "726270315811-ifeuae1n1rki0dvfrkmaq0t0i802cs1d.apps.googleusercontent.com";
        options.ClientSecret = "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";
    });

// Configuracion de politicas de autorizacion
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("EmpleadoPolicy", policy =>
    {
        policy.RequireRole("Empleado"); // Requiere el rol "Empleado" para acceder
    });

    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.RequireRole("Admin"); // Requiere el rol "Admin" para acceder
    });
});

// Definir una nueva politica que combine las politicas existentes
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOEmpleadoPolicy", policy =>
    {
        policy.RequireRole("Admin"); // Requiere el rol "Admin" para acceder
    });
});

// Configuracion de MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configuracion del pipeline de solicitud HTTP
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); // Habilita la autenticacion
app.UseAuthorization(); // Habilita la autorizacion

// Rutas de controlador predeterminadas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Landingpage}/{id?}");

app.MapControllerRoute(
    name: "logout",
    pattern: "/Account/Logout",
    defaults: new { controller = "Account", action = "Logout" }
);

app.MapRazorPages();

// Creacion de roles y asignacion de roles a usuarios
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

// Crear roles si no existen
var rolesToCreate = new List<string> { "Admin", "Empleado" };
foreach (var roleName in rolesToCreate)
{
    var roleExists = await roleManager.RoleExistsAsync(roleName);
    if (!roleExists)
    {
        var result = await roleManager.CreateAsync(new IdentityRole(roleName));
        if (!result.Succeeded)
        {
            // Manejar el error si la creacion del rol falla
        }
    }
}

// Buscar usuario por correo electronico y asignar roles
var adminEmail = "bejaranomariel4@gmail.com";
var adminUser = await userManager.FindByEmailAsync(adminEmail);

if (adminUser != null)
{
    // Asignar el rol de "Admin" al usuario
    await userManager.AddToRoleAsync(adminUser, "Admin");
}

// Agregar roles "Empleado" a los usuarios especificados
var usersToAddAsEmployees = new List<string> { "bejaranomariel3@gmail.com", "bejaranomariel6@gmail.com" };
foreach (var userEmail in usersToAddAsEmployees)
{
    var userToAdd = await userManager.FindByEmailAsync(userEmail);
    if (userToAdd != null)
    {
        // Verificar si el usuario no tiene roles asignados antes de agregar el rol "Empleado"
        var userRoles = await userManager.GetRolesAsync(userToAdd);
        if (userRoles == null || !userRoles.Any())
        {
            await userManager.AddToRoleAsync(userToAdd, "Empleado");
        }
    }
}

app.Run(); // Ejecutar la aplicacion
