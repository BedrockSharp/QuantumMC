using QuantumMC.Player;

namespace QuantumMC.Event.Impl
{
    public class PlayerKickEvent : Event
    {
        public Player.Player Player { get; }
        public bool IsCancelled { get; set; }
        public string? KickMessage { get; set; }

        public PlayerKickEvent(Player.Player player, string? kickMessage)
        {
            Player = player;
            KickMessage = kickMessage;
        }
    }
}
