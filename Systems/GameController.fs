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

type GameController() = 
    inherit SystemBase()
    
    let snakeSystem =
        World.DefaultGameObjectInjectionWorld.GetExistingSystem<SnakeController>()
        
    let em = World.DefaultGameObjectInjectionWorld.EntityManager

    let snakeCollisionQuery = em.CreateEntityQuery [|ComponentType.ReadOnly<SnakeCollision>()|]

    override this.OnUpdate() =
        use arr = snakeCollisionQuery.ToEntityArray Allocator.TempJob
        Seq.tryHead arr |> function
            | Some e ->
                Debug.Log "Game Over"    
                snakeSystem.Enabled <- false
                em.DestroyEntity e
            | None -> ()

