var builder = WebApplication.CreateBuilder(args);

// Agregar los servicios a la colección de servicios (como controladores)
builder.Services.AddControllers().AddNewtonsoftJson();

// Define la política de CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins("http://localhost:8100")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});
// Agregar el servicio de Swagger para documentación
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecificOrigins");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
