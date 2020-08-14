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

[<UpdateAfter(typeof<PlayerInputReceiver>)>]
type CameraController() =
    inherit SystemBase()
    
    let gameSettings = Resources.Load<GameSettings> "ScriptableObjects/GameSettings"
    let startPosition = gameSettings.StartPosition    
    let radius = gameSettings.CameraDistance
    let angle = gameSettings.CameraTurnAngle

    let em = World.DefaultGameObjectInjectionWorld.EntityManager

    let cameraQuery = em.CreateEntityQuery [|ComponentType.ReadWrite<CameraComp>()|]

    override __.OnUpdate() =
        let mutable transform, dir = forQuery1 cameraQuery (fun e ->
                      ( (em.GetComponentObject<Transform> e).transform,
                        (em.GetComponentData<CameraComp> e).direction.ToVector3) )
        
        transform.position <- startPosition.ToVector3
        
        transform.Rotate(Vector3(1.f, 0.f, 0.f), dir.y * angle * Time.deltaTime)
        transform.Rotate(Vector3(0.f, 1.f, 0.f), -dir.x * angle * Time.deltaTime, Space.World)

        transform.Translate(Vector3(0.f, 0.f, -radius))