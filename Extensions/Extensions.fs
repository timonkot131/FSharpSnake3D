namespace FSharpSnake

open UnityEngine
open Unity.Entities
open Unity.Collections
open Unity.Transforms
open Unity.Rendering
open Unity.Mathematics
open System

module Extensions =
    type EntityManager with
        member self.AddComponentF (comp: 'a) (e: Entity) =
            self.AddComponentData<'a>(e, comp) |> ignore
            e
        member self.AddSharedComponentF (comp: 'a) (e: Entity) = 
            self.AddSharedComponentData<'a>(e, comp) |> ignore
            e
        member self.SetComponentDataF(comp: 'a) (e: Entity) = 
            self.SetComponentData<'a>(e, comp) |> ignore
            e
        member self.SetSharedComponentDataF(comp: 'a) (e: Entity) = 
            self.SetSharedComponentData<'a>(e, comp) |> ignore
            e                

    type EntityQuery with
        member self.ForEach (func: Entity->unit) =
            use entityArray = self.ToEntityArray Allocator.TempJob
            entityArray |> Seq.iter func

        member self.ForEachComponent<'T when 'T :> IComponentData
                                         and 'T :struct and 'T :>System.ValueType
                                         and 'T : (new : unit -> 'T)> (func: Entity->'T->unit) =
            let entityManager = World.DefaultGameObjectInjectionWorld.EntityManager
            use entityArray = self.ToEntityArray Allocator.TempJob       
            for entity in entityArray do
                func entity <| entityManager.GetComponentData<'T> entity

    type NativeArray<'T when 'T: struct and 'T :> System.ValueType and 'T :(new: unit -> 'T)> with
        member self.GetSlice(startId: int option, endId: int option) =
            match startId, endId with
                | None, None -> self.Slice()
                | Some s, None -> self.Slice(s)
                | Some s, Some e -> self.Slice(s, e)
                | None, Some e -> invalidArg "startId" "NativeArray can't be sliced with only length taked"
                    
    type float3 with
           static member Up = float3(0.f, 1.f, 0.f)
           static member Down = float3(0.f, -1.f, 0.f)
           static member Left = float3(-1.f, 0.f, 0.f)
           static member Right = float3(1.f, 0.f, 0.f)
           static member Forward = float3(0.f, 0.f, 1.f)
           static member Back = float3(0.f, 0.f, -1.f)
           member self.Plus (f: float3) = float3(self.x + f.x, self.y + f.y, self.z + f.z)
