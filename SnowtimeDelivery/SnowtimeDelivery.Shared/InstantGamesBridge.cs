namespace InstantGamesBridge
{
    public enum VisibilityState
    {
        Hidden, Visible
    }

    public interface IPlatformModule
    {
        string id { get; }

        string language { get; }
    }

    public interface IGameModule
    {
        VisibilityState visibilityState { get; }

        System.Action<VisibilityState> onVisibilityStateCahged { get; set; }
    }

    public interface IBridge
    {
        IPlatformModule platform { get; }

        IGameModule game { get; }
    }
}