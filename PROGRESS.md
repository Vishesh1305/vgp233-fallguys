# PROGRESS: ClockworkTempo (VGP233 Assignment 3)

## Current Status
- Phase: 0 (Project Foundation)
- Last completed subphase: 0.2 Unity project creation, packages, folder structure
- In progress: 0.3 Git setup (ignore rules fixed for repo layout, awaiting first commit and push)
- Next subphase: 0.4 Player Settings, resolution, physics layers

## Workflow
- Claude maintains this file directly on disk after every subphase.
- Commit and push after every subphase, then re-sync the repo into the claude.ai project.

## Completed Subphases
- [x] 0.1 Kickoff decisions
- [x] 0.2 Project creation, packages, folders
- [ ] 0.3 Git setup
- [ ] 0.4 Player Settings and physics layers

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
- Fixed: first `git add .` pulled in ClockworkTempo/Library and Temp because .gitignore paths were anchored to the repo root instead of the Unity project folder

## Tuning Values
- None yet