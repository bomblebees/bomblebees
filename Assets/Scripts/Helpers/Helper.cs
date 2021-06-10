public static class Helper
{
    public static int GetIndexInArray(int index, int length)
    {
        var trim = index % length;
        var nonNegative = trim + length;
        return nonNegative % length;
    }
}
