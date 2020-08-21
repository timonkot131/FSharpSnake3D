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

[<UpdateAfter(typeof<SnakeController>)>]
type SnakeDraw() =
    inherit SystemBase()
    
    let gameSettings = Resources.Load<GameSettings> "ScriptableObjects/GameSettings"
    let boundsSize = 
        let vec = gameSettings.BoundsSize
        float3(vec.x, vec.y, vec.z)
    let startPos = gameSettings.StartPosition
    let dangerZone = gameSettings.DangerZone
    let dangerMaterial = gameSettings.DangerMaterial
    let snakeMesh = gameSettings.SnakeMesh
    let snakeMaterial = gameSettings.SnakeMaterial
    let ranges = rangesStartToBounds startPos boundsSize

    let em = World.DefaultGameObjectInjectionWorld.EntityManager

    let snakeSegmentsQuery = em.CreateEntityQuery [|ComponentType.ReadOnly<SnakeSegments>()|]
    let snakeMovedQuery = em.CreateEntityQuery [|ComponentType.ReadOnly<SnakeMoved>()|]
    
    let setMaterial (e: Entity) (mat: Material) =
        let rendMesh = em.GetSharedComponentData<RenderMesh>(e)
        em.SetSharedComponentData'
        <| RenderMesh(mesh = rendMesh.mesh, material = mat)
        <| e
        |> ignore
    
    let setMesh (e: Entity) (mesh: Mesh) =
        let rendMesh = em.GetSharedComponentData<RenderMesh>(e)
        em.SetSharedComponentData'
        <| RenderMesh(mesh = mesh, material = rendMesh.material)
        <| e
        |> ignore
        
    let (|InBounds|NearBounds|NotNearOrInBounds|) (ranges, segment: float3) =

        let nearBounds (ranges: Ranges) (segment: float3) =     
            List.exists2 (fun (pl: float32 list) p-> not <| List.contains p pl )
                [ [ranges.minX + dangerZone..ranges.maxX - dangerZone];
                  [ranges.minY + dangerZone..ranges.maxY - dangerZone];
                  [ranges.minZ + dangerZone..ranges.maxZ - dangerZone] ]
                [segment.x; segment.y; segment.z] 

        match ranges, segment with
        | r, s when inBounds r s -> InBounds
        | r, s when nearBounds r s -> NearBounds
        | _, _ -> NotNearOrInBounds
        
    override __.OnUpdate() = 
        use arr = snakeMovedQuery.ToEntityArray Allocator.TempJob
        if not <| Seq.isEmpty arr then
            em.DestroyEntity snakeMovedQuery
            use segments = snakeSegmentsQuery.ToEntityArray Allocator.TempJob
            segments |> Seq.iter (fun e ->
                setMesh e snakeMesh
                let pos = em.GetComponentData<Translation>(e).Value
                match ranges, pos with
                | NearBounds | InBounds-> setMaterial e dangerMaterial
                | NotNearOrInBounds-> setMaterial e snakeMaterial)