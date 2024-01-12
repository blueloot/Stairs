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
/// This class emits events for property changes, allowing for responsive design and interaction within 
/// the game or application.
/// 
/// Note: This class is designed to be used with the Godot Engine and was written in Godot 4.2
/// </summary>
/// <author>Blueloot</author>
/// <version>1.0.1</version>
/// <changelog>
/// performance improvements:
/// - when making changes to the stair, only the affected steps are updated instead of a full rebuild
/// TODO: improve performance on "changing step count" by only adding/removing steps as needed
/// TODO: update ramp as well (CreateCollisionRamp(float stepH, float stepD, int i))
/// </changelog>
/// <license>CC0 - Public Domain</license>

using Godot;
using System;
using System.Collections.Generic;

namespace Blueloot.Stairs;

[Tool, GlobalClass, Icon("res://icon.svg")]
public partial class Stair : Node3D
{
	// Constants
	private const string GROUP = "STAIRS";
	private const string CHILD_GROUP = "STAIRS_CHILDREN";
	private const string CHILD_GROUP_RAMP = "STAIRS_CHILDREN_RAMP";

	// Events
	public event Action<Stair, uint> LayerChanged;
	public event Action<Stair, uint> MaskChanged;
	public event Action<Stair, Vector3> DimensionsChanged;
	public event Action<Stair, int> StepsChanged;
	public event Action<Stair, bool> IsFloatingChanged;
	public event Action<Stair, bool> IsSpiralChanged;
	public event Action<Stair, float> ChangedSpiralAmount;
	public event Action<Stair, bool> UseRampChanged;
	public event Action<Stair, BaseMaterial3D> MaterialChanged;

	// Fields
	private BaseMaterial3D _material;
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
	private float _spiralAmount = 2.04f;
	private bool _useRamp;

	// Properties
	[ExportGroup("Appearance"), Export]
	public BaseMaterial3D Material
	{
		get => _material;
		set => SetProperty(ref _material, value, MaterialChanged, UpdateMaterial);
	}

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
		set => SetProperty(ref _height, value, DimensionsChanged, UpdateStair);
	}

	[ExportGroup("Dimensions"), Export(PropertyHint.Range, "0.01, 50, 0.01, or_greater")]
	public float Width
	{
		get => _width;
		set => SetProperty(ref _width, value, DimensionsChanged, UpdateStair);
	}

	[ExportGroup("Dimensions"), Export(PropertyHint.Range, "0.01, 50, 0.01, or_greater")]
	public float Length
	{
		get => _length;
		set => SetProperty(ref _length, value, DimensionsChanged, UpdateStair);
	}

	[ExportGroup("Dimensions"), Export(PropertyHint.Range, "1, 50, 1, or_greater")]
	public int Steps
	{
		get => _steps;
		set => SetProperty(ref _steps, value, StepsChanged, InitializeStair);
	}

	[ExportGroup("Settings"), Export]
	public bool IsFloating
	{
		get => _isFloating;
		set => SetProperty(ref _isFloating, value, IsFloatingChanged, UpdateStair);
	}

	[ExportGroup("Settings"), Export]
	public bool IsSpiral
	{
		get => _isSpiral;
		set => SetProperty(ref _isSpiral, value, IsSpiralChanged, UpdateStair);
	}

	[ExportGroup("Settings"), Export(PropertyHint.Range, "0.01, 50, 0.01, or_greater")]
	public float SpiralAmount
	{
		get => _spiralAmount;
		set => SetProperty(ref _spiralAmount, value, ChangedSpiralAmount, UpdateStair);
	}

	[ExportGroup("Settings"), Export]
	public bool UseRamp
	{
		get => _useRamp;
		set => SetProperty(ref _useRamp, value, UseRampChanged, InitializeStair);
	}

	// Godot Engine Specific Methods

	public override void _Ready()
	{
		base._Ready();
		AddToGroup(GROUP);
		InitializeStair();
	}

	// Private Methods

	private void InitializeStair()
	{
		ClearChildren();
		BuildStairStructure();
	}

	private void UpdateStair()
	{
		var stepH = Height / Steps;
		var stepD = Length / Steps;
		var spiralH = 0.0f;
		var index = 0;

		foreach (Node child in GetChildren())
		{
			if (child is MeshInstance3D mesh)
			{
				SetDimensionForThisStepIndex(stepH, stepD, ref spiralH, mesh, index);
				index++;
			}
		}

		index = 0;
		if (UseRamp && !IsSpiral)
		{
			foreach (Node child in GetChildren())
			{
				if (child is StaticBody3D rampBody)
				{
					rampBody.Position = new Vector3(0, (stepH * .5f) + stepH * index, (stepD * .5f) + stepD * index);

					var top = stepH * .5f;
					var btm = -stepH * .5f;
					var left = -Width * .5f;
					var right = Width * .5f;
					var front = stepD * .5f;
					var back = -stepD * .5f;

					Vector3[] rampVertices =
					{
						new(left, btm, back-stepD),
						new(left, top, back),
						new(right, btm, back-stepD),
						new(right, top, back)
					};

					(rampBody.GetChild<CollisionShape3D>(0).Shape as ConvexPolygonShape3D).Points = rampVertices;

					index++;
				}
			}
		}
	}


	private void UpdateMaterial()
	{
		foreach (Node child in GetChildren())
		{
			if (child is MeshInstance3D mesh)
			{
				mesh.MaterialOverride = Material;
			}
		}
	}

	private void SetDimensionForThisStepIndex(float stepH, float stepD, ref float spiralH, MeshInstance3D mesh, int i)
	{
		if (IsFloating)
		{
			if (IsSpiral)
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
			else
			{
				mesh.Scale = new Vector3(Width, stepH, stepD);
				mesh.Position = new Vector3(
					0,
					(stepH * .5f) + (stepH * i),
					(stepD * .5f) + (stepD * i)
				);
				mesh.Rotation = new Vector3(0, 0, 0);
			}
		}
		else
		{
			mesh.Scale = new Vector3(Width, stepH * (i + 1), stepD);
			mesh.Position = new Vector3(
				0,
				(stepH * .5f) + (stepH * .5f * i),
				(stepD * .5f) + (stepD * i)
			);
			mesh.Rotation = new Vector3(0, 0, 0);
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

	private void BuildStairStructure()
	{
		var stepH = Height / Steps;
		var stepD = Length / Steps;
		var spiralH = 0.0f;

		for (int i = 0; i < Steps; i++)
		{
			var mesh = new MeshInstance3D
			{
				Mesh = new BoxMesh(),
				Scale = new Vector3(Width, stepH, Length)
			};

			var sBody = new StaticBody3D
			{
				CollisionLayer = CollisionLayer,
				CollisionMask = CollisionMask
			};

			var cShape = new CollisionShape3D { Shape = new BoxShape3D() };

			SetDimensionForThisStepIndex(stepH, stepD, ref spiralH, mesh, i);

			AddChild(mesh);
			mesh.AddChild(sBody);
			mesh.AddToGroup(CHILD_GROUP);
			mesh.Owner = Owner;
			sBody.AddChild(cShape);
			sBody.AddToGroup(CHILD_GROUP);
			sBody.Owner = Owner;
			cShape.AddToGroup(CHILD_GROUP);
			cShape.Owner = Owner;

			if (UseRamp && !IsSpiral)
			{
				CreateCollisionRamp(stepH, stepD, i);
			}
		}

		UpdateMaterial();
	}

	private void CreateCollisionRamp(float stepH, float stepD, int i)
	{
		var top = stepH * .5f;
		var btm = -stepH * .5f;
		var left = -Width * .5f;
		var right = Width * .5f;
		var front = stepD * .5f;
		var back = -stepD * .5f;

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
