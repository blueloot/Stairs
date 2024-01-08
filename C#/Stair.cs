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
/// <version>1.0.0</version>
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

	// Events
	public event Action<Stair, uint> LayerChanged;
	public event Action<Stair, uint> MaskChanged;
	public event Action<Stair, Vector3> DimensionsChanged;
	public event Action<Stair, int> StepsChanged;
	public event Action<Stair, bool> IsFloatingChanged;
	public event Action<Stair, bool> IsSpiralChanged;
	public event Action<Stair, float> ChangedSpiralRadius;
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
		set => SetProperty(ref _collisionLayer, value, LayerChanged);
	}

	[ExportGroup("Collision Info"), Export(PropertyHint.Layers3DPhysics)]
	public uint CollisionMask
	{
		get => _collisionMask;
		set => SetProperty(ref _collisionMask, value, MaskChanged);
	}

	[ExportGroup("Collision Info/Ramp"), Export(PropertyHint.Layers3DPhysics)]
	public uint RampCollisionLayer
	{
		get => _rampCollisionLayer;
		set => SetProperty(ref _rampCollisionLayer, value, LayerChanged);
	}

	[ExportGroup("Collision Info/Ramp"), Export(PropertyHint.Layers3DPhysics)]
	public uint RampCollisionMask
	{
		get => _rampCollisionMask;
		set => SetProperty(ref _rampCollisionMask, value, MaskChanged);
	}

	[ExportGroup("Dimensions"), Export(PropertyHint.Range, "0.01, 50, 0.01, or_greater")]
	public float Height
	{
		get => _height;
		set => SetProperty(ref _height, value, DimensionsChanged);
	}

	[ExportGroup("Dimensions"), Export(PropertyHint.Range, "0.01, 50, 0.01, or_greater")]
	public float Width
	{
		get => _width;
		set => SetProperty(ref _width, value, DimensionsChanged);
	}

	[ExportGroup("Dimensions"), Export(PropertyHint.Range, "0.01, 50, 0.01, or_greater")]
	public float Length
	{
		get => _length;
		set => SetProperty(ref _length, value, DimensionsChanged);
	}

	[ExportGroup("Dimensions"), Export(PropertyHint.Range, "1, 50, 1, or_greater")]
	public int Steps
	{
		get => _steps;
		set => SetProperty(ref _steps, value, StepsChanged);
	}

	[ExportGroup("Settings"), Export]
	public bool IsFloating
	{
		get => _isFloating;
		set => SetProperty(ref _isFloating, value, IsFloatingChanged);
	}

	[ExportGroup("Settings"), Export]
	public bool IsSpiral
	{
		get => _isSpiral;
		set => SetProperty(ref _isSpiral, value, IsSpiralChanged);
	}

	[ExportGroup("Settings"), Export(PropertyHint.Range, "0.01, 50, 0.01, or_greater")]
	public float SpiralAmount
	{
		get => _spiralAmount;
		set => SetProperty(ref _spiralAmount, value, ChangedSpiralRadius);
	}

	[ExportGroup("Settings"), Export]
	public bool UseRamp
	{
		get => _useRamp;
		set => SetProperty(ref _useRamp, value, UseRampChanged);
	}

	// Godot Engine

	public override void _Ready()
	{
		base._Ready();
		AddToGroup(GROUP);
		UpdateStair();
	}

	// Private Methods

	private void UpdateStair()
	{
		ClearChildren();
		CreateChildren();
	}

	private void ClearChildren()
	{
		foreach (Node child in GetChildren()) child.QueueFree();
	}

	private void CreateChildren()
	{
		var stepHeight = Height / Steps;
		var stepDepth = Length / Steps;
		var spiralHeight = 0.0f;

		for (int i = 0; i < Steps; i++)
		{
			var mesh = CreateMesh(stepHeight);
			var sBody = CreateStaticBody();
			var cShape = CreateBoxCollisionShape();

			if (IsFloating)
			{
				HandleFloatingStair(stepHeight, stepDepth, ref spiralHeight, i, mesh);
			}
			else
			{
				HandleNormalStair(stepHeight, stepDepth, i, mesh);
			}

			AddToNodeTree(mesh, sBody, cShape);

			if (UseRamp && !IsSpiral)
			{
				CreateCollisionRamp(stepHeight, stepDepth, i);
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

	private void CreateCollisionRamp(float stepHeight, float stepDepth, int i)
	{
		var top = stepHeight / 2;
		var btm = -stepHeight / 2;
		var left = -Width / 2;
		var right = Width / 2;
		var front = stepDepth / 2;
		var back = -stepDepth / 2;

		Vector3[] rampVertices =
		{
			new(left, btm, back-stepDepth),
			new(left, top, back),
			new(right, btm, back-stepDepth),
			new(right, top, back)
		};

		var rampShape = new CollisionShape3D
		{
			Shape = new ConvexPolygonShape3D { Points = rampVertices }
		};

		var rampBody = new StaticBody3D()
		{
			Position = new Vector3(0, (stepHeight * .5f) + stepHeight * i, (stepDepth * .5f) + stepDepth * i)
		};

		AddChild(rampBody);
		rampBody.AddChild(rampShape);
		rampShape.Owner = Owner;
		rampBody.Owner = Owner;

		rampBody.CollisionLayer = RampCollisionLayer;
		rampBody.CollisionMask = RampCollisionMask;
	}

	private void HandleNormalStair(float stepHeight, float stepDepth, int i, MeshInstance3D mesh)
	{
		mesh.Scale = new Vector3(Width, stepHeight * (i + 1), stepDepth);
		mesh.Position = new Vector3(
			0,
			(stepHeight * .5f) + (stepHeight * .5f * i),
			(stepDepth * .5f) + (stepDepth * i)
		);
	}

	private void HandleFloatingStair(float stepHeight, float stepDepth, ref float spiralHeight, int i, MeshInstance3D mesh)
	{
		if (IsSpiral)
		{
			CreateSpiralingStair(stepHeight, stepDepth, ref spiralHeight, i, mesh);
		}
		else
		{
			CreateNormalStair(stepHeight, stepDepth, i, mesh);
		}
	}

	private void CreateNormalStair(float stepHeight, float stepDepth, int i, MeshInstance3D mesh)
	{
		mesh.Scale = new Vector3(Width, stepHeight, stepDepth);

		mesh.Position = new Vector3(
			0,
			(stepHeight * .5f) + (stepHeight * i),
			(stepDepth * .5f) + (stepDepth * i)
		);
	}

	private void CreateSpiralingStair(float stepHeight, float stepDepth, ref float spiralHeight, int i, MeshInstance3D mesh)
	{
		float angle = (i - 1) * SpiralAmount * Mathf.Pi / Steps;
		mesh.Position = new Vector3(
			Mathf.Cos(angle) * SpiralAmount,
			(stepHeight * .5f) + spiralHeight,
			Mathf.Sin(angle) * SpiralAmount
		);
		spiralHeight += stepHeight;
		mesh.Rotation = new Vector3(0, -angle, 0);
		mesh.Scale = new Vector3(Width, stepHeight, stepDepth * (SpiralAmount + (SpiralAmount * .5f)));
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

	private void SetProperty<T>(ref T field, T value, Action<Stair, T> eventHandler)
	{
		if (!EqualityComparer<T>.Default.Equals(field, value))
		{
			field = value;
			eventHandler?.Invoke(this, value);
			UpdateStair();
		}
	}

	private void SetProperty<T>(ref T field, T value, Action<Stair, Vector3> eventHandler)
	{
		if (!EqualityComparer<T>.Default.Equals(field, value))
		{
			field = value;
			eventHandler?.Invoke(
				this,
				new Vector3(_width, _height, _length)
			);
			UpdateStair();
		}
	}
}
