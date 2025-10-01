using UnityEngine;

public class CursorLocker : MonoBehaviour
{
    [SerializeField] bool lockOnStart = true;
    bool wantLock;

    void Start() => SetLock(lockOnStart);

    void Update()
    {
        // ESC to unlock
        if (Input.GetKeyDown(KeyCode.Escape)) SetLock(false);

        // Click to re-lock (only if focused)
        if (!wantLock && Input.GetMouseButtonDown(0) && Application.isFocused)
            SetLock(true);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) SetLock(false);           // auto-unlock when unfocused
        // don't auto-lock on regain; wait for click so you don't trap the cursor
    }

    void SetLock(bool locked)
    {
        wantLock = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible   = !locked;
    }
}
