using Serilog;

namespace QuantumMC
{
    public class QuantumMC
    {
        public static void Main(string[] args)
        {
        Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u4}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            try
            {
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