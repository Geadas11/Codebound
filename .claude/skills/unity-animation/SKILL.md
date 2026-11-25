---
name: unity-animation
description: Configure Animator controllers, state machines, blend trees, Timeline, procedural animation, and tweening libraries for Unity games.
---

# unity-animation

Configure Animator controllers, state machines, blend trees, Timeline, procedural animation, and tweening libraries for Unity games.

## When to use
Activate when the user mentions:
- Animator controller design or state machines
- Blend trees (1D, 2D, direct)
- Animation layers and avatar masks
- Timeline or Playable API
- DOTween, LeanTween, or tweening
- Procedural animation or IK
- Animation events or root motion
- Sprite animation (2D)
- Animation performance optimization

## Capabilities
1. Animator architecture — state machine design, sub-state machines, parameter strategy.
2. Blend trees — locomotion blending, directional movement, aim offsets.
3. Animation layers — upper/lower body split, additive layers, avatar masks.
4. Timeline — cutscenes, sequencing, custom tracks.
5. Procedural animation — IK, look-at, procedural walk cycles.
6. Tweening — DOTween patterns, UI animation, juice effects.
7. 2D animation — sprite sheets, skeletal animation (2D Animation package), frame-by-frame.

## Architecture Decision Guide

| Need | Solution |
|---|---|
| Character locomotion | Animator + Blend Tree (2D Freeform) |
| Combat combos | Sub-state machine + AnimatorOverrideController |
| Cutscenes | Timeline + Cinemachine |
| UI transitions | DOTween or USS transitions |
| Procedural movement | Playable API or Animation Rigging |
| Thousands of animated entities | GPU instancing + custom animation |

## Unity version support
- Unity 6+: Full support (Animation Rigging 2.0)
- Unity 2022.3 LTS: Supported
- Unity 2021.3 LTS: Supported

## Limitations
- Animator controllers can become unwieldy for complex characters; consider a code-driven approach such as Animancer where appropriate.
- Root motion requires careful setup to avoid sliding.
- Timeline can be excessive for simple sequences; use tweening for lightweight UI effects.

## Codebound industrial-machine guidance
For machines, prefer separate animated components when motion must be independently controlled:
- Conveyor belt loop
- Inserter arm pivot and extension
- Rotating fan, wheel, or rotor
- Press/piston movement
- Indicator lights and warning states

Use animation parameters driven by actual machine state (working, idle, blocked, no power). Do not play a visual "working" animation when the simulation says the machine is stopped. Keep animations lightweight when many machines are visible.
