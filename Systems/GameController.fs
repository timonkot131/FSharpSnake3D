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
    let gamePauseQuery = em.CreateEntityQuery [|ComponentType.ReadOnly<PausePressed>()|]
    let gameOverQuery = em.CreateEntityQuery [|ComponentType.ReadOnly<GameOver>()|]

    override __.OnUpdate() =
        use arr = gamePauseQuery.ToComponentDataArray<PausePressed> Allocator.TempJob
        use gameOver = gameOverQuery.ToComponentDataArray<GameOver> Allocator.TempJob
        Seq.tryHead gameOver |> function
            | Some e -> match arr.Length with 
                        | 1 -> 
                            em.CreateEntity [|ComponentType.ReadOnly<GameRestart>()|] |> ignore
                            em.DestroyEntity gameOverQuery
                            em.DestroyEntity gamePauseQuery
                        | _ -> ()
            | None   -> match arr.Length with
                        | 0 -> snakeSystem.Enabled <- true
                        | 1 -> snakeSystem.Enabled <- false
                        | 2 -> em.DestroyEntity gamePauseQuery
                        | _ -> ()

        use arr = snakeCollisionQuery.ToEntityArray Allocator.TempJob
        Seq.tryHead arr
            |> Option.iter (fun e ->
                                Debug.Log "Game Over"    
                                em.CreateEntity [|ComponentType.ReadOnly<GameOver>()|] |> ignore
                                em.DestroyEntity e)



