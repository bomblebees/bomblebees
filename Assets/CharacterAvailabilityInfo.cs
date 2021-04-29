using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class CharacterAvailabilityInfo : NetworkBehaviour
{
    [SyncVar(hook = nameof(CharacterAvailabilityChanged))] 
    public bool character1 = true, character2 = true, character3 = true, character4 = true;
    
    public void CharacterAvailabilityChanged(bool oldCharacterAvailability, bool newCharacterAvailability) { }
}
