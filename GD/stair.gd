## Stair.gd - A customizable 3D stair node for Godot Engine.
##
## This class provides a flexible way to create and manipulate stair structures in a 3D environment 
## within the Godot Engine. 
##
## Features include adjustable dimensions (height, width, length), the number of steps, and special 
## properties such as floating and spiral configurations. 
## 
## Collision layers and masks are also customizable. The class supports dynamic updates to its properties, 
## with corresponding visual and physical changes in the scene. 
##
## Usage:
## - Copy Stair.gd into your Godot project.
## - Add new Stair nodes to your scene as required.
## - Customize the stair properties using the Godot Editor or via scripts.
##  
## This class emits events for property changes, allowing for responsive design and interaction within 
## the game or application.
## 
## Note: This class is designed to be used with the Godot Engine and was written in Godot 4.2
##
## Author: Blueloot
##
## Version: 1.0.1
## 
## Changelog:
## 	performance improvements:
##		- when making changes to the stair, only the affected steps are updated instead of a full rebuild
##
## Licence: CC0 - Public Domain
##
## Url: https://github.com/blueloot/Stairs/tree/main

## TODO: improve performance on "changing step count" by only adding/removing steps as needed

@tool
@icon("res://icon.svg")

class_name StairGD extends Node3D

const GROUP      : 		String = "STAIRS"
const CHILD_GROUP:		String = "STAIRS_CHILDREN"
const CHILD_GROUP_RAMP: String = "STAIRS_CHILDREN_RAMP"

signal layer_changed
signal mask_changed
signal dimensions_changed
signal steps_changed
signal is_floating_changed
signal is_spiral_changed
signal sprial_amount_changed
signal use_ramp_changed
signal material_changed

#region Export vars 
@export_group("Materials")
var material: BaseMaterial3D = null:
	set(value):
		material = value
		material_changed.emit(value)
		update_material()

@export_group("Collision Info")
@export_flags_3d_physics()
var collision_layer: int = 0b1:
	set(value):
		collision_layer = value
		layer_changed.emit(value)
		update_collision_layers()

@export_group("Collision Info")
@export_flags_3d_physics()
var collision_mask: int = 0b1:
	set(value):
		collision_mask = value
		mask_changed.emit(value)
		update_collision_layers()

@export_group("Collision Info/Ramp")
@export_flags_3d_physics()
var ramp_collision_layer: int = 0b1:
	set(value):
		ramp_collision_layer = value
		layer_changed.emit(value)
		update_collision_layers()

@export_group("Collision Info/Ramp")
@export_flags_3d_physics()
var ramp_collision_mask: int = 0b1:
	set(value):
		ramp_collision_mask = value
		mask_changed.emit(value)
		update_collision_layers()

@export_group("Dimensions")
@export_range(0.01, 50, 0.01, "or_greater")
var height: float = 10:
	set(value):
		height = value
		dimensions_changed.emit(width,height,length)
		update_stair()

@export_group("Dimensions")
@export_range(0.01, 50, 0.01, "or_greater")
var width: float = 1:
	set(value):
		width = value
		dimensions_changed.emit(width,height,length)
		update_stair()
		
@export_group("Dimensions")
@export_range(0.01, 50, 0.01, "or_greater")
var length: float = 10:
	set(value):
		length = value
		dimensions_changed.emit(width,height,length)
		update_stair()

@export_group("Dimensions")
@export_range(1, 50, 1, "or_greater")
var steps: float = 10:
	set(value):
		steps = value
		steps_changed.emit(steps)
		initialize_stairs()

@export_group("Settings")
@export
var is_floating: bool = false:
	set(value):
		is_floating = value
		is_floating_changed.emit(value)
		update_stair()

@export_group("Settings")
@export
var is_spiral: bool = false:
	set(value):
		is_spiral = value
		is_spiral_changed.emit(value)
		update_stair()

@export_group("Settings")
@export_range(0.01, 50, 0.01, "or_greater")
var spiral_amount: float = 2:
	set(value):
		spiral_amount = value
		sprial_amount_changed.emit(value)
		update_stair()

@export_group("Settings")
@export
var use_ramp: bool = false:
	set(value):
		use_ramp = value
		use_ramp_changed.emit(value)
		initialize_stairs()
#endregion vars

func _ready() -> void:
	add_to_group(GROUP)
	initialize_stairs()
	
func initialize_stairs() -> void:
	clear_children()
	build_stair_structure()

func clear_children() -> void:
	for child:Node in get_children():
		child.queue_free()

func build_stair_structure() -> void:
	var step_h :	float = height / steps
	var step_d :	float = length / steps
	var spiral_h :	float = 0
	
	for i:int in steps:
		var mesh : MeshInstance3D = MeshInstance3D.new()
		mesh.mesh = BoxMesh.new()
		mesh.scale = Vector3(width, step_h, length)
		
		var sbody : StaticBody3D = StaticBody3D.new()
		sbody.collision_layer = collision_layer
		sbody.collision_mask = collision_mask
		
		var cshape : CollisionShape3D = CollisionShape3D.new()
		cshape.shape = BoxShape3D.new()

		spiral_h = set_dimensions_for_this_step_index(step_h, step_d, spiral_h, mesh, i)

		add_child(mesh)
		mesh.add_child(sbody)
		mesh.add_to_group(CHILD_GROUP)
		mesh.owner = owner
		sbody.add_child(cshape)
		sbody.add_to_group(CHILD_GROUP)
		sbody.owner = owner
		cshape.add_to_group(CHILD_GROUP)
		cshape.owner = owner

		if (use_ramp && !is_spiral):
			create_collision_ramp(step_h, step_d, i)
	
	update_material()

func set_dimensions_for_this_step_index(step_h:float, step_d:float, spiral_h:float, mesh:MeshInstance3D, i:int) -> float:
	if is_floating:
		if is_spiral:
			var angle:float = (i-1) * spiral_amount * PI / steps
			mesh.position = Vector3(
				cos(angle)*spiral_amount, 
				(step_h*.5)+spiral_h, 
				sin(angle)*spiral_amount)
			spiral_h += step_h
			mesh.rotation = Vector3(0, -angle, 0)
			mesh.scale = Vector3(width, step_h, step_d*(spiral_amount+(spiral_amount*.5)))
		else :
			mesh.scale = Vector3(width, step_h, step_d)
			mesh.position = Vector3(
				0, 
				(step_h*.5) + (step_h*i), 
				(step_d*.5) + (step_d*i))
			mesh.rotation = Vector3(0, 0, 0)
	else:
		mesh.scale = Vector3(width, step_h*(i+1), step_d)
		mesh.position = Vector3(
			0, 
			(step_h*.5) + (step_h*.5*i), 
			(step_d*.5) + (step_d*i))
		mesh.rotation = Vector3(0, 0, 0)
	return spiral_h

func update_material() -> void:
	for child:Node in get_children():
		if child is MeshInstance3D:
			var mesh:MeshInstance3D = child as MeshInstance3D
			mesh.material_override = material

func create_collision_ramp(step_h:float, step_d:float, i:int) -> void:
	var top = step_h * 0.5
	var btm = -step_h * 0.5
	var left = -width * 0.5
	var right = width * 0.5
	var back = -step_d * 0.5
	
	var ramp_vertices:Array[Vector3] = []
	ramp_vertices.append(Vector3(left, btm, back-step_d))
	ramp_vertices.append(Vector3(left, top, back))
	ramp_vertices.append(Vector3(right, btm, back-step_d))
	ramp_vertices.append(Vector3(right, top, back))
	
	var ramp_shape:CollisionShape3D = CollisionShape3D.new()
	ramp_shape.shape = ConvexPolygonShape3D.new()
	ramp_shape.shape.points = ramp_vertices
	
	var ramp_body: StaticBody3D = StaticBody3D.new()
	ramp_body.position = Vector3(0, (step_h*.5)+step_h*i, (step_d*.5) + step_d*i)
	
	add_child(ramp_body)
	ramp_body.add_child(ramp_shape)
	ramp_body.add_to_group(CHILD_GROUP_RAMP)
	ramp_body.owner = owner
	
	ramp_shape.add_to_group(CHILD_GROUP_RAMP)
	ramp_shape.owner = owner

	ramp_body.collision_layer = ramp_collision_layer
	ramp_body.collision_mask = ramp_collision_mask

func update_stair() -> void:
	var stepH: float = height / steps
	var stepD: float = length / steps
	var spiralH: float = 0
	var index: int = 0

	for child:Node in get_children():
		if child is MeshInstance3D:
			spiralH = set_dimensions_for_this_step_index(stepH, stepD, spiralH, child as MeshInstance3D, index)
			index += 1
	
	index = 0
	if (use_ramp && !is_spiral):
		for child:Node in get_children():
			if child is StaticBody3D:
				var ramp_body: StaticBody3D = child as StaticBody3D
				ramp_body.position = Vector3(0, (stepH*.5)+stepH*index, (stepD*.5) + stepD*index)

				var top: float = stepH * 0.5
				var btm: float = -stepH * 0.5
				var left: float = -width * 0.5
				var right: float = width * 0.5
				# var front: float = stepD * 0.5
				var back: float = -stepD * 0.5

				var ramp_vertices:Array[Vector3] = []
				ramp_vertices.append(Vector3(left, btm, back-stepD))
				ramp_vertices.append(Vector3(left, top, back))
				ramp_vertices.append(Vector3(right, btm, back-stepD))
				ramp_vertices.append(Vector3(right, top, back))

				ramp_body.get_child(0).shape.points = ramp_vertices

				index += 1

func update_collision_layers() -> void:
	for child:Node in get_children():
		check_child(child)
	
func check_child(child:Node) -> void:
	set_layer(child)
	for c:Node in child.get_children():
		check_child(c)

func set_layer(child:Node) -> void:
	if child is StaticBody3D:
		var body:StaticBody3D = child as StaticBody3D
		if body.is_in_group(CHILD_GROUP_RAMP):
			body.collision_layer = ramp_collision_layer
			body.collision_mask = ramp_collision_mask
		elif body.is_in_group(CHILD_GROUP):
			body.collision_layer = collision_layer
			body.collision_mask = collision_mask
