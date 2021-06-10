using UnityEngine;

public class RandomLevelGeneration
{
    private int width = 19;
    private int height = 11;

    private char[] tileTypes = { 'r', 'b', 'g', 'y', 'p', 'w' };

    void Awake() {
        generateArray();
    }

    public char[,] generateArray() {
        
        char[,] result = new char[height,width];

        for (int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                result[i,j] = tileTypes[Random.Range(0,tileTypes.Length)];
            }
        }

        return result;
    }

    public ref int getWidth()
    {
        return ref width;
    }
    public ref int getHeight()
    {
        return ref height;
    }
}
