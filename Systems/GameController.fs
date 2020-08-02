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

    member this.SnakeCollisionQuery = this.GetEntityQuery [|ComponentType.ReadOnly<SnakeCollision>()|]

    override this.OnUpdate() =
        this.SnakeCollisionQuery.ForEachComponent(fun e (comp: SnakeCollision) -> 
            Debug.Log "Game Over"    
            snakeSystem.Enabled <- false
            em.DestroyEntity e
        )

