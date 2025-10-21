using UnityEngine;

[System.Serializable]
public class FileData
{
    public string fileName;
    public string extension;
    public FileCategory correctCategory;
    public bool isMalicious; // 바이러스나 중복 파일 여부
}
