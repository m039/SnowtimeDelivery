using Microsoft.Xna.Framework.Input;
using System;

namespace Game1
{
    public static class InputSystem
    {
        public static bool IsResetButtonDown(KeyboardState keyboardState)
        {
            return Keyboard.GetState().IsKeyDown(Keys.F5) && !keyboardState.IsKeyDown(Keys.F5);
        }

        public static bool IsAnyButtonDown()
        {
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

        public static bool IsJumpButtonDown(KeyboardState keyboardState, GamePadState? gamePadState)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && !keyboardState.IsKeyDown(Keys.Space))
                return true;

            if (gamePadState != null && gamePadState.Value.IsButtonUp(Buttons.A)
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
            return Keyboard.GetState().IsKeyDown(Keys.Left)
#if !BLAZORGL
                || GamePad.GetState(0).IsButtonDown(Buttons.DPadLeft)
#endif
                ;
        }

        public static bool IsRightButtonDown()
        {
            return Keyboard.GetState().IsKeyDown(Keys.Right)
#if !BLAZORGL
                || GamePad.GetState(0).IsButtonDown(Buttons.DPadRight)
#endif
                ;
        }

        public static bool IsJumpButtonUp(KeyboardState keyboardState, GamePadState? gamePadState)
        {
            if (!Keyboard.GetState().IsKeyDown(Keys.Space) && keyboardState.IsKeyDown(Keys.Space))
            {
                return true;
            }

            if (gamePadState != null && gamePadState.Value.IsButtonDown(Buttons.A)
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
            return Keyboard.GetState().IsKeyDown(Keys.Down)
#if !BLAZORGL
                || GamePad.GetState(0).IsButtonDown(Buttons.DPadDown)
#endif
                ;
        }
    }
}