# Spatial Obby Template

## Quick Start
The simplest way to get started is by working with/making a copy of the `ObbyTemplate` scene. It has all the required manager objects already created and a few example obby nodes.

### Creating a new ObbyNode
- Add a copy of the `EmptyObbyNode` prefab into the scene
- Drag the obby node into the `Nodes` list of the `ObbyCourse` you want to add it to
  - Reorder the nodes in the list to change their order in the course
- Add any obby elements/objects you want!
  - It's best to have all the objects of a node as children of the `ObbyNode`.
- Make sure to move the `Target` GameObject to where you want the *next* node to start.
  - Changing the rotation of the `Target` GameObject also determines the orientation of the next node

## Documentation

### Obby Platform
The `ObbyPlatform` component is used to apply effects to the player and register custom collision events.
- `Actor Effect` various effects applied to the player during contact with the platform.
  - `None` does nothing, use if you want to to add your own custom collision events.
  - `Kill` kills the player on touch.
  - `Force` applies a force on the player while they are in contact with the platform
    - Applies a vector representing the force onto the player in either local or world space
  - `Trampoline` bounces the player into the air on touch
    - You can customize the height the player bounces with the `TrampolineHeight` field.
- Events
  - `OnPlayerEnter` is invoked the first frame a player collides with the platform, does not get invoked again until player stops touching the platform
  - `OnPlayerExit` is invoked the first frame the player leaves the platform after having been in contact with it
  - `OnPlayerStay` is invoked every frame the player is in contact with the platform

### Obby Zone
The `Obby Zone` component acts as a trigger: the player does not collide with it, but will register events while the player is inside.
- `Actor Effect`
  - `None`, `Kill`, `Force` are the same as the `ObbyPlatform` actor effects
  - `Speed Multiplier` multiplies the player's speed by a given value while they are inside the zone
    - Multiplier can be negative (reversed directions), magnitude ranges from `0.1` to `10`.
- Events
  -  `OnPlayerEnter`, `OnPlayerExit`, `OnPlayerStay` are the same as `ObbyPlatform`

### Obby Waypoint Carousel
Moves any number of platforms along a path or loop dictated by waypoints.  
*Requires at least 2 waypoints.*
- `Waypoints` list of transforms that sequentially form the path the platforms will follow.
- `Platforms` list of transforms representing the platforms that will follow the path. Don't necessarily have to be `ObbyPlatforms`.
- `LoopType`
  - `Loop` turns the waypoint path into a loop by connecting the first and last waypoints.
  - `PingPong` platforms reverse direction upon reaching either end of the path.
    - `Pause At Ends` if enabled, exposes a field that represents the time in seconds to pause at each end of the path.
- `SpacingType`
  - `Max` platforms are evenly spaced as far apart as possible.
  - `Custom` exposes a field that allows you to add any amount of spacing between `0` and the maximum spacing.

### Obby Node
Each `ObbyNode` has a `Node Platform` and a `Target` gameobject.
- `Node Platform` is used for checkpoint purposes, by default it is the platform of the node itself.
  - If you want to add a custom checkpoint object, you must assign a different `Obby Platform` to the `Node Platform` field.
- The position and rotation of the `Target` GameObject determines the orientation and position of the next node in the course.

### Obby Game Manager
Required for obby functionality
- `Default Course` the course the player starts at by default, currently also determines which course is loaded when rejoining
- `Allow Course Hopping` in a space with multiple courses, allows players to switch between courses midway.  
  - If false, the player will be teleported back to the last node they were on in the previous course.
- `Teleport Player To Node On Start` enables save/load, teleports player to last node they previously reached upon joining
  - If player is new, teleports to first node.
- `New Node Particles` particle effect played when player reaches new node.

### Obby Smooth Camera
- `Freeze Camera Duration` how long to freeze the camera upon player death (seconds).
- `Death Particles` particle effect played when player dies.
