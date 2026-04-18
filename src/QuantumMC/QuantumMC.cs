using Serilog;

namespace QuantumMC
{
    public class QuantumMC
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ThreadName", "Main Thread")
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss}] [{ThreadName}] [{Level:u4}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            try
            {
                Log.Information("Starting QuantumMC v{Version}...", Utils.Version.Current);
                
                var server = new Server();
                server.Start();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Server crashed!");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}