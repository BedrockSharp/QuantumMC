using QuantumMC.Player;

namespace QuantumMC.Event.Impl
{
    public class PlayerJoinEvent : Event
    {
        public Player.Player Player { get; }
        public bool IsCancelled { get; set; }
        public string? JoinMessage { get; set; }

        public PlayerJoinEvent(Player.Player player, string? joinMessage)
        {
            Player = player;
            JoinMessage = joinMessage;
        }
    }
}
