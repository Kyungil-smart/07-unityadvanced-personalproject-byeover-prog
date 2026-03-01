#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace GnalIhu.Rhythm.Editor
{
    public static class RhythmToolsMenu
    {
        //내가 지은 이름 이지만... 별로다...
        [MenuItem("Tools/리듬도사/리듬/필수 폴더 만들기")]
        public static void CreateFolders()
        {
            CreateFolderIfMissing("Assets", "_Game");
            CreateFolderIfMissing("Assets/_Game", "Scripts");
            CreateFolderIfMissing("Assets/_Game/Scripts", "Rhythm");
            CreateFolderIfMissing("Assets/_Game/Scripts/Rhythm", "Editor");
            CreateFolderIfMissing("Assets/_Game", "SO");
            CreateFolderIfMissing("Assets/_Game/SO", "Rhythm");
            CreateFolderIfMissing("Assets/_Game", "Prefabs");
            CreateFolderIfMissing("Assets/_Game/Prefabs", "Rhythm");

            AssetDatabase.Refresh();
            Debug.Log("[리듬도사] 리듬 폴더 생성 완료");
        }

        private static void CreateFolderIfMissing(string parent, string folderName)
        {
            string path = $"{parent}/{folderName}";
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, folderName);
        }

        [MenuItem("Tools/리듬도사/리듬/리듬 시스템 기본 오브젝트 생성")]
        public static void CreateRhythmSystem()
        {
            var root = new GameObject("RhythmSystem");
            Undo.RegisterCreatedObjectUndo(root, "Create RhythmSystem");

            var conductor = root.AddComponent<GnalIhu.Rhythm.RhythmConductor>();
            var spawner = root.AddComponent<GnalIhu.Rhythm.RhythmSpawner>();
            spawner.AssignConductor(conductor);

            var lanesRoot = new GameObject("Lanes");
            Undo.RegisterCreatedObjectUndo(lanesRoot, "Create Lanes");
            lanesRoot.transform.SetParent(root.transform);

            var laneA = new GameObject("Lane_A");
            Undo.RegisterCreatedObjectUndo(laneA, "Create Lane_A");
            laneA.transform.SetParent(lanesRoot.transform);

            var lane = laneA.AddComponent<GnalIhu.Rhythm.RhythmLane>();

            // 노드 5개 샘플 생성(위치/박수는 사용자가 조정)
            for (int i = 0; i < 5; i++)
            {
                var nodeGo = new GameObject($"Node_{i:00}");
                Undo.RegisterCreatedObjectUndo(nodeGo, "Create Node");
                nodeGo.transform.SetParent(laneA.transform);

                nodeGo.transform.position = new Vector3(6f - (i * 1.5f), 0f, 0f);

                var node = nodeGo.AddComponent<GnalIhu.Rhythm.RhythmLaneNode>();
                node.beatsFromPrevious = 1f;
            }

            Selection.activeGameObject = root;
            Debug.Log("[리듬도사] RhythmSystem 샘플 생성 완료 (LaneId/패턴/노드 위치는 Inspector에서 설정)");
        }
    }
}
#endif