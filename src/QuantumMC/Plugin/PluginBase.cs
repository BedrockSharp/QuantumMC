using System.Threading.Tasks;
using QuantumMC.Event;
using QuantumMC.Commands;

namespace QuantumMC.Plugin
{
    public abstract class PluginBase
    {
        public IPluginInfo Info { get; internal set; } = null!;
        public PluginLogger Logger { get; internal set; } = null!;

        public virtual Task OnEnable() => Task.CompletedTask;
        public virtual Task OnDisable() => Task.CompletedTask;

        protected void RegisterCommand(string commandName, CommandExecutor executor)
        {
            CommandRegistry.Register(commandName, executor);
        }

        protected void RegisterListener(Listener listener)
        {
            Server.Instance.PluginManager.EventManager.RegisterListeners(listener);
        }
    }
}
