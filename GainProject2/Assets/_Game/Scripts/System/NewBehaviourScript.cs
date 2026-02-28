public readonly struct BeatInfo
{
    public readonly int BeatIndex;
    public readonly double BeatDspTime;
    public readonly float BeatDuration;

    public BeatInfo(int beatIndex, double beatDspTime, float beatDuration)
    {
        BeatIndex = beatIndex;
        BeatDspTime = beatDspTime;
        BeatDuration = beatDuration;
    }
}