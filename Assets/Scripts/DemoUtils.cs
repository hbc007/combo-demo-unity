using UnityEngine.Diagnostics;

public class DemoUtils
{
    // Start is called before the first frame update
    public static void ForceCrash()
    {
        // Forcing a crash
        Utils.ForceCrash(ForcedCrashCategory.AccessViolation);
    }
}
