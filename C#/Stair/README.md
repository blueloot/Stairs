# Stair

## Overview

`Stair.cs` is a customizable 3D stair node for the Godot Engine. This class offers a flexible way to create and manipulate stair structures in a 3D environment within Godot.

### Features

- **Adjustable Dimensions**: Customize the height, width, and length of the stairs.
- **Variable Step Count**: Define the number of steps.
- **Special Properties**: Includes options for floating and spiral stair configurations.
- **Customizable Collision Layers and Masks**: Tailor collision properties to suit your scene.
- **Dynamic Property Updates**: Changes to properties result in corresponding visual and physical updates in the scene.
- **Event Emissions**: The class emits events for property changes, enabling responsive design and interaction.
- **Materials**: Apply materials like any normal godot 3d mesh instance

### Usage

1. Copy `Stair.cs` into your Godot project.
2. Add new Stair nodes to your scene as required.
3. Customize the stair properties using the Godot Editor or via scripts.

### Note

This class is designed for use with the Godot Engine and was written for Godot 4.2.

## Screenshots

![Stair Example 1](/ss1.png?raw=true "Stair Example 1")
![Stair Example 2](/ss2.png?raw=true "Stair Example 2")
![Stair Example 3](/ss3.png?raw=true "Stair Example 3")

## Version

- 1.0.1

### Changelog

- **1.0.1**
  - Performance improvements: When updating the stair, only the affected steps are updated instead of a full rebuild.
- **1.0.0**
  - Initial release

## License

This project is licensed under the CC0 - Public Domain. Feel free to use it in your projects.

## More Information

For more details, visit the [GitHub repository](https://github.com/blueloot/Stairs/blob/main/C%23/Stair.cs).