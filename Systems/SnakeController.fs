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

type CleanCreateJob =
    struct 
        val commandBuffer: EntityCommandBuffer
        val snakeArray: NativeArray<float3>
        val segmentsToDelete: NativeArray<Entity>
        val segmentToCopy: Entity       

        new (commandBuffer, snakeArray, segmentsToDelete, segmentToCopy) =
            {commandBuffer = commandBuffer;
            snakeArray = snakeArray;
            segmentsToDelete = segmentsToDelete;
            segmentToCopy = segmentToCopy}

        member private this.createEntity i pos =
            let e = this.commandBuffer.Instantiate(this.segmentToCopy)
            Debug.Log(e.ToString() + " " + i.ToString())
            let last = this.snakeArray.Length - 1
            this.commandBuffer.AddComponent(e, new Translation(Value = pos))
            match i with        
                | 0 -> this.commandBuffer.AddComponent(e, SnakeHead())
                | x when x = last -> this.commandBuffer.AddComponent(e, SnakeEnd())
                | _ -> ()

        interface IJob with
            member this.Execute() =
            //    for entity in this.segmentsToDelete do
            //        this.commandBuffer.DestroyEntity(entity)
              
                this.snakeArray |> Seq.iteri this.createEntity
     end

[<UpdateAfter(typeof<PlayerInputReceiver>)>]
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

    member this.SnakeArchetype = em.CreateArchetype [|
        ComponentType.ReadWrite<Translation>()
        ComponentType.ReadWrite<SnakeSegments>()
        ComponentType.ReadWrite<LocalToWorld>()
        ComponentType.ReadWrite<RenderBounds>()
        ComponentType.ReadWrite<RenderMesh>()
    |]

    member this.Segment = 
        this.SnakeArchetype
        |> em.CreateEntity
        |> em.SetSharedComponentDataF (RenderMesh(mesh=snakeMesh, material=snakeMaterial))
        
    member this.MoveSnake() =
        let direction = 
            use array = this.DirectionQuery.ToEntityArray(Allocator.TempJob)
            em.GetComponentData<SnakeDirection>(array.[0]).direction

        let snakeBuffer =
            use array = this.SnakeArrayQuery.ToEntityArray(Allocator.TempJob)
            em.GetBuffer<SnakeArrayBuffer>(array.[0])

        snakeBuffer.RemoveAt(snakeBuffer.Length-1)
        snakeBuffer.Insert(0, new SnakeArrayBuffer(snakeBuffer.Reinterpret<float3>().[0] + direction))

        let snakeArray = snakeBuffer.Reinterpret<float3>().ToNativeArray Allocator.TempJob
        let commandBuffer = new EntityCommandBuffer(Allocator.TempJob)
        let segments = this.SegmentQuery.ToEntityArray(Allocator.TempJob)

        //em.DestroyEntity(this.SegmentQuery)

        CleanCreateJob(
            commandBuffer,
            snakeArray,
            segments,
            this.Segment).Schedule().Complete()

        commandBuffer.Dispose()
        segments.Dispose()
        snakeArray.Dispose()
                
    override this.OnCreate() =
        em.CreateArchetype [||]
        |> em.CreateEntity
        |> em.AddComponentF (Tick())
        |> ignore
       
        this.SnakeArchetype
        |> em.CreateEntity
        |> em.AddComponentF (SnakeHead())
        |> em.SetComponentDataF (Translation(Value=startPosition))
        |> em.SetSharedComponentDataF (RenderMesh(mesh=snakeMesh, material=snakeMaterial))
        |> ignore
        ()

        this.SnakeArchetype
        |> em.CreateEntity
        |> em.AddComponentF (SnakeEnd())
        |> em.SetComponentDataF (Translation(Value=float3(startPosition.x + 1.f, startPosition.y, startPosition.z)))
        |> em.SetSharedComponentDataF (RenderMesh(mesh=snakeMesh, material=snakeMaterial))
        |> ignore
        ()

        let buffer =
            em.CreateArchetype [| ComponentType.ReadWrite<SnakeArrayBuffer>() |]
            |> em.CreateEntity
            |> em.GetBuffer<SnakeArrayBuffer>

        buffer.Add (SnakeArrayBuffer(float3(0.f,0.f,0.f))) |> ignore
        buffer.Add (SnakeArrayBuffer(float3(1.f,0.f,0.f))) |> ignore
        buffer.Add (SnakeArrayBuffer(float3(2.f,0.f,0.f))) |> ignore
                                                              
    override this.OnUpdate() =
        this.TickQuery.ForEach(fun(e) ->
            em.AddComponentF (Delay(Time.time + tickDelay)) e |> ignore
            this.MoveSnake()
            em.RemoveComponent<Tick> e                        |> ignore
        )

        this.DelayQuery.ForEachComponent(fun e (comp: Delay) ->
             if (Time.time > comp.duration) then
                em.RemoveComponent<Delay> e |> ignore
                em.AddComponentF (Tick()) e |> ignore
        )

           