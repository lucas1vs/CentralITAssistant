
using CentralITAssistant.Application.Abstractions.Persistence;
using CentralITAssistant.Application.Abstractions.Services;
using CentralITAssistant.Application.UseCases.Employees;
using CentralITAssistant.Application.UseCases.WhatsApp;
using CentralITAssistant.Infrastructure.Persistence.Context;
using CentralITAssistant.Infrastructure.Persistence.Repositories;
using CentralITAssistant.Infrastructure.Services.AI;
using CentralITAssistant.Infrastructure.Services.Email;
using CentralITAssistant.Infrastructure.Services.Security;
using CentralITAssistant.Infrastructure.Services.Telegram;
using CentralITAssistant.Infrastructure.Services.WhatsAppApp;
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

builder.Services.AddScoped<
    IPendingWhatsAppVerificationRepository,
    PendingWhatsAppVerificationRepository>();

builder.Services.AddScoped<
    IWhatsAppContactRepository,
    WhatsAppContactRepository>();

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

// WhatsApp Service
builder.Services.AddScoped<IWhatsAppService, WhatsAppService>();

// Telegram HTTP Client
builder.Services.AddHttpClient("Telegram", client =>
{
    client.BaseAddress = new Uri("https://api.telegram.org/");
});

// Telegram Services
builder.Services.AddScoped<IMessageChannel>(serviceProvider =>
{
    var httpClientFactory = serviceProvider
        .GetRequiredService<IHttpClientFactory>();

    var configuration = serviceProvider
        .GetRequiredService<IConfiguration>();

    var botToken = configuration["Telegram:BotToken"]
        ?? throw new InvalidOperationException(
            "O token do bot do Telegram não foi configurado.");

    var httpClient = httpClientFactory.CreateClient("Telegram");

    return new TelegramBotService(httpClient, botToken);
});

// Telegram Background Service
builder.Services.AddHostedService<TelegramPollingService>();

// Meta AI Service
var metaApiKey = builder.Configuration["Meta:ApiKey"]
    ?? throw new InvalidOperationException(
        "A chave Meta:ApiKey não foi configurada.");

var metaModel = builder.Configuration["Meta:Model"]
    ?? "muse-spark-1.3";

// Meta HTTP Client
builder.Services.AddHttpClient("MetaAI");

// AI Service
builder.Services.AddScoped<IAiService>(serviceProvider =>
{
    var httpClientFactory = serviceProvider
        .GetRequiredService<IHttpClientFactory>();

    var httpClient = httpClientFactory.CreateClient("MetaAI");

    return new MetaAiService(
        httpClient,
        metaApiKey,
        metaModel);
});

// Employee Use Cases
builder.Services.AddScoped<RegisterEmployeeUseCase>();
builder.Services.AddScoped<VerifyEmployeeEmailUseCase>();
builder.Services.AddScoped<ResendEmployeeVerificationCodeUseCase>();

// WhatsApp Use Cases
builder.Services.AddScoped<RequestWhatsAppVerificationUseCase>();
builder.Services.AddScoped<VerifyWhatsAppNumberUseCase>();
builder.Services.AddScoped<ResendWhatsAppVerificationCodeUseCase>();

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
