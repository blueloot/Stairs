/// <summary>
/// SceneManager.cs and LoadingScreen.cs - Utility classes for managing scene transitions in Godot Engine.
///
/// These classes facilitate seamless scene transitions with a customizable loading screen. 
/// SceneManager handles the logic of scene loading and transitions, 
/// while LoadingScreen provides a visual interface during the loading process.
/// 
/// Features include configurable scene replacement options, 
/// loading screen animations, and support for asynchronous scene loading.
///
/// Usage:
/// - Copy SceneManager.cs, LoadingScreen.cs and LoadingScreen.tscn into your Godot project.
/// - Use SceneManager.Load to initiate scene transitions.
/// - Customize the LoadingScreen.tscn scene in Godot editor for specific UI/UX needs.
/// - Set SceneManager.ReplaceRoot to true or false based on whether you want to replace 
/// the entire root node or keep a persistent root (like a GameManager).
/// 
/// See the README.md file for more information.
///
/// </summary>
/// <author>Blueloot</author>
/// <version>1.0.1</version>
/// <target>Godot 4.2.stable.mono</target>
/// <changelog>
/// - 1.0.1: Improved documentation
/// - 1.0.0: Initial release, featuring basic scene transition functionality with loading screen.
/// </changelog>
/// <license>CC0 - Public Domain</license>
/// <url>https://github.com/blueloot/Stairs/tree/main/C%23/LoadingScreen</url>
///
/// TODO: 
///  - proper fade in/out even when the "ReplaceRootNodeOnLoad" option is true, if possible.
///  - support parent scenes. currently only supports children of the root node (or the root node itself).
///    basically to support cases where a scene should be loaded as a child of another scene.
///  - support sceneloading without deleting the current scene.

using System.Threading.Tasks;
using Godot;
using GC = Godot.Collections;

namespace Blueloot.Loading;


public static class SceneManager
{
	// Path to the loading screen scene.
	public static readonly string LoadingScreenScenePath = "res://C#/LoadingScreen/LoadingScreen.tscn";

	// Determines if sub-threads should be used for loading.
	public static readonly bool UseSubThreads = true;

	// Option to replace the root node with the loader scene or not.
	// if false: the loader will be added as a child of root. leaving the root node and its children intact.
	//           this means in order to delete the correct scene/level you need to previously set the
	// 		 	 ActiveScene property to the scene you want to delete. this only applies for the first scene
	//			 loaded, after that the ActiveScene property will be set automatically.
	// if true: the loader will replace the root node. the root node will be deleted.
	//			simplest option, but good for simple non-complex games that doesn't deal
	//			with a lot of data or are in-scene-contained.
	public static bool ReplaceRootNodeOnLoad { get; set; } = false;
	public static void SetReplaceRootMode(bool value) => ReplaceRootNodeOnLoad = value;

	// Reference to the currently active scene (the scene to delete).
	private static Node activeScene = null;
	public static Node ActiveScene
	{
		get => activeScene;
		set
		{
			activeScene = value;
			activeScene.AddToGroup("QueueFree");
		}
	}

	// Path to the next scene to be loaded.
	public static string NextScenePath { get; private set; } = "";

	// Helper method to get the main SceneTree.
	private static SceneTree GetTree() => Engine.GetMainLoop() as SceneTree;

	// Initiates the loading of a new scene.
	// scenePath: Path to the scene that needs to be loaded.
	public static void Load(string scenePath)
	{
		NextScenePath = scenePath;

		// Load and instantiate the loader scene.
		if (ResourceLoader.Load(LoadingScreenScenePath) is not PackedScene loaderScene)
		{
			GD.PrintErr("Failed to load loader scene");
			return;
		}

		Node loaderNode = loaderScene?.Instantiate();

		// Replace the current scene with the loader scene.
		// or add the loader scene as a child of the current scene.
		if (ReplaceRootNodeOnLoad)
		{
			var currentScene = GetTree().CurrentScene;

			GetTree().Root.RemoveChild(currentScene);
			currentScene.QueueFree();

			GetTree().Root.AddChild(loaderNode);
			GetTree().CurrentScene = loaderNode;
		}
		else
		{
			GetTree().CurrentScene.AddChild(loaderNode);
		}
	}

}

public partial class LoadingScreen : CanvasLayer
{
	// Enum to manage the loading screen's current status.
	private enum Status { Idle, LoadNewScene, Hide, Finish }
	private Status status = Status.Idle;

	public override void _Ready()
	{
		// this is a hack to get the current scene, but,
		// it is not a reliable way since the scene could be in another position in the tree
		// instead the scene should either be passed as an argument for SceneManager.Load, 
		// or set during the scene's _Ready method (see scene1.cs for an example)
		if (SceneManager.ActiveScene == null
		&& !SceneManager.ReplaceRootNodeOnLoad)
		{
			SceneManager.ActiveScene = GetTree().CurrentScene.GetChild(0);
			GD.PrintRich($"[color=#1E90FF]LoadingScreen.cs[/color] : " +
						 $"[color=#FF4136]Warning:[/color] " +
						 $"[color=#FFDC00]SceneManager.ActiveScene[/color] is not set. " +
						 $"Defaulting to the first child '[color=#2ECC40]{SceneManager.ActiveScene?.Name}'[/color]' of the current scene. " +
						 $"[color=#7FDBFF]Caution: This might not be the intended scene![/color] " +
						 $"Set [color=#FFDC00]SceneManager.ActiveScene[/color] manually before the first scene load to ensure correct scene management. " +
						 $"Typically done in a GameManager or similar. " +
						 $"[color=#FF851B]Note: This is only critical for the first load, as SceneManager will handle subsequent loads automatically.[/color]");
		}

		// Start the fade-in animation and prepare to load the new scene.
		PlayAndWaitAnimation("fade_in").ContinueWith(_ => status = Status.LoadNewScene);
		ResourceLoader.LoadThreadedRequest(SceneManager.NextScenePath, "", SceneManager.UseSubThreads);
	}

	public async override void _Process(double delta)
	{
		switch (status)
		{
			case Status.LoadNewScene:
				// Check the loading status and proceed to hide once done.
				if (Process() == ResourceLoader.ThreadLoadStatus.Loaded)
					status = Status.Hide;
				break;

			case Status.Hide:
				// gradually show the new scene by fading out the loading screen.
				PreloadNewScene();
				await PlayAndWaitAnimation("fade_in", true);
				status = Status.Finish;
				break;

			case Status.Finish:
				// Finalize the loading process, switch scenes if necessary, delete self.
				Finish();
				break;
		}
	}

	private void PreloadNewScene()
	{
		if (!SceneManager.ReplaceRootNodeOnLoad)
		{
			var scene = ResourceLoader.LoadThreadedGet(SceneManager.NextScenePath) as PackedScene;
			var newScene = scene?.Instantiate();
			if (newScene == null) return;
			GetTree().CurrentScene.AddChild(newScene);
			SceneManager.ActiveScene = newScene;
		}
	}

	private void Finish()
	{
		// Change to the new scene or remove the loading screen based on configuration.
		if (SceneManager.ReplaceRootNodeOnLoad)
		{
			var scene = ResourceLoader.LoadThreadedGet(SceneManager.NextScenePath) as PackedScene;
			GetTree().ChangeSceneToPacked(scene);
		}
		else
		{
			QueueFree();
		}
	}

	private ResourceLoader.ThreadLoadStatus Process()
	{
		// Update the loading progress.
		GC.Array progress = new();
		var status = ResourceLoader.LoadThreadedGetStatus(SceneManager.NextScenePath, progress);

		float percent = (float)progress[0] * 100;

		// Update UI elements based on loading progress.
		var pb = GetNodeOrNull<ProgressBar>("ProgressBar");
		var label = GetNodeOrNull<Label>("Label");
		if (pb != null) pb.Value = percent;
		if (label != null) label.Text = $"{percent}%";

		// Remove old scenes marked for deletion.
		GC.Array<Node> nodes = GetTree().GetNodesInGroup("QueueFree");
		foreach (Node node in nodes) node.QueueFree();

		return status;
	}


	// Helper methods for animations.
	private AnimationPlayer PlayAnimation(string animation, bool backwards = false)
	{
		var anim = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
		if (backwards) anim?.PlayBackwards(animation);
		else anim?.Play(animation);
		return anim;
	}

	public async Task PlayAndWaitAnimation(string animation, bool backwards = false)
	{
		var anim = PlayAnimation(animation, backwards);
		if (anim == null) return;
		await ToSignal(anim, "animation_finished");
	}
}
