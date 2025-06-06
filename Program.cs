using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SisAlmacenProductos.Data;

var builder = WebApplication.CreateBuilder(args);

// Agregamos ApplicationDbContext con conexión a MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 23)) // Cambia si usas otra versión
    ));

// Agregamos soporte para controladores con vistas
builder.Services.AddControllersWithViews();

// Agregamos servicios de sesión
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de sesión
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ? Agregamos servicios de autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";     // Redirigir al login si no está autenticado
        options.LogoutPath = "/Account/Logout";   // Para cerrar sesión
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Opcional: duración de la cookie
    });

var app = builder.Build();

// Configuración del pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();           // ?? Primero sesiones
app.UseAuthentication();    // ?? Luego autenticación
app.UseAuthorization();     // ? Finalmente autorización

// Ruta por defecto al Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
