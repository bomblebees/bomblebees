using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class InvisibleBlocker : NetworkBehaviour
{
    // when it spawns
    // check whos inside trigger, add to accessors list
    // it you're not in the accessors list, youll be blocked... how? 
    // then in start, use IgnoreCollision between the invisblocker and everyone in the accessors
    // however, onTriggerExit, that player is ejected from the list, and unset IgnoreCollisionae
    // the invisible blocker blocks anyone 
    
    // keep track of players that start in it
    public override void OnStartServer()
    {
        base.OnStartServer();
        
    }
    
}
