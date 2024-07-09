using Microsoft.Xna.Framework;
using System.IO;

namespace Game1
{
    public static class IOSystem
    {
        public static string ReadFile(string file)
        {
            using (var streamReader = new StreamReader(TitleContainer.OpenStream(file)))
            {
                return streamReader.ReadToEnd();
            }
        }
    }
}