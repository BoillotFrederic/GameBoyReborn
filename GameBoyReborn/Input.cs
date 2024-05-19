﻿// ---------
// Input set
// ---------
using Raylib_cs;
using SharpDX.XInput;

#pragma warning disable CA2211

namespace GameBoyReborn
{
    public class Input
    {
        #region Input handle

        // Set
        private readonly static int Gamepad = 0;

        // Keys/buttons set/state
        // ----------------------

        // D-PAD
        public static bool DPadDown = false;
        public static bool DPadLeft = false;
        public static bool DPadRight = false;
        public static bool DPadUp = false;

        // XABY-PAD
        public static bool XabyPadA = false;
        public static bool XabyPadB = false;
        public static bool XabyPadX = false;
        public static bool XabyPadY = false;

        // MIDDLE-PAD
        public static bool MiddlePadLeft = false;
        public static bool MiddlePadRight = false;
        public static bool MiddlePadCenter = false;

        // TRIGGER-PAD
        public static int DeadZoneTrigger = 40;
        public static bool TriggerPadLB = false;
        public static bool TriggerPadRB = false;
        public static bool TriggerPadLT = false;
        public static bool TriggerPadRT = false;

        // AXIS-PAD
        public static int DeadZoneStickLeft = 40;
        public static int DeadZoneStickRight = 40;
        public static bool AxisLeftPadDown = false;
        public static bool AxisLeftPadLeft = false;
        public static bool AxisLeftPadRight = false;
        public static bool AxisLeftPadUp = false;
        public static bool AxisRightPadDown = false;
        public static bool AxisRightPadLeft = false;
        public static bool AxisRightPadRight = false;
        public static bool AxisRightPadUp = false;
        public static bool AxisLS = false;
        public static bool AxisRS = false;

        // ADDITIONAL KEYS
        public static bool KeyAlt = false;
        public static bool KeyEnter = false;
        public static bool KeyM = false;
        public static bool KeyP = false;
        public static bool KeyG = false;
        public static bool KeyC = false;
        public static bool KeyS = false;

        // Mouse
        public static bool MouseLeftClick = false;
        public static bool MouseLeftDoubleClick = false;

        // Double click handle
        // -------------------

        private static bool MouseDown = false;
        private static int MouseClickCount = 0;
        private static float LastClickTime = 0;

        private static bool DoubleClick()
        {
            if (MouseLeftClick && !MouseDown)
            {
                MouseDown = true;

                if (Raylib.GetTime() - LastClickTime <= 0.3f)
                {
                    MouseClickCount++;
                    if (MouseClickCount == 2)
                    {
                        MouseClickCount = 0;
                        return true;
                    }
                }
                else
                MouseClickCount = 1;

                LastClickTime = (float)Raylib.GetTime();
            }
            else if (!MouseLeftClick)
            MouseDown = false;

            return false;
        }

        // Button press interpreter
        // ------------------------

        // Pressed
        private static readonly Dictionary<string, bool> ButtonPressed = new();
        private static readonly Dictionary<string, bool> ButtonPressedDisable = new();

        public static bool Pressed(string buttonName, bool buttonStat)
        {
            if (!ButtonPressed.ContainsKey(buttonName))
            {
                ButtonPressed[buttonName] = false;
                ButtonPressedDisable[buttonName] = false;
            }

            if(ButtonPressedDisable[buttonName])
            ButtonPressed[buttonName] = false;

            if (!buttonStat)
            {
                ButtonPressed[buttonName] = false;
                ButtonPressedDisable[buttonName] = false;
            }

            else if (buttonStat && !ButtonPressedDisable[buttonName])
            {
                ButtonPressed[buttonName] = true;
                ButtonPressedDisable[buttonName] = true;
            }

            return ButtonPressed[buttonName];
        }

        // Auto repeat
        private static readonly Dictionary<string, float> ElapsedTime = new();

        public static bool Repeat(string buttonName, bool buttonStat, float repeatTime)
        {
            if (!ElapsedTime.ContainsKey(buttonName))
            ElapsedTime[buttonName] = 200;

            if (!buttonStat)
            {
                ElapsedTime[buttonName] = 200;
                return false;
            }

            ElapsedTime[buttonName] += Raylib.GetFrameTime();
            if (ElapsedTime[buttonName] >= repeatTime)
            {
                ElapsedTime[buttonName] = 0;
                return true;
            }

            return false;
        }

        // Key or pad dectection
        // ---------------------

        public static bool IsPad = false;
        private static bool FirstLoop = true;

        private static void KeyOrPad(bool GamePadAvailable)
        {
            if (GamePadAvailable)
            {
                if (FirstLoop)
                IsPad = true;

                // Toogle pad/keyboard
                foreach (GamepadButton btnTest in (GamepadButton[])Enum.GetValues(typeof(GamepadButton)))
                if (Raylib.IsGamepadButtonPressed(Gamepad, btnTest))
                IsPad = true;

                foreach (GamepadAxis axiTest in (GamepadAxis[])Enum.GetValues(typeof(GamepadAxis)))
                {
                    float movement = Raylib.GetGamepadAxisMovement(Gamepad, axiTest);
                    GamepadAxis trigLeft = GamepadAxis.GAMEPAD_AXIS_LEFT_TRIGGER;
                    GamepadAxis trigRight = GamepadAxis.GAMEPAD_AXIS_RIGHT_TRIGGER;

                    if ((axiTest == trigLeft || axiTest == trigRight) && movement > -1) IsPad = true;
                    else if (axiTest != trigLeft && axiTest != trigRight && movement != 0) IsPad = true;
                }
            }

            // Toogle pad/keyboard
            foreach (KeyboardKey keyTest in (KeyboardKey[]) Enum.GetValues(typeof(KeyboardKey)))
            if (Raylib.IsKeyDown(keyTest))
            IsPad = false;

            foreach (MouseButton MouTest in (MouseButton[]) Enum.GetValues(typeof(MouseButton)))
            if (Raylib.IsMouseButtonPressed(MouTest))
            IsPad = false;

            if(Raylib.GetMouseWheelMove() != 0)
            IsPad = false;

            if (IsPad && Raylib.IsCursorOnScreen())
            Raylib.HideCursor();

            if(!IsPad && Raylib.IsCursorHidden())
            Raylib.ShowCursor();

            if (FirstLoop)
            FirstLoop = false;
        }

        // Update
        // ------
        public static void Update()
        {
            // Check game pad is available
            // ---------------------------

            bool gamePadAvailable = Raylib.IsGamepadAvailable(Gamepad);
            KeyOrPad(gamePadAvailable);

            // Shorten and simplify redundancy
            // -------------------------------

            static bool keyDown(KeyboardKey key)
            {
                return Raylib.IsKeyDown(Remap(key));
            }

            bool buttonDown(GamepadButton button)
            {
                if (!gamePadAvailable) return false;
                return Raylib.IsGamepadButtonDown(Gamepad, button);
            }

            bool triggerDown(GamepadAxis trigger)
            {
                if (!gamePadAvailable) return false;
                return Raylib.GetGamepadAxisMovement(Gamepad, trigger) + 1 > 2 * (DeadZoneTrigger / 100.0f);
            }

            void axisMove(GamepadAxis axis, int deadZone, ref bool left, ref bool right)
            {
                if (gamePadAvailable)
                {
                    float axisValue = Raylib.GetGamepadAxisMovement(Gamepad, axis);

                    right = axisValue > 1 * (deadZone / 100.0f) && axisValue > 0;
                    left = axisValue < -1 * (deadZone / 100.0f) && axisValue < 0;
                }
                else
                left = right = false;
            }

            // Listening keys/buttons/axis/mouse
            // ---------------------------------

            // D-PAD
            DPadDown = keyDown(KeyboardKey.KEY_DOWN) || buttonDown(GamepadButton.GAMEPAD_BUTTON_LEFT_FACE_DOWN);
            DPadLeft = keyDown(KeyboardKey.KEY_LEFT) || buttonDown(GamepadButton.GAMEPAD_BUTTON_LEFT_FACE_LEFT);
            DPadRight = keyDown(KeyboardKey.KEY_RIGHT) || buttonDown(GamepadButton.GAMEPAD_BUTTON_LEFT_FACE_RIGHT);
            DPadUp = keyDown(KeyboardKey.KEY_UP) || buttonDown(GamepadButton.GAMEPAD_BUTTON_LEFT_FACE_UP);

            // XABY-PAD
            XabyPadA = keyDown(KeyboardKey.KEY_KP_0) || buttonDown(GamepadButton.GAMEPAD_BUTTON_RIGHT_FACE_DOWN);
            XabyPadB = keyDown(KeyboardKey.KEY_KP_1) || buttonDown(GamepadButton.GAMEPAD_BUTTON_RIGHT_FACE_RIGHT);
            XabyPadX = keyDown(KeyboardKey.KEY_KP_2) || buttonDown(GamepadButton.GAMEPAD_BUTTON_RIGHT_FACE_LEFT);
            XabyPadY = keyDown(KeyboardKey.KEY_KP_3) || buttonDown(GamepadButton.GAMEPAD_BUTTON_RIGHT_FACE_UP);

            // MIDDLE-PAD
            MiddlePadLeft = keyDown(KeyboardKey.KEY_KP_4) || buttonDown(GamepadButton.GAMEPAD_BUTTON_MIDDLE_LEFT);
            MiddlePadRight = keyDown(KeyboardKey.KEY_KP_6) || buttonDown(GamepadButton.GAMEPAD_BUTTON_MIDDLE_RIGHT);
            MiddlePadCenter = keyDown(KeyboardKey.KEY_KP_5) || buttonDown(GamepadButton.GAMEPAD_BUTTON_MIDDLE);

            // TRIGGER-PAD
            TriggerPadLB = keyDown(KeyboardKey.KEY_KP_7) || buttonDown(GamepadButton.GAMEPAD_BUTTON_LEFT_TRIGGER_1);
            TriggerPadRB = keyDown(KeyboardKey.KEY_KP_9) || buttonDown(GamepadButton.GAMEPAD_BUTTON_RIGHT_TRIGGER_1);
            TriggerPadLT = keyDown(KeyboardKey.KEY_KP_DIVIDE) || triggerDown(GamepadAxis.GAMEPAD_AXIS_LEFT_TRIGGER);
            TriggerPadRT = keyDown(KeyboardKey.KEY_KP_MULTIPLY) || triggerDown(GamepadAxis.GAMEPAD_AXIS_RIGHT_TRIGGER);

            // THUMB
            AxisLS = buttonDown(GamepadButton.GAMEPAD_BUTTON_LEFT_THUMB);
            AxisRS = buttonDown(GamepadButton.GAMEPAD_BUTTON_RIGHT_THUMB);

            // AXIS-PAD
            axisMove(GamepadAxis.GAMEPAD_AXIS_LEFT_X, DeadZoneStickLeft, ref AxisLeftPadLeft, ref AxisLeftPadRight);
            axisMove(GamepadAxis.GAMEPAD_AXIS_LEFT_Y, DeadZoneStickLeft, ref AxisLeftPadUp, ref AxisLeftPadDown);
            axisMove(GamepadAxis.GAMEPAD_AXIS_RIGHT_X, DeadZoneStickRight, ref AxisRightPadLeft, ref AxisRightPadRight);
            axisMove(GamepadAxis.GAMEPAD_AXIS_RIGHT_Y, DeadZoneStickRight, ref AxisRightPadUp, ref AxisRightPadDown);

            // CLICK
            MouseLeftClick = Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT);
            MouseLeftDoubleClick = DoubleClick();

            // ADDITIONAL KEYS
            KeyAlt = keyDown(KeyboardKey.KEY_LEFT_ALT);
            KeyEnter = keyDown(KeyboardKey.KEY_ENTER);
            KeyM = keyDown(KeyboardKey.KEY_M);
            KeyP = keyDown(KeyboardKey.KEY_P);
            KeyG = keyDown(KeyboardKey.KEY_G);
            KeyC = keyDown(KeyboardKey.KEY_C);
            KeyS = keyDown(KeyboardKey.KEY_S);

            // Special actions
            // ---------------

            // Debug enable/disable
            if (Program.Emulation != null && gamePadAvailable)
            if (Pressed("Debug", XabyPadA && XabyPadB && XabyPadX && XabyPadY && TriggerPadLB && TriggerPadRB && TriggerPadLT && TriggerPadRT && AxisRS))
            Program.Emulation.DebugEnable = !Program.Emulation.DebugEnable;

            // Toggle fullscreen
            if(Pressed("Fullscreen", KeyAlt && KeyEnter))
            Program.ToogleFullScreen();

            // Open menu in game
            if (Program.Emulation != null)
            if (Pressed("MenuGame", (AxisLS && AxisRS) || KeyM))
            {
                Program.Emulation.MenuIsOpen = true;
                Program.Emulation.Pause();
                DrawGUI.Action("MenuGame");
            }

            // Draw menu in game
            if (Program.Emulation != null && Program.Emulation.MenuIsOpen)
            DrawGUI.MenuInGame();
        }

        #endregion

        #region Remap

        // Remap keyboard
        // --------------

        private static readonly string KeyboardSelect = "AZERTY";

        private static KeyboardKey Remap(KeyboardKey key)
        {
            return KeyboardSelect switch
            {
                "AZERTY" => ToAzerty(key),
                _ => key,
            };
        }

        private static KeyboardKey ToAzerty(KeyboardKey key)
        {
            return key switch
            {
                KeyboardKey.KEY_A => KeyboardKey.KEY_Q,
                KeyboardKey.KEY_Q => KeyboardKey.KEY_A,
                KeyboardKey.KEY_W => KeyboardKey.KEY_Z,
                KeyboardKey.KEY_Z => KeyboardKey.KEY_W,
                KeyboardKey.KEY_SEMICOLON => KeyboardKey.KEY_M,
                KeyboardKey.KEY_M => KeyboardKey.KEY_SEMICOLON,
                KeyboardKey.KEY_LEFT_BRACKET => KeyboardKey.KEY_M,
                KeyboardKey.KEY_COMMA => KeyboardKey.KEY_SEMICOLON,
                _ => key,
            };
        }

        #endregion

        #region Other

        // Vibration
        // ---------
        public static void Vibration(bool enable)
        {
            // Initialize XInput
            var controller = new Controller(UserIndex.One);

            if(controller.IsConnected)
            {
                // Set vibration
                Vibration vibration = new()
                {
                    LeftMotorSpeed = (ushort)(enable ? 65535 : 0),
                    RightMotorSpeed = (ushort)(enable ? 65535 : 0)
                };

                controller.SetVibration(vibration);
            }
        }

        #endregion
    }
}