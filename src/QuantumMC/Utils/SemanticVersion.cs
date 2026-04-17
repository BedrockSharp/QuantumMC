using System.Text.RegularExpressions;

namespace QuantumMC.Utils
{
    public class SemanticVersion
    {
        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }
        public string? Prerelease { get; }

        public SemanticVersion(int major, int minor, int patch, string? prerelease = null)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Prerelease = prerelease;
        }

        public static SemanticVersion Parse(string version)
        {
            var match = Regex.Match(version, @"^v?(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(?:-(?<prerelease>[0-9A-Za-z.-]+))?$");
            if (!match.Success)
                throw new ArgumentException("Invalid Semantic Version format", nameof(version));

            int major = int.Parse(match.Groups["major"].Value);
            int minor = int.Parse(match.Groups["minor"].Value);
            int patch = int.Parse(match.Groups["patch"].Value);
            string? prerelease = match.Groups["prerelease"].Success ? match.Groups["prerelease"].Value : null;

            return new SemanticVersion(major, minor, patch, prerelease);
        }

        public override string ToString()
        {
            string baseVersion = $"{Major}.{Minor}.{Patch}";
            return string.IsNullOrEmpty(Prerelease) ? baseVersion : $"{baseVersion}-{Prerelease}";
        }
    }
}
