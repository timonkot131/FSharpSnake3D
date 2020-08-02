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

type FoodSpawn() = 
    inherit SystemBase()

    let gameSettings = Resources.Load<GameSettings> "ScriptableObjects/GameSettings"
    let bounds = gameSettings.BoundsSize
    let startPos = gameSettings.StartPosition
    let foodMesh = gameSettings.FoodMesh
    let foodMat = gameSettings.FoodMaterial

    let em = World.DefaultGameObjectInjectionWorld.EntityManager
    
    let minmaxRelative s bnds = ((s - bnds) / 2.f, (s + bnds) / 2.f)
    
    let minX, maxX = minmaxRelative startPos.x bounds.x
    let minY, maxY = minmaxRelative startPos.y bounds.y
    let minZ, maxZ = minmaxRelative startPos.z bounds.z

    let randRange (min:float32) max = Random.Range(min,max) |> math.round

    let rec getRandomPoint segments=
        let randPoint = float3 (
                            randRange minX maxX,
                            randRange minY maxY,
                            randRange minZ maxZ
                        )

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
            |> em.SetComponentDataF (Translation(Value=point))
            |> em.SetSharedComponentDataF (RenderMesh(mesh = foodMesh, material = foodMat))
            |> ignore

    