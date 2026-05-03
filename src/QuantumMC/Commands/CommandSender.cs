namespace QuantumMC.Commands
{
    public interface ICommandSender
    {
        string GetLanguage();
        void SendMessage(string message);
        Server GetServer();
        string GetName();
        int GetScreenLineHeight();
        void SetScreenLineHeight(int height);
    }
}