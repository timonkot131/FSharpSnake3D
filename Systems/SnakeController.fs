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
open Unity.Jobs

[<UpdateAfter(typeof<FoodSpawn>)>]
type SnakeController() =
    inherit SystemBase()  
    
    let gameSettings = Resources.Load<GameSettings> "ScriptableObjects/GameSettings"
    let tickDelay = gameSettings.SnakeTick
    let snakeMesh = gameSettings.SnakeMesh
    let snakeMaterial = gameSettings.SnakeMaterial
    let startPosition = gameSettings.StartPosition    
    
    let em = World.DefaultGameObjectInjectionWorld.EntityManager
        
    member this.TickQuery = this.GetEntityQuery [|ComponentType.ReadOnly<Tick>()|]
    member this.DelayQuery = this.GetEntityQuery [|ComponentType.ReadOnly<Delay>()|]
    member this.SegmentQuery = this.GetEntityQuery [|ComponentType.ReadOnly<SnakeSegments>()|]
    member this.SnakeArrayQuery = this.GetEntityQuery [|ComponentType.ReadOnly<SnakeArrayBuffer>()|]
    member this.DirectionQuery = this.GetEntityQuery [|ComponentType.ReadOnly<SnakeDirection>()|]
    member this.SnakeFoodQuery = this.GetEntityQuery [|ComponentType.ReadOnly<SnakeFood>()|]

    member this.SnakeArchetype = em.CreateArchetype [|
        ComponentType.ReadWrite<Translation>()
        ComponentType.ReadWrite<SnakeSegments>()
        ComponentType.ReadWrite<LocalToWorld>()
        ComponentType.ReadWrite<RenderBounds>()
        ComponentType.ReadWrite<RenderMesh>()
    |]

    member this.isOnFood snakeHeadPos =
        use foodArray = this.SnakeFoodQuery.ToEntityArray Allocator.TempJob
        let foodPos = em.GetComponentData<Translation> foodArray.[0]
        foodPos.Value = snakeHeadPos

    member this.MoveSnake() =
        let direction = 
            use array = this.DirectionQuery.ToEntityArray Allocator.TempJob
            em.GetComponentData<SnakeDirection>(array.[0]).direction

        let getSnakeBuffer() =
            use array = this.SnakeArrayQuery.ToEntityArray Allocator.TempJob
            em.GetBuffer<SnakeArrayBuffer>(array.[0])
            
        let newHeadPos = getSnakeBuffer().Reinterpret<float3>().[0] + direction

        if this.isOnFood newHeadPos then
            use array = this.SnakeFoodQuery.ToEntityArray Allocator.TempJob
            em.DestroyEntity array.[0]
        else
            let buff = getSnakeBuffer()
            buff.RemoveAt <| buff.Length-1

        let buff = getSnakeBuffer()

        buff.Insert(0, SnakeArrayBuffer newHeadPos)

        use snakeArray = buff.Reinterpret<float3>().ToNativeArray Allocator.TempJob
      
        em.DestroyEntity this.SegmentQuery

        let createEntity i pos =
            let last = snakeArray.Length - 1
            let e = em.CreateEntity this.SnakeArchetype
                    |> em.SetComponentData' (Translation(Value = pos))
                    |> em.SetSharedComponentData' (RenderMesh(mesh=snakeMesh, material=snakeMaterial))
            match i with        
                | 0 -> em.AddComponent' <| SnakeHead() <|e                  |> ignore
                | x when x = last -> em.AddComponent' <| SnakeEnd() <| e    |> ignore
                | _ -> ()

        match Seq.exists (fun x -> x = snakeArray.[0]) (Seq.tail snakeArray) with
            | true -> em.CreateEntity [|ComponentType.ReadOnly<SnakeCollision>()|] |> ignore
            | false -> snakeArray |> Seq.iteri createEntity

    override this.OnCreate() =
        em.CreateArchetype [||]
        |> em.CreateEntity
        |> em.AddComponent' (Tick())
        |> ignore
       
        this.SnakeArchetype
        |> em.CreateEntity
        |> em.AddComponent' (SnakeHead())
        |> em.SetComponentData' (Translation(Value=startPosition))
        |> em.SetSharedComponentData' (RenderMesh(mesh=snakeMesh, material=snakeMaterial))
        |> ignore

        this.SnakeArchetype
        |> em.CreateEntity
        |> em.AddComponent' (SnakeEnd())
        |> em.SetComponentData' (Translation(Value=float3(startPosition.x + 1.f, startPosition.y, startPosition.z)))
        |> em.SetSharedComponentData' (RenderMesh(mesh=snakeMesh, material=snakeMaterial))
        |> ignore

        let buffer =
            em.CreateArchetype [| ComponentType.ReadWrite<SnakeArrayBuffer>() |]
            |> em.CreateEntity
            |> em.GetBuffer<SnakeArrayBuffer>
        
        let createBuffer x y z = float3(x, y, z) |> SnakeArrayBuffer

        buffer.Add <| createBuffer 0.f 0.f 0.f |> ignore
        buffer.Add <| createBuffer 1.f 0.f 0.f |> ignore
        buffer.Add <| createBuffer 2.f 0.f 0.f |> ignore
                                                              
    override this.OnUpdate() =
        this.TickQuery.ForEach(fun(e) ->
            em.AddComponent' (Delay(Time.time + tickDelay)) e           |> ignore
            this.MoveSnake()
            em.CreateEntity [|ComponentType.ReadOnly<SnakeMoved>()|]    |> ignore
            em.RemoveComponent<Tick> e                                  |> ignore
        )

        this.DelayQuery.ForEachComponent(fun e (comp: Delay) ->
             if (Time.time > comp.duration) then
                em.RemoveComponent<Delay> e |> ignore
                em.AddComponent' (Tick()) e |> ignore
        )

           