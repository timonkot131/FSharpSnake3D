namespace FSharpSnake.Components

open UnityEngine
open Unity.Entities
open Unity.Collections
open Unity.Transforms
open Unity.Rendering
open Unity.Mathematics

[<Struct>]
type SnakeEnd = interface IComponentData

type SnakeSegments  = 
    struct interface IComponentData
       val direction : float3
    end

[<Struct>]
type SnakeHead = interface IComponentData

type PlayerInpur = 
    struct interface IComponentData
         val direction : float3
    end

type BoundLine =
    struct interface IComponentData
        val p1: float3
        val p2: float3
        new(p1:float3, p2:float3) = {p1 = p1; p2 = p2}
    end

[<Struct>]
type Tick = interface IComponentData

type Delay =
    struct interface IComponentData
        val duration: float32
        new(duration: float32) = {duration = duration}
    end

type SnakeDirection =
    struct interface IComponentData
        val direction: float3
        new(direction: float3) = {direction = direction}
    end

[<Struct>]
type GameOver = interface IComponentData

[<Struct>]
type GameRestart = interface IComponentData

type SnakeArrayBuffer =
    struct interface IBufferElementData
        val pos: float3
        new(pos: float3) = {pos = pos}
    end

[<Struct>]
type SnakeCollision = interface IComponentData

[<Struct>]
type public SnakeFood = interface IComponentData

[<Struct>]
type SnakeMoved = interface IComponentData

[<Struct>]
type PausePressed = interface IComponentData