using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class PlayerOnline : Player
{
   protected override void ApplyMovement()
   {
      if (!isLocalPlayer){return;}
      
      base.ApplyMovement();
   }
}
