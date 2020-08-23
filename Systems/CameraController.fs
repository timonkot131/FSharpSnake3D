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
    let compassPos = gameSettings.CompassPosition

    let em = World.DefaultGameObjectInjectionWorld.EntityManager

    let cameraQuery = em.CreateEntityQuery [|ComponentType.ReadWrite<CameraComp>()|]
    let compassQuery = em.CreateEntityQuery [|ComponentType.ReadWrite<CompassComp>()|]

    override __.OnUpdate() =
        let cameraTransform, dir = forQuery1 cameraQuery (fun e ->
            ( (em.GetComponentObject<Transform> e).transform,
              (em.GetComponentData<CameraComp> e).direction.ToVector3) )

        let compassTransform = forQuery1 compassQuery (fun e ->
            (em.GetComponentObject<Transform> e).transform)
        
        cameraTransform.position <- startPosition.ToVector3
        
        cameraTransform.Rotate(Vector3(1.f, 0.f, 0.f), dir.y * angle * Time.deltaTime)
        cameraTransform.Rotate(Vector3(0.f, 1.f, 0.f), -dir.x * angle * Time.deltaTime, Space.World)

        cameraTransform.Translate <| Vector3(0.f, 0.f, -radius)
        
        compassTransform.position <- cameraTransform.position
        compassTransform.Translate(compassPos, cameraTransform)
