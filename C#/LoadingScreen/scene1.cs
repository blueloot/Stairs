using Godot;
using Blueloot.Loading;

public partial class scene1 : Control
{
	public override void _Ready()
	{
		var btn = GetNodeOrNull<Button>("Button");
		btn.Pressed += () =>
		{
			SceneManager.Load("res://C#/LoadingScreen/scene2.tscn", "fade_in");
		};
	}
}
