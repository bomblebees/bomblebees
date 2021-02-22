using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1 : HexArray
{
    private int width = 16;
    private int height = 11;

    private char[,] array =
    { 
       //        1    2    3    4    5    6    7    8    9   10
       // /* A */ {'e', 'e', 'e', 'e', 'e', 'b', 'e', 'e', 'e', 'e'},  
       // /* B */ {'e', 'e', 'e', 'b', 'e', 'e', 'b', 'e', 'e', 'e'},  
       // /* C */ {'e', 'b', 'e', 'e', 'b', 'e', 'e', 'e', 'e', 'e'},  
       // /* D */ {'e', 'e', 'e', 'e', 'd', 'e', 'e', 'e', 'e', 'e'},  
       // /* E */ {'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e'},  
       // /* F */ {'e', 'e', 'e', 'e', 'e', 'b', 'e', '', 'e', 'e'},  
       // /* G */ {'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e'},  
       // /* H */ {'e', 'e', 'e', 'b', 'e', 'e', 'e', 'e', 'e', 'e'},  
       // /* I */ {'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e'},  
       // /* J */ {'e', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b'}, 
       
       // 0,0
       {'e', 'e', 'e', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e' },
       {'e', 'e', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e' },
       {'e', 'e', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'y', 'b', 'b', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e' },
       {'e', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e' },
       {'e', 'b', 'b', 'b', 'b', 'b', 'r', 'b', 'r', 'b', 'b', 'b', 'b', 'e', 'e', 'e', 'e', 'e', 'e', 'e' },
       {'e', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'r', 'r', 'r', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e' },
       {'e', 'e', 'b', 'b', 'b', 'b', 'b', 'g', 'b', 'g', 'b', 'g', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e' },
       {'e', 'e', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'r', 'r', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e' },
       {'e', 'e', 'e', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'b', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e', 'e' },
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