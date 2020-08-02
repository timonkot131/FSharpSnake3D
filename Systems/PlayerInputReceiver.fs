namespace FSharpSnake.Components

open Unity.Entities
open UnityEngine
open Unity.Mathematics
open FSharpSnake.Extensions


type PlayerInputReceiver() = 
    inherit SystemBase()

    let em = World.DefaultGameObjectInjectionWorld.EntityManager

    let directionEntity =
        Debug.Log("aaee")
        em.CreateArchetype [||]
        |> em.CreateEntity
        |> em.AddComponentF (new SnakeDirection(float3.Left))

    member this.ApplyDirection dir =
        Debug.Log(dir.ToString())
        em.SetComponentData(directionEntity, new SnakeDirection(dir))

    override this.OnUpdate() =
        if Input.GetKeyUp KeyCode.Keypad4 then this.ApplyDirection float3.Left
        if Input.GetKeyUp KeyCode.Keypad6 then this.ApplyDirection float3.Right
        if Input.GetKeyUp KeyCode.Keypad2 then this.ApplyDirection float3.Back
        if Input.GetKeyUp KeyCode.Keypad8 then this.ApplyDirection float3.Forward
        if Input.GetKeyUp KeyCode.Keypad7 then this.ApplyDirection float3.Up
        if Input.GetKeyUp KeyCode.Keypad1 then this.ApplyDirection float3.Down