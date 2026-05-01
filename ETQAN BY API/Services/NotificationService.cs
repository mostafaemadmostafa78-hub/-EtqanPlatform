using Etqan.Hubs;
using ETQAN.API.Data;
using ETQAN_BY_API.Model;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.SignalR;
using MimeKit;
using MimeKit.Utils;

public class NotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IConfiguration _config; 

    public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext, IConfiguration config)
    {
        _context = context;
        _hubContext = hubContext;
        _config = config;
    }

    public async Task SendNotificationAsync(string userId, string title, string message, string type, string? actionUrl = null)
    {
        var user = await _context.Users.FindAsync(userId);

        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            NotificationType = type,
            ActionUrl = actionUrl,
            CreatedAt = DateTime.Now
        };
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        await _hubContext.Clients.Group(userId).SendAsync("ReceiveNotification", new
        {
            title = notification.Title,
            message = notification.Message,
            type = notification.NotificationType
        });

        if (user != null && !string.IsNullOrEmpty(user.Email))
        {
            await SendEmailAsync(user.Email, title, message);
        }
    }

    private async Task SendEmailAsync(string email, string subject, string body, string? actionUrl = null)
    {
        var emailSettings = _config.GetSection("EmailSettings");
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("منصة إتقان", emailSettings["Email"]));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = subject;

        var bodyBuilder = new BodyBuilder();

        bodyBuilder.HtmlBody = $@"
    <div style='direction: rtl; font-family: ""Tahoma"", sans-serif; background-color: #ffffff; padding: 20px;'>
        <div style='max-width: 600px; margin: 0 auto; border: 1px solid #f0f0f0; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 10px rgba(0,0,0,0.02);'>
            
            
            <div style='background-color: #ffffff; padding: 25px; text-align: center; border-bottom: 2px solid #A9D1DB;'>
                <span style='color: #40798C; font-size: 28px; font-weight: bold; letter-spacing: 1px;'>إتقان | ETQAN</span>
                <p style='color: #75A5B3; margin: 5px 0 0 0; font-size: 12px;'>كل شيءٍ بإتقان</p>
            </div>

            <div style='padding: 45px 30px; background-color: #ffffff; text-align: center;'>
                <h2 style='color: #40798C; font-size: 24px; margin-bottom: 20px;'>{subject}</h2>
                
                <div style='background-color: #f9fbfc; border-right: 4px solid #75A5B3; padding: 25px; margin: 25px 0; text-align: right;'>
                    <p style='color: #40798C; font-size: 17px; line-height: 1.8; margin: 0;'>{body}</p>
                </div>

                {(string.IsNullOrEmpty(actionUrl) ? "" : $@"
                    <div style='margin-top: 35px;'>
                        <a href='https://etqan-project.com{actionUrl}' 
                           style='background-color: #40798C; color: #ffffff; padding: 14px 35px; text-decoration: none; border-radius: 4px; font-weight: bold; display: inline-block;'>
                           عرض التفاصيل في المنصة
                        </a>
                    </div>
                ")}
            </div>

     
            <div style='background-color: #40798C; padding: 30px 20px; text-align: center;'>
                <p style='color: #ffffff; font-size: 13px; margin-bottom: 10px;'>
                    منصة متكاملة تجمع الحرفيين والعملاء في مكان واحد
                </p>
                <div style='width: 40px; height: 1px; background-color: #A9D1DB; margin: 15px auto;'></div>
                <p style='color: #A9D1DB; font-size: 11px; margin: 0;'>
                    © جميع الحقوق محفوظة لمنصة إتقان - {DateTime.Now.Year}
                </p>
            </div>
        </div>
    </div>";

        emailMessage.Body = bodyBuilder.ToMessageBody();

        using (var client = new SmtpClient())
        {
            try
            {
                await client.ConnectAsync(emailSettings["Host"], int.Parse(emailSettings["Port"]), MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(emailSettings["Email"], emailSettings["Password"]);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
            catch { /* Handle error */ }
        }
    }

}

