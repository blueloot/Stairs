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
/// <version>1.0</version>
/// <target>Godot 4.2.stable.mono</target>
/// <changelog>
/// - 1.0: Initial release, featuring basic scene transition functionality with loading screen.
/// </changelog>
/// <license>CC0 - Public Domain</license>
/// <url>https://github.com/blueloot/Stairs/tree/main/C%23/LoadingScreen</url>
///
/// TODO: 
///  - proper fade in/out even when the "ReplaceRoot" option is true, if possible.
///  - remove the hack to get the current scene

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
	public static bool ReplaceRoot { get; set; } = false;

	// Reference to the currently active scene (excluding the loading screen and root node).
	public static Node ActiveScene = null;

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
		if (loaderNode == null)
		{
			GD.PrintErr("Failed to instantiate loader scene");
			return;
		}

		// Replace the current scene with the loader scene.
		// or add the loader scene as a child of the current scene.
		if (ReplaceRoot)
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

	// Flag to ensure the scene is loaded only once when SceneManager.ReplaceRoot is false
	private bool isLoaded = false;

	public override void _Ready()
	{
		// TODO: this is a hack to get the current scene
		// it is not a reliable way since the scene could be in another position in the tree
		if (SceneManager.ActiveScene == null)
		{
			SceneManager.ActiveScene = GetTree().CurrentScene.GetChild(0);
			SceneManager.ActiveScene.AddToGroup("QueueFree");
		}

		// Start the fade-in animation and prepare to load the new scene.
		FadeIn().ContinueWith(_ => status = Status.LoadNewScene);
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
				// Preload the new scene if applicable and start the fade-out process.
				PreloadNewScene();
				await FadeOut().ContinueWith(_ => status = Status.Finish);
				break;

			case Status.Finish:
				// Finalize the loading process, switch scenes if necessary, delete self.
				Finish();
				break;
		}
	}

	private void PreloadNewScene()
	{
		// Ensure the new scene is loaded and added only once.
		if (!isLoaded && !SceneManager.ReplaceRoot)
		{
			isLoaded = true;
			var scene = ResourceLoader.LoadThreadedGet(SceneManager.NextScenePath) as PackedScene;
			var newScene = scene.Instantiate();
			GetTree().CurrentScene.AddChild(newScene);
			SceneManager.ActiveScene = newScene;
			SceneManager.ActiveScene.AddToGroup("QueueFree");
		}
	}

	private void Finish()
	{
		// Change to the new scene or remove the loading screen based on configuration.
		if (SceneManager.ReplaceRoot)
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
	private AnimationPlayer PlayAnim(string animation, bool backwards = false)
	{
		var anim = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
		if (backwards) anim?.PlayBackwards(animation);
		else anim?.Play(animation);
		return anim;
	}

	public async Task FadeIn()
	{
		var anim = PlayAnim("fade_in");
		if (anim == null) return;
		await ToSignal(anim, "animation_finished");
	}

	public async Task FadeOut()
	{
		var anim = PlayAnim("fade_in", true);
		if (anim == null) return;
		await ToSignal(anim, "animation_finished");
	}
}
