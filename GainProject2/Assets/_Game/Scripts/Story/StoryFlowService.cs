using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class StoryFlowService : MonoBehaviour
{
    public static StoryFlowService Instance { get; private set; }

    [Header("씬")]
    [SerializeField, Tooltip("스토리 UI가 있는 씬 이름")]
    private string storySceneName = "StoryScene";

    private StoryDataSO pendingStory;

    public bool HasPendingStory => pendingStory != null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RequestStory(StoryDataSO story)
    {
        pendingStory = story;
        if (!string.IsNullOrWhiteSpace(storySceneName))
        {
            SceneManager.LoadScene(storySceneName);
        }
    }

    public StoryDataSO ConsumePendingStory()
    {
        var story = pendingStory;
        pendingStory = null;
        return story;
    }
}