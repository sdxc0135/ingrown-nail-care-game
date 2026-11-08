using System;
using UnityEngine;

/// <summary>
/// A self-contained vertical prototype for the ingrown-nail care game.
/// It uses Unity's immediate-mode UI so the existing sample scene can run it
/// without any prefab or Canvas setup. Replace this UI with authored prefabs
/// once the interaction direction is approved.
/// </summary>
public sealed class FootCareGameController : MonoBehaviour
{
    private readonly string[] toolNames = { "클린 버블", "말랑 핀셋", "진정 크림", "포근 붕대" };
    private readonly string[] toolIcons = { "●", "✂", "●", "▰" };
    private readonly string[] motionText = { "가로로 문질러 소독하기", "끝부분에서 아래로 당기기", "원을 그리며 크림 바르기", "길게 끌어 붕대 감기" };
    private readonly float[] motionThresholds = { 150f, 120f, 230f, 180f };
    private readonly Color coral = new Color(1f, .47f, .43f);
    private readonly Color navy = new Color(.06f, .10f, .20f);
    private readonly Color teal = new Color(.05f, .35f, .39f);
    private readonly Color cream = new Color(1f, .976f, .93f);

    private Texture2D toePhoto;
    private AudioSource audioSource;
    private Rect treatmentRect;
    private bool started;
    private bool ended;
    private bool dragging;
    private bool soundOn = true;
    private int step;
    private int selectedTool = -1;
    private int score;
    private int coins = 248;
    private float gestureProgress;
    private Vector2 lastPointer;
    private float timeRemaining = 35f;
    private float soundCooldown;
    private string feedback = "진료 시작 버튼을 눌러주세요";
    private float feedbackUntil;

    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;
    private GUIStyle smallStyle;
    private GUIStyle cardStyle;
    private GUIStyle buttonStyle;
    private GUIStyle selectedButtonStyle;

    private void Awake()
    {
        toePhoto = Resources.Load<Texture2D>("ingrown-nail-macro");
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = .55f;
    }

    private void Update()
    {
        if (!started || ended) return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            EndGame(false);
            return;
        }

        HandlePointer();
        if (feedbackUntil > 0f && Time.time > feedbackUntil) feedback = string.Empty;
    }

    private void HandlePointer()
    {
        Vector2 pointer = Vector2.zero;
        bool down = false;
        bool held = false;
        bool up = false;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            pointer = new Vector2(touch.position.x, Screen.height - touch.position.y);
            down = touch.phase == TouchPhase.Began;
            held = touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
            up = touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled;
        }
        else
        {
            pointer = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            down = Input.GetMouseButtonDown(0);
            held = Input.GetMouseButton(0);
            up = Input.GetMouseButtonUp(0);
        }

        if (down && treatmentRect.Contains(pointer)) BeginGesture(pointer);
        if (held && dragging) MoveGesture(pointer);
        if (up && dragging) EndGesture();
    }

    private void BeginGesture(Vector2 pointer)
    {
        if (selectedTool < 0)
        {
            ShowFeedback("먼저 도구를 골라 주세요", false);
            PlayTone(150f, .10f, .025f);
            return;
        }

        if (selectedTool != step)
        {
            ShowFeedback("지금 단계에 맞는 도구가 아니에요", false);
            PlayTone(150f, .10f, .025f);
            return;
        }

        dragging = true;
        lastPointer = pointer;
        PlayActionSound(true);
    }

    private void MoveGesture(Vector2 pointer)
    {
        float distance = Vector2.Distance(lastPointer, pointer);
        if (distance < .5f) return;

        gestureProgress += distance;
        lastPointer = pointer;
        PlayActionSound(false);
        if (gestureProgress >= motionThresholds[step]) CompleteStep();
    }

    private void EndGesture()
    {
        dragging = false;
        if (gestureProgress < motionThresholds[Mathf.Clamp(step, 0, 3)]) ShowFeedback("조금 더 움직여 주세요", false);
    }

    private void CompleteStep()
    {
        dragging = false;
        score += 250;
        coins += 12;
        ShowFeedback(step == 1 ? "쏙! 깔끔하게 정리했어요." : "좋아요! 다음 케어로 넘어가요.", true);
        PlaySuccessSound(step);
        step++;
        selectedTool = -1;
        gestureProgress = 0f;

        if (step >= 4) EndGame(true);
    }

    private void EndGame(bool success)
    {
        dragging = false;
        ended = true;
        started = false;
        feedback = success ? "케어 완료! 발끝이 한결 편안해졌어요." : "시간이 다 됐어요. 다시 천천히 해볼까요?";
        feedbackUntil = Time.time + 999f;
        if (success) PlaySuccessSound(4);
    }

    private void StartGame()
    {
        started = true;
        ended = false;
        step = 0;
        selectedTool = -1;
        score = 0;
        timeRemaining = 35f;
        gestureProgress = 0f;
        feedback = "도구를 고른 뒤 화면을 움직여 주세요";
        feedbackUntil = Time.time + 3f;
        EnsureAudio();
        PlayTone(520f, .12f, .04f);
    }

    private void SelectTool(int tool)
    {
        if (!started || ended) return;
        selectedTool = tool;
        gestureProgress = 0f;
        bool correct = tool == step;
        feedback = correct ? motionText[tool] : "이 순서의 도구는 아니에요";
        feedbackUntil = Time.time + 2f;
        PlayTone(correct ? 460f : 150f, .08f, correct ? .035f : .025f);
    }

    private void ResetGame()
    {
        started = false;
        ended = false;
        dragging = false;
        step = 0;
        selectedTool = -1;
        score = 0;
        timeRemaining = 35f;
        gestureProgress = 0f;
        feedback = "진료 시작 버튼을 눌러주세요";
        feedbackUntil = 0f;
    }

    private void OnGUI()
    {
        BuildStyles();
        DrawBackground();

        float width = Mathf.Min(Screen.width - 40f, 760f);
        float left = (Screen.width - width) * .5f;
        float top = 20f;
        float panelHeight = Mathf.Min(Screen.height - 40f, 980f);
        Rect panel = new Rect(left, top, width, panelHeight);
        DrawPanel(panel);

        Rect header = new Rect(panel.x + 24f, panel.y + 20f, panel.width - 48f, 40f);
        GUI.Label(new Rect(header.x, header.y, 300f, 34f), "🩹  발톱 구조대", titleStyle);
        GUI.Label(new Rect(header.x, header.y + 28f, 260f, 18f), "TINY CARE · REAL TOUCH", smallStyle);
        GUI.Label(new Rect(header.xMax - 110f, header.y + 3f, 110f, 28f), $"🪙 {coins}", smallStyle);

        float y = header.yMax + 32f;
        GUI.Label(new Rect(panel.x + 24f, y, panel.width - 48f, 30f), "케어 룸 01  ·  말랑 발끝 클리닉", bodyStyle);
        GUI.Label(new Rect(panel.x + 24f, y + 32f, panel.width - 48f, 22f), $"남은 시간  {Mathf.CeilToInt(timeRemaining)}초", smallStyle);

        treatmentRect = new Rect(panel.x + 24f, y + 64f, panel.width - 48f, panel.width - 48f);
        DrawTreatment(treatmentRect);

        float toolY = treatmentRect.yMax + 18f;
        DrawStepLine(new Rect(panel.x + 24f, toolY, panel.width - 48f, 26f));
        DrawTools(new Rect(panel.x + 24f, toolY + 42f, panel.width - 48f, 120f));

        if (!started && !ended)
        {
            if (GUI.Button(new Rect(panel.x + 24f, panel.yMax - 66f, panel.width - 48f, 46f), "진료 시작하기", buttonStyle)) StartGame();
        }
        else if (ended)
        {
            GUI.Label(new Rect(panel.x + 24f, panel.yMax - 102f, panel.width - 48f, 30f), feedback, bodyStyle);
            if (GUI.Button(new Rect(panel.x + 24f, panel.yMax - 66f, panel.width - 48f, 46f), "다시 케어하기", buttonStyle)) ResetGame();
        }
        else
        {
            GUI.Label(new Rect(panel.x + 24f, panel.yMax - 66f, panel.width - 48f, 44f), feedback, smallStyle);
        }
    }

    private void DrawBackground()
    {
        Color old = GUI.color;
        GUI.color = navy;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = old;
    }

    private void DrawPanel(Rect panel)
    {
        Color old = GUI.color;
        GUI.color = cream;
        GUI.Box(panel, GUIContent.none, cardStyle);
        GUI.color = old;
    }

    private void DrawTreatment(Rect rect)
    {
        Color old = GUI.color;
        GUI.color = teal;
        GUI.Box(rect, GUIContent.none, cardStyle);
        GUI.color = Color.white;
        if (toePhoto != null) GUI.DrawTexture(rect, toePhoto, ScaleMode.ScaleAndCrop, true);
        else GUI.Box(rect, "실사 매크로 이미지가 아직 임포트되지 않았어요", cardStyle);

        Color ring = selectedTool == step ? coral : new Color(1f, 1f, 1f, .55f);
        GUI.color = ring;
        float ringSize = Mathf.Clamp(rect.width * .19f, 72f, 120f);
        Rect target = new Rect(rect.x + rect.width * .51f, rect.y + rect.height * .36f, ringSize, ringSize);
        GUI.Box(target, GUIContent.none, cardStyle);
        GUI.color = Color.white;

        GUI.Label(new Rect(rect.x + 16f, rect.y + 16f, rect.width * .56f, 28f), started ? (selectedTool >= 0 ? motionText[selectedTool] : "먼저 도구를 선택해 주세요") : "진료 시작 버튼을 눌러주세요", smallStyle);
        GUI.Label(new Rect(rect.xMax - 120f, rect.y + 16f, 104f, 28f), step >= 4 ? "케어 완료" : (started ? $"{step + 1}/4 단계" : "준비 중"), smallStyle);

        float hudY = rect.yMax - 50f;
        GUI.color = new Color(0f, 0f, 0f, .66f);
        GUI.Box(new Rect(rect.x + 14f, hudY, rect.width - 28f, 36f), GUIContent.none, cardStyle);
        GUI.color = Color.white;
        float percent = selectedTool == step ? Mathf.Clamp01(gestureProgress / motionThresholds[Mathf.Clamp(step, 0, 3)]) : 0f;
        GUI.Label(new Rect(rect.x + 26f, hudY + 8f, rect.width - 100f, 20f), started ? "손가락으로 움직여 실제 동작을 해보세요" : "게임 속 케어 연출입니다", smallStyle);
        GUI.Label(new Rect(rect.xMax - 62f, hudY + 8f, 44f, 20f), $"{Mathf.RoundToInt(percent * 100f)}%", smallStyle);
        GUI.color = Color.Lerp(new Color(.25f, .55f, .42f), coral, percent);
        GUI.Box(new Rect(rect.x + 26f, hudY + 27f, (rect.width - 88f) * percent, 4f), GUIContent.none, cardStyle);
        GUI.color = old;
    }

    private void DrawStepLine(Rect rect)
    {
        string[] labels = { "● 소독", "✂ 정리", "● 보호", "▰ 마무리" };
        float itemWidth = rect.width / 4f;
        for (int i = 0; i < labels.Length; i++)
        {
            GUI.color = i < step ? new Color(.10f, .52f, .40f) : (i == step ? coral : new Color(.45f, .48f, .56f));
            GUI.Label(new Rect(rect.x + itemWidth * i, rect.y, itemWidth, rect.height), labels[i], smallStyle);
        }
        GUI.color = Color.white;
    }

    private void DrawTools(Rect rect)
    {
        GUI.Label(new Rect(rect.x, rect.y, rect.width, 24f), "CARE TOOLS · 도구를 골라요", bodyStyle);
        float gap = 8f;
        float buttonWidth = (rect.width - gap * 3f) / 4f;
        for (int i = 0; i < toolNames.Length; i++)
        {
            Rect buttonRect = new Rect(rect.x + (buttonWidth + gap) * i, rect.y + 34f, buttonWidth, 62f);
            GUIStyle style = selectedTool == i ? selectedButtonStyle : buttonStyle;
            if (GUI.Button(buttonRect, $"{toolIcons[i]}\n{toolNames[i]}", style)) SelectTool(i);
        }
    }

    private void BuildStyles()
    {
        if (titleStyle != null) return;
        titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, normal = { textColor = navy } };
        bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold, normal = { textColor = navy } };
        smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, wordWrap = true, normal = { textColor = navy } };
        cardStyle = new GUIStyle(GUI.skin.box) { normal = { background = MakeTexture(2, 2, Color.white) } };
        buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 13, fontStyle = FontStyle.Bold, wordWrap = true, normal = { textColor = navy, background = MakeTexture(2, 2, new Color(.96f, .95f, .92f)) } };
        selectedButtonStyle = new GUIStyle(buttonStyle) { normal = { textColor = coral, background = MakeTexture(2, 2, new Color(1f, .88f, .84f)) } };
    }

    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Texture2D texture = new Texture2D(width, height);
        for (int x = 0; x < width; x++) for (int y = 0; y < height; y++) texture.SetPixel(x, y, color);
        texture.Apply();
        return texture;
    }

    private void EnsureAudio()
    {
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.enabled = soundOn;
    }

    private void PlayActionSound(bool start)
    {
        if (!soundOn || Time.time < soundCooldown) return;
        soundCooldown = Time.time + .09f;
        if (step == 0) PlayNoise(.055f, 1800f, .018f);
        if (step == 1) PlayTone(start ? 900f : 1450f, .05f, .035f);
        if (step == 2) PlayTone(180f, .06f, .025f);
        if (step == 3) PlayNoise(.07f, 620f, .02f);
    }

    private void PlaySuccessSound(int completedStep)
    {
        if (!soundOn) return;
        if (completedStep == 1) PlayNoise(.18f, 2200f, .035f);
        if (completedStep == 2) { PlayTone(980f, .08f, .055f); PlayTone(1480f, .10f, .04f); }
        if (completedStep == 3) { PlayTone(240f, .14f, .035f); PlayTone(320f, .16f, .03f); }
        if (completedStep == 4) { PlayNoise(.20f, 720f, .035f); PlayTone(520f, .12f, .035f); }
    }

    private void PlayTone(float frequency, float duration, float volume)
    {
        PlayTone(frequency, duration, volume, "sine");
    }

    private void PlayTone(float frequency, float duration, float volume, string waveform)
    {
        if (!soundOn) return;
        EnsureAudio();
        const int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        AudioClip clip = AudioClip.Create("footcare-tone", sampleCount, 1, sampleRate, false);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleRate;
            float envelope = Mathf.Sin(Mathf.PI * i / sampleCount);
            float wave = waveform == "triangle" ? Mathf.PingPong(t * frequency, 1f) * 2f - 1f : Mathf.Sin(t * frequency * Mathf.PI * 2f);
            samples[i] = wave * envelope * volume;
        }
        clip.SetData(samples, 0);
        audioSource.PlayOneShot(clip);
        Destroy(clip, duration + .2f);
    }

    private void PlayNoise(float duration, float frequency, float volume)
    {
        if (!soundOn) return;
        EnsureAudio();
        const int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duration);
        AudioClip clip = AudioClip.Create("footcare-texture", sampleCount, 1, sampleRate, false);
        float[] samples = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            float envelope = 1f - i / (float)sampleCount;
            samples[i] = UnityEngine.Random.Range(-1f, 1f) * envelope * volume;
        }
        clip.SetData(samples, 0);
        audioSource.PlayOneShot(clip);
        Destroy(clip, duration + .2f);
    }
}
