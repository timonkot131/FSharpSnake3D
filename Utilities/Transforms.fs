module Transforms

open Unity.Mathematics
type Ranges =   
      { minX: float32; maxX:float32
        minY: float32; maxY:float32
        minZ: float32; maxZ:float32 }

let private minmaxRelative s (bnds: float32) = ((s - bnds) / 2.f, (s + bnds) / 2.f)

let rangesStartToBounds (s: float3) (bounds: float3) =
    let minX, maxX = minmaxRelative s.x bounds.x
    let minY, maxY = minmaxRelative s.y bounds.y
    let minZ, maxZ = minmaxRelative s.z bounds.z
    { minX = minX; maxX = maxX
      minY = minY; maxY = maxY
      minZ = minZ; maxZ = maxZ }