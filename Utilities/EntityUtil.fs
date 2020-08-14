module EntityUtil

open UnityEngine
open Unity.Entities
open Unity.Collections
open Unity.Transforms
open Unity.Rendering
open Unity.Mathematics
open Transforms

let forQuery (q: EntityQuery) (func: NativeArray<Entity> -> _) =
    use a = q.ToEntityArray Allocator.TempJob
    func a

let forQuery1 (q: EntityQuery) (func: Entity -> _) =
    use a = q.ToEntityArray Allocator.TempJob
    func a.[0]
    