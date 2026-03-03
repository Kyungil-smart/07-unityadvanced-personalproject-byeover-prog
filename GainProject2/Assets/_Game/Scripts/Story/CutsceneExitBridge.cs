using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class CutsceneExitBridge : MonoBehaviour
{
    [Header("종료 동작")]
    [SerializeField, Tooltip("0=튜토리얼, 1~8=스테이지")]
    private int nextStageIndex = 0;

    [SerializeField, Tooltip("스테이지로 가지 않고 씬 이동을 쓰려면 체크")]
    private bool useSceneLoad;

    [SerializeField, Tooltip("useSceneLoad가 켜졌을 때 로드할 씬 이름")]
    private string nextSceneName;

    [Header("옵션")]
    [SerializeField, Tooltip("종료 시 타임스케일을 1로 복구")]
    private bool restoreTimeScale = true;

    [SerializeField, Tooltip("종료 시 오디오 일시정지 해제")]
    private bool resumeAudio = true;

    public void Exit()
    {
        if (restoreTimeScale) Time.timeScale = 1f;

        if (resumeAudio && GameManager.Instance != null)
            GameManager.Instance.SetPaused(false);

        if (!useSceneLoad)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.RequestStage(nextStageIndex);
            return;
        }

        if (!string.IsNullOrWhiteSpace(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }
}