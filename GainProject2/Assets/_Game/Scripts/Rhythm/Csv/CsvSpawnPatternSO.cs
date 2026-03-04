using UnityEngine;

[CreateAssetMenu(menuName = "리듬 도사/리듬/CSV/스폰 패턴", fileName = "SO_CsvSpawnPattern")]
public sealed class CsvSpawnPatternSO : ScriptableObject
{
    [Header("CSV")]
    [SerializeField] private TextAsset csv;

    [Header("파싱")]
    [SerializeField, Tooltip("구분자")] private char delimiter = ',';
    [SerializeField, Tooltip("헤더가 첫 줄에 있다고 가정")] private bool hasHeader = true;

    public TextAsset Csv => csv;
    public char Delimiter => delimiter;
    public bool HasHeader => hasHeader;
}