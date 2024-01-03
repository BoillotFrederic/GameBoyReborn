// ---------
// Input set
// ---------
using Raylib_cs;

namespace GameBoyReborn
{
    public class Input
    {
        // Set
        private readonly static int Gamepad = 0;
        private static int _DeadZoneTrigger = 10;
        private static int _DeadZoneStickLeft = 10;
        private static int _DeadZoneStickRight = 10;

        // Keys/buttons state
        // ------------------

        // D-PAD
        private static bool _DPadDown = false;
        private static bool _DPadLeft = false;
        private static bool _DPadRight = false;
        private static bool _DPadUp = false;

        // XABY-PAD
        private static bool _XabyPadA = false;
        private static bool _XabyPadB = false;
        private static bool _XabyPadX = false;
        private static bool _XabyPadY = false;

        // MIDDLE-PAD
        private static bool _MiddlePadLeft = false;
        private static bool _MiddlePadRight = false;
        private static bool _MiddlePadCenter = false;

        // TRIGGER-PAD
        private static bool _TriggerPadLB = false;
        private static bool _TriggerPadRB = false;
        private static bool _TriggerPadLT = false;
        private static bool _TriggerPadRT = false;

        // AXIS-PAD
        private static bool _AxisLeftPadDown = false;
        private static bool _AxisLeftPadLeft = false;
        private static bool _AxisLeftPadRight = false;
        private static bool _AxisLeftPadUp = false;
        private static bool _AxisRightPadDown = false;
        private static bool _AxisRightPadLeft = false;
        private static bool _AxisRightPadRight = false;
        private static bool _AxisRightPadUp = false;

        // Getter / setter
        // ---------------
        public static int DeadZoneTrigger
        {
            get { return _DeadZoneTrigger; }
            set { _DeadZoneTrigger = value; }
        }

        public static int DeadZoneStickLeft
        {
            get { return _DeadZoneStickLeft; }
            set { _DeadZoneStickLeft = value; }
        }

        public static int DeadZoneStickRight
        {
            get { return _DeadZoneStickRight; }
            set { _DeadZoneStickRight = value; }
        }

        // D-PAD
        public static bool DPadDown { get { return _DPadDown; } }
        public static bool DPadLeft { get { return _DPadLeft; } }
        public static bool DPadRight { get { return _DPadRight; } }
        public static bool DPadUp { get { return _DPadUp; } }

        // XABY-PAD
        public static bool XabyPadA { get { return _XabyPadA; } }
        public static bool XabyPadB { get { return _XabyPadB; } }
        public static bool XabyPadX { get { return _XabyPadX; } }
        public static bool XabyPadY { get { return _XabyPadY; } }

        // MIDDLE-PAD
        public static bool MiddlePadLeft { get { return _MiddlePadLeft; } }
        public static bool MiddlePadRight { get { return _MiddlePadRight; } }
        public static bool MiddlePadCenter { get { return _MiddlePadCenter; } }

        // TRIGGER-PAD
        public static bool TriggerPadLB { get { return _TriggerPadLB; } }
        public static bool TriggerPadRB { get { return _TriggerPadRB; } }
        public static bool TriggerPadLT { get { return _TriggerPadLT; } }
        public static bool TriggerPadRT { get { return _TriggerPadRT; } }

        // AXIS-PAD
        public static bool AxisLeftPadDown { get { return _AxisLeftPadDown; } }
        public static bool AxisLeftPadLeft { get { return _AxisLeftPadLeft; } }
        public static bool AxisLeftPadRight { get { return _AxisLeftPadRight; } }
        public static bool AxisLeftPadUp { get { return _AxisLeftPadUp; } }
        public static bool AxisRightPadDown { get { return _AxisRightPadDown; } }
        public static bool AxisRightPadLeft { get { return _AxisRightPadLeft; } }
        public static bool AxisRightPadRight { get { return _AxisRightPadRight; } }
        public static bool AxisRightPadUp { get { return _AxisRightPadUp; } }

        public static void Set()
        {
            // Init keys/buttons
            // -----------------

            // D-PAD
            _DPadDown = false;
            _DPadLeft = false;
            _DPadRight = false;
            _DPadUp = false;

            // XABY-PAD
            _XabyPadA = false;
            _XabyPadB = false;
            _XabyPadX = false;
            _XabyPadY = false;

            // MIDDLE-PAD
            _MiddlePadLeft = false;
            _MiddlePadRight = false;
            _MiddlePadCenter = false;

            // TRIGGER-PAD
            _TriggerPadLB = false;
            _TriggerPadRB = false;
            _TriggerPadLT = false;
            _TriggerPadRT = false;

            // AXIS-PAD
            _AxisLeftPadDown = false;
            _AxisLeftPadLeft = false;
            _AxisLeftPadRight = false;
            _AxisLeftPadUp = false;
            _AxisRightPadDown = false;
            _AxisRightPadLeft = false;
            _AxisRightPadRight = false;
            _AxisRightPadUp = false;

            // Keyboards
            // ---------

            // D-PAD
            if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN)) _DPadDown = true;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT)) _DPadLeft = true;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT)) _DPadRight = true;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_UP)) _DPadUp = true;

            // XABY-PAD
            if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_0)) _XabyPadA = true;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_1)) _XabyPadB = true;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_2)) _XabyPadX = true;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_3)) _XabyPadY = true;

            // MIDDLE-PAD
            if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_4)) _MiddlePadLeft = true;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_6)) _MiddlePadRight = true;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_5)) _MiddlePadCenter = true;

            // TRIGGER-PAD
            if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_7)) _TriggerPadLB = true;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_9)) _TriggerPadRB = true;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_DIVIDE)) _TriggerPadLT = true;
            if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_MULTIPLY)) _TriggerPadRT = true;

            // XBOX gamepad
            // ------------
            if (Raylib.IsGamepadAvailable(Gamepad))
            {
                // D-PAD
                if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_LEFT_FACE_DOWN)) _DPadDown = true;
                if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_LEFT_FACE_LEFT)) _DPadLeft = true;
                if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_LEFT_FACE_RIGHT)) _DPadRight = true;
                if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_LEFT_FACE_UP)) _DPadUp = true;

                // XABY-PAD
                if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_RIGHT_FACE_DOWN)) _XabyPadA = true;
                if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_RIGHT_FACE_RIGHT)) _XabyPadB = true;
                if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_RIGHT_FACE_LEFT)) _XabyPadX = true;
                if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_RIGHT_FACE_UP)) _XabyPadY = true;

                // MIDDLE-PAD
                if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_MIDDLE_LEFT)) _MiddlePadLeft = true;
                if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_MIDDLE_RIGHT)) _MiddlePadRight = true;
                if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_MIDDLE)) _MiddlePadCenter = true;

                // TRIGGER-PAD
                if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_LEFT_TRIGGER_1)) _TriggerPadLB = true;
                if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_RIGHT_TRIGGER_1)) _TriggerPadRB = true;
                if (Raylib.GetGamepadAxisMovement(Gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_TRIGGER) + 1 > 2 * (_DeadZoneTrigger / 100.0f)) _TriggerPadLT = true;
                if (Raylib.GetGamepadAxisMovement(Gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_TRIGGER) + 1 > 2 * (_DeadZoneTrigger / 100.0f)) _TriggerPadRT = true;

                // AXIS-PAD
                float AxisLeftX = Raylib.GetGamepadAxisMovement(Gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_X);
                float AxisLeftY = Raylib.GetGamepadAxisMovement(Gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_Y);

                if (AxisLeftY > (1.0f * (_DeadZoneStickLeft / 100.0f)) && AxisLeftY > 0) _AxisLeftPadDown = true;
                if (AxisLeftY < (-1.0f * (_DeadZoneStickLeft / 100.0f)) && AxisLeftY < 0) _AxisLeftPadUp = true;
                if (AxisLeftX > 1 * (_DeadZoneStickLeft / 100.0f) && AxisLeftX > 0) _AxisLeftPadRight = true;
                if (AxisLeftX < -1 * (_DeadZoneStickLeft / 100.0f) && AxisLeftX < 0) _AxisLeftPadLeft = true;

                float AxisRightX = Raylib.GetGamepadAxisMovement(Gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_X);
                float AxisRightY = Raylib.GetGamepadAxisMovement(Gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_Y);

                if (AxisRightY > (1.0f * (_DeadZoneStickRight / 100.0f)) && AxisRightY > 0) _AxisRightPadDown = true;
                if (AxisRightY < (-1.0f * (_DeadZoneStickRight / 100.0f)) && AxisRightY < 0) _AxisRightPadUp = true;
                if (AxisRightX > 1 * (_DeadZoneStickRight / 100.0f) && AxisRightX > 0) _AxisRightPadRight = true;
                if (AxisRightX < -1 * (_DeadZoneStickRight / 100.0f) && AxisRightX < 0) _AxisRightPadLeft = true;
            }
        }
    }
}