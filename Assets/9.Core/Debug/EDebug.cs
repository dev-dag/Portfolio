using System.Diagnostics;
using Debug = UnityEngine.Debug;

/// <summary>
/// 디버그 빌드에서만 포함되는 로그 기능을 포함하는 클래스
/// </summary>
public static class EDebug
{
    [Conditional("DEBUG")]
    public static void Log(string str)
    {
        Debug.Log(str);
    }

    [Conditional("DEBUG")]
    public static void LogError(string str)
    {
        Debug.LogError(str);
    }

    [Conditional("DEBUG")]
    public static void LogWarning(string str)
    {
        Debug.LogWarning(str);
    }
}
