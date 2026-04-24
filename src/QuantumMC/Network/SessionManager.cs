using System.Collections.Concurrent;
using System.Net;
using Serilog;

namespace QuantumMC.Network
{
    public class SessionManager
    {
        private readonly ConcurrentDictionary<IPEndPoint, PlayerSession> _sessions = new();
        private long _entityIdCounter = 1;
        public long GetNextEntityId() => Interlocked.Increment(ref _entityIdCounter);

        public int OnlineCount => _sessions.Count;

        public void AddSession(IPEndPoint endPoint, PlayerSession session)
        {
            _sessions.TryAdd(endPoint, session);
            Log.Information("Session added for {EndPoint} (Online: {Count})", endPoint, OnlineCount);
        }

        public void RemoveSession(IPEndPoint endPoint)
        {
            if (_sessions.TryRemove(endPoint, out _))
            {
                Log.Debug("Session removed for {EndPoint} (Online: {Count})", endPoint, OnlineCount);
            }
        }

        public PlayerSession? GetSession(IPEndPoint endPoint)
        {
            _sessions.TryGetValue(endPoint, out var session);
            return session;
        }

        public IEnumerable<PlayerSession> GetAllSessions()
        {
            return _sessions.Values;
        }
    }
}
