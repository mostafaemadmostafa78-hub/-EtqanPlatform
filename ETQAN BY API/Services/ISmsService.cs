public interface ISmsService
{
    Task<bool> SendSmsAsync(string mobileNumber, string message);
}

// مثال بسيط (Mock) للتجربة:
public class MockSmsService : ISmsService
{
    public Task<bool> SendSmsAsync(string mobileNumber, string message)
    {
        // هنا بتنادي الـ API بتاع شركة الـ SMS
        Console.WriteLine($"إرسال رسالة لـ {mobileNumber}: {message}");
        return Task.FromResult(true);
    }
}