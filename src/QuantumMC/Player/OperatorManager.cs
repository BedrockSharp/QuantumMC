using System.Text.Json;
using Serilog;

namespace QuantumMC.Player
{
    public class OperatorManager
    {
        private readonly string _filePath;
        private HashSet<string> _operators = new(StringComparer.OrdinalIgnoreCase);

        public OperatorManager(string dataFolder)
        {
            _filePath = Path.Combine(dataFolder, "ops.json");
            Load();
        }

        public void Load()
        {
            if (!File.Exists(_filePath))
            {
                _operators = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                return;
            }

            try
            {
                var json = File.ReadAllText(_filePath);
                var list = JsonSerializer.Deserialize<List<string>>(json);
                _operators = new HashSet<string>(list ?? new List<string>(), StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to load operators from {File}", _filePath);
            }
        }

        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(_operators.ToList(), new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to save operators to {File}", _filePath);
            }
        }

        public bool IsOp(string username) => _operators.Contains(username);

        public void AddOp(string username)
        {
            if (_operators.Add(username))
            {
                Save();
            }
        }

        public void RemoveOp(string username)
        {
            if (_operators.Remove(username))
            {
                Save();
            }
        }
    }
}
