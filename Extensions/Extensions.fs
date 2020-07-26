namespace FSharpSnake

open UnityEngine
open Unity.Entities
open Unity.Collections
open Unity.Transforms
open Unity.Rendering
open Unity.Mathematics

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
            let entityArray = self.ToEntityArray(Allocator.TempJob)
            for entity in entityArray do func entity
            entityArray.Dispose()|> ignore

        member self.ForEachComponent<'T when 'T :> IComponentData
                                         and 'T :struct and 'T :>System.ValueType
                                         and 'T : (new : unit -> 'T)> (func: Entity->'T->unit) =
            let entityManager = World.DefaultGameObjectInjectionWorld.EntityManager
            let entityArray = self.ToEntityArray(Allocator.TempJob)       
            for entity in entityArray do
                func entity (entityManager.GetComponentData<'T>(entity))
            entityArray.Dispose()|> ignore

    type DynamicBuffer<'T when 'T: struct and 'T :> System.ValueType and 'T :(new: unit -> 'T)> with
        member self.GetSlice(startId, endId) =
            let s = defaultArg startId 0
            let e = defaultArg endId self.Length - 1

            let result = new DynamicBuffer<'T>()
            result.CopyFrom(self)
            result.RemoveRange(0, s)
            result.RemoveRange(e, self.Length - e)
            result

    type float3 with
           static member Up = float3(0.f, 1.f, 0.f)
           static member Down = float3(0.f, -1.f, 0.f)
           static member Left = float3(-1.f, 0.f, 0.f)
           static member Right = float3(1.f, 0.f, 0.f)
           static member Forward = float3(0.f, 0.f, 1.f)
           static member Back = float3(0.f, 0.f, -1.f)
           member self.Plus (f: float3) = float3(self.x + f.x, self.y + f.y, self.z + f.z)
