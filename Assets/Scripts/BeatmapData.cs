using System;

[Serializable]
public class NoteData
{
    public float beat;
    public int holeID;
    public int enemyID;
}

[Serializable]
public class BeatmapData
{
    public string songName;
    public float bpm;
    public NoteData[] notes;
}