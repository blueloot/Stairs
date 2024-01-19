using Godot;
using Blueloot.Loading;

public partial class scene1 : Control
{
	public override void _Ready()
	{

		// disable the replace root mode.
		// this is used to tell the loading screen to add itself as a child of root
		// if this is set to true, the loading screen will replace the root scene
		SceneManager.SetReplaceRootMode(true);

		// set the active scene to this scene to tell the loading screen which scene it should queue free
		// SceneManager.ActiveScene = this;


		// what scene to load on button press
		var newScene = "res://C#/LoadingScreen/scene2.tscn";

		// get the button and add a pressed signal
		var btn = GetNodeOrNull<Button>("Button");
		btn.Pressed += () => SceneManager.Load(newScene);
	}
}
