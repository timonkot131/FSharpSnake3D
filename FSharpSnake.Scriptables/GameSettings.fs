namespace FSharpSnake.Scriptables

open UnityEngine
open Unity.Entities
open Unity.Collections
open Unity.Transforms
open Unity.Rendering
open Unity.Mathematics

[<CreateAssetMenu(fileName = "GameSettings", menuName = "Game Settings", order = 51)>]
type GameSettings() =
    inherit ScriptableObject()

    [<SerializeField>]
    let mutable snakeMesh: Mesh = null
    [<SerializeField>]
    let mutable snakeMaterial: Material = null
    [<SerializeField>]
    let mutable snakeTick: float32 = 2.0f

    [<SerializeField>]
    let mutable lineMaterial: Material = null
    [<SerializeField>]
    let mutable lineMesh: Mesh = null
    [<SerializeField>]
    let mutable lineScale: float32 = 0.1f

    [<SerializeField>]
    let mutable foodMaterial: Material = null
    [<SerializeField>]
    let mutable foodMesh: Mesh = null

    [<SerializeField>]
    let mutable dangerZone: float32 = 3.f
    [<SerializeField>]
    let mutable dangerMaterial: Material = null

    [<SerializeField>]
    let mutable boundsSize: Vector3 = Vector3(0.f, 0.f, 0.f)
    
    [<SerializeField>]
    let mutable startPosition: float3 = float3(0.0f, 0.0f, 0.0f)

    [<SerializeField>]
    let mutable cameraTurnAngle: float32 = 10.f
    [<SerializeField>]
    let mutable cameraDistance: float32 = 20.f

    [<SerializeField>]
    let mutable compassPosition: Vector3 = Vector3(0.f, 0.f, 0.f)

    member __.SnakeMesh
        with get () = snakeMesh
    member __.SnakeMaterial
        with get () = snakeMaterial
    member __.SnakeTick
        with get () = snakeTick

    member __.LineMaterial
        with get () = lineMaterial
    member __.LineMesh
        with get () = lineMesh
    member __.LineScale
           with get () = lineScale

    member __.FoodMaterial
        with get () = foodMaterial
    member __.FoodMesh
        with get () = foodMesh

    member __.DangerZone
        with get() = dangerZone
    member __.DangerMaterial
        with get() = dangerMaterial

    member __.BoundsSize
        with get () = boundsSize

    member __.StartPosition
        with get () = startPosition

    member __.CameraDistance
        with get() = cameraDistance
    member __.CameraTurnAngle
        with get() = cameraTurnAngle

    member __.CompassPosition
           with get() = compassPosition
