namespace Dosiero.Configuration;

public static class PathExtensions
{
    extension(Path)
    {
        public static string ExpandUnixPath(string path)
        {
            if (!path.StartsWith('~'))
            {
                return path;
            }

            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, path.TrimStart('~', '/'));
        }
    }
}
