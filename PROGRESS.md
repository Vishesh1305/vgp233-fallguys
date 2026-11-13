# PROGRESS: ClockworkTempo (VGP233 Assignment 3)

## Current Status
- Phase: 1 (Player and Camera)
- Last completed subphase: 0.4 Player Settings, resolution, physics layers
- In progress: 1.1 Placeholder bean character
- Next subphase: 1.2 Input Actions asset

## Repository
- Remote: https://github.com/Vishesh1305/vgp233-fallguys (branch main)
- Local: Z:\LaSalle\Unity\Assignment_03

## Workflow
- Claude maintains this file directly on disk after every subphase.
- Commit and push after every subphase, then re-sync the repo into the claude.ai project.

## Completed Subphases
- [x] 0.1 Kickoff decisions
- [x] 0.2 Project creation, packages, folders
- [x] 0.3 Git setup (pushed to main)
- [x] 0.4 Player Settings and physics layers (verified from ProjectSettings files on disk)
- [ ] 1.1 Placeholder bean character
- [ ] 1.2 Input Actions asset
- [ ] 1.3 PlayerController
- [ ] 1.4 Cinemachine third person camera
- [ ] 1.5 Feel tuning pass

## Key Decisions
| Area | Decision | Reason |
|---|---|---|
| Engine | Unity 6.3 LTS, 6000.3.26f1 | Installed LTS, Unity 6 APIs (Rigidbody.linearVelocity) |
| Render pipeline | URP (Universal 3D template) | Spec baseline, Global Volume for post processing |
| Theme | C: Clockwork Tempo (brass and gear clock tower) | Chosen at kickoff |
| Twist | Moving obstacles read phase from one Conductor clock synced to music BPM | Readable, learnable, audio supports B1 |
| Twist safety rule | Conductor is optional; obstacles fall back to local time if none exists | Core criteria never depend on audio sync |
| Palette | #B08D57 brass, #2E3A4F slate navy, #E8DCC2 parchment, #C0392B signal red | Theme identity |
| Scene layout | Single gameplay scene (Course); main menu is a GameManager state | Restart is a scene reload; no cross scene lifetime bugs |
| Player controller | Rigidbody based | Obstacles must physically push the player |
| Camera | Cinemachine 3.x CinemachineCamera (Orbital Follow, Rotation Composer, Input Axis Controller) | Unity 6 baseline |
| Input | Input System (new) | Spec baseline; wiring approach chosen in 1.2 |
| Namespace | ClockworkTempo (single, no sub namespaces) | Coding standards |
| Private fields | _camelCase with [SerializeField] | Distinguishes fields from locals and properties |
| Repo layout | Git root is Z:\LaSalle\Unity\Assignment_03; Unity project is the ClockworkTempo/ subfolder | README, Docs and PROGRESS.md sit outside the Unity project; .gitignore paths are prefixed with /ClockworkTempo/ |
| Constants | PascalCase `private const` for fixed spec values (for example 1920x1080) | Spec constants are not tuning values, so they stay out of the inspector |
| Resolution | Player Settings 1920x1080 Windowed, not resizable, no fullscreen switch, plus DisplayBootstrap (RuntimeInitializeOnLoadMethod BeforeSceneLoad) calling Screen.SetResolution | Standalone players can restore a cached window size from the registry; the boot call overrides it |
| Physics layers | 6 Player, 7 Ground, 8 Obstacle, 9 Trigger | Layer masks for ground check, camera collision and trigger filtering |
| Collision matrix | Trigger collides only with Player; Obstacle off with Obstacle and Ground; Player off with Player | Triggers fire only for the player; no wasted contact pairs between level pieces |
| Physics queries | Queries Hit Triggers off | Ground checks and raycasts never hit checkpoint or kill volumes |
| Player hierarchy | Player root (Rigidbody, CapsuleCollider, pivot at feet) with a Visual child holding Body and Visor meshes (no colliders) | Feet pivot makes spawn points sit on the floor; only Visual rotates to face movement |
| Player rotation | Rigidbody rotation frozen on X, Y and Z; facing is applied to the Visual child | Capsule collider is symmetric on Y, so obstacles cannot spin or tip the player and facing stays under script control |
| Player physics | Mass 1, Interpolate, Continuous Dynamic, PM_Player_Frictionless (0 friction, Minimum combine) | Smooth visuals at 50 Hz physics, no tunnelling, no sticking to walls; deceleration is done in code |
| Asset naming | M_ materials, PM_ physics materials, prefabs by plain name | Easy to filter in the Project window |
| Version control | Git, Git LFS for textures, models, audio, fonts | Keeps binary history out of the main object store |
| Serialization | Visible Meta Files, Force Text | Readable diffs, mergeable scenes and prefabs |

## Package Versions
- com.unity.render-pipelines.universal: 17.3.0
- com.unity.probuilder: 6.1.4
- com.unity.cinemachine: 3.1.7
- com.unity.inputsystem: 1.20.0
- com.unity.ugui (includes TextMeshPro): 2.0.0
- Editor: 6000.3.26f1 (de8167e7440c)

## Folder Structure
Assets/_Project/{Art/Models, Art/Textures, Audio/Music, Audio/SFX, Materials, Prefabs, Scenes, ScriptableObjects, Scripts/{Core, Player, Obstacles, Level, Rhythm, UI}, Settings, ThirdParty, UI/Fonts, UI/Sprites}

## Known Bugs
- None open
- Watch: ProjectSettings.asset still has defaultIsNativeResolution: 1 (Unity may hide or ignore this flag in Windowed mode, unverified); DisplayBootstrap forces 1920x1080 in builds regardless. Confirm in the 7.4 build test.
- Fixed: first `git add .` pulled in ClockworkTempo/Library and Temp because .gitignore paths were anchored to the repo root instead of the Unity project folder

## Tuning Values
- None yet