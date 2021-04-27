using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class CharacterAvailabilityInfo : NetworkBehaviour
{
    [SyncVar] public bool character1, character2, character3, character4;
}
