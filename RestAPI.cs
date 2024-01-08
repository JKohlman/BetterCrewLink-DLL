using BCLDLL.Model;
using Il2CppSystem.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace BCLDLL
{
    public class RestAPI
    {
        public bool loaded = false;
        public RestAPI(string port)
        {
            Serve(port);
            loaded = true;
        }

        void Serve(string port)
        {
            var app = WebApplication.Create();
            app.MapGet("/status", this.GetStatus);
            app.MapGet("/data", this.GetData);
            app.RunAsync("http://localhost:" + port);
        }

        void GetStatus(HttpResponse response)
        {
            response.WriteAsync("OK");
        }

        void GetData(HttpResponse response)
        {
            try
            {
                response.WriteAsJsonAsync(new AmongUsState());
            }
            catch (System.Exception e)
            {
                response.StatusCode = 500;
                response.WriteAsync(e.ToString());
            }
        }
    }
}
