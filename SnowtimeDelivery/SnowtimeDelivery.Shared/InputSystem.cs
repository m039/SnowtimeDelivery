using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System;
using System.Collections.Generic;

namespace Game1
{
    public static class InputSystem
    {
        public static Rectangle buttonLeftInputRectangle;
        public static Rectangle buttonRightInputRectangle;
        public static Rectangle buttonDownInputRectangle;
        public static Rectangle buttonUpInputRectangle;

        private static bool s_ButtonDownState;

        private static KeyboardState s_KeyboardState;

        private static GamePadState? s_GamePadState;

        private static List<(int, Vector2)> s_TouchPressed = new();

        private static bool CustomTouchProcessing = true;

        public static bool IsResetButtonDown()
        {
            return Keyboard.GetState().IsKeyDown(Keys.F5) && !s_KeyboardState.IsKeyDown(Keys.F5);
        }

        public static bool IsAnyButtonDown()
        {
            if (Game1.isMobile)
            {
                if (s_TouchPressed.Count > 0)
                {
                    return true;
                }

                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                {
                    return true;
                }

                var touchCol = TouchPanel.GetState();

                foreach (var touch in touchCol)
                {
                    if (touch.State == TouchLocationState.Pressed)
                    {
                        return true;
                    }
                }
            }

            if (Keyboard.GetState().GetPressedKeyCount() != 0
#if !BLAZORGL
                    || GamePad.GetState(0).IsButtonDown(Buttons.A)
#endif
                    )
            {
                return true;
            }

            return false;
        }

        public static GamePadState? GetCurrentGamePadState()
        {
#if !BLAZORGL
            return GamePad.GetState(0);
#else
            return null;
#endif
        }

        public static GamePadThumbSticks? GetCurrentGamePadThumbSticks()
        {
#if !BLAZORGL
            return GamePad.GetState(0).ThumbSticks;
#else
            return null;
#endif
        }

        public static bool IsJumpButtonDown()
        {
            if (IsButtonDown(buttonUpInputRectangle, out bool result) && result && !s_ButtonDownState)
                return true;

            if (Keyboard.GetState().IsKeyDown(Keys.Space) && !s_KeyboardState.IsKeyDown(Keys.Space))
                return true;

            if (s_GamePadState != null && s_GamePadState.Value.IsButtonUp(Buttons.A)
#if !BLAZORGL
                && GamePad.GetState(0).IsButtonDown(Buttons.A)
#endif
                )
            {
                return true;
            }

            return false;
        }

        public static bool IsLeftButtonDown()
        {
            if (IsButtonDown(buttonLeftInputRectangle, out bool result) && result)
                return true;

            return Keyboard.GetState().IsKeyDown(Keys.Left)
#if !BLAZORGL
                || GamePad.GetState(0).IsButtonDown(Buttons.DPadLeft)
#endif
                ;
        }

        public static bool IsRightButtonDown()
        {
            if (IsButtonDown(buttonRightInputRectangle, out bool result) && result)
                return true;

            return Keyboard.GetState().IsKeyDown(Keys.Right)
#if !BLAZORGL
                || GamePad.GetState(0).IsButtonDown(Buttons.DPadRight)
#endif
                ;
        }

        public static bool IsJumpButtonUp()
        {
            if (!IsButtonDown(buttonUpInputRectangle, out bool result) && result && s_ButtonDownState)
            {
                return true;
            }

            if (!Keyboard.GetState().IsKeyDown(Keys.Space) && s_KeyboardState.IsKeyDown(Keys.Space))
            {
                return true;
            }

            if (s_GamePadState != null && s_GamePadState.Value.IsButtonDown(Buttons.A)
#if !BLAZORGL
                && GamePad.GetState(0).IsButtonUp(Buttons.A)
#endif
                )
            {
                return true;
            }

            return false;
        }

        public static bool IsDownButtonDown()
        {
            if (IsButtonDown(buttonDownInputRectangle, out bool result))
            {
                return result;
            }

            return Keyboard.GetState().IsKeyDown(Keys.Down)
#if !BLAZORGL
                || GamePad.GetState(0).IsButtonDown(Buttons.DPadDown)
#endif
                ;
        }

        static bool IsButtonDown(Rectangle rectangle, out bool result)
        {
            if (!Game1.isMobile) {
                result = false;
                return false;
            }

            result = false;

            if (s_TouchPressed.Count > 0)
            {
                foreach (var touch in s_TouchPressed)
                {
                    if (rectangle.Contains(touch.Item2))
                    {
                        result = true;
                        return true;
                    }
                }
            }

            if (rectangle.Contains(Mouse.GetState().Position) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                return true;

            var touchCol = TouchPanel.GetState();

            foreach (var touch in touchCol)
            {
                if (touch.State == TouchLocationState.Pressed && rectangle.Contains(touch.Position))
                {
                    result = true;
                    break;
                }
            }

            return true;
        }

        public static void Update()
        {
            // Cache the input state in order to find presses and releases.

            if (IsButtonDown(buttonUpInputRectangle, out bool result))
            {
                s_ButtonDownState = result;
            }

            s_KeyboardState = Keyboard.GetState();
            s_GamePadState = InputSystem.GetCurrentGamePadState();
        }

        public static void TouchStart(int id, int x, int y)
        {
            if (!CustomTouchProcessing)
                return;

            for (int i = 0; i < s_TouchPressed.Count; i++)
            {
                if (s_TouchPressed[i].Item1 == id)
                {
                    s_TouchPressed[i] = (id, new Vector2(x, y));
                    return;
                }
            }

            s_TouchPressed.Add((id, new Vector2(x, y)));
        }

        public static void TouchCancel()
        {
            if (!CustomTouchProcessing)
                return;

            s_TouchPressed.Clear();
        }

        public static string GetLog()
        {
            var str = "";

            for (int i = 0; i < s_TouchPressed.Count; i++)
            {
                var t = s_TouchPressed[i];
                str += $"{i} => id: {t.Item1}, p: {t.Item2}\n";
            }

            return str;
        }
    }
}