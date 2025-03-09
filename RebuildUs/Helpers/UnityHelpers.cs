namespace RebuildUs.Helpers;

internal static class UnityHelpers
{
    internal static void Destroy(this UnityEngine.Object obj)
    {
        UnityEngine.Object.Destroy(obj);
    }
}