using ClosedXML.Excel;
using ManejoPresupuestoApp.Models;
using ManejoPresupuestoApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

//Se crea una autenticación a nivel de aplicación
var politicaUsuariosAutenticados = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
// Add services to the container.
builder.Services.AddControllersWithViews(opciones =>
{
    opciones.Filters.Add(new AuthorizeFilter(politicaUsuariosAutenticados));    
});

//Idiomas
builder.Services.AddLocalization(opt => { opt.ResourcesPath = "Resources"; } );
builder.Services.AddMvc()
        .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
        .AddDataAnnotationsLocalization();

builder.Services.AddTransient<IServiciosUsuarios, ServiciosUsuarios>();
builder.Services.AddTransient<IRepositoriosTiposCuentas, RepositoriosTiposCuentas>();
builder.Services.AddTransient<IRepositoriosCuentas, RepositoriosCuentas>();
builder.Services.AddTransient<IRepositoriosCategorias, RepositoriosCategorias>();
builder.Services.AddTransient<IRepositoriosTransacciones, RepositoriosTransacciones>();
builder.Services.AddTransient<IRepositorioUsuarios, RepositorioUsuarios>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<IServiciosReporte, ServiciosReporte>();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddTransient<IXLWorkbook,XLWorkbook>();
builder.Services.AddTransient<IUserStore<Usuario>,UsuarioStore>();
builder.Services.AddTransient<SignInManager<Usuario>>();
builder.Services.AddIdentityCore<Usuario>(opciones =>
{
    opciones.Password.RequireNonAlphanumeric = false;
});
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
        options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
    }
).AddCookie(IdentityConstants.ApplicationScheme, opciones =>
{
    opciones.LoginPath = "/usuarios/Login";
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

//var supportedCultures = new[] { new CultureInfo("es-ES") };
//app.UseRequestLocalization(new RequestLocalizationOptions()
//{
//    DefaultRequestCulture = new RequestCulture("es-ES"),
//    SupportedCultures = supportedCultures,
//    SupportedUICultures = supportedCultures
//});


var supportedCultures = new[] { "es-ES", "en-US" };
var localizationOptions = new RequestLocalizationOptions().SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Transacciones}/{action=Index}/{id?}");

app.Run();
