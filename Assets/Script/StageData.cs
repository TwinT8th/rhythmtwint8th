using UnityEngine;

[System.Serializable]
public class StageData
{
    public string stageName;
    public string bgmName;
    public string patternFile;
    public Sprite background;
    public GameObject characterPrefab;
    public float bpm = 90f;
}