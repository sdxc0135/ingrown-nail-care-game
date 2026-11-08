using UnityEngine;

/// <summary>
/// V0.1 core interaction prototype.
/// The scene uses runtime primitives until production 3D assets are approved.
/// A player must physically drag the tweezer, rotate it, close it near the
/// nail fragment, carry the fragment, and release it over the tray.
/// </summary>
public sealed class ToeProcedureV01 : MonoBehaviour
{
    private Camera procedureCamera;
    private GameObject toe;
    private GameObject nail;
    private GameObject nailFragment;
    private GameObject tray;
    private GameObject tweezer;
    private GameObject leftJaw;
    private GameObject rightJaw;
    private Transform toolTip;
    private AudioSource audioSource;

    private Material skinMaterial;
    private Material nailMaterial;
    private Material fragmentMaterial;
    private Material metalMaterial;
    private Material trayMaterial;

    private bool dragging;
    private bool jawsClosed;
    private bool fragmentHeld;
    private bool completed;
    private bool twoFingerGesture;
    private Vector3 dragOffset;
    private Vector2 previousFingerMidpoint;
    private float previousFingerDistance;
    private float previousFingerAngle;
    private float audioCooldown;
    private int score;
    private string status = "핀셋 손잡이를 손가락으로 직접 잡아보세요.";
    private string lastInput = "대기 중";

    private readonly Color clinicalBlue = new Color(.055f, .11f, .18f);
    private readonly Color clinicalTeal = new Color(.07f, .30f, .32f);
    private readonly Color coral = new Color(1f, .42f, .36f);

    private void Awake()
    {
        BuildPrototypeScene();
    }

    private void Update()
    {
        if (completed) return;

        if (Input.touchCount >= 2)
        {
            HandleTwoFingerGesture();
            return;
        }

        twoFingerGesture = false;
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 screen = touch.position;
            if (touch.phase == TouchPhase.Began) BeginToolDrag(screen);
            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) MoveTool(screen);
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) EndToolDrag();
        }
        else
        {
            if (Input.GetMouseButtonDown(0)) BeginToolDrag(Input.mousePosition);
            if (Input.GetMouseButton(0)) MoveTool(Input.mousePosition);
            if (Input.GetMouseButtonUp(0)) EndToolDrag();
            if (Input.GetKeyDown(KeyCode.Space)) ToggleJaws();
            if (Input.GetKeyDown(KeyCode.R)) ResetProcedure();
        }

        if (fragmentHeld && toolTip != null && nailFragment != null) nailFragment.transform.position = toolTip.position;
    }

    private void BuildPrototypeScene()
    {
        procedureCamera = Camera.main;
        if (procedureCamera == null) return;

        procedureCamera.transform.position = new Vector3(0f, .05f, -7.5f);
        procedureCamera.transform.LookAt(new Vector3(0f, .15f, 0f));
        procedureCamera.fieldOfView = 33f;
        procedureCamera.backgroundColor = clinicalBlue;

        HideSampleSceneGeometry("Ground");
        HideSampleSceneGeometry("Player");

        skinMaterial = MakeMaterial(new Color(.78f, .39f, .26f), 0f, .38f);
        nailMaterial = MakeMaterial(new Color(.88f, .72f, .55f), 0f, .58f);
        fragmentMaterial = MakeMaterial(new Color(.98f, .78f, .54f), 0f, .52f);
        metalMaterial = MakeMaterial(new Color(.35f, .43f, .47f), .82f, .72f);
        trayMaterial = MakeMaterial(new Color(.04f, .35f, .36f), .18f, .48f);

        toe = CreatePrimitive("Toe_Prototype", PrimitiveType.Sphere, new Vector3(0f, .05f, 0f), new Vector3(2.9f, 1.85f, 1.7f), skinMaterial);
        nail = CreatePrimitive("NailPlate_Prototype", PrimitiveType.Cube, new Vector3(0f, .62f, -.88f), new Vector3(1.55f, .12f, .92f), nailMaterial);
        nail.transform.rotation = Quaternion.Euler(0f, 0f, -3f);
        nailFragment = CreatePrimitive("NailFragment_Target", PrimitiveType.Cube, new Vector3(.63f, .64f, -1.02f), new Vector3(.34f, .12f, .37f), fragmentMaterial);
        nailFragment.transform.rotation = Quaternion.Euler(0f, 8f, -18f);

        tray = CreatePrimitive("FragmentTray", PrimitiveType.Cube, new Vector3(2.2f, -1.85f, -.25f), new Vector3(1.7f, .08f, 1.05f), trayMaterial);
        CreatePrimitive("TrayLip", PrimitiveType.Cube, new Vector3(2.2f, -1.63f, -.48f), new Vector3(1.7f, .38f, .08f), trayMaterial);

        BuildTweezers();
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        status = "V0.1 · 핀셋을 발톱 조각으로 가져가세요.";
    }

    private void BuildTweezers()
    {
        tweezer = new GameObject("Tweezers_DirectManipulation");
        tweezer.transform.position = new Vector3(-2.2f, -1.35f, -1.35f);
        tweezer.transform.rotation = Quaternion.Euler(0f, 0f, -7f);

        leftJaw = CreatePrimitive("Tweezer_LeftJaw", PrimitiveType.Cube, Vector3.zero, new Vector3(.12f, 1.55f, .10f), metalMaterial, tweezer.transform);
        rightJaw = CreatePrimitive("Tweezer_RightJaw", PrimitiveType.Cube, Vector3.zero, new Vector3(.12f, 1.55f, .10f), metalMaterial, tweezer.transform);
        leftJaw.transform.localPosition = new Vector3(-.12f, .15f, 0f);
        rightJaw.transform.localPosition = new Vector3(.12f, .15f, 0f);
        toolTip = new GameObject("Tweezer_Tip").transform;
        toolTip.SetParent(tweezer.transform);
        toolTip.localPosition = new Vector3(0f, .92f, -.04f);
        toolTip.localScale = Vector3.one * .12f;
        UpdateJawVisuals();
    }

    private void HideSampleSceneGeometry(string objectName)
    {
        GameObject sample = GameObject.Find(objectName);
        if (sample == null) return;
        foreach (Renderer renderer in sample.GetComponentsInChildren<Renderer>()) renderer.enabled = false;
        foreach (Collider collider in sample.GetComponentsInChildren<Collider>()) collider.enabled = false;
    }

    private void BeginToolDrag(Vector2 screen)
    {
        if (tweezer == null || procedureCamera == null) return;
        Vector2 toolScreen = procedureCamera.WorldToScreenPoint(tweezer.transform.position);
        if (Vector2.Distance(screen, toolScreen) > 170f)
        {
            status = "핀셋 손잡이 가까이를 직접 터치하세요.";
            lastInput = "잡기 실패";
            PlayTone(120f, .08f, .025f);
            return;
        }

        Vector3 world = ScreenToToolPlane(screen);
        dragOffset = tweezer.transform.position - world;
        dragging = true;
        lastInput = "핀셋 이동 중";
        PlayTexture(.05f, .018f);
    }

    private void MoveTool(Vector2 screen)
    {
        if (!dragging || tweezer == null) return;
        Vector3 world = ScreenToToolPlane(screen);
        Vector3 next = world + dragOffset;
        next.x = Mathf.Clamp(next.x, -3.2f, 3.2f);
        next.y = Mathf.Clamp(next.y, -2.0f, 2.1f);
        tweezer.transform.position = next;
        lastInput = "직접 이동";
        if (Vector3.Distance(toolTip.position, nailFragment.transform.position) < .5f) status = "접촉 영역에 들어왔어요. 핀셋을 닫으세요.";
        if (fragmentHeld) nailFragment.transform.position = toolTip.position;
    }

    private void EndToolDrag()
    {
        dragging = false;
        if (fragmentHeld) nailFragment.transform.position = toolTip.position;
    }

    private void HandleTwoFingerGesture()
    {
        Touch a = Input.GetTouch(0);
        Touch b = Input.GetTouch(1);
        Vector2 midpoint = (a.position + b.position) * .5f;
        float distance = Vector2.Distance(a.position, b.position);
        float angle = Mathf.Atan2(b.position.y - a.position.y, b.position.x - a.position.x) * Mathf.Rad2Deg;

        if (!twoFingerGesture || a.phase == TouchPhase.Began || b.phase == TouchPhase.Began)
        {
            twoFingerGesture = true;
            previousFingerMidpoint = midpoint;
            previousFingerDistance = distance;
            previousFingerAngle = angle;
            ToggleJaws();
            return;
        }

        if (tweezer != null)
        {
            Vector3 move = ScreenToToolPlane(midpoint) - ScreenToToolPlane(previousFingerMidpoint);
            tweezer.transform.position += move;
            float zoomDelta = distance - previousFingerDistance;
            procedureCamera.fieldOfView = Mathf.Clamp(procedureCamera.fieldOfView - zoomDelta * .035f, 22f, 50f);
            float rotateDelta = Mathf.DeltaAngle(previousFingerAngle, angle);
            tweezer.transform.Rotate(Vector3.forward, rotateDelta);
            lastInput = "두 손가락 · 이동 / 회전 / 확대";
        }

        previousFingerMidpoint = midpoint;
        previousFingerDistance = distance;
        previousFingerAngle = angle;
    }

    private Vector3 ScreenToToolPlane(Vector2 screen)
    {
        Ray ray = procedureCamera.ScreenPointToRay(screen);
        Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, -1.35f));
        return plane.Raycast(ray, out float enter) ? ray.GetPoint(enter) : tweezer.transform.position;
    }

    private void ToggleJaws()
    {
        jawsClosed = !jawsClosed;
        UpdateJawVisuals();
        lastInput = jawsClosed ? "핀셋 닫힘" : "핀셋 열림";
        PlayTone(jawsClosed ? 1180f : 820f, .08f, .04f);

        if (jawsClosed && !fragmentHeld && Vector3.Distance(toolTip.position, nailFragment.transform.position) < .48f)
        {
            fragmentHeld = true;
            status = "파지 성공. 분리된 조각을 트레이로 옮기세요.";
            PlayTone(1540f, .10f, .05f);
        }
        else if (!jawsClosed && fragmentHeld)
        {
            fragmentHeld = false;
            if (IsOverTray(nailFragment.transform.position))
            {
                completed = true;
                score = 1000;
                status = "V0.1 완료 · 조각을 직접 파지해 트레이에 놓았어요.";
                PlayTone(520f, .12f, .05f);
                PlayTone(760f, .14f, .04f);
            }
            else
            {
                status = "트레이 위에서 핀셋을 열어 조각을 놓으세요.";
                PlayTone(190f, .10f, .025f);
            }
        }
    }

    private bool IsOverTray(Vector3 position)
    {
        return tray != null && Vector2.Distance(new Vector2(position.x, position.y), new Vector2(tray.transform.position.x, tray.transform.position.y)) < 1.0f;
    }

    private void UpdateJawVisuals()
    {
        if (leftJaw == null || rightJaw == null) return;
        float angle = jawsClosed ? 7f : 20f;
        leftJaw.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
        rightJaw.transform.localRotation = Quaternion.Euler(0f, 0f, -angle);
    }

    private GameObject CreatePrimitive(string objectName, PrimitiveType type, Vector3 position, Vector3 scale, Material material, Transform parent = null)
    {
        GameObject created = GameObject.CreatePrimitive(type);
        created.name = objectName;
        created.transform.position = position;
        created.transform.localScale = scale;
        if (parent != null) created.transform.SetParent(parent, true);
        Renderer renderer = created.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        return created;
    }

    private Material MakeMaterial(Color color, float metallic, float smoothness)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        return material;
    }

    private void OnGUI()
    {
        GUI.color = Color.white;
        GUI.Label(new Rect(22f, 18f, 340f, 28f), "V0.1  ·  REAL TOUCH PROTOTYPE", new GUIStyle(GUI.skin.label) { fontSize = 14, normal = { textColor = Color.white } });
        GUI.Label(new Rect(22f, 46f, Screen.width - 44f, 50f), status, new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true, normal = { textColor = Color.white } });
        GUI.Label(new Rect(Screen.width - 170f, 18f, 145f, 28f), $"SCORE  {score}", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight, fontSize = 14, normal = { textColor = Color.white } });

        GUI.Box(new Rect(18f, Screen.height - 94f, Screen.width - 36f, 72f), GUIContent.none);
        GUI.Label(new Rect(32f, Screen.height - 84f, Screen.width - 64f, 24f), "한 손: 핀셋 이동   ·   두 손가락: 회전/확대   ·   스페이스 또는 두 손가락 탭: 개폐", new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = true, normal = { textColor = Color.white } });
        if (GUI.Button(new Rect(Screen.width - 156f, Screen.height - 55f, 124f, 28f), jawsClosed ? "핀셋 열기" : "핀셋 닫기")) ToggleJaws();
        if (GUI.Button(new Rect(32f, Screen.height - 55f, 80f, 28f), "리셋")) ResetProcedure();
        GUI.Label(new Rect(122f, Screen.height - 55f, 220f, 28f), lastInput, new GUIStyle(GUI.skin.label) { fontSize = 11, normal = { textColor = new Color(.78f, .85f, .86f) } });
    }

    private void ResetProcedure()
    {
        if (nailFragment != null) nailFragment.transform.position = new Vector3(.63f, .64f, -1.02f);
        if (tweezer != null) { tweezer.transform.position = new Vector3(-2.2f, -1.35f, -1.35f); tweezer.transform.rotation = Quaternion.identity; }
        procedureCamera.fieldOfView = 33f;
        jawsClosed = false;
        fragmentHeld = false;
        completed = false;
        score = 0;
        status = "핀셋 손잡이를 손가락으로 직접 잡아보세요.";
        UpdateJawVisuals();
    }

    private void PlayTone(float frequency, float duration, float volume)
    {
        if (audioSource == null) return;
        const int sampleRate = 44100;
        int count = Mathf.CeilToInt(sampleRate * duration);
        AudioClip clip = AudioClip.Create("footcare-tone", count, 1, sampleRate, false);
        float[] data = new float[count];
        for (int i = 0; i < count; i++) data[i] = Mathf.Sin(i / (float)sampleRate * frequency * Mathf.PI * 2f) * Mathf.Sin(Mathf.PI * i / count) * volume;
        clip.SetData(data, 0);
        audioSource.PlayOneShot(clip);
        Destroy(clip, duration + .2f);
    }

    private void PlayTexture(float duration, float volume)
    {
        if (audioSource == null || Time.time < audioCooldown) return;
        audioCooldown = Time.time + .08f;
        const int sampleRate = 44100;
        int count = Mathf.CeilToInt(sampleRate * duration);
        AudioClip clip = AudioClip.Create("footcare-texture", count, 1, sampleRate, false);
        float[] data = new float[count];
        for (int i = 0; i < count; i++) data[i] = Random.Range(-1f, 1f) * (1f - i / (float)count) * volume;
        clip.SetData(data, 0);
        audioSource.PlayOneShot(clip);
        Destroy(clip, duration + .2f);
    }
}

