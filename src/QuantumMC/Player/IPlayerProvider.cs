namespace QuantumMC.Player
{
    public interface IPlayerProvider
    {
        void SavePlayer(Player player);
        bool LoadPlayer(Player player);
    }
}
