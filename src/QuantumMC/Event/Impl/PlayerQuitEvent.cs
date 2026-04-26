using QuantumMC.Player;

namespace QuantumMC.Event.Impl
{
    public class PlayerQuitEvent : Event
    {
        public Player.Player Player { get; }
        public bool IsCancelled { get; set; }
        public string? QuitMessage { get; set; }

        public PlayerQuitEvent(Player.Player player, string? quitMessage)
        {
            Player = player;
            QuitMessage = quitMessage;
        }
    }
}
