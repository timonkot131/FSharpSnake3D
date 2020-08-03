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

type SnakeBounds() =
    inherit SystemBase()
    
    let gameSettings = Resources.Load<GameSettings> "ScriptableObjects/GameSettings"
    let boundsSize = 
        let vec = gameSettings.BoundsSize
        float3(vec.x, vec.y, vec.z)
    let lineMaterial = gameSettings.LineMaterial
    let lineMesh = gameSettings.LineMesh
    let lineScale = gameSettings.LineScale
    let startPos = gameSettings.StartPosition
    let dangerZone = gameSettings.DangerZone
    let dangerMaterial = gameSettings.DangerMaterial
    let snakeMaterial = gameSettings.SnakeMaterial

    let em = World.DefaultGameObjectInjectionWorld.EntityManager
    let snakeSegmentsQuery = em.CreateEntityQuery [|ComponentType.ReadOnly<SnakeSegments>()|]
    let snakeMovedQuery = em.CreateEntityQuery [|ComponentType.ReadOnly<SnakeMoved>()|]
    
    let setMaterial (e: Entity) (mat: Material) =
        let rendMesh = em.GetSharedComponentData<RenderMesh>(e)
        em.SetSharedComponentData' 
        <| RenderMesh(mesh = rendMesh.mesh, material = mat) <| e
        |> ignore      


 
    let (|InBounds|NearBounds|NotNearOrInBounds|) (ranges, segment: float3) =
        match ranges, segment with
            | { minX = pos}, _ | { maxX = pos}, _ when pos = segment.x -> InBounds
            | { minY = pos}, _ | { maxY = pos}, _ when pos = segment.y -> InBounds
            | { minZ = pos}, _ | { maxZ = pos}, _ when pos = segment.z -> InBounds
            | _, segment when segment.x <= ranges.minX + dangerZone ||
                              segment.x >= ranges.maxX - dangerZone -> NearBounds
            | _, segment when segment.y <= ranges.minY + dangerZone ||
                              segment.y >= ranges.maxY - dangerZone -> NearBounds
            | _, segment when segment.z <= ranges.minZ + dangerZone ||
                              segment.z >= ranges.maxZ - dangerZone -> NearBounds
            | _, _ -> NotNearOrInBounds


    override this.OnCreate() =

        let p1 = float3(startPos.x - boundsSize.x / 2.f, startPos.y - boundsSize.y / 2.f, startPos.z - boundsSize.z /2.f) 
        let p2 = float3(p1.x, p1.y, p1.z + boundsSize.z )
        let p3 = float3(p1.x + boundsSize.x, p1.y, p1.z)
        let p4 = float3(p1.x + boundsSize.x, p1.y, p1.z + boundsSize.z)
        let p5 = float3(p1.x, p1.y + boundsSize.y, p1.z)
        let p6 = float3(p2.x, p2.y + boundsSize.y, p2.z)
        let p7 = float3(p3.x, p3.y + boundsSize.y, p3.z)
        let p8 = float3(p4.x, p4.y + boundsSize.y, p4.z)
        
        let entityManager = World.DefaultGameObjectInjectionWorld.EntityManager 
        
        let archetype = entityManager.CreateArchetype [|
            ComponentType.ReadOnly<CompositeScale>()
            ComponentType.ReadOnly<Translation>()
            ComponentType.ReadWrite<BoundLine>()
            ComponentType.ReadWrite<RenderMesh>()
            ComponentType.ReadWrite<LocalToWorld>()
            ComponentType.ReadWrite<RenderBounds>()
        |] 

        let (|YScale|XScale|ZScale|) (p1: float3, p2: float3) =
            if p1.y <> p2.y then YScale else
            if p1.z = p2.z then XScale else ZScale
       
        let createLine(pt1:float3, pt2:float3) =
            let scale : float4x4 =
                match (pt1, pt2) with
                | XScale -> float4x4.Scale(math.abs(pt1.x-pt2.x), lineScale, lineScale)
                | YScale -> float4x4.Scale(lineScale, math.abs(pt1.y-pt2.y), lineScale)
                | ZScale -> float4x4.Scale(lineScale, lineScale, math.abs(pt1.z-pt2.z))
                
            let center =
                let vector = Vector3.Lerp(Vector3(pt1.x,pt1.y,pt1.z), Vector3(pt2.x,pt2.y,pt2.z), 0.5f)
                float3(vector.x, vector.y, vector.z)

            entityManager.CreateEntity archetype
            |> entityManager.SetComponentData' (BoundLine(pt1, pt2))
            |> entityManager.SetComponentData' (Translation(Value = center))
            |> entityManager.SetComponentData' (CompositeScale(Value = scale))
            |> entityManager.SetSharedComponentData' (RenderMesh(mesh=lineMesh, material=lineMaterial))
            |> ignore

        createLine(p1, p5); createLine(p2, p6); createLine(p3, p7); createLine(p4, p8)
        createLine(p1, p2); createLine(p1, p3); createLine(p4, p2); createLine(p4, p3)
        createLine(p5, p6); createLine(p5, p7); createLine(p8, p6); createLine(p8, p7)

    override this.OnUpdate() = 
        use arr = snakeMovedQuery.ToEntityArray Allocator.TempJob
        if not <| Seq.isEmpty arr then
            use segments = snakeSegmentsQuery.ToEntityArray Allocator.TempJob
            let head = Seq.find (fun e -> em.HasComponent<SnakeHead> e) segments
            let headPos = em.GetComponentData<Translation>(head).Value
            let bounds = rangesStartToBounds startPos boundsSize
            segments |> Seq.iter (fun e ->
                let pos = em.GetComponentData<Translation>(e).Value
                match bounds, pos with
                    | NearBounds -> setMaterial e dangerMaterial
                    | InBounds-> em.CreateEntity [|ComponentType.ReadOnly<SnakeCollision>()|] |> ignore
                    | NotNearOrInBounds-> setMaterial e snakeMaterial)