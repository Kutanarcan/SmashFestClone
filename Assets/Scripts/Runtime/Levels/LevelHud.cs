using Game.Core;
using Game.Core.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelHud : MonoBehaviour, ITickable
{
    [SerializeField] private TMP_Text ballsText;

    [SerializeField] private Button restartButton;

    [SerializeField] private Button nextButton;

    private LevelFlowController flow;
    private int shownBalls = -1;

    public void Initialize(LevelFlowController controller)
    {
        flow = controller;

        if (restartButton != null) restartButton.onClick.AddListener(OnRestart);
        if (nextButton != null) nextButton.onClick.AddListener(OnNext);

        HideButtons();
    }

    private void OnDestroy()
    {
        if (restartButton != null) restartButton.onClick.RemoveListener(OnRestart);
        if (nextButton != null) nextButton.onClick.RemoveListener(OnNext);
    }

    public void Tick(float deltaTime)
    {
        LevelSession session = flow != null ? flow.Session : null;

        if (session == null)
        {
            HideButtons();
            return;
        }

        if (session.BallsRemaining != shownBalls)
        {
            shownBalls = session.BallsRemaining;
            SetText(ballsText, shownBalls.ToString());
        }

        SetActive(restartButton, session.State == LevelState.Lost);
        SetActive(nextButton, session.State == LevelState.Won && flow.Progress.HasNext);
    }

    private void HideButtons()
    {
        shownBalls = -1;

        SetActive(restartButton, false);
        SetActive(nextButton, false);
    }

    private void OnRestart() => flow?.Restart();

    private void OnNext() => flow?.Next();

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null && target.text != value) target.text = value;
    }

    private static void SetActive(Button button, bool visible)
    {
        if (button != null && button.gameObject.activeSelf != visible)
            button.gameObject.SetActive(visible);
    }
}
