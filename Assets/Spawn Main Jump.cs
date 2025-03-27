using UnityEngine;
using UnityEditor;

public class IrregularCircularSpawner : EditorWindow
{
    public GameObject[] prefabs; // 프리팹 배열
    public float radiusX = 5f; // X축 반경
    public float radiusZ = 5f; // Z축 반경
    public float objectSize = 1f; // 오브젝트 크기 (간격 조정)
    public float irregularity = 0.3f; // 불규칙성 정도 (0이면 완전한 원형)
    public bool randomRotation = true; // 랜덤 회전 여부

    [MenuItem("Tools/Spawn Irregular Circle")]
    public static void ShowWindow()
    {
        GetWindow<IrregularCircularSpawner>("Irregular Circle Spawner");
    }

    void OnGUI()
    {
        GUILayout.Label("Prefab Spawner Settings", EditorStyles.boldLabel);

        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty prefabsProperty = serializedObject.FindProperty("prefabs");
        EditorGUILayout.PropertyField(prefabsProperty, true);
        serializedObject.ApplyModifiedProperties();

        radiusX = EditorGUILayout.FloatField("Radius X", radiusX);
        radiusZ = EditorGUILayout.FloatField("Radius Z", radiusZ);
        objectSize = EditorGUILayout.FloatField("Object Size", objectSize);
        irregularity = EditorGUILayout.Slider("Irregularity", irregularity, 0f, 1f);
        randomRotation = EditorGUILayout.Toggle("Random Rotation", randomRotation);

        if (GUILayout.Button("Spawn Prefabs") && prefabs.Length > 0)
        {
            SpawnPrefabs();
        }
    }

    void SpawnPrefabs()
    {
        if (prefabs.Length == 0 || objectSize <= 0) return;

        float averageRadius = (radiusX + radiusZ) / 2f; // 평균 반경
        int objectCount = Mathf.Max(1, Mathf.FloorToInt((2 * Mathf.PI * averageRadius) / objectSize)); // 빈틈 없이 배치할 개수 계산

        for (int i = 0; i < objectCount; i++)
        {
            float angle = i * (360f / objectCount) * Mathf.Deg2Rad; // 균등한 간격으로 배치

            // 불규칙성 추가: 반경에 작은 랜덤 값 적용
            float randomOffset = 1 + Random.Range(-irregularity, irregularity);
            float finalRadiusX = radiusX * randomOffset;
            float finalRadiusZ = radiusZ * randomOffset;

            Vector3 position = new Vector3(Mathf.Cos(angle) * finalRadiusX, 0, Mathf.Sin(angle) * finalRadiusZ); // 불규칙한 원 좌표 계산

            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)]; // 랜덤 프리팹 선택
            GameObject newObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            newObject.transform.position = position;

            // 랜덤 회전 적용
            if (randomRotation)
                newObject.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            else
                newObject.transform.LookAt(Vector3.zero);

            Undo.RegisterCreatedObjectUndo(newObject, "Spawn Irregular Prefab");
            EditorUtility.SetDirty(newObject);
        }
    }
}
