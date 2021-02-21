using System.Collections;
using System.Collections.Generic;
using UnityEngine;

interface HexArray
{
    ref int getWidth();
    ref int getHeight();
    ref char[,] getArray();

}
