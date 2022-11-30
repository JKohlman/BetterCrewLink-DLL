using Microsoft.AspNetCore.Builder;

namespace BCLDLL
{
    public class API
    {
        public bool loaded = false;
        public API()
        {
            loaded = true;
            Serve();
        }

        static void Serve()
        {
            var app = WebApplication.Create();
            app.UseStaticFiles();
            app.MapGet("/", testDefault);
            app.MapGet("/data", getData);
            app.RunAsync("http://localhost:1234");
        }

        static string testDefault()
        {
            BetterCrewLink.Logger.LogDebug("");
            return "";
        }

        static string getData()
        {
            return "GAME DATA HERE";
        }

    }
}
