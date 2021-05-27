using UnityEngine;

public class DebugCheats : MonoBehaviour
{
    private void Update()
    {
        if (!Input.GetKey(KeyCode.C)) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Log");
            Debug.LogWarning("LogWarning");
            Debug.LogAssertion("LogAssertion");
            Debug.LogError("LogError");
            Debug.LogFormat("LogFormat");
            Debug.LogAssertionFormat("LogAssertionFormat");
            Debug.LogErrorFormat("LogErrorFormat");
            Debug.LogWarningFormat("LogWarningFormat");
        }
    }
}
