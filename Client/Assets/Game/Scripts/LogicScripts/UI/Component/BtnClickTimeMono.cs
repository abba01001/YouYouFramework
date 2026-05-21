using UnityEngine;

public class BtnClickTimeMono : MonoBehaviour
{
    private long clickTime = 0;

    public bool CanClick(long time)
    {
        var able = time > clickTime + 200;
        clickTime = time;
        return able;
    }
}