# Backrooms - The Queue

A new Backrooms level: **The Thousand Waits**, the indoor queue hall of a family boat ride that nobody is riding. Twenty-five lanes of galvanised rails fold back and forth across a 48 x 64 m hall under humming strip lights; a painted mural of meadows, rivers and far-off castles wraps every wall; wait-time signs count down from 120 minutes; the TVs show static and a safety video on loop. At the far end the line finally reaches a storybook castle, a loading platform with numbered gates and empty boats floating in the flume - and through the stone archway at the entrance, the queue simply starts again.

The pack is a complete modular kit: mural walls (four matching quarters), archway, exit and corner modules, floor, roof and trusses; a snap-together queue-rail system with posts, U-turns, ends and belt stanchions; the ride station (castle facade, flume, platform with gates, boats, dispatch booth, turnstiles); signage, ceiling rig and queue dressing.

Every mesh, texture, mural, sign and decal in this pack is original content built procedurally in Blender/Python for this pack (no scanned, purchased or third-party marketplace content, and no text-to-image or text-to-3D generators).

## Key numbers

- **69 assets**: 18 modular architecture / structure pieces, 49 props and 2 decals - 50,012 triangles for the whole kit.
- **6 LOD1 meshes** for the heavier props, named `<asset>_LOD1` (about 40 % of the triangles).
- **24 texture sets**, 121 PNG files.
- Demo scene `V3_Queue_ThousandWaits`: 1,449 objects, 548,694 triangles, 105 lights.
- Real-world scale (1 unit = 1 m), clean UV0 + a dedicated non-overlapping lightmap UV1 on every mesh.
- Curved parts use enough segments to stay smooth in close-ups, and hard-surface props have bevelled edges with weighted normals, so nothing looks faceted at gameplay distances.

## Folder structure

```
Backrooms_Queue.blend       Blender 5.1 source: kit overview scene + the demo level, every asset in the Asset Browser
blender_assets.cats.txt     Asset Browser catalogs (keep it next to the .blend)
fbx/<Category>/             one FBX per asset (Unity / Unreal ready)
fbx/LOD1/                   LOD1 meshes
fbx/Levels/                 the demo level as a single FBX
glb/                        glTF binary: Backrooms_Queue_<Category>.glb = kit pieces, V3_Queue_ThousandWaits.glb = the demo level
resources/textures/         all PNG textures + atlas layout JSON
unity/Editor/               BackroomsQueueBuilder.cs - one-click materials, prefabs (with LOD groups) and the demo scene
unity/Layouts/              the demo level as a JSON layout + Materials.json (used by the Unity builder)
scr/                        preview renders
Asset_List.csv              every asset with triangle count, size and materials
```

## Modular conventions

- **Hall modules** are 4 m wide and 6 m high (truss bottom chord at 5.2 m). Wall modules run +X from the pivot with the mural face on the pivot plane (Blender -Y = Unity +Z). The four mural walls `SM_QUE_Wall_4m_A..D` are consecutive quarters of one 16 m painting - place them A, B, C, D, A ... to keep the landscape continuous. `SM_QUE_Wall_Archway_8m` takes two modules; two archways placed back to back (one in each hall) form an open passage between halls.
- **Queue rails** stand on the floor (pivot = floor level under the rail axis): `SM_QUE_Rail_1m/2m/4m` run +X from a post at X=0 (the 4 m piece carries a middle post); close a run with `SM_QUE_Rail_Post`, turn it with `SM_QUE_Rail_UTurn_1m` (1 m lane pitch) or stop it with `SM_QUE_Rail_End`. Clamp-on signs (`SM_QUE_Sign_Wait_*`, `KeepMoving`, `NoLineJumping`) share the post pivot.
- **Ceiling rig**: strip lights, TVs, speakers, cameras and pennants hang from Z = 5.2 m (the truss bottom chord); their pivot is the hanging point. Lit and unlit strip lights are separate meshes (`SM_QUE_Light_Strip` / `_Off`).
- **Ride station**: `SM_QUE_Flume_4m` and `SM_QUE_Platform_4m_Gates*` share a pivot and run +X in 4 m steps; the boats float at their pivot height inside the flume. The castle facade is 12 m wide with the flume portal and a guest door.
- **Wall-mounted props** (boards, poster cases, plaque, ride lockers) have their back on the pivot and face -Y. Floor props sit on their pivot (bottom centre) and face -Y.
- Screens, poster cases, the EXIT sign and the castle lanterns use emissive materials (`M_QUE_Atlas_Lit`, `M_QUE_Bulb_Warm`, `M_QUE_FluorescentTube`).

## Textures

Each set `T_<Name>_*` contains:

| Suffix | Use |
|---|---|
| `_BaseColor` | Albedo (sRGB). RGBA for atlases and grating (alpha = cut-out). |
| `_Normal` | Tangent-space normal map, **OpenGL / Y+** (Unity, Blender, Godot). In Unreal tick *Flip Green Channel*. |
| `_ORM` | R = Ambient Occlusion, G = Roughness, B = Metallic (Unreal, glTF, Godot). Linear. |
| `_MetallicSmoothness` | Unity URP / Built-in: RGB = Metallic, A = Smoothness. Linear. |
| `_MaskMap` | Unity HDRP: R = Metallic, G = AO, B = Detail mask, A = Smoothness. Linear. |
| `_Emission` | Emission colour (signs, screens, lamps). |

Atlas layouts (UV rectangles of every sign, screen and decal) are in `resources/textures/Atlas_*_layout.json`.

## Unity (Built-in, URP or HDRP)

1. Create `Assets/BackroomsQueue/` with these sub-folders and copy the files in:
   - `Models/`   <- the contents of `fbx/` (keep the sub-folders)
   - `Textures/` <- the contents of `resources/textures/`
   - `Layouts/`  <- the contents of `unity/Layouts/` (including `Materials.json`)
   - `Editor/`   <- `unity/Editor/BackroomsQueueBuilder.cs`
2. Run **Tools > Backrooms Queue > Build Everything**. It detects your render pipeline, sets the texture import types, creates the materials, creates a prefab per model (mesh colliders and static flags on structure, box colliders on props, LOD groups for the assets that ship with a LOD1), then rebuilds the demo level as a Unity scene in `Assets/BackroomsQueue/Scenes/` with its lights (spot directions included), ambient colour and a set of saved views.
3. For the best look bake lighting (the lamps, screens and signs use emissive materials) or keep the realtime lights the builder places (Mixed mode, so they also contribute to the bake). Lightmap UVs are included (UV1), so leave *Generate Lightmap UVs* off.

FBX files are exported with Forward -Z / Up Y and transforms applied, so they import at scale 1 with zero rotation. HDRP light intensities are set as a starting point; check them once in your HDRP version.

## Unreal Engine

Import the FBX files (*Combine Meshes* off, *Generate Lightmap UVs* off - UV channel 1 is the lightmap). Import the textures, tick *Flip Green Channel* on the `_Normal` maps and set `_ORM` maps to *Masks (no sRGB)*. Build a master material with BaseColor, Normal and ORM, then create instances per texture set. `fbx/Levels/` contains the demo level as one file. Import `<asset>_LOD1` as LOD 1 of its asset if you want the extra LOD.

## Blender

Open `Backrooms_Queue.blend`. The scene `00_Kit_Overview` holds every asset (also in the Asset Browser under *Current File*, sorted into catalogs). The `V3_Queue_ThousandWaits` scene is the demo level with lights and cameras, set up for Cycles. Level objects are linked duplicates of the kit meshes, so editing a kit mesh updates the level. Textures are loaded from `resources/textures/` relative to the .blend.

## Asset list

Sizes are in Blender axes, X x Y x Z with Z up (in Unity: X x Z x Y).

### Queue hall architecture

| Asset | Tris | Size (m) |
|---|---:|---|
| `SM_QUE_Column` | 3456 | 0.52 x 0.52 x 5.8 |
| `SM_QUE_Floor_4m` | 2 | 4.0 x 4.0 x 0.0 |
| `SM_QUE_RoofTruss_4m` | 272 | 0.06 x 4.12 x 0.6 |
| `SM_QUE_Roof_4m` | 34 | 4.0 x 4.0 x 0.2 |
| `SM_QUE_Wall_4m_A` | 88 | 4.0 x 0.36 x 6.0 |
| `SM_QUE_Wall_4m_B` | 88 | 4.0 x 0.36 x 6.0 |
| `SM_QUE_Wall_4m_C` | 88 | 4.0 x 0.36 x 6.0 |
| `SM_QUE_Wall_4m_D` | 88 | 4.0 x 0.36 x 6.0 |
| `SM_QUE_Wall_4m_Exit` | 286 | 4.0 x 0.36 x 6.0 |
| `SM_QUE_Wall_Archway_8m` | 1366 | 8.0 x 0.642 x 6.12 |
| `SM_QUE_Wall_CornerColumn` | 16 | 0.52 x 0.52 x 6.0 |

### Queue rails & stanchions

| Asset | Tris | Size (m) |
|---|---:|---|
| `SM_QUE_Rail_1m` | 472 | 1.075 x 0.15 x 1.024 |
| `SM_QUE_Rail_2m` | 472 | 2.075 x 0.15 x 1.024 |
| `SM_QUE_Rail_4m` | 880 | 4.075 x 0.15 x 1.024 |
| `SM_QUE_Rail_End` | 648 | 2.075 x 0.15 x 1.024 |
| `SM_QUE_Rail_Post` | 408 | 0.15 x 0.15 x 1.0 |
| `SM_QUE_Rail_UTurn_1m` | 1400 | 0.577 x 1.048 x 1.024 |
| `SM_QUE_Stanchion_Belt` | 1056 | 2.14 x 0.34 x 1.0 |

### Ride station, flume & boats

| Asset | Tris | Size (m) |
|---|---:|---|
| `SM_QUE_Boat_Blue` | 2904 | 3.436 x 1.68 x 0.82 |
| `SM_QUE_Boat_Red` | 2904 | 3.436 x 1.68 x 0.82 |
| `SM_QUE_DispatchBooth` | 1220 | 2.198 x 2.298 x 3.34 |
| `SM_QUE_Flume_4m` | 224 | 4.0 x 2.5 x 0.45 |
| `SM_QUE_Platform_4m_Gates1-2` | 1018 | 4.075 x 3.04 x 1.79 |
| `SM_QUE_Platform_4m_Gates3-4` | 1018 | 4.075 x 3.04 x 1.79 |
| `SM_QUE_Platform_4m_Gates5-6` | 1018 | 4.075 x 3.04 x 1.79 |
| `SM_QUE_Platform_4m_Gates7-8` | 1018 | 4.075 x 3.04 x 1.79 |
| `SM_QUE_Station_Facade_12m` | 4198 | 14.34 x 1.94 x 6.54 |
| `SM_QUE_Turnstile` | 374 | 0.734 x 0.8 x 1.144 |

### Signage & wait-time signs

| Asset | Tris | Size (m) |
|---|---:|---|
| `SM_QUE_Board_RideRules` | 14 | 0.88 x 0.051 x 0.88 |
| `SM_QUE_HeightCheck` | 244 | 0.4 x 0.4 x 1.68 |
| `SM_QUE_Plaque` | 14 | 0.88 x 0.051 x 0.48 |
| `SM_QUE_PosterCase_0` | 16 | 0.8 x 0.056 x 1.08 |
| `SM_QUE_PosterCase_1` | 16 | 0.8 x 0.056 x 1.08 |
| `SM_QUE_PosterCase_2` | 16 | 0.8 x 0.056 x 1.08 |
| `SM_QUE_PosterCase_3` | 16 | 0.8 x 0.056 x 1.08 |
| `SM_QUE_Sign_Express` | 344 | 0.64 x 0.44 x 1.46 |
| `SM_QUE_Sign_KeepMoving` | 108 | 0.6 x 0.064 x 0.28 |
| `SM_QUE_Sign_LineStarts` | 344 | 0.74 x 0.44 x 1.42 |
| `SM_QUE_Sign_NoLineJumping` | 108 | 0.7 x 0.064 x 0.29 |
| `SM_QUE_Sign_QueueMap` | 344 | 0.64 x 0.44 x 1.52 |
| `SM_QUE_Sign_ThisWay` | 14 | 0.84 x 0.051 x 0.24 |
| `SM_QUE_Sign_Wait_120` | 108 | 0.5 x 0.064 x 0.43 |
| `SM_QUE_Sign_Wait_15` | 108 | 0.5 x 0.064 x 0.43 |
| `SM_QUE_Sign_Wait_30` | 108 | 0.5 x 0.064 x 0.43 |
| `SM_QUE_Sign_Wait_45` | 108 | 0.5 x 0.064 x 0.43 |
| `SM_QUE_Sign_Wait_60` | 108 | 0.5 x 0.064 x 0.43 |
| `SM_QUE_Sign_Wait_90` | 108 | 0.5 x 0.064 x 0.43 |

### Queue dressing

| Asset | Tris | Size (m) |
|---|---:|---|
| `SM_QUE_Barrel` | 1240 | 0.64 x 0.64 x 0.9 |
| `SM_QUE_Bench` | 692 | 1.8 x 0.5 x 0.886 |
| `SM_QUE_Crate` | 728 | 0.816 x 0.616 x 0.6 |
| `SM_QUE_Decal_Arrow` | 2 | 0.5 x 0.5 x 0.0 |
| `SM_QUE_Decal_WaitHere` | 2 | 1.0 x 0.5 x 0.0 |
| `SM_QUE_FireExtinguisher` | 640 | 0.189 x 0.2 x 0.57 |
| `SM_QUE_LampPost` | 1036 | 0.32 x 0.597 x 2.7 |
| `SM_QUE_MistingFan` | 3576 | 0.855 x 0.715 x 2.07 |
| `SM_QUE_RecycleCan` | 3048 | 0.6 x 0.6 x 1.1 |
| `SM_QUE_RideLockers` | 3882 | 2.4 x 0.62 x 1.88 |
| `SM_QUE_TrashCan` | 3048 | 0.6 x 0.6 x 1.1 |
| `SM_QUE_WaterFountain` | 852 | 0.5 x 0.51 x 0.9 |
| `SM_QUE_WetFloorSign` | 52 | 0.3 x 0.312 x 0.603 |

### Ceiling rig (lights, TVs, speakers)

| Asset | Tris | Size (m) |
|---|---:|---|
| `SM_QUE_Light_Strip` | 200 | 2.4 x 0.22 x 0.502 |
| `SM_QUE_Light_Strip_Off` | 200 | 2.4 x 0.22 x 0.502 |
| `SM_QUE_Pennants_8m` | 364 | 8.001 x 0.006 x 0.861 |
| `SM_QUE_SecurityCam` | 264 | 0.18 x 0.18 x 0.12 |
| `SM_QUE_Speaker` | 228 | 0.33 x 0.403 x 0.85 |
| `SM_QUE_TV_Hung_NoSignal` | 174 | 0.592 x 0.544 x 1.291 |
| `SM_QUE_TV_Hung_SafetyVideo` | 174 | 0.592 x 0.544 x 1.291 |
| `SM_QUE_TV_Hung_Static` | 174 | 0.592 x 0.544 x 1.291 |
| `SM_QUE_TV_Hung_WaitScreen` | 174 | 0.592 x 0.544 x 1.291 |

### LOD1 meshes

| Asset | Tris | Size (m) |
|---|---:|---|
| `SM_QUE_Boat_Blue_LOD1` | 1161 | 3.436 x 1.68 x 0.82 |
| `SM_QUE_Boat_Red_LOD1` | 1161 | 3.436 x 1.68 x 0.82 |
| `SM_QUE_RecycleCan_LOD1` | 1219 | 0.6 x 0.6 x 1.1 |
| `SM_QUE_RideLockers_LOD1` | 1552 | 2.399 x 0.618 x 1.878 |
| `SM_QUE_Station_Facade_12m_LOD1` | 1679 | 14.34 x 1.94 x 6.54 |
| `SM_QUE_TrashCan_LOD1` | 1219 | 0.6 x 0.6 x 1.1 |

## Credits & licence notes

- All models, textures, murals, signs and decals are original content created for this pack.
- Sign lettering uses the DejaVu typefaces (Bitstream Vera / DejaVu free font licence, which permits use in products), rendered into the textures.
- The ride (The Willowmere Voyage), the park and every name on signs are invented. The mural is an original painting made procedurally for this pack. No real logos or trademarks are included.
