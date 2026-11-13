# PROGRESS: ClockworkTempo (VGP233 Assignment 3)

## Current Status
- Phase: 1 (Player and Camera)
- Last completed subphase: 1.3 PlayerController
- In progress: 1.4 Cinemachine third person camera
- Next subphase: 1.5 Feel tuning pass

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
- [x] 1.1 Placeholder bean character (prefab verified on disk)
- [x] 1.2 Input Actions asset (generated class and gamepad bindings verified on disk)
- [x] 1.3 PlayerController (gravity scale added after feel test)
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
| Input wiring | PlayerControls.inputactions (map Gameplay: Move, Look, Jump, Pause) with a generated C# class, owned by PlayerInputReader on the Player | Typed, compile checked access with no string lookups; explicit Enable and Disable per map; no inspector event wiring to break silently |
| Input reading | PlayerInputReader polls in Update (DefaultExecutionOrder -100) and exposes MoveInput, JumpPressed, JumpHeld and a PausePressed event | Every consumer sees the same frame's input; physics code never reads devices directly |
| Look input | Read by Cinemachine Input Axis Controller through an InputActionReference to PlayerControls/Gameplay/Look | Camera owns its own input; no duplicate plumbing through the player |
| Gamepad support | Added at user request: left stick and d-pad move, right stick look, South button jump, Start pause (Gamepad control scheme) | Not graded by the spec (keyboard and mouse is required); kept in the same actions so no extra code paths |
| Look scaling | Mouse delta binding uses Cinemachine's DeltaTimeScale processor (per frame to per second) then ScaleVector2(0.1); right stick uses ScaleVector2(180, 110); axis Gain is 1 on X and -1 on Y, CancelDeltaTime off | Both devices reach Cinemachine as degrees per second, so one axis setting fits both with no custom code (verified against Cinemachine 3.1.7 source) |
| Camera rig | PlayerFollowCamera: CinemachineCamera (FOV 65) + OrbitalFollow (Sphere, radius 7, vertical -10 to 55, no recentering) + RotationComposer (no damping) + InputAxisController + Deoccluder; tracks Player/CameraTarget at 1.5 m | Spec third person follow camera with mouse orbit, vertical clamp and collision |
| Camera collision | Deoccluder pulls the camera forward; Collide Against Default and Ground only, Ignore Tag Player, camera radius 0.3 | Moving obstacles do not make the camera snap in every time they cross the view |
| Cursor | CursorLockController on GameSystems: SetLocked(bool), re-locks on focus regain | GameManager will drive it per state in Phase 4 |
| Menus and camera input (for Phase 4) | Disable the CinemachineInputAxisController while paused or in menus | It reads Look from the asset directly, so disabling PlayerInputReader alone does not stop camera look |
| Menu navigation (for Phase 4) | Gamepad needs a first selected button on each menu and pause panel | Otherwise UI cannot be driven without a mouse |
| Pause bindings | Escape and P | Escape also releases the cursor in the Editor, so P is the reliable key for Play mode testing |
| Project-wide actions | Disabled; template InputSystem_Actions asset removed | Avoids a second, unused actions asset being auto enabled at startup |
| Generated code | Assets/_Project/Scripts/Player/PlayerControls.cs is generated by the Input System importer | Excluded from coding standards review; never edit it by hand |
| Movement model | Horizontal velocity steered toward input x MaxSpeed with Vector3.MoveTowards, applied as a VelocityChange delta | Gives acceleration and deceleration, and keeps obstacle pushes instead of overwriting velocity |
| Movement tuning | PlayerMovementSettings ScriptableObject (Assets/_Project/ScriptableObjects) | Changes made in Play mode persist, which makes the 1.5 tuning pass fast |
| Ground check | SphereCast down from the bottom capsule sphere against GroundMask (Default, Ground, Obstacle), triggers ignored, max slope 45 degrees | Gives the ground normal for slopes and the hit collider for moving platforms in 5.3 |
| Slopes | While grounded, the gravity component along the ground plane is cancelled | Frictionless collider would otherwise slide down ramps |
| Jump | v = sqrt(2gh) from JumpHeight using the scaled rising gravity (g x GravityScale); coyote time and jump buffer windows; 0.1 s ground lockout after a jump | Height is set in metres, not as a force value |
| Gravity feel | Airborne GravityScale 2.5, then x1.6 while falling and x2.0 while rising with jump released (own jump only) | Real world 9.81 with a 2.2 m jump gives about 1.1 s of airtime and reads as floaty; scaled gravity keeps the height and cuts airtime to about 0.75 s |
| Facing | Visual child rotated toward the input direction in Update with RotateTowards | Visual only, so it never touches physics |
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
| Setting | Value |
|---|---|
| Max Speed | 8 |
| Acceleration / Deceleration | 60 / 70 |
| Turn Speed | 720 |
| Air Control | 0.4 |
| Jump Height | 2.2 |
| Coyote Time / Jump Buffer | 0.12 / 0.12 |
| Gravity Scale | 2.5 |
| Fall / Low Jump Multiplier | 1.6 / 2.0 |
| Max Fall Speed | 30 |
| Ground Check Distance / Max Slope | 0.15 / 45 |
| Camera FOV / Orbit Radius | 65 / 7 |
| Camera Vertical Range / Start | -10 to 55 / 20 |
| Mouse Look Scale | 0.1 degrees per pixel |
| Stick Look Speed | 180 / 110 degrees per second |
| Look Accel / Decel Time | 0.05 / 0.05 |