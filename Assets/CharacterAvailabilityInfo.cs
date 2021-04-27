using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class CharacterAvailabilityInfo : NetworkBehaviour
{
    [SyncVar] public bool character1 = true, 
        character2 = true, 
        character3 = true, 
        character4 = true;
}
