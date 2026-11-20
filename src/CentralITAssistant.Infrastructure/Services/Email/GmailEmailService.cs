
using CentralITAssistant.Application.Abstractions.Services;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Net.Mail;

namespace CentralITAssistant.Infrastructure.Services.Email;

public sealed class GmailEmailService : IEmailService
{
    private const string SmtpHost = "smtp.gmail.com";
    private const int SmtpPort = 587;

    private readonly string _senderEmail;
    private readonly string _appPassword;

    public GmailEmailService(string senderEmail, string appPassword)
    {
        if (string.IsNullOrWhiteSpace(senderEmail))
            throw new ArgumentException("O e-mail remetente não foi informado.", nameof(senderEmail));

        if (string.IsNullOrWhiteSpace(appPassword))
            throw new ArgumentException("A senha de aplicativo não foi informada.", nameof(appPassword));

        _senderEmail = senderEmail.Trim();
        _appPassword = appPassword.Replace(" ", string.Empty).Trim();
    }

    public async Task SendEmailVerificationCodeAsync(
        string email,
        string verificationCode,
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress("Central IT Assistant", _senderEmail));
        message.To.Add(MailboxAddress.Parse(email));

        message.Subject = "Código de verificação - Central IT Assistant";

        var body = new BodyBuilder
        {
            TextBody = $"""
                Olá!

                Recebemos uma solicitação de cadastro no Central IT Assistant.

                Seu código de verificação é: {verificationCode}

                O código expira em 5 minutos.

                Caso não tenha solicitado este cadastro, ignore este e-mail.
                """,

            HtmlBody = $"""
                <h2>Central IT Assistant</h2>
                <p>Recebemos uma solicitação de cadastro.</p>
                <p>Seu código de verificação é:</p>
                <h1>{verificationCode}</h1>
                <p>O código expira em <strong>5 minutos</strong>.</p>
                <p>Caso não tenha solicitado este cadastro, ignore este e-mail.</p>
                """
        };

        message.Body = body.ToMessageBody();

        using var smtpClient = new MailKit.Net.Smtp.SmtpClient();

        await smtpClient.ConnectAsync(
            SmtpHost,
            SmtpPort,
            SecureSocketOptions.StartTls,
            cancellationToken);

        await smtpClient.AuthenticateAsync(
            _senderEmail,
            _appPassword,
            cancellationToken);

        await smtpClient.SendAsync(message, cancellationToken);

        await smtpClient.DisconnectAsync(true, cancellationToken);
    }
}
