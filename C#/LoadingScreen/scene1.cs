using Godot;
using Blueloot.Loading;

public partial class scene1 : Control
{
	public override void _Ready()
	{

		// replace root mode.
		// this is used to tell the loading screen to add itself as a child of root or replace root.
		// if this is set to true, the loading screen will replace the root scene
		//		(fade in/out effect using a blank canvas)
		// if this is set to false, the loading screen will be added as a child of root
		//		(fade in/out effect using new/old scene)
		// default is [false]
		SceneManager.SetReplaceRootMode(true);

		// set the active scene to this scene to tell the loading screen which scene
		// it should queue free after loading
		SceneManager.ActiveScene = this;

		// what scene to load on button press
		var newScene = "res://C#/LoadingScreen/scene2.tscn";

		// get the button and add a pressed signal
		var btn = GetNodeOrNull<Button>("Button");
		btn.Pressed += () => SceneManager.Load(newScene);
	}
}
