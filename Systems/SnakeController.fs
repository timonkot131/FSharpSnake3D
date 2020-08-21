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
open EntityUtil
open Transforms
open Unity.Jobs

[<UpdateAfter(typeof<FoodSpawn>)>]
type SnakeController() =
    inherit SystemBase()  
    
    let gameSettings = Resources.Load<GameSettings> "ScriptableObjects/GameSettings"
    let tickDelay = gameSettings.SnakeTick
    let boundSize = gameSettings.BoundsSize
    let startPosition = gameSettings.StartPosition    

    let ranges = rangesStartToBounds startPosition boundSize.ToFloat3
    
    let em = World.DefaultGameObjectInjectionWorld.EntityManager
    
    let tickQuery = em.CreateEntityQuery [|ComponentType.ReadOnly<Tick>()|]
    let delayQuery = em.CreateEntityQuery [|ComponentType.ReadOnly<Delay>()|]
    let segmentQuery = em.CreateEntityQuery [|ComponentType.ReadOnly<SnakeSegments>()|]
    let snakeArrayQuery = em.CreateEntityQuery [|ComponentType.ReadOnly<SnakeArrayBuffer>()|]
    let directionQuery = em.CreateEntityQuery [|ComponentType.ReadOnly<SnakeDirection>()|]
    let snakeFoodQuery = em.CreateEntityQuery [|ComponentType.ReadOnly<SnakeFood>()|]
    let restartQuery = em.CreateEntityQuery [|ComponentType.ReadOnly<GameRestart>()|]
    let gameOverQuery = em.CreateEntityQuery [|ComponentType.ReadOnly<GameOver>()|]

    let isOnFood snakeHeadPos =
        let foodPos = forQuery1 snakeFoodQuery em.GetComponentData<Translation>
        foodPos.Value = snakeHeadPos

    let snakeArchetype = em.CreateArchetype [|
        ComponentType.ReadWrite<Translation>()
        ComponentType.ReadWrite<SnakeSegments>()
        ComponentType.ReadWrite<LocalToWorld>()
        ComponentType.ReadWrite<RenderBounds>()
        ComponentType.ReadWrite<RenderMesh>()
    |]

    let fillBuffer (bufferArray: DynamicBuffer<SnakeArrayBuffer>) = 
        let createBuffer x y z = float3(startPosition.x + x, startPosition.y + y, startPosition.z + z) |> SnakeArrayBuffer

        bufferArray.Add <| createBuffer 0.f 0.f 0.f |> ignore
        bufferArray.Add <| createBuffer 1.f 0.f 0.f |> ignore
        bufferArray.Add <| createBuffer 2.f 0.f 0.f |> ignore

    let moveSnake() =
        let direction = (forQuery1 directionQuery em.GetComponentData<SnakeDirection>).direction
        let getSnakeBuffer() = forQuery1 snakeArrayQuery em.GetBuffer<SnakeArrayBuffer>
            
        let newHeadPos = getSnakeBuffer().Reinterpret<float3>().[0] + direction

        if isOnFood newHeadPos then
           em.DestroyEntity snakeFoodQuery
        else
            let buff = getSnakeBuffer() //Unity deallocates snakeBuffer after deleting entity
            buff.RemoveAt <| buff.Length-1

        let buff = getSnakeBuffer()

        buff.Insert(0, SnakeArrayBuffer newHeadPos)

        use snakeArray = buff.Reinterpret<float3>().ToNativeArray Allocator.TempJob
      
        em.DestroyEntity segmentQuery

        let last = snakeArray.Length - 1

        let createEntity i pos =
            let e = em.CreateEntity snakeArchetype
                    |> em.SetComponentData' (Translation(Value = pos))
            match i with        
            | 0 -> em.AddComponent' <| SnakeHead() <|e                  |> ignore
            | x when x = last -> em.AddComponent' <| SnakeEnd() <| e    |> ignore
            | _ -> ()

        match Seq.exists (fun x -> x = snakeArray.[0]) (Seq.tail snakeArray) with
        | true -> em.CreateEntity [|ComponentType.ReadOnly<SnakeCollision>()|] |> ignore
        | false -> snakeArray |> Seq.iteri createEntity

        if inBounds ranges snakeArray.[0] then em.CreateEntity [|ComponentType.ReadOnly<SnakeCollision>()|] |> ignore

    override __.OnCreate() =
            em.CreateArchetype [| ComponentType.ReadWrite<SnakeArrayBuffer>() |]
            |> em.CreateEntity
            |> em.GetBuffer<SnakeArrayBuffer>
            |> fillBuffer

            em.CreateEntity [| ComponentType.ReadOnly<Tick>() |] |> ignore
    
    override __.OnUpdate() =
        tickQuery.ForEach(fun(e) ->
            em.AddComponent' (Delay(Time.time + tickDelay)) e |> ignore
            if forQuery gameOverQuery Seq.isEmpty then
                moveSnake()
                em.CreateEntity [|ComponentType.ReadOnly<SnakeMoved>()|] |> ignore
            em.RemoveComponent<Tick> e |> ignore
        )

        use restart = restartQuery.ToEntityArray Allocator.TempJob
        Seq.tryHead restart |> Option.iter (fun e ->
                em.DestroyEntity e
                let buffer = forQuery1 snakeArrayQuery em.GetBuffer<SnakeArrayBuffer>
                buffer.Clear()
                fillBuffer buffer
            )
        
        delayQuery.ForEachComponent(fun e (comp: Delay) ->
             if (Time.time > comp.duration) then
                em.RemoveComponent<Delay> e |> ignore
                em.AddComponent<Tick> e |> ignore
        )     
