using UnityEngine;

public class BootTest : MonoBehaviour
{
    private static bool installed;

    private void Awake()
    {
        if (installed)
        {
            enabled = false;
            return;
        }

        installed = true;
        if (GetComponent<ToeProcedureV01>() == null) gameObject.AddComponent<ToeProcedureV01>();
    }
}

