
using CentralITAssistant.Application.Abstractions.Persistence;
using CentralITAssistant.Application.Abstractions.Services;
using CentralITAssistant.Application.UseCases.Employees;
using CentralITAssistant.Infrastructure.Persistence.Context;
using CentralITAssistant.Infrastructure.Persistence.Repositories;
using CentralITAssistant.Infrastructure.Services.Email;
using CentralITAssistant.Infrastructure.Services.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Authorization
builder.Services.AddAuthorization();

// Entity Framework Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

builder.Services.AddScoped<
    IPendingEmployeeRegistrationRepository,
    PendingEmployeeRegistrationRepository>();

// Verification Code Service
var verificationSecret = builder.Configuration["Security:VerificationCodeSecret"]
    ?? throw new InvalidOperationException(
        "A chave Security:VerificationCodeSecret não foi configurada.");

builder.Services.AddSingleton<IVerificationCodeService>(
    _ => new VerificationCodeService(verificationSecret));

// Email Service - Gmail
var gmailAddress = builder.Configuration["Email:GmailAddress"]
    ?? throw new InvalidOperationException(
        "O endereço do Gmail não foi configurado.");

var gmailAppPassword = builder.Configuration["Email:GmailAppPassword"]
    ?? throw new InvalidOperationException(
        "A senha de aplicativo do Gmail não foi configurada.");

builder.Services.AddScoped<IEmailService>(
    _ => new GmailEmailService(gmailAddress, gmailAppPassword));

// Use Cases
builder.Services.AddScoped<RegisterEmployeeUseCase>();
builder.Services.AddScoped<VerifyEmployeeEmailUseCase>();

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
