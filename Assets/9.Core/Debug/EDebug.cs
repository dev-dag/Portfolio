using System.Diagnostics;
using Debug = UnityEngine.Debug;

/// <summary>
/// ����� ���忡���� ���ԵǴ� �α� ����� �����ϴ� Ŭ����
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
