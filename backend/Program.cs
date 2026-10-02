using onboardingKycApi.Service;

var builder = WebApplication.CreateBuilder(args);

//Configuración de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<IKycService, KycService>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

//entorno de desarrollo
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//aplicar CORS
app.UseCors("AllowReactApp");

//redirección HTTPS
app.UseHttpsRedirection();

//carpeta de archivos
app.UseStaticFiles();

//seguridad yrutas
app.UseAuthorization();
app.MapControllers();

app.Run();