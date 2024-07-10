using InstantGamesBridge;
using Microsoft.JSInterop;
using Microsoft.Xna.Framework;
using System;
using System.IO;

namespace SnowtimeDelivery.Pages
{
    public partial class Index
    {
        Game1.Game1 _game;

        Bridge _bride;

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (firstRender)
            {
                JsRuntime.InvokeAsync<object>("initRenderJS", DotNetObjectReference.Create(this));
            }
        }

        const bool Quet = true;

        [JSInvokable]
        public void TickDotNet()
        {
            // init game
            if (_game == null)
            {
                var previousOut = Console.Out;

                if (Quet)
                {
                    Console.SetOut(TextWriter.Null);
                }

                _bride = new Bridge(this);

                _game = new Game1.Game1(_bride);
                _game.onStart += () => Console.SetOut(previousOut);
                _game.Run();
            }

            // run gameloop
            _game.Tick();
        }

        [JSInvokable]
        public void OnVisibilityStateChanged(string visibilityState)
        {
            _bride.game.onVisibilityStateCahged?.Invoke(BridgeExtensions.ParseVisibilityState(visibilityState));
        }

        public class PlatformModule : IPlatformModule
        {
            readonly Bridge bridge;

            public PlatformModule(Bridge bridge)
            {
                this.bridge = bridge;
            }

            public string id => bridge.JsSync.Invoke<string>("bridgePlatformId");

            public string language => bridge.JsSync.Invoke<string>("bridgePlatformLanguage");
        }

        public class GameModule : IGameModule
        {
            readonly Bridge bridge;

            public GameModule(Bridge bridge)
            {
                this.bridge = bridge;
            }

            public VisibilityState visibilityState => BridgeExtensions.ParseVisibilityState(bridge.JsSync.Invoke<string>("bridgeGameVisibilityState"));

            public Action<VisibilityState> onVisibilityStateCahged { get; set; }
        }

        public class Bridge : IBridge
        {
            readonly IPlatformModule _platform;

            readonly IGameModule _game;

            readonly Index _index;

            public Bridge(Index index)
            {
                _index = index;
                _platform = new PlatformModule(this);
                _game = new GameModule(this);
            }

            public IJSRuntime JsAsync => _index.JsRuntime;

            public IJSInProcessRuntime JsSync => (IJSInProcessRuntime)_index.JsRuntime;

            public IPlatformModule platform => _platform;

            public IGameModule game => _game;
        }
    }

    public static class BridgeExtensions
    {
        public static VisibilityState ParseVisibilityState(string value)
        {
            if (value == "visible")
            {
                return VisibilityState.Visible;
            }
            else
            {
                return VisibilityState.Hidden;
            }
        }
    }
}
