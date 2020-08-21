module Transforms

open Unity.Mathematics

type Ranges =   
      { minX: float32; maxX:float32
        minY: float32; maxY:float32
        minZ: float32; maxZ:float32 }

let minmaxRelative s (bnds: float32) = ((s - bnds) / 2.f, (s + bnds) / 2.f)

let rangesStartToBounds (s: float3) (bounds: float3): Ranges =
    let minX, maxX = minmaxRelative s.x bounds.x
    let minY, maxY = minmaxRelative s.y bounds.y
    let minZ, maxZ = minmaxRelative s.z bounds.z
    { minX = minX; maxX = maxX
      minY = minY; maxY = maxY
      minZ = minZ; maxZ = maxZ }

let inBounds (ranges: Ranges) (segment: float3) : bool =
    List.exists2 (fun (p1: float32 list) p2-> p1.[0] = p2 || p1.[1] = p2)
        [[ranges.minX; ranges.maxX]; [ranges.minY; ranges.maxY]; [ranges.minZ; ranges.maxZ]]
        [segment.x; segment.y; segment.z] 