using Microsoft.Xna.Framework.Media;

namespace Game1
{
    public interface IMusicPlayer
    {
        public void Play(Song song);

        public void Pause();

        public void Resume();
    }

    public class SimpleMusicPlayer : IMusicPlayer
    {
        public void Pause()
        {
            MediaPlayer.Pause();
        }

        public void Play(Song song)
        {
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(song);
        }

        public void Resume()
        {
            MediaPlayer.Resume();
        }
    }
}