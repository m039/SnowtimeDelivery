using Game1;
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

        [JSInvokable]
        public void OnTouchStart(int id, int x, int y)
        {
            InputSystem.TouchStarted(id, x, y);
        }

        [JSInvokable]
        public void OnTouchCancel(int id)
        {
            InputSystem.TouchCancel(id);
        }

        public class PlatformModule : IPlatformModule
        {
            readonly Bridge bridge;

            public PlatformModule(Bridge bridge)
            {
                this.bridge = bridge;
            }

            public string id => bridge.js.Invoke<string>("bridgePlatformId");

            public string language => bridge.js.Invoke<string>("bridgePlatformLanguage");
        }

        public class GameModule : IGameModule
        {
            readonly Bridge bridge;

            public GameModule(Bridge bridge)
            {
                this.bridge = bridge;
            }

            public VisibilityState visibilityState => BridgeExtensions.ParseVisibilityState(bridge.js.Invoke<string>("bridgeGameVisibilityState"));

            public Action<VisibilityState> onVisibilityStateCahged { get; set; }
        }

        public class DeviceModule : IDeviceModule
        {
            readonly Bridge bridge;

            public DeviceModule(Bridge bridge)
            {
                this.bridge = bridge;
            }
            
            public DeviceType type => BridgeExtensions.ParseDeviceType(bridge.js.Invoke<string>("bridgeDeviceType"));
        }

        public class Bridge : IBridge
        {
            readonly IPlatformModule _platform;

            readonly IGameModule _game;

            readonly Index _index;

            readonly IDeviceModule _device;

            public Bridge(Index index)
            {
                _index = index;
                _platform = new PlatformModule(this);
                _game = new GameModule(this);
                _device = new DeviceModule(this);
            }

            public IJSRuntime JsAsync => _index.JsRuntime;

            public IJSInProcessRuntime js => (IJSInProcessRuntime)_index.JsRuntime;

            public IPlatformModule platform => _platform;

            public IGameModule game => _game;

            public IDeviceModule device => _device;
        }
    }

    public static class BridgeExtensions
    {
        public static DeviceType ParseDeviceType(string value)
        {
            if (value == "tv")
            {
                return DeviceType.TV;
            } else if (value == "mobile")
            {
                return DeviceType.Mobile;
            } else if (value == "tablet")
            {
                return DeviceType.Tablet;
            } else
            {
                return DeviceType.Desktop;
            }
        }

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
