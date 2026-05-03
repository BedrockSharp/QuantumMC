namespace QuantumMC.Utils
{
    public static class TextFormat
    {
        public const char Escape = '§';

        public const string Black = "§0";
        public const string DarkBlue = "§1";
        public const string DarkGreen = "§2";
        public const string DarkAqua = "§3";
        public const string DarkRed = "§4";
        public const string DarkPurple = "§5";
        public const string Gold = "§6";
        public const string Gray = "§7";
        public const string DarkGray = "§8";
        public const string Blue = "§9";
        public const string Green = "§a";
        public const string Aqua = "§b";
        public const string Red = "§c";
        public const string LightPurple = "§d";
        public const string Yellow = "§e";
        public const string White = "§f";
        public const string MinecoinGold = "§g";

        public const string Obfuscated = "§k";
        public const string Bold = "§l";
        public const string Italic = "§o";
        public const string Reset = "§r";

        public static string Clean(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            var result = new System.Text.StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == Escape)
                {
                    i++;
                    continue;
                }
                result.Append(text[i]);
            }
            return result.ToString();
        }
        
        public static string Colorize(string text)
        {
            return text.Replace('&', Escape);
        }

        private static readonly Dictionary<char, string> AnsiCodes = new()
        {
            ['0'] = "\x1b[30m",
            ['1'] = "\x1b[34m",
            ['2'] = "\x1b[32m",
            ['3'] = "\x1b[36m",
            ['4'] = "\x1b[31m",
            ['5'] = "\x1b[35m",
            ['6'] = "\x1b[33m",
            ['7'] = "\x1b[37m",
            ['8'] = "\x1b[90m",
            ['9'] = "\x1b[94m",
            ['a'] = "\x1b[92m",
            ['b'] = "\x1b[96m",
            ['c'] = "\x1b[91m",
            ['d'] = "\x1b[95m",
            ['e'] = "\x1b[93m",
            ['f'] = "\x1b[97m",
            ['g'] = "\x1b[33m",
            ['k'] = "\x1b[5m",
            ['l'] = "\x1b[1m",
            ['o'] = "\x1b[3m",
            ['r'] = "\x1b[0m"
        };

        public static string ToAnsi(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var result = new System.Text.StringBuilder();
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == Escape && i + 1 < text.Length)
                {
                    char code = char.ToLower(text[i + 1]);
                    if (AnsiCodes.TryGetValue(code, out var ansi))
                    {
                        result.Append(ansi);
                        i++;
                        continue;
                    }
                }
                result.Append(text[i]);
            }
            result.Append("\x1b[0m");
            return result.ToString();
        }
    }
}
