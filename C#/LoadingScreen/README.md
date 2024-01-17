# Scene Management

## Overview

`SceneManager` is a utility class in Godot that simplifies scene transitions, especially useful for adding a loading screen between scenes. This document explains how to use the `SceneManager` class, along with the `LoadingScreen` class, to manage scene transitions effectively.

## Example Usage

To see the SceneManager and LoadingScreen classes in action, you can explore the provided example scenes:

- `scene0.tscn`: Demonstrates an example similar to a `GameManager`, showcasing a persistent root node across scene transitions.
- `scene1.tscn` and `scene2.tscn`: These scenes are used to illustrate standard scene transitions using the SceneManager.

Simply copy these scene files into your Godot project and open them to understand how scene transitions are handled in different scenarios.

## Usage

### Basic Scene Transition

To perform a basic scene transition, use the `SceneManager.Load` method. This method replaces the current scene with a new one, using a loading screen.

Example:

```csharp
SceneManager.Load("res://path/to/your/new_scene.tscn");
```

### Scene Transition with Signals

You can trigger scene transitions in response to signals, such as a button press.

Example:

```csharp
var btn = GetNodeOrNull<Button>("Button");
var newScene = "res://path/to/your/new_scene.tscn";

btn.Pressed += () => SceneManager.Load(newScene);
```

In this example, when the button is pressed, the specified new scene is loaded.

### Using **ReplaceRoot** Property

The `ReplaceRoot` property in `SceneManager` determines how the scene transition is handled:

* `ReplaceRoot = true`: The entire root node is replaced with the loading screen, and subsequently with the new scene. This is suitable for simple games where each scene is independent.

* `ReplaceRoot = false`: The loading screen is added as a child to the current scene's root. After loading, the new scene is also added as a child. This is useful when you have a persistent `GameManager` or similar node that should remain throughout the game.

Example with `GameManager` as Root
Set `ReplaceRoot` to `false`:

```csharp
SceneManager.ReplaceRoot = false;
```

Use `SceneManager.Load` as usual. The `GameManager` node will persist across scene transitions.

## LoadingScreen Class

The `LoadingScreen` class manages the display and animation of the loading screen during scene transitions. It automatically handles fade-in and fade-out animations between scenes.

### Customization

You can customize the loading screen by modifying the `LoadingScreen` scene in the Godot editor, such as changing the animation or adding additional UI elements.
