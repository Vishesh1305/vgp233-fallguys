using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.SceneManagement;

namespace ClockworkTempo
{
    /// <summary>
    /// Editor tool that builds the static greybox from Docs/LevelDesign.md as ProBuilder meshes.
    /// Everything it creates is named with the GB_ prefix, so running it again replaces only its own
    /// pieces and never touches hand built geometry (Start_Platform, the Piston Hall wall, ramp prefabs).
    /// </summary>
    public static class GreyboxBuilder
    {
        private const string MenuPath = "Tools/Clockwork Tempo/Build Greybox";
        private const string UndoName = "Build Greybox";
        private const string GeneratedPrefix = "GB_";
        private const string LevelRootName = "Level";
        private const string GroundLayerName = "Ground";

        private const string MaterialFolder = "Assets/_Project/Materials/Greybox/";
        private const string RampWidePrefabPath = "Assets/_Project/Prefabs/Level/Ramp_8w.prefab";
        private const string RampNarrowPrefabPath = "Assets/_Project/Prefabs/Level/Ramp_2w.prefab";

        // Smoothness of the placeholder disc edges; must be even for ProBuilder.
        private const int DiscSides = 32;

        private const StaticEditorFlags LevelStaticFlags = StaticEditorFlags.ContributeGI
            | StaticEditorFlags.OccluderStatic
            | StaticEditorFlags.OccludeeStatic
            | StaticEditorFlags.BatchingStatic
            | StaticEditorFlags.ReflectionProbeStatic;

        private enum Surface
        {
            Floor,
            Pad,
            Wall,
            Hazard,
            Moving
        }

        /// <summary>One axis aligned box. Centre and size are in world metres (section parents sit at the origin).</summary>
        private readonly struct BoxSpec
        {
            public readonly string Section;
            public readonly string Name;
            public readonly Vector3 Centre;
            public readonly Vector3 Size;
            public readonly Surface Surface;
            public readonly bool IsStatic;
            public readonly bool HasCollider;

            public BoxSpec(string section, string name, Vector3 centre, Vector3 size, Surface surface,
                bool isStatic = true, bool hasCollider = true)
            {
                Section = section;
                Name = name;
                Centre = centre;
                Size = size;
                Surface = surface;
                IsStatic = isStatic;
                HasCollider = hasCollider;
            }
        }

        /// <summary>One flat cylinder lying on Y. Centre is the middle of the cylinder.</summary>
        private readonly struct DiscSpec
        {
            public readonly string Section;
            public readonly string Name;
            public readonly Vector3 Centre;
            public readonly float Radius;
            public readonly float Thickness;

            public DiscSpec(string section, string name, Vector3 centre, float radius, float thickness)
            {
                Section = section;
                Name = name;
                Centre = centre;
                Radius = radius;
                Thickness = thickness;
            }
        }

        /// <summary>A hand built prefab placed at a fixed position.</summary>
        private readonly struct PrefabSpec
        {
            public readonly string Section;
            public readonly string Name;
            public readonly string Path;
            public readonly Vector3 Position;

            public PrefabSpec(string section, string name, string path, Vector3 position)
            {
                Section = section;
                Name = name;
                Path = path;
                Position = position;
            }
        }

        // Floors are 1 m thick, so centre y = top height - 0.5. Values match Docs/LevelDesign.md.
        private static readonly BoxSpec[] Boxes =
        {
            // 01 Gear Run, top y 0
            new BoxSpec("01_GearRun", "Floor_A", new Vector3(0f, -0.5f, 17f), new Vector3(10f, 1f, 10f), Surface.Floor),
            new BoxSpec("01_GearRun", "LowBarrier", new Vector3(0f, 0.4f, 18.3f), new Vector3(10f, 0.8f, 0.6f), Surface.Wall),
            new BoxSpec("01_GearRun", "Floor_B", new Vector3(0f, -0.5f, 30.25f), new Vector3(10f, 1f, 11.5f), Surface.Floor),
            new BoxSpec("01_GearRun", "Floor_C", new Vector3(0f, -0.5f, 40.5f), new Vector3(10f, 1f, 3f), Surface.Floor),
            new BoxSpec("01_GearRun", "CheckpointPad_1", new Vector3(0f, -0.5f, 45f), new Vector3(10f, 1f, 6f), Surface.Pad),

            // 02 Piston Hall, top y 2 (left wall is hand built)
            new BoxSpec("02_PistonHall", "Floor", new Vector3(0f, 1.5f, 69.5f), new Vector3(8f, 1f, 35f), Surface.Floor),
            new BoxSpec("02_PistonHall", "CheckpointPad_2", new Vector3(0f, 1.5f, 90f), new Vector3(8f, 1f, 6f), Surface.Pad),

            // 03 Escapement Bridge, top y 4 (moving platforms are static placeholders until Phase 5)
            new BoxSpec("03_EscapementBridge", "Island_A", new Vector3(0f, 3.5f, 99f), new Vector3(4f, 1f, 4f), Surface.Floor),
            new BoxSpec("03_EscapementBridge", "Placeholder_MovingPlatform_1", new Vector3(0f, 3.5f, 105f), new Vector3(4f, 1f, 4f), Surface.Moving, false),
            new BoxSpec("03_EscapementBridge", "Island_B", new Vector3(0f, 3.5f, 110.5f), new Vector3(4f, 1f, 3f), Surface.Floor),
            new BoxSpec("03_EscapementBridge", "Placeholder_MovingPlatform_2", new Vector3(0f, 3.5f, 116f), new Vector3(4f, 1f, 4f), Surface.Moving, false),
            new BoxSpec("03_EscapementBridge", "Island_C", new Vector3(0f, 3.5f, 121.5f), new Vector3(4f, 1f, 3f), Surface.Floor),
            new BoxSpec("03_EscapementBridge", "Placeholder_MovingPlatform_3", new Vector3(0f, 3.5f, 127f), new Vector3(4f, 1f, 4f), Surface.Moving, false),
            new BoxSpec("03_EscapementBridge", "CheckpointPad_3", new Vector3(0f, 3.5f, 134f), new Vector3(8f, 1f, 6f), Surface.Pad),

            // 04 Pendulum Gallery, top y 6 (beam split around the rest pads so no surfaces overlap)
            new BoxSpec("04_PendulumGallery", "Beam_A", new Vector3(0f, 5.5f, 145.5f), new Vector3(2f, 1f, 9f), Surface.Floor),
            new BoxSpec("04_PendulumGallery", "RestPad_A", new Vector3(0f, 5.5f, 151.5f), new Vector3(6f, 1f, 3f), Surface.Floor),
            new BoxSpec("04_PendulumGallery", "Beam_B", new Vector3(0f, 5.5f, 156f), new Vector3(2f, 1f, 6f), Surface.Floor),
            new BoxSpec("04_PendulumGallery", "RestPad_B", new Vector3(0f, 5.5f, 160.5f), new Vector3(6f, 1f, 3f), Surface.Floor),
            new BoxSpec("04_PendulumGallery", "Beam_C", new Vector3(0f, 5.5f, 166.5f), new Vector3(2f, 1f, 9f), Surface.Floor),
            new BoxSpec("04_PendulumGallery", "CheckpointPad_4", new Vector3(0f, 5.5f, 174f), new Vector3(8f, 1f, 6f), Surface.Pad),

            // 05 Clock Face Climb, falling tile placeholders (top y 10.5, 0.5 m thick, 0.1 m seams)
            new BoxSpec("05_ClockFaceClimb", "Placeholder_Tile_A1", new Vector3(-2f, 10.25f, 200f), new Vector3(1.9f, 0.5f, 1.9f), Surface.Hazard, false),
            new BoxSpec("05_ClockFaceClimb", "Placeholder_Tile_A2", new Vector3(0f, 10.25f, 200f), new Vector3(1.9f, 0.5f, 1.9f), Surface.Hazard, false),
            new BoxSpec("05_ClockFaceClimb", "Placeholder_Tile_A3", new Vector3(2f, 10.25f, 200f), new Vector3(1.9f, 0.5f, 1.9f), Surface.Hazard, false),
            new BoxSpec("05_ClockFaceClimb", "Placeholder_Tile_B1", new Vector3(-2f, 10.25f, 202f), new Vector3(1.9f, 0.5f, 1.9f), Surface.Hazard, false),
            new BoxSpec("05_ClockFaceClimb", "Placeholder_Tile_B2", new Vector3(0f, 10.25f, 202f), new Vector3(1.9f, 0.5f, 1.9f), Surface.Hazard, false),
            new BoxSpec("05_ClockFaceClimb", "Placeholder_Tile_B3", new Vector3(2f, 10.25f, 202f), new Vector3(1.9f, 0.5f, 1.9f), Surface.Hazard, false),
            new BoxSpec("05_ClockFaceClimb", "Placeholder_Tile_C1", new Vector3(-2f, 10.25f, 204f), new Vector3(1.9f, 0.5f, 1.9f), Surface.Hazard, false),
            new BoxSpec("05_ClockFaceClimb", "Placeholder_Tile_C2", new Vector3(0f, 10.25f, 204f), new Vector3(1.9f, 0.5f, 1.9f), Surface.Hazard, false),
            new BoxSpec("05_ClockFaceClimb", "Placeholder_Tile_C3", new Vector3(2f, 10.25f, 204f), new Vector3(1.9f, 0.5f, 1.9f), Surface.Hazard, false),
            new BoxSpec("05_ClockFaceClimb", "Placeholder_Tile_D1", new Vector3(-2f, 10.25f, 206f), new Vector3(1.9f, 0.5f, 1.9f), Surface.Hazard, false),
            new BoxSpec("05_ClockFaceClimb", "Placeholder_Tile_D2", new Vector3(0f, 10.25f, 206f), new Vector3(1.9f, 0.5f, 1.9f), Surface.Hazard, false),
            new BoxSpec("05_ClockFaceClimb", "Placeholder_Tile_D3", new Vector3(2f, 10.25f, 206f), new Vector3(1.9f, 0.5f, 1.9f), Surface.Hazard, false),

            // 06 Finish, top y 10.5
            new BoxSpec("06_Finish", "Platform", new Vector3(0f, 10f, 213f), new Vector3(12f, 1f, 12f), Surface.Pad),
            new BoxSpec("06_Finish", "FinishLine", new Vector3(0f, 10.51f, 209.3f), new Vector3(12f, 0.02f, 0.6f), Surface.Wall, true, false),
            new BoxSpec("06_Finish", "Arch_PostLeft", new Vector3(-5.5f, 13f, 209.3f), new Vector3(1f, 5f, 1f), Surface.Wall),
            new BoxSpec("06_Finish", "Arch_PostRight", new Vector3(5.5f, 13f, 209.3f), new Vector3(1f, 5f, 1f), Surface.Wall),
            new BoxSpec("06_Finish", "Arch_Beam", new Vector3(0f, 16f, 209.3f), new Vector3(12f, 1f, 1f), Surface.Wall),
        };

        // Spinning disc placeholders: tops at y 7.5, 9 and 10.5, 0.5 m thick.
        private static readonly DiscSpec[] Discs =
        {
            new DiscSpec("05_ClockFaceClimb", "Placeholder_Disc_1", new Vector3(0f, 7.25f, 181f), 3f, 0.5f),
            new DiscSpec("05_ClockFaceClimb", "Placeholder_Disc_2", new Vector3(0f, 8.75f, 188f), 3f, 0.5f),
            new DiscSpec("05_ClockFaceClimb", "Placeholder_Disc_3", new Vector3(0f, 10.25f, 195f), 3f, 0.5f),
        };

        // Hand built ramp prefabs (pivot at the box centre): 2 m rise over 4 m.
        private static readonly PrefabSpec[] Prefabs =
        {
            new PrefabSpec("01_GearRun", "Ramp_To_PistonHall", RampWidePrefabPath, new Vector3(0f, 1f, 50f)),
            new PrefabSpec("02_PistonHall", "Ramp_To_EscapementBridge", RampWidePrefabPath, new Vector3(0f, 3f, 95f)),
            new PrefabSpec("03_EscapementBridge", "Ramp_To_PendulumGallery", RampNarrowPrefabPath, new Vector3(0f, 5f, 139f)),
        };

        [MenuItem(MenuPath)]
        private static void BuildGreybox()
        {
            Scene scene = SceneManager.GetActiveScene();
            Transform levelRoot = FindRoot(scene, LevelRootName);
            if (levelRoot == null)
            {
                EditorUtility.DisplayDialog(UndoName, $"No root object named '{LevelRootName}' in the open scene.", "OK");
                return;
            }

            int groundLayer = LayerMask.NameToLayer(GroundLayerName);
            if (groundLayer < 0)
            {
                EditorUtility.DisplayDialog(UndoName, $"Layer '{GroundLayerName}' does not exist.", "OK");
                return;
            }

            Dictionary<Surface, Material> materials = LoadMaterials();
            if (materials == null)
            {
                return;
            }

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(UndoName);
            int undoGroup = Undo.GetCurrentGroup();

            int removed = ClearGenerated(levelRoot);
            int created = 0;

            foreach (BoxSpec box in Boxes)
            {
                ProBuilderMesh mesh = ShapeGenerator.GenerateCube(PivotLocation.Center, box.Size);
                Setup(mesh, GetSection(levelRoot, box.Section), box.Name, box.Centre, materials[box.Surface],
                    groundLayer, box.IsStatic, box.HasCollider);
                created++;
            }

            foreach (DiscSpec disc in Discs)
            {
                ProBuilderMesh mesh = ShapeGenerator.GenerateCylinder(PivotLocation.Center, DiscSides, disc.Radius, disc.Thickness, 0);
                Setup(mesh, GetSection(levelRoot, disc.Section), disc.Name, disc.Centre, materials[Surface.Moving],
                    groundLayer, false, true);
                created++;
            }

            foreach (PrefabSpec spec in Prefabs)
            {
                if (PlacePrefab(spec, GetSection(levelRoot, spec.Section)))
                {
                    created++;
                }
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"{UndoName}: removed {removed} old pieces, created {created} pieces.");
        }

        private static void Setup(ProBuilderMesh mesh, Transform parent, string name, Vector3 centre, Material material,
            int layer, bool isStatic, bool hasCollider)
        {
            GameObject go = mesh.gameObject;
            go.name = GeneratedPrefix + name;
            go.layer = layer;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = centre;

            if (hasCollider)
            {
                go.AddComponent<MeshCollider>();
            }

            // ProBuilder stores materials per face; ToMesh and Refresh rebuild the Unity mesh and collider from them.
            mesh.SetMaterial(mesh.faces, material);
            mesh.ToMesh();
            mesh.Refresh();

            GameObjectUtility.SetStaticEditorFlags(go, isStatic ? LevelStaticFlags : (StaticEditorFlags)0);
            Undo.RegisterCreatedObjectUndo(go, UndoName);
        }

        private static bool PlacePrefab(PrefabSpec spec, Transform parent)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(spec.Path);
            if (prefab == null)
            {
                Debug.LogWarning($"{UndoName}: prefab not found at {spec.Path}, skipped {spec.Name}.");
                return false;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = GeneratedPrefix + spec.Name;
            instance.transform.localPosition = spec.Position;
            Undo.RegisterCreatedObjectUndo(instance, UndoName);
            return true;
        }

        private static int ClearGenerated(Transform levelRoot)
        {
            // Collect first, then destroy, so the hierarchy is not modified while it is being walked.
            List<GameObject> toRemove = new List<GameObject>();
            foreach (Transform section in levelRoot)
            {
                foreach (Transform child in section)
                {
                    if (child.name.StartsWith(GeneratedPrefix))
                    {
                        toRemove.Add(child.gameObject);
                    }
                }
            }

            foreach (GameObject go in toRemove)
            {
                Undo.DestroyObjectImmediate(go);
            }

            return toRemove.Count;
        }

        private static Transform GetSection(Transform levelRoot, string sectionName)
        {
            Transform section = levelRoot.Find(sectionName);
            if (section != null)
            {
                return section;
            }

            GameObject created = new GameObject(sectionName);
            Undo.RegisterCreatedObjectUndo(created, UndoName);
            created.transform.SetParent(levelRoot, false);
            return created.transform;
        }

        private static Transform FindRoot(Scene scene, string rootName)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name == rootName)
                {
                    return root.transform;
                }
            }

            return null;
        }

        private static Dictionary<Surface, Material> LoadMaterials()
        {
            Dictionary<Surface, Material> materials = new Dictionary<Surface, Material>
            {
                { Surface.Floor, AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "M_Grey_Floor.mat") },
                { Surface.Pad, AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "M_Grey_Pad.mat") },
                { Surface.Wall, AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "M_Grey_Wall.mat") },
                { Surface.Hazard, AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "M_Grey_Hazard.mat") },
                { Surface.Moving, AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "M_Grey_Moving.mat") },
            };

            foreach (KeyValuePair<Surface, Material> pair in materials)
            {
                if (pair.Value == null)
                {
                    EditorUtility.DisplayDialog(UndoName, $"Missing greybox material for {pair.Key} in {MaterialFolder}.", "OK");
                    return null;
                }
            }

            return materials;
        }
    }
}
