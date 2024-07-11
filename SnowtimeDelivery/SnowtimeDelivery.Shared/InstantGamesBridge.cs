using System.Collections.Generic;

namespace InstantGamesBridge
{
    public enum VisibilityState
    {
        Hidden, Visible
    }

    public enum DeviceType
    {
        Desktop, Mobile, Tablet, TV
    }

    public enum PlatformMessage
    {
        GameReady,
        InGameLoadingStarted,
        InGameLoadingStopped,
        GameplayStarted,
        GameplayStopped,
        PlayerGotAchievement
    }

    public interface IDeviceModule
    {
        public DeviceType type { get; }
    }

    public interface IPlatformModule
    {
        string id { get; }

        string language { get; }

        void sendMessage(PlatformMessage message);
    }

    public interface IGameModule
    {
        VisibilityState visibilityState { get; }

        System.Action<VisibilityState> onVisibilityStateCahged { get; set; }
    }

    public interface ILeaderboardModule
    {
        bool isSupported { get; }
        void setScore(Dictionary<string, object> options);
    }

    public interface IBridge
    {
        IPlatformModule platform { get; }

        IGameModule game { get; }

        IDeviceModule device { get; }

        ILeaderboardModule leaderboard { get; }
    }
}