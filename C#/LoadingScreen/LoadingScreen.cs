using System.Threading.Tasks;
using Godot;
using GC = Godot.Collections;

namespace Blueloot.Loading;

public static class SceneManager
{
	public static readonly string Loader = "res://C#/LoadingScreen/LoadingScreen.tscn";
	public static string NextScenePath { get; private set; } = "";
	public static string Animation { get; private set; } = "";

	private static SceneTree GetTree() => Engine.GetMainLoop() as SceneTree;

	public static void Load(string scenePath, string animation = "fade_in")
	{
		NextScenePath = scenePath;
		Animation = animation;

		if (ResourceLoader.Load(Loader) is PackedScene loaderScene)
		{
			var loaderNode = loaderScene.Instantiate();
			if (loaderNode is Node loaderAsNode)
			{
				var currentScene = GetTree().CurrentScene;
				if (currentScene != null)
				{
					GetTree().Root.RemoveChild(currentScene);
					currentScene.QueueFree();
				}
				GetTree().Root.AddChild(loaderAsNode);
				GetTree().CurrentScene = loaderAsNode;
			}
		}
		else
		{
			GD.PrintErr("Failed to load loader scene");
		}
	}

}

public partial class LoadingScreen : CanvasLayer
{
	private enum Status
	{
		Idle,
		Show,
		Begin,
		Process,
		Hide,
		Finish
	}
	private Status status = Status.Idle;

	public bool useSubThreads = true;

	private bool Loading = false;

	public override void _Ready()
	{
		status = Status.Show;

		var anim = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
		anim?.Play("idle");

		ResourceLoader.LoadThreadedRequest(SceneManager.NextScenePath, "", useSubThreads);
	}

	public async override void _Process(double delta)
	{
		switch (status)
		{
			case Status.Show:
				await FadeIn();
				status = Status.Begin;
				break;

			case Status.Begin:
				Begin();
				status = Status.Process;
				break;

			case Status.Process:
				if (Process() == ResourceLoader.ThreadLoadStatus.Loaded)
					status = Status.Hide;
				break;

			case Status.Hide:
				if (!Loading)
				{
					await FadeOut();
					Loading = true;
					status = Status.Finish;
				}
				break;

			case Status.Finish:
				if (Loading)
				{
					var scene = ResourceLoader.LoadThreadedGet(SceneManager.NextScenePath);
					GetTree().ChangeSceneToPacked(scene as PackedScene);
				}
				Finish();
				break;
		}
	}

	private void Finish()
	{
		// delete self
	}


	private void Begin()
	{
		// delete old scene	
	}

	private ResourceLoader.ThreadLoadStatus Process()
	{
		GC.Array progress = new();
		var status = ResourceLoader.LoadThreadedGetStatus(SceneManager.NextScenePath, progress);

		float percent = (float)progress[0] * 100;

		var pb = GetNodeOrNull<ProgressBar>("ProgressBar");
		var label = GetNodeOrNull<Label>("Label");

		if (pb != null) pb.Value = percent;
		if (label != null) label.Text = $"{percent}%";

		return status;
	}


	// animations
	public async Task FadeIn()
	{
		var anim = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
		anim?.Play("fade_in");
		await ToSignal(anim, "animation_finished");
	}
	public async Task FadeOut()
	{
		var anim = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");
		anim?.PlayBackwards("fade_in");
		await ToSignal(anim, "animation_finished");
	}

}
