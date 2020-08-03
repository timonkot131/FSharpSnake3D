namespace FSharpSnake.Systems

open UnityEngine
open Unity.Entities
open Unity.Collections
open Unity.Transforms
open Unity.Rendering
open Unity.Mathematics
open FSharpSnake.Scriptables
open FSharpSnake.Extensions
open FSharpSnake.Components
open Transforms

type FoodSpawn() = 
    inherit SystemBase()

    let gameSettings = Resources.Load<GameSettings> "ScriptableObjects/GameSettings"
    let bounds =
        let vec = gameSettings.BoundsSize
        float3(vec.x, vec.y, vec.z)
    let startPos = gameSettings.StartPosition
    let foodMesh = gameSettings.FoodMesh
    let foodMat = gameSettings.FoodMaterial

    let em = World.DefaultGameObjectInjectionWorld.EntityManager
    
    let ranges = Transforms.rangesStartToBounds startPos <| bounds

    let randRange (min:float32) max = Random.Range(min+1.f, max-1.f) |> math.round

    let rec getRandomPoint segments=
        let randPoint =
            float3 ( randRange ranges.minX ranges.maxX,
                     randRange ranges.minY ranges.maxY,
                     randRange ranges.minZ ranges.maxZ )

        if Seq.exists (fun x -> x = randPoint) segments then
            getRandomPoint segments
        else randPoint

    member this.SnakeBufferQuery = this.GetEntityQuery [|ComponentType.ReadOnly<SnakeArrayBuffer>()|] 
    member this.SnakeFoodQuery = this.GetEntityQuery [|ComponentType.ReadOnly<SnakeFood>()|]

    override this.OnUpdate() =
        use foodArray = this.SnakeFoodQuery.ToEntityArray Allocator.TempJob
        
        if Seq.isEmpty foodArray then
            use snakeArray = this.SnakeBufferQuery.ToEntityArray Allocator.TempJob
            let buffer = em.GetBuffer<SnakeArrayBuffer> snakeArray.[0]
            use segments = buffer.Reinterpret<float3>().ToNativeArray Allocator.TempJob
            let point = getRandomPoint segments
        
            em.CreateEntity [|
                ComponentType.ReadOnly<SnakeFood>()
                ComponentType.ReadWrite<Translation>()
                ComponentType.ReadWrite<LocalToWorld>()
                ComponentType.ReadWrite<RenderBounds>()
                ComponentType.ReadWrite<RenderMesh>()
            |]
            |> em.SetComponentData' (Translation(Value=point))
            |> em.SetSharedComponentData' (RenderMesh(mesh = foodMesh, material = foodMat))
            |> ignore

    