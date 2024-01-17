using Godot;
using Blueloot.Loading;

public partial class scene1 : Control
{
	public override void _Ready()
	{
		var btn = GetNodeOrNull<Button>("Button");

		var newScene = "res://C#/LoadingScreen/scene2.tscn";

		btn.Pressed += () => SceneManager.Load(newScene);
	}
}
