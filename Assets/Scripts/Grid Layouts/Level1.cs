using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1 : HexArray
{
    private int width = 10;
    private int height = 10;

    private char[,] array =
    {
        // 1   2    3    4    5    6    7    8    9   10
        {'e', 'e', 'e', 'e', 'e', 'b', 'e', 'e', 'e', 'e'}, // A 
        {'e', 'e', 'e', 'b', 'e', 'e', 'b', 'e', 'e', 'e'}, // B 
        {'e', 'b', 'e', 'e', 'b', 'e', 'e', 'e', 'e', 'e'}, // C 
        {'e', 'e', 'e', 'e', 'd', 'e', 'e', 'e', 'e', 'e'}, // D 
        {'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e'}, // E 
        {'e', 'e', 'e', 'e', 'e', 'b', 'e', 'e', 'e', 'e'}, // F 
        {'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e'}, // G 
        {'e', 'e', 'e', 'b', 'e', 'e', 'e', 'e', 'e', 'e'}, // H 
        {'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e'}, // I 
        {'e', 'b', 'b', 'e', 'e', 'e', 'e', 'e', 'e', 'e'}, // J
    };

    public ref int getWidth()
    {
        return ref width;
    }
    public ref int getHeight()
    {
        return ref height;
    }

    //
    // getArray(): Returns a reference to the array because copying it would be expensive.
    //
    public ref char[,] getArray()
    {
        return ref array;
    }

}