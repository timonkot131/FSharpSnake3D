namespace FSharpSnake.Components

open UnityEngine
open Unity.Entities
open Unity.Collections
open Unity.Transforms
open Unity.Rendering
open Unity.Mathematics

type SnakeEnd = 
    struct interface IComponentData
    end

type SnakeSegments  = 
    struct interface IComponentData
       val direction : float3
    end

type SnakeHead =
    struct interface IComponentData
    end

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

type Tick =
    struct interface IComponentData
    end

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

type SnakeArrayBuffer =
    struct interface IBufferElementData
        val pos: float3
        new(pos: float3) = {pos = pos}
    end

type SnakeCollision =
    struct interface IComponentData
    end