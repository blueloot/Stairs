# Scene Management

## Overview

`SceneManager` and `LoadingScreen` are utility classes in Godot designed to streamline scene transitions, particularly useful for implementing a loading screen between scenes. This document outlines how to effectively utilize these classes for managing scene transitions.


## Example Usage

Explore the provided example scenes to see `SceneManager` and `LoadingScreen` in action:

- `scene0.tscn`: Demonstrates a setup akin to a `GameManager`, showing how a persistent root node can be maintained across scene transitions.
- `scene1.tscn` and `scene2.tscn`: Illustrate standard scene transitions managed by `SceneManager`.

Copy these scene files into your Godot project and open them to understand the various ways scene transitions can be managed.

## Usage

### Basic Scene Transition

To execute a basic scene transition, use `SceneManager.Load`. This method swaps the current scene with a new one, implementing a loading screen for the transition.

```csharp
SceneManager.Load("res://new_scene.tscn");
```

### Scene Transition with Signals

Trigger scene transitions in response to events, like a button press:


```csharp
var btn = GetNodeOrNull<Button>("Button");
var newScene = "res://new_scene.tscn";

btn.Pressed += () => SceneManager.Load(newScene);
```

Here, pressing the button initiates the loading of the new scene.

### Using **ReplaceRootNodeOnLoad** Property

`ReplaceRootNodeOnLoad` in `SceneManager` affects the scene transition method:

- `ReplaceRootNodeOnLoad = true`: Replaces the entire root node with the loading screen, and then the new scene. Suitable for simpler games with independent scenes.

- `ReplaceRootNodeOnLoad = false`: Adds the loading screen as a child of the current scene's root. The new scene is then also added as a child, preserving a persistent root node like `GameManager`.

Example with `GameManager` as Root

For a persistent GameManager, set ReplaceRootNodeOnLoad to false:

```csharp
SceneManager.ReplaceRootNodeOnLoad = false;
```

Then, use SceneManager.Load as normal. The GameManager node will persist through scene transitions.

### Note on `ActiveScene`

The `ActiveScene` property in `SceneManager` plays a critical role in identifying which scene should be replaced or removed during the transition process. It's particularly important when `ReplaceRootNodeOnLoad` is set to `false`, as it ensures the correct scene is targeted for replacement.

For the first scene load, it's advisable to manually set the `ActiveScene` property, especially in complex projects where the scene structure might not be straightforward. This can be done in a global script like `GameManager` or immediately before the first call to `SceneManager.Load`.

Failure to set `ActiveScene` appropriately may result in unintended scenes being removed from the scene tree, leading to potential issues in scene management.

Have a look at `scene1.cs` for more info.

## LoadingScreen Class

`LoadingScreen` handles the visual aspects and animations of the loading screen during scene transitions. It manages fade-in and fade-out animations automatically.

### Customization

Modify the `LoadingScreen.tscn` scene in the Godot editor to suit specific UI/UX needs, such as altering animations or adding UI elements. Then implement these changes to the class in code.
