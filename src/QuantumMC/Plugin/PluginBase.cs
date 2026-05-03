using System.Threading.Tasks;
using QuantumMC.Event;
using QuantumMC.Command;

namespace QuantumMC.Plugin
{
    public abstract class PluginBase
    {
        public IPluginInfo Info { get; internal set; } = null!;
        public PluginLogger Logger { get; internal set; } = null!;

        public virtual Task OnEnable() => Task.CompletedTask;
        public virtual Task OnDisable() => Task.CompletedTask;

        protected void RegisterCommand(Command.Command command)
        {
            Server.Instance.CommandMap.Register(Info.Name, command);
        }

        protected void RegisterListener(Listener listener)
        {
            Server.Instance.PluginManager.EventManager.RegisterListeners(listener);
        }
    }
}
