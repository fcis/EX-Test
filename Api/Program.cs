using Api.Extensions;
using Api.Middleware;
using Application.Interfaces;
using Infrastructure.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add API layer services
builder.Services.AddApiServices(builder.Configuration);

// Add CurrentUserService as scoped
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use custom exception handling middleware
app.UseExceptionHandling();

app.UseHttpsRedirection();

// Use CORS
app.UseCors("CorsPolicy");

// Use Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();