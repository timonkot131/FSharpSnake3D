namespace FSharpSnake.Systems

open UnityEngine
open Unity.Entities
open Unity.Collections
open Unity.Transforms
open Unity.Rendering
open Unity.Mathematics
open FSharpSnake

type SnakeController() =
    inherit SystemBase()

    let gameSettings = Resources.Load<GameSettings> "ScriptableObjects/GameSettings"
    let tickDelay = gameSettings.SnakeTick
    let snakeMesh = gameSettings.SnakeMesh
    let snakeMaterial = gameSettings.SnakeMaterial
    let startPosition = gameSettings.StartPosition

    let entityManager = World.DefaultGameObjectInjectionWorld.EntityManager 

    member this.TickQuery = this.GetEntityQuery [|ComponentType.ReadOnly<Tick>()|]
    member this.DelayQuery = this.GetEntityQuery [|ComponentType.ReadOnly<Delay>()|]
    member this.HeadQuery = this.GetEntityQuery [|ComponentType.ReadOnly<SnakeHead>()|]
    member this.SnakeArray = new NativeArray<float3>([|float3(0.f, 0.f, 0.f); float3(1.f, 0.f, 0.f)|], Allocator.Temp)

    member this.MoveSnake =
        let headPosition = this.SnakeArray
        let segment = entityManager.GetComponentObject<SnakeSegments> snakeHead

        let newPosition = headPosition.Value.Plus(segment.direction)
        entityManager.SetComponentDataF (Translation(Value = newPosition)) snakeHead

    override this.OnCreate() =
        entityManager.CreateArchetype [||]
        |> entityManager.CreateEntity
        |> entityManager.AddComponentF (Tick())
        |> ignore

        entityManager.CreateArchetype [|
            ComponentType.ReadWrite<Translation>()
            ComponentType.ReadWrite<RenderMesh>()
            ComponentType.ReadWrite<LocalToWorld>()
            ComponentType.ReadWrite<RenderBounds>()
            ComponentType.ReadWrite<SnakeHead>()
            ComponentType.ReadWrite<SnakeSegments>()
        |]
        |> entityManager.CreateEntity
        |> entityManager.SetComponentDataF (Translation(Value=startPosition))
        |> entityManager.SetSharedComponentDataF (RenderMesh(mesh=snakeMesh, material=snakeMaterial))
        |> ignore
        ()

        entityManager.CreateArchetype [|
            ComponentType.ReadWrite<Translation>()
            ComponentType.ReadWrite<RenderMesh>()
            ComponentType.ReadWrite<LocalToWorld>()
            ComponentType.ReadWrite<RenderBounds>()
            ComponentType.ReadWrite<SnakeEnd>()
            ComponentType.ReadWrite<SnakeSegments>()
        |]
        |> entityManager.CreateEntity
        |> entityManager.SetComponentDataF (Translation(Value=float3(startPosition.x + 1.f, startPosition.y, startPosition.z)))
        |> entityManager.SetSharedComponentDataF (RenderMesh(mesh=snakeMesh, material=snakeMaterial))
        |> ignore
        ()

    override this.OnUpdate() =
        this.TickQuery.ForEachComponent(fun e (comp: Tick) ->
             entityManager.AddComponentF (Delay(Time.time + tickDelay)) e |> ignore
             entityManager.RemoveComponent<Tick> e |> ignore
        )

        this.DelayQuery.ForEachComponent(fun e (comp: Delay) ->
             if (Time.time > comp.duration) then
                entityManager.RemoveComponent<Delay> e |> ignore
                entityManager.AddComponentF (Tick()) e |> ignore
                Debug.Log("working like a clock!")
        )

    override this.OnDestroy() = this.SnakeArray.Dispose()