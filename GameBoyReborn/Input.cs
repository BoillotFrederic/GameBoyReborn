// ---------
// Input set
// ---------
using Raylib_cs;

public static class Input
{
    // Set
    private static int Gamepad = 0;
    public static int DeadZoneTrigger = 10;
    public static int DeadZoneStickLeft = 10;
    public static int DeadZoneStickRight = 10;

    // Keys/buttons state
    // ------------------

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
    public static bool TriggerPadLB = false;
    public static bool TriggerPadRB = false;
    public static bool TriggerPadLT = false;
    public static bool TriggerPadRT = false;

    // AXIS-PAD
    public static bool AxisLeftPadDown = false;
    public static bool AxisLeftPadLeft = false;
    public static bool AxisLeftPadRight = false;
    public static bool AxisLeftPadUp = false;
    public static bool AxisRightPadDown = false;
    public static bool AxisRightPadLeft = false;
    public static bool AxisRightPadRight = false;
    public static bool AxisRightPadUp = false;

    public static void Set()
    {
        // Init keys/buttons
        // -----------------

        // D-PAD
        DPadDown = false;
        DPadLeft = false;
        DPadRight = false;
        DPadUp = false;

        // XABY-PAD
        XabyPadA = false;
        XabyPadB = false;
        XabyPadX = false;
        XabyPadY = false;

        // MIDDLE-PAD
        MiddlePadLeft = false;
        MiddlePadRight = false;
        MiddlePadCenter = false;

        // TRIGGER-PAD
        TriggerPadLB = false;
        TriggerPadRB = false;
        TriggerPadLT = false;
        TriggerPadRT = false;

        // AXIS-PAD
        AxisLeftPadDown = false;
        AxisLeftPadLeft = false;
        AxisLeftPadRight = false;
        AxisLeftPadUp = false;
        AxisRightPadDown = false;
        AxisRightPadLeft = false;
        AxisRightPadRight = false;
        AxisRightPadUp = false;

        // Keyboards
        // ---------

        // D-PAD
        if (Raylib.IsKeyDown(KeyboardKey.KEY_DOWN)) DPadDown = true;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_LEFT)) DPadLeft = true;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_RIGHT)) DPadRight = true;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_UP)) DPadUp = true;

        // XABY-PAD
        if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_0)) XabyPadA = true;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_1)) XabyPadB = true;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_2)) XabyPadX = true;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_3)) XabyPadY = true;

        // MIDDLE-PAD
        if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_4)) MiddlePadLeft = true;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_6)) MiddlePadRight = true;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_5)) MiddlePadCenter = true;

        // TRIGGER-PAD
        if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_7)) TriggerPadLB = true;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_9)) TriggerPadRB = true;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_DIVIDE)) TriggerPadLT = true;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_KP_MULTIPLY)) TriggerPadRT = true;

        // XBOX gamepad
        // ------------
        if (Raylib.IsGamepadAvailable(Gamepad))
        {
            // D-PAD
            if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_LEFT_FACE_DOWN)) DPadDown = true;
            if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_LEFT_FACE_LEFT)) DPadLeft = true;
            if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_LEFT_FACE_RIGHT)) DPadRight = true;
            if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_LEFT_FACE_UP)) DPadUp = true;

            // XABY-PAD
            if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_RIGHT_FACE_DOWN)) XabyPadA = true;
            if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_RIGHT_FACE_RIGHT)) XabyPadB = true;
            if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_RIGHT_FACE_LEFT)) XabyPadX = true;
            if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_RIGHT_FACE_UP)) XabyPadY = true;

            // MIDDLE-PAD
            if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_MIDDLE_LEFT)) MiddlePadLeft = true;
            if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_MIDDLE_RIGHT)) MiddlePadRight = true;
            if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_MIDDLE)) MiddlePadCenter = true;

            // TRIGGER-PAD
            if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_LEFT_TRIGGER_1)) TriggerPadLB = true;
            if (Raylib.IsGamepadButtonDown(Gamepad, GamepadButton.GAMEPAD_BUTTON_RIGHT_TRIGGER_1)) TriggerPadRB = true;
            if (Raylib.GetGamepadAxisMovement(Gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_TRIGGER) + 1 > 2 * (DeadZoneTrigger / 100.0f)) TriggerPadLT = true;
            if (Raylib.GetGamepadAxisMovement(Gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_TRIGGER) + 1 > 2 * (DeadZoneTrigger / 100.0f)) TriggerPadRT = true;

            // AXIS-PAD
            float AxisLeftX = Raylib.GetGamepadAxisMovement(Gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_X);
            float AxisLeftY = Raylib.GetGamepadAxisMovement(Gamepad, GamepadAxis.GAMEPAD_AXIS_LEFT_Y);

            if (AxisLeftY > (1.0f * (DeadZoneStickLeft / 100.0f)) && AxisLeftY > 0) AxisLeftPadDown = true;
            if (AxisLeftY < (-1.0f * (DeadZoneStickLeft / 100.0f)) && AxisLeftY < 0) AxisLeftPadUp = true;
            if (AxisLeftX > 1 * (DeadZoneStickLeft / 100.0f) && AxisLeftX > 0) AxisLeftPadRight = true;
            if (AxisLeftX < -1 * (DeadZoneStickLeft / 100.0f) && AxisLeftX < 0) AxisLeftPadLeft = true;

            float AxisRightX = Raylib.GetGamepadAxisMovement(Gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_X);
            float AxisRightY = Raylib.GetGamepadAxisMovement(Gamepad, GamepadAxis.GAMEPAD_AXIS_RIGHT_Y);

            if (AxisRightY > (1.0f * (DeadZoneStickRight / 100.0f)) && AxisRightY > 0) AxisRightPadDown = true;
            if (AxisRightY < (-1.0f * (DeadZoneStickRight / 100.0f)) && AxisRightY < 0) AxisRightPadUp = true;
            if (AxisRightX > 1 * (DeadZoneStickRight / 100.0f) && AxisRightX > 0) AxisRightPadRight = true;
            if (AxisRightX < -1 * (DeadZoneStickRight / 100.0f) && AxisRightX < 0) AxisRightPadLeft = true;
        }

 
        // Show keys/buttons
        // -----------------

        // D-PAD
        Raylib.DrawText("D-PAD : Down : " + DPadDown, 10, 10, 20, Color.BLUE);
        Raylib.DrawText("D-PAD : Left : " + DPadLeft, 10, 30, 20, Color.BLUE);
        Raylib.DrawText("D-PAD : Right : " + DPadRight, 10, 50, 20, Color.BLUE);
        Raylib.DrawText("D-PAD : Up : " + DPadUp, 10, 70, 20, Color.BLUE);

        // XABY-PAD
        Raylib.DrawText("XABY-PAD : A : " + XabyPadA, 10, 90, 20, Color.BLUE);
        Raylib.DrawText("XABY-PAD : B : " + XabyPadB, 10, 110, 20, Color.BLUE);
        Raylib.DrawText("XABY-PAD : X : " + XabyPadX, 10, 130, 20, Color.BLUE);
        Raylib.DrawText("XABY-PAD : Y : " + XabyPadY, 10, 150, 20, Color.BLUE);

        // MIDDLE-PAD
        Raylib.DrawText("MIDDLE-PAD : Back : " + MiddlePadLeft, 10, 170, 20, Color.BLUE);
        Raylib.DrawText("MIDDLE-PAD : Start : " + MiddlePadRight, 10, 190, 20, Color.BLUE);
        Raylib.DrawText("MIDDLE-PAD : Center : " + MiddlePadCenter, 10, 210, 20, Color.BLUE);

        // TRIGGER-PAD
        Raylib.DrawText("TRIGGER-PAD : LB : " + TriggerPadLB, 10, 230, 20, Color.BLUE);
        Raylib.DrawText("TRIGGER-PAD : RB : " + TriggerPadRB, 10, 250, 20, Color.BLUE);
        Raylib.DrawText("TRIGGER-PAD : LT : " + TriggerPadLT, 10, 270, 20, Color.BLUE);
        Raylib.DrawText("TRIGGER-PAD : RT : " + TriggerPadRT, 10, 290, 20, Color.BLUE);

        // AXIS-PAD
        Raylib.DrawText("AXIS-PAD : LEFT Down : " + AxisLeftPadDown, 10, 310, 20, Color.BLUE);
        Raylib.DrawText("AXIS-PAD : LEFT Left : " + AxisLeftPadLeft, 10, 330, 20, Color.BLUE);
        Raylib.DrawText("AXIS-PAD : LEFT Right : " + AxisLeftPadRight, 10, 350, 20, Color.BLUE);
        Raylib.DrawText("AXIS-PAD : LEFT Up : " + AxisLeftPadUp, 10, 370, 20, Color.BLUE);
        Raylib.DrawText("AXIS-PAD : RIGHT Down : " + AxisRightPadDown, 10, 390, 20, Color.BLUE);
        Raylib.DrawText("AXIS-PAD : RIGHT Left : " + AxisRightPadLeft, 10, 410, 20, Color.BLUE);
        Raylib.DrawText("AXIS-PAD : RIGHT Right : " + AxisRightPadRight, 10, 430, 20, Color.BLUE);
        Raylib.DrawText("AXIS-PAD : RIGHT Up : " + AxisRightPadUp, 10, 450, 20, Color.BLUE);

    }
}