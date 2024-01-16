/// <summary>
/// Mouse.cs - A utility class for managing the mouse in Godot Engine.
/// 
/// This class provides functionalities to manage the mouse cursor and detect double clicks in the Godot Engine.
/// Features include showing and hiding the mouse cursor and checking for double clicks on UI elements or anywhere.
/// 
/// Usage:
/// - Copy Mouse.cs into your Godot project.
/// - Use the static methods to manage the mouse cursor or check for double clicks.
/// </summary>
/// <author>Blueloot</author>
/// <version>1.0</version>
/// <target>Godot 4.2.stable.mono</target>
/// <changelog>
/// - 1.0: Initial release.
/// </changelog>
/// <license>CC0 - Public Domain</license>
/// <url>https://github.com/blueloot/Stairs/blob/main/C%23/Mouse.cs</url>

namespace Blueloot.Mouse;

using Godot;

public static class Mouse
{
    /// <returns>true if the mouse cursor is visible; otherwise, false.</returns>
    public static bool IsVisible => Input.MouseMode == Input.MouseModeEnum.Visible;

    /// <returns>true if the mouse cursor is captured; otherwise, false.</returns>
    public static bool IsHidden => Input.MouseMode == Input.MouseModeEnum.Captured;

    /// <returns>true if the mouse cursor is disabled; otherwise, false.</returns>
    public static bool IsDisabled => Input.MouseMode == Input.MouseModeEnum.Hidden;

    /// <summary>Shows the mouse cursor.</summary>
    public static void Show() => Input.MouseMode = Input.MouseModeEnum.Visible;

    /// <summary>Hides the mouse cursor.</summary>
    /// <remarks>The mouse cursor will be captured to the window and hidden in the center of the screen.</remarks>
    public static void Hide() => Input.MouseMode = Input.MouseModeEnum.Captured;

    /// <summary>Checks for a double click</summary>
    /// <remarks>
    /// Supply an index to check for double clicks on a specific item in an ItemList or other UI elements, 
    /// or leave it empty (-1) to check for double clicks anywhere.
    /// </remarks>
    /// <param name="mouseButton">The mouse button to check for a double click, default is left button.</param>
    /// <param name="index">The index of the item to check for a double click, default is -1 (anywhere).</param>
    /// <returns>true if a double click was recognized; otherwise, false.</returns>
    public static bool DoubleClicked(MouseButton mouseButton = MouseButton.Left, int index = -1)
    => MouseExtensions.DoubleClicked(mouseButton, index);
}

internal static class MouseExtensions
{
    private const float DoubleClickThreshold = 0.3f; // Threshold time for double click detection.

    private static int LastClickedIndex { get; set; } = -1; // Index of the last clicked UI element.
    private static float LastClickTime { get; set; } = 0f; // Time of the last click event.
    private static MouseButton LastButtonClicked { get; set; } = MouseButton.None; // Last clicked mouse button. 

    internal static bool DoubleClicked(MouseButton mouseButton = MouseButton.Left, int index = -1)
    {
        if (IsMouseButtonPressedWithCondition(mouseButton, index) && IsWithinDoubleClickThreshold())
        {
            ResetClickInfo();
            return true;
        }

        UpdateClickInfo(mouseButton, index);
        return false;

        // local functions
        bool IsMouseButtonPressedWithCondition(MouseButton mouseButton, int index)
            => Input.IsMouseButtonPressed(mouseButton) &&
               LastButtonClicked == mouseButton &&
               index == LastClickedIndex;

        bool IsWithinDoubleClickThreshold()
            => GetCurrentTimeInSeconds() - LastClickTime <= DoubleClickThreshold;

        float GetCurrentTimeInSeconds()
            => Time.GetTicksMsec() / 1000f;

        void UpdateClickInfo(MouseButton mouseButton, int index)
        {
            LastClickedIndex = index;
            LastClickTime = GetCurrentTimeInSeconds();
            LastButtonClicked = mouseButton;
        }

        void ResetClickInfo()
        {
            LastClickedIndex = -1;
            LastClickTime = 0f;
            LastButtonClicked = MouseButton.None;
        }
    }
}
