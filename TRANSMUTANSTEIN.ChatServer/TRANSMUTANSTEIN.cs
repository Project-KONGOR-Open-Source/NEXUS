namespace TRANSMUTANSTEIN.ChatServer;

public class TRANSMUTANSTEIN
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddHostedService<ChatService>();

        WebApplication app = builder.Build();

        app.UseHttpsRedirection();

        app.Run();
    }
}
