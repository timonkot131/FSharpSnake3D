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
    let mutable boundsSize: Vector3 = Vector3(0.f, 0.f, 0.f)

    [<SerializeField>]
    let mutable startPosition: float3 = float3(0.0f, 0.0f, 0.0f)

    member this.SnakeMesh
        with get () = snakeMesh
    member this.SnakeMaterial
        with get () = snakeMaterial
    member this.SnakeTick
        with get () = snakeTick

    member this.LineMaterial
        with get () = lineMaterial
    member this.LineMesh
        with get () = lineMesh
    member this.LineScale
           with get () = lineScale

    member this.BoundsSize
        with get () = boundsSize

    member this.StartPosition
        with get () = startPosition