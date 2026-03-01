using System;

public sealed class GameEventHub
{
    public event Action<GameState> GameStateChanged;
    public event Action<int> StageRequested;
    public event Action<int> StageStarted;
    public event Action<int> StageEnded;

    public event Action<BeatInfo> Beat;
    public event Action NodeSuccess;
    public event Action NodeMiss;
    public event Action SongEnded;
    public event Action<int> StageCleared;

    public void RaiseGameStateChanged(GameState state) => GameStateChanged?.Invoke(state);
    public void RaiseStageRequested(int stageIndex) => StageRequested?.Invoke(stageIndex);
    public void RaiseStageStarted(int stageIndex) => StageStarted?.Invoke(stageIndex);
    public void RaiseStageEnded(int stageIndex) => StageEnded?.Invoke(stageIndex);

    public void RaiseBeat(BeatInfo beatInfo) => Beat?.Invoke(beatInfo);
    public void RaiseNodeSuccess() => NodeSuccess?.Invoke();
    public void RaiseNodeMiss() => NodeMiss?.Invoke();
    public void RaiseSongEnded() => SongEnded?.Invoke();
    public void RaiseStageCleared(int stageIndex) => StageCleared?.Invoke(stageIndex);
}