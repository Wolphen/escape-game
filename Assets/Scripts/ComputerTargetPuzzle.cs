using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>Des manches de tir dans un ordre aleatoire allument le moniteur.</summary>
[DisallowMultipleComponent]
public class ComputerTargetPuzzle : MonoBehaviour
{
    [SerializeField] private PuzzleBlaster acceptedGun;
    [Tooltip("Facultatif. Renseigne dans Main 1 pour jouer cette epreuve apres le coffre.")]
    [SerializeField] private SafeDoor requiredSafe;
    [SerializeField] private Gamerule gameRule;
    [Tooltip("Cibles numerotees 1, 2, 3... L'ordre de chaque manche est melange.")]
    [SerializeField] private PcPowerTarget[] targets;
    [Header("Difficulte")]
    [Min(1)] [SerializeField] private int roundsToWin = 3;
    [Tooltip("Le chrono commence au premier bon tir de chaque tentative.")]
    [Min(1f)] [SerializeField] private float secondsPerRound = 10f;
    [Header("Affichage et sons")]
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private GameObject poweredScreen;
    [SerializeField] private TMP_Text screenText;
    [SerializeField] private Light screenLight;
    [SerializeField] private AudioSource feedbackSource;
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip errorClip;
    [SerializeField] private AudioClip completionClip;
    [SerializeField] private UnityEvent onCompleted = new UnityEvent();

    public bool IsPowered { get; private set; }
    public int NextTargetIndex { get; private set; }
    public int CurrentRound { get; private set; }
    public int RoundsToWin => Mathf.Max(1, roundsToWin);
    public bool IsRoundRunning { get; private set; }
    public float RemainingSeconds => IsRoundRunning ? Mathf.Max(0f, roundDeadline - Time.time) : Mathf.Max(1f, secondsPerRound);
    public bool IsUnlocked => requiredSafe == null || requiredSafe.IsOpen;
    private bool wasUnlocked;
    private int[] roundOrder;
    private float roundDeadline;
    private int displayedSeconds;
    private string roundFeedback;

    private void Awake()
    {
        IsPowered = false;
        CurrentRound = 1;
        NextTargetIndex = 0;
        IsRoundRunning = false;
        if (poweredScreen != null) poweredScreen.SetActive(false);
        if (screenLight != null) screenLight.enabled = false;
        if (screenText != null) screenText.text = "Fin";
        ShuffleOrder();
        UpdateTargets();
        wasUnlocked = IsUnlocked;
        UpdateInstructions();
    }

    private void Update()
    {
        if (IsUnlocked != wasUnlocked)
        {
            wasUnlocked = IsUnlocked;
            UpdateInstructions();
        }
        if (!IsPowered && IsUnlocked && IsRoundRunning)
        {
            if (Time.time >= roundDeadline)
                RetryRound("Temps écoulé ! Recommence cette manche.");
            else if (Mathf.CeilToInt(RemainingSeconds) != displayedSeconds)
                UpdateInstructions();
        }
    }

    public void RegisterHit(PuzzleBlaster source, PcPowerTarget target)
    {
        if (!isActiveAndEnabled || IsPowered || !IsUnlocked || source == null || source != acceptedGun ||
            target == null || targets == null || targets.Length == 0 || roundOrder == null)
            return;
        int index = System.Array.IndexOf(targets, target);
        if (index < 0)
            return;
        // Un tir arrive exactement a la limite : le delai reste prioritaire.
        if (IsRoundRunning && Time.time >= roundDeadline)
        {
            RetryRound("Temps écoulé ! Recommence cette manche.");
            return;
        }
        // Ne pas punir un second tir sur une cible deja validee dans cette manche.
        for (int i = 0; i < NextTargetIndex; i++)
            if (roundOrder[i] == index) return;
        if (index != roundOrder[NextTargetIndex])
        {
            RetryRound("Mauvais ordre ! Recommence cette manche.");
            return;
        }

        if (!IsRoundRunning)
        {
            IsRoundRunning = true;
            roundDeadline = Time.time + Mathf.Max(1f, secondsPerRound);
        }
        roundFeedback = null;

        NextTargetIndex++;
        UpdateTargets();
        if (NextTargetIndex == targets.Length)
        {
            IsRoundRunning = false;
            if (CurrentRound < RoundsToWin)
            {
                CurrentRound++;
                NextTargetIndex = 0;
                ShuffleOrder();
                roundFeedback = "Manche réussie ! Lis le nouvel ordre.";
                UpdateTargets();
                UpdateInstructions();
                Play(completionClip);
                return;
            }
            IsPowered = true;
            if (poweredScreen != null) poweredScreen.SetActive(true);
            if (screenText != null) screenText.text = "Fin";
            if (screenLight != null) screenLight.enabled = true;
            if (gameRule != null) gameRule.MarkComputerPowered();
            UpdateInstructions();
            Play(completionClip);
            onCompleted.Invoke();
        }
        else
        {
            Play(hitClip);
            UpdateInstructions();
        }
    }

    private void UpdateTargets()
    {
        if (targets == null) return;
        for (int i = 0; i < targets.Length; i++)
        {
            bool hit = false;
            for (int j = 0; roundOrder != null && j < NextTargetIndex; j++)
                if (roundOrder[j] == i) hit = true;
            if (targets[i] != null) targets[i].SetHit(hit);
        }
    }

    private void RetryRound(string message)
    {
        NextTargetIndex = 0;
        IsRoundRunning = false;
        roundFeedback = message;
        UpdateTargets();
        UpdateInstructions();
        Play(errorClip);
    }

    private void ShuffleOrder()
    {
        int count = targets != null ? targets.Length : 0;
        int[] previous = roundOrder;
        roundOrder = new int[count];
        for (int i = 0; i < count; i++) roundOrder[i] = i;
        for (int i = count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int swap = roundOrder[i]; roundOrder[i] = roundOrder[j]; roundOrder[j] = swap;
        }
        // Deux manches consecutives ne demandent pas exactement le meme ordre.
        if (count > 1 && previous != null && previous.Length == count)
        {
            bool same = true;
            for (int i = 0; i < count; i++) if (previous[i] != roundOrder[i]) same = false;
            if (same)
            {
                int swap = roundOrder[0]; roundOrder[0] = roundOrder[1]; roundOrder[1] = swap;
            }
        }
    }

    private void UpdateInstructions()
    {
        if (instructionText == null) return;
        displayedSeconds = Mathf.CeilToInt(RemainingSeconds);
        if (IsPowered) { instructionText.text = "PC ALLUMÉ"; return; }
        if (!IsUnlocked)
        {
            instructionText.text = "ALIMENTATION VERROUILLÉE\nOuvre le coffre, puis utilise le pistolet.";
            return;
        }
        var orderText = new StringBuilder();
        if (roundOrder != null)
            for (int i = 0; i < roundOrder.Length; i++)
            {
                if (i > 0) orderText.Append(" → ");
                orderText.Append(roundOrder[i] + 1);
            }
        string status = !string.IsNullOrEmpty(roundFeedback) ? roundFeedback : IsRoundRunning ?
            NextTargetIndex + "/" + roundOrder.Length + " cibles • " + displayedSeconds + " s restantes" :
            displayedSeconds + " s dès le premier bon tir";
        instructionText.text = "ALLUME LE PC — MANCHE " + CurrentRound + "/" + RoundsToWin +
            "\nOrdre : " + orderText + "\n" + status + "\nGrip : tenir • Gâchette : tirer";
    }

    private void Play(AudioClip clip)
    {
        if (feedbackSource != null && clip != null) feedbackSource.PlayOneShot(clip);
    }
}
