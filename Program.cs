using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SisAlmacenProductos.Data;

var builder = WebApplication.CreateBuilder(args);

// Agregamos ApplicationDbContext con conexi�n a MySQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 23)) // Cambia si usas otra versi�n
    ));

// ✅ Agregamos servicio de cache en memoria
builder.Services.AddMemoryCache();

// Agregamos soporte para controladores con vistas
builder.Services.AddControllersWithViews();

// Agregamos servicios de sesi�n
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de sesi�n
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ? Agregamos servicios de autenticaci�n con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";     // Redirigir al login si no est� autenticado
        options.LogoutPath = "/Account/Logout";   // Para cerrar sesi�n
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Opcional: duraci�n de la cookie
    });

var app = builder.Build();

// Configuraci�n del pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();           // ?? Primero sesiones
app.UseAuthentication();    // ?? Luego autenticaci�n
app.UseAuthorization();     // ? Finalmente autorizaci�n

// Ruta por defecto al Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
