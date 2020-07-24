namespace FSharpSnake.Systems

open Unity.Entities
open UnityEngine

[<GenerateAuthoringComponent>]
type PlayerInput() = 
    inherit SystemBase()

    override this.OnUpdate() = 
        if Input.GetKey KeyCode.Keypad5 then Debug.Log("it working my nigga")