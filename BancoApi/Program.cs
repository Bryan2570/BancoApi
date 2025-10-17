using BancoApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Cors", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200", "https://c769e0ac0420.ngrok-free.app/api/v1/client") 
            .AllowAnyHeader() 
            .AllowAnyMethod(); 
    });
});


builder.Services.AddDbContext<DbBancoPruebaContext>(options =>
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("dbSQL"));
    });

var app = builder.Build();


app.UseCors("Cors");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{                                            
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();                                                               
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
