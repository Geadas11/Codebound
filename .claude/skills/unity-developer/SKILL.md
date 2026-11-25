---
name: unity-developer
description: Build and maintain Unity games with optimized C# scripts, efficient rendering, asset management, profiling, and cross-platform practices.
---

# Unity Developer

## Provenance
Curated working version based on the LobeHub listing for `sickn33/antigravity-awesome-skills` (`skills/unity-developer/SKILL.md`). The original upstream file could not be retrieved in full during packaging; this file preserves the verified scope and adds a practical project-safe workflow. See README for details.

## Use this skill when
- Working on a Unity game or simulation.
- Implementing or refactoring C# gameplay, UI, rendering, or asset workflows.
- Investigating performance, memory, build, or platform issues.
- Integrating models, textures, animations, audio, or Addressables.

## Do not use when
- The task is unrelated to Unity.
- The task requires an external service or tool that is not configured.
- The request would require broad architectural changes without first auditing the existing project.

## Working procedure
1. Clarify the goal, Unity version, render pipeline, target platforms, and constraints.
2. Inspect the existing project structure and relevant scenes, prefabs, scripts, packages, and assets before changing anything.
3. Preserve established gameplay and project architecture unless a change is explicitly approved.
4. Prefer modular, testable C#; use null-safe component access and avoid unnecessary allocations in hot paths.
5. Use profiling evidence before claiming performance improvements.
6. For rendering, consider URP/HDRP compatibility, batching, GPU instancing, LODs, occlusion culling, and shader cost where relevant.
7. For assets, verify import settings, texture compression, mesh complexity, materials, animation compression, and dependency management.
8. Validate in the Unity Editor and, where possible, with a build for the target platform.
9. Report changed files, tests performed, known limitations, and rollback considerations.

## Codebound-specific safeguards
- Treat Codebound as an existing Unity project, not a blank template.
- Do not replace or rewrite gameplay systems just to make an isolated visual task easier.
- Do not remove saves, simulation, programming tools, multiplayer, inventory, or HUD functionality.
- Keep menu UI separate from gameplay UI; verify scene/state transitions rather than covering gameplay HUD elements with opaque panels.
- Keep generated art assets in clearly named folders and preserve source exports.
- Never claim the game was tested unless the Editor/Play Mode or a build was actually run.

## Performance topics
- Profile CPU, GPU, memory, and garbage collection before optimizing.
- Consider object pooling for frequently spawned objects.
- Avoid per-frame allocations and repeated expensive scene searches.
- Use Jobs/Burst/ECS only where the project and workload justify them.
- Check draw calls, overdraw, texture sizes, mesh complexity, and shader variants.

## Asset integration checklist
- Confirm file format and importer support.
- Check scale, pivot, orientation, normals, UVs, materials, and textures.
- Check animation clips, rig hierarchy, and root motion if applicable.
- Create prefabs only after validating the imported asset.
- Test at the actual gameplay/menu camera distance.
