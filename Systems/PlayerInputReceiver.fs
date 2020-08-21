namespace FSharpSnake.Components

open Unity.Entities
open UnityEngine
open Unity.Mathematics
open FSharpSnake.Extensions
open EntityUtil
open Unity.Jobs

type PlayerInputReceiver() = 
    inherit SystemBase()

    let em = World.DefaultGameObjectInjectionWorld.EntityManager

    let cameraQuery = em.CreateEntityQuery [|ComponentType.ReadOnly<CameraComp>()|]

    let snakeDirectionEntity =
        em.CreateEntity [||]
        |> em.AddComponent' (SnakeDirection(float3.Left))

    let applySnakeDirection dir =
        em.SetComponentData(snakeDirectionEntity, SnakeDirection(dir))

    let applyCameraDirection dir =
        forQuery1 cameraQuery <| em.SetComponentData' (CameraComp(direction = dir)) |> ignore

    override __.OnUpdate() =
        if Input.GetKeyUp KeyCode.Keypad4 then applySnakeDirection float3.Left
        if Input.GetKeyUp KeyCode.Keypad6 then applySnakeDirection float3.Right
        if Input.GetKeyUp KeyCode.Keypad2 then applySnakeDirection float3.Back
        if Input.GetKeyUp KeyCode.Keypad8 then applySnakeDirection float3.Forward
        if Input.GetKeyUp KeyCode.Keypad7 then applySnakeDirection float3.Up
        if Input.GetKeyUp KeyCode.Keypad1 then applySnakeDirection float3.Down

        applyCameraDirection float3.zero
        if Input.GetKey KeyCode.LeftArrow then applyCameraDirection float3.Left
        if Input.GetKey KeyCode.RightArrow then applyCameraDirection float3.Right
        if Input.GetKey KeyCode.UpArrow then applyCameraDirection float3.Up
        if Input.GetKey KeyCode.DownArrow then applyCameraDirection float3.Down

        if Input.GetKeyUp KeyCode.Space then em.CreateEntity [|ComponentType.ReadOnly<PausePressed>()|] |> ignore