namespace QuantumMC.Commands.Default
{
    public class TpsCommand : CommandExecutor
    {
        private static double _tps = 20.0;
        private static DateTime _lastTick = DateTime.Now;
        private static int _tickCount = 0;

        public static void Tick()
        {
            _tickCount++;
            var now = DateTime.Now;
            double elapsed = (now - _lastTick).TotalSeconds;

            if (elapsed >= 1.0)
            {
                _tps = Math.Min(20.0, _tickCount / elapsed);
                _tickCount = 0;
                _lastTick = now;
            }
        }

        public static double GetTPS() => _tps;

        public override bool OnCommand(ICommandSender sender, Command command, string label, string[] args)
        {
            double tps = GetTPS();
            string quality = tps switch
            {
                >= 19 => "§aExcellent",
                >= 15 => "§eGood",
                >= 10 => "§6Fair",
                _      => "§cPoor"
            };

            sender.SendMessage($"§7Server TPS: {quality} §7({tps:F1} TPS)");
            return true;
        }
    }
}