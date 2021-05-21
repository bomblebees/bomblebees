using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helper
{
    public static int GetIndexInArray(int index, int length)
    {
        var trim = index % length;
        var nonNegative = trim + length;
        return nonNegative % length;
    }
}
