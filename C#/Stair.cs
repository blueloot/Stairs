/// <summary>
/// Stair.cs - A customizable 3D stair node for Godot Engine.
/// 
/// This class provides a flexible way to create and manipulate stair structures in a 3D environment 
/// within the Godot Engine. 
/// 
/// Features include adjustable dimensions (height, width, length), the number of steps, and special 
/// properties such as floating and spiral configurations. 
/// 
/// Collision layers and masks are also customizable. The class supports dynamic updates to its properties, 
/// with corresponding visual and physical changes in the scene. 
///
/// Usage:
/// - Copy Stair.cs into your Godot project.
/// - Add new Stair nodes to your scene as required.
/// - Customize the stair properties using the Godot Editor or via scripts.
/// 
/// This class emits events for property changes, allowing for responsive design and interaction within the game or application.
/// 
/// Note: This class is designed to be used with the Godot Engine and was written in Godot 4.2
/// </summary>
/// <author>Blueloot</author>
/// <version>1.0.0b</version>
/// <changelog>
/// performance improvements:
/// - when updating collision layers, only the collision layers of the stair is updated instead of the entire stair being recreated
/// - when updating the height, only the height of the stair is updated instead of the entire stair being recreated
/// TODO: changing width, length, steps, isFloating, isSpiral, spiralAmount, should also be updated in the same way
/// </changelog>
/// <license>CC0 - Public Domain</license>

using Godot;
using System;
using System.Collections.Generic;

namespace Blueloot.Stairs;

[Tool, GlobalClass, Icon("res://icon.svg")]
public partial class Stair : Node3D
{
	public const string GROUP = "STAIRS";
	public const string CHILD_GROUP = "STAIRS_CHILDREN";
	public const string CHILD_GROUP_RAMP = "STAIRS_CHILDREN_RAMP";

	// Events
	public event Action<Stair, uint> LayerChanged;
	public event Action<Stair, uint> MaskChanged;
	public event Action<Stair, Vector3> DimensionsChanged;
	public event Action<Stair, int> StepsChanged;
	public event Action<Stair, bool> IsFloatingChanged;
	public event Action<Stair, bool> IsSpiralChanged;
	public event Action<Stair, float> ChangedSpiralAmount;
	public event Action<Stair, bool> UseRampChanged;

	// Fields
	private uint _collisionLayer = 0b1;
	private uint _collisionMask = 0b1;
	private uint _rampCollisionLayer = 0b1;
	private uint _rampCollisionMask = 0b1;
	private float _height = 10f;
	private float _width = 2f;
	private float _length = 10f;
	private int _steps = 10;
	private bool _isFloating;
	private bool _isSpiral;
	private float _spiralAmount = 2f;
	private bool _useRamp;

	// Properties
	[ExportGroup("Collision Info"), Export(PropertyHint.Layers3DPhysics)]
	public uint CollisionLayer
	{
		get => _collisionLayer;
		set => SetProperty(ref _collisionLayer, value, LayerChanged, UpdateCollisionLayers);
	}

	[ExportGroup("Collision Info"), Export(PropertyHint.Layers3DPhysics)]
	public uint CollisionMask
	{
		get => _collisionMask;
		set => SetProperty(ref _collisionMask, value, MaskChanged, UpdateCollisionLayers);
	}

	[ExportGroup("Collision Info/Ramp"), Export(PropertyHint.Layers3DPhysics)]
	public uint RampCollisionLayer
	{
		get => _rampCollisionLayer;
		set => SetProperty(ref _rampCollisionLayer, value, LayerChanged, UpdateCollisionLayers);
	}

	[ExportGroup("Collision Info/Ramp"), Export(PropertyHint.Layers3DPhysics)]
	public uint RampCollisionMask
	{
		get => _rampCollisionMask;
		set => SetProperty(ref _rampCollisionMask, value, MaskChanged, UpdateCollisionLayers);
	}

	[ExportGroup("Dimensions"), Export(PropertyHint.Range, "0.01, 50, 0.01, or_greater")]
	public float Height
	{
		get => _height;
		set => SetProperty(ref _height, value, DimensionsChanged, UpdateHeight);
	}

	[ExportGroup("Dimensions"), Export(PropertyHint.Range, "0.01, 50, 0.01, or_greater")]
	public float Width
	{
		get => _width;
		set => SetProperty(ref _width, value, DimensionsChanged, RemakeStair);
	}

	[ExportGroup("Dimensions"), Export(PropertyHint.Range, "0.01, 50, 0.01, or_greater")]
	public float Length
	{
		get => _length;
		set => SetProperty(ref _length, value, DimensionsChanged, RemakeStair);
	}

	[ExportGroup("Dimensions"), Export(PropertyHint.Range, "1, 50, 1, or_greater")]
	public int Steps
	{
		get => _steps;
		set => SetProperty(ref _steps, value, StepsChanged, RemakeStair);
	}

	[ExportGroup("Settings"), Export]
	public bool IsFloating
	{
		get => _isFloating;
		set => SetProperty(ref _isFloating, value, IsFloatingChanged, RemakeStair);
	}

	[ExportGroup("Settings"), Export]
	public bool IsSpiral
	{
		get => _isSpiral;
		set => SetProperty(ref _isSpiral, value, IsSpiralChanged, RemakeStair);
	}

	[ExportGroup("Settings"), Export(PropertyHint.Range, "0.01, 50, 0.01, or_greater")]
	public float SpiralAmount
	{
		get => _spiralAmount;
		set => SetProperty(ref _spiralAmount, value, ChangedSpiralAmount, RemakeStair);
	}

	[ExportGroup("Settings"), Export]
	public bool UseRamp
	{
		get => _useRamp;
		set => SetProperty(ref _useRamp, value, UseRampChanged, RemakeStair);
	}

	// Godot Engine Specific Methods

	public override void _Ready()
	{
		base._Ready();
		AddToGroup(GROUP);
		RemakeStair();
	}

	// Private Methods

	private void RemakeStair()
	{
		ClearChildren();
		RemakeChildren();
	}

	private void UpdateHeight()
	{
		var stepH = Height / Steps;
		var stepD = Length / Steps;
		var spiralH = 0.0f;

		foreach (Node child in GetChildren())
		{
			if (child is MeshInstance3D mesh)
			{
				if (IsFloating)
				{
					if (IsSpiral)
					{
						float angle = (mesh.GetIndex() - 1) * SpiralAmount * Mathf.Pi / Steps;
						mesh.Position = new Vector3(
							Mathf.Cos(angle) * SpiralAmount,
							(stepH * .5f) + spiralH,
							Mathf.Sin(angle) * SpiralAmount
						);
						spiralH += stepH;
						mesh.Rotation = new Vector3(0, -angle, 0);
						mesh.Scale = new Vector3(Width, stepH, stepD * (SpiralAmount + (SpiralAmount * .5f)));
					}
					else
					{
						mesh.Scale = new Vector3(Width, stepH, stepD);
						mesh.Position = new Vector3(
							0,
							(stepH * .5f) + (stepH * mesh.GetIndex()),
							(stepD * .5f) + (stepD * mesh.GetIndex())
						);
					}
				}
				else
				{
					mesh.Scale = new Vector3(Width, stepH * (mesh.GetIndex() + 1), stepD);
					mesh.Position = new Vector3(
						0,
						(stepH * .5f) + (stepH * .5f * mesh.GetIndex()),
						(stepD * .5f) + (stepD * mesh.GetIndex())
					);
				}
			}
		}
	}

	private void UpdateCollisionLayers()
	{
		foreach (Node child in GetChildren())
			CheckChild(child);

		void CheckChild(Node curChild)
		{
			SetLayer(curChild);
			foreach (var child in curChild.GetChildren())
				CheckChild(child);
		}

		void SetLayer(Node child)
		{
			if (child is StaticBody3D body)
			{
				if (body.IsInGroup(CHILD_GROUP_RAMP))
				{
					body.CollisionLayer = RampCollisionLayer;
					body.CollisionMask = RampCollisionMask;
				}
				else if (body.IsInGroup(CHILD_GROUP))
				{
					body.CollisionLayer = CollisionLayer;
					body.CollisionMask = CollisionMask;
				}
			}
		}
	}


	private void ClearChildren()
	{
		foreach (Node child in GetChildren()) child.QueueFree();
	}

	private void RemakeChildren()
	{
		var stepH = Height / Steps;
		var stepD = Length / Steps;
		var spiralH = 0.0f;

		for (int i = 0; i < Steps; i++)
		{
			var mesh = CreateMesh(stepH);
			var sBody = CreateStaticBody();
			var cShape = CreateBoxCollisionShape();

			if (IsFloating)
			{
				HandleFloatingStair(stepH, stepD, ref spiralH, i, mesh);
			}
			else
			{
				CreateNormalStair(stepH, stepD, i, mesh);
			}

			AddToNodeTree(mesh, sBody, cShape);

			if (UseRamp && !IsSpiral)
			{
				CreateCollisionRamp(stepH, stepD, i);
			}
		}
	}

	private void AddToNodeTree(MeshInstance3D mesh, StaticBody3D sBody, CollisionShape3D cShape)
	{
		AddChild(mesh);

		mesh.AddChild(sBody);
		mesh.AddToGroup(CHILD_GROUP);
		mesh.Owner = Owner;

		sBody.AddChild(cShape);
		sBody.AddToGroup(CHILD_GROUP);
		sBody.Owner = Owner;

		cShape.AddToGroup(CHILD_GROUP);
		cShape.Owner = Owner;
	}

	private void CreateCollisionRamp(float stepH, float stepD, int i)
	{
		var top = stepH / 2;
		var btm = -stepH / 2;
		var left = -Width / 2;
		var right = Width / 2;
		var front = stepD / 2;
		var back = -stepD / 2;

		Vector3[] rampVertices =
		{
			new(left, btm, back-stepD),
			new(left, top, back),
			new(right, btm, back-stepD),
			new(right, top, back)
		};

		var rampShape = new CollisionShape3D
		{
			Shape = new ConvexPolygonShape3D { Points = rampVertices }
		};

		var rampBody = new StaticBody3D()
		{
			Position = new Vector3(0, (stepH * .5f) + stepH * i, (stepD * .5f) + stepD * i)
		};

		AddChild(rampBody);
		rampBody.AddChild(rampShape);
		rampBody.AddToGroup(CHILD_GROUP_RAMP);
		rampBody.Owner = Owner;

		rampShape.AddToGroup(CHILD_GROUP_RAMP);
		rampShape.Owner = Owner;

		rampBody.CollisionLayer = RampCollisionLayer;
		rampBody.CollisionMask = RampCollisionMask;
	}

	private void CreateNormalStair(float stepH, float stepD, int i, MeshInstance3D mesh)
	{
		mesh.Scale = new Vector3(Width, stepH * (i + 1), stepD);
		mesh.Position = new Vector3(
			0,
			(stepH * .5f) + (stepH * .5f * i),
			(stepD * .5f) + (stepD * i)
		);
	}

	private void HandleFloatingStair(float stepH, float stepD, ref float spiralH, int i, MeshInstance3D mesh)
	{
		if (IsSpiral)
		{
			CreateSpiralingStair(stepH, stepD, ref spiralH, i, mesh);
		}
		else
		{
			CreateFloatingStair(stepH, stepD, i, mesh);
		}
	}

	private void CreateFloatingStair(float stepH, float stepD, int i, MeshInstance3D mesh)
	{
		mesh.Scale = new Vector3(Width, stepH, stepD);

		mesh.Position = new Vector3(
			0,
			(stepH * .5f) + (stepH * i),
			(stepD * .5f) + (stepD * i)
		);
	}

	private void CreateSpiralingStair(float stepH, float stepD, ref float spiralH, int i, MeshInstance3D mesh)
	{
		float angle = (i - 1) * SpiralAmount * Mathf.Pi / Steps;
		mesh.Position = new Vector3(
			Mathf.Cos(angle) * SpiralAmount,
			(stepH * .5f) + spiralH,
			Mathf.Sin(angle) * SpiralAmount
		);
		spiralH += stepH;
		mesh.Rotation = new Vector3(0, -angle, 0);
		mesh.Scale = new Vector3(Width, stepH, stepD * (SpiralAmount + (SpiralAmount * .5f)));
	}

	private StaticBody3D CreateStaticBody()
	{
		return new StaticBody3D
		{
			CollisionLayer = CollisionLayer,
			CollisionMask = CollisionMask
		};
	}

	private static CollisionShape3D CreateBoxCollisionShape()
	{
		return new CollisionShape3D { Shape = new BoxShape3D() };
	}

	private MeshInstance3D CreateMesh(float stepHeight)
	{
		return new MeshInstance3D
		{
			Mesh = new BoxMesh(),
			Scale = new Vector3(Width, stepHeight, Length)
		};
	}

	private void SetProperty<T>(ref T field, T value, Action<Stair, T> eventHandler, Action methodCall)
	{
		if (!EqualityComparer<T>.Default.Equals(field, value))
		{
			field = value;
			eventHandler?.Invoke(this, value);
			methodCall();
		}
	}

	private void SetProperty<T>(ref T field, T value, Action<Stair, Vector3> eventHandler, Action methodCall)
	{
		if (!EqualityComparer<T>.Default.Equals(field, value))
		{
			field = value;
			eventHandler?.Invoke(
				this,
				new Vector3(_width, _height, _length)
			);
			methodCall();
		}
	}
}
