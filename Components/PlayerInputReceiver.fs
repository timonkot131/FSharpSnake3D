namespace FSharpSnake.Components

open Unity.Entities
open UnityEngine
open Unity.Mathematics
open FSharpSnake.Extensions


type PlayerInputReceiver() = 
    inherit MonoBehaviour()

    let em = World.DefaultGameObjectInjectionWorld.EntityManager

    member this.SelfEntity =
        let settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null)
        GameObjectConversionUtility.ConvertGameObjectHierarchy(this.gameObject, settings)

    member this.ApplyDirection dir = em.SetComponentData(this.SelfEntity, new SnakeDirection(dir))

    member this.Start() = em.AddComponentData(this.SelfEntity, new SnakeDirection(float3.Left))

    member this.Update() =
        if Input.GetKeyUp KeyCode.Keypad4 then this.ApplyDirection float3.Left
        if Input.GetKeyUp KeyCode.Keypad6 then this.ApplyDirection float3.Right
        if Input.GetKeyUp KeyCode.Keypad2 then this.ApplyDirection float3.Back
        if Input.GetKeyUp KeyCode.Keypad8 then this.ApplyDirection float3.Forward
        if Input.GetKeyUp KeyCode.Keypad7 then this.ApplyDirection float3.Up
        if Input.GetKeyUp KeyCode.Keypad1 then this.ApplyDirection float3.Down