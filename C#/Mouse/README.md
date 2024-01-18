# Mouse Utility Class for Godot Engine

## Overview

The `Mouse.cs` class offers a range of functionalities to manage the mouse cursor and detect double-click events in Godot Engine applications. It simplifies the process of controlling the mouse cursor's visibility and interacting with UI elements.

### Features

- **Cursor Visibility Management**: Easily show or hide the mouse cursor within your Godot project.
- **Double Click Detection**: Check for double clicks either on specific UI elements or anywhere on the screen.
- **Cursor State Queries**: Quickly check the current state of the mouse cursor (visible, hidden, or disabled).

### Usage

1. Copy `Mouse.cs` into your Godot project.
2. Utilize the static methods provided by the `Mouse` class to manage the mouse cursor or to check for double-click events.

## Documentation

### `Show()`

Makes the mouse cursor visible.

```csharp
public static void Show()
```

#### Usage

```csharp
Mouse.Show();
```

### `Hide()`

Hides the mouse cursor and captures it to the center of the screen.

```csharp
public static void Hide()
```

#### Usage

```csharp
Mouse.Hide();
```

### `DoubleClicked(MouseButton mouseButton = MouseButton.Left, int index = -1)`

Checks for a double-click event. You can specify the mouse button and the index of the UI element. The index is optional and defaults to -1, meaning it will check for double clicks anywhere.

```csharp
public static bool DoubleClicked(MouseButton mouseButton = MouseButton.Left, int index = -1)
```

#### Parameters

- `mouseButton`: The mouse button to check for a double click. Defaults to `MouseButton.Left`.
- `index`: The index of the item to check for a double click. Defaults to -1 (anywhere).

#### Returns

`true` if a double-click event is detected; otherwise, `false`.

#### Usage

```csharp
// Check for a double-click with the left mouse button
bool isDoubleClicked = Mouse.DoubleClicked(MouseButton.Left);

// Check for double-clicks on items within an `ItemList`, iterate through the items and use the `DoubleClicked` method with the index of each item.
ItemList itemList = GetNode<ItemList>("MyItemList");
for (int i = 0; i < itemList.ItemCount; i++)
{
    if (Mouse.DoubleClicked(MouseButton.Left, i))
    {
        // Double-click detected on the item at index i
        GD.Print("Double-clicked on item at index: " + i.ToString());
    }
}
```

## Properties

### `IsVisible`

Gets a value indicating whether the mouse cursor is currently visible.

```csharp
public static bool IsVisible { get; }
```

### `IsHidden`

Gets a value indicating whether the mouse cursor is currently captured and hidden.

```csharp
public static bool IsHidden { get; }
```

### `IsDisabled`

Gets a value indicating whether the mouse cursor is currently disabled.

```csharp
public static bool IsDisabled { get; }
```

## Version

1.0

### Changelog

- 1.0: Initial release.

## License

This class is released under the CC0 - Public Domain license.
