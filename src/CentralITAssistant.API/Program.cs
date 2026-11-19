
using CentralITAssistant.Application.Abstractions.Persistence;
using CentralITAssistant.Application.Abstractions.Services;
using CentralITAssistant.Infrastructure.Persistence.Repositories;
using CentralITAssistant.Infrastructure.Persistence.Context;
using CentralITAssistant.Infrastructure.Services.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Entity Framework Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

// Verification Code Service
var verificationSecret = builder.Configuration["Security:VerificationCodeSecret"]
    ?? throw new InvalidOperationException(
        "A chave Security:VerificationCodeSecret não foi configurada.");

builder.Services.AddSingleton<IVerificationCodeService>(
    _ => new VerificationCodeService(verificationSecret));

builder.Services.AddScoped<
    IPendingEmployeeRegistrationRepository,
    PendingEmployeeRegistrationRepository>();

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
