using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using Mirror;
using Mirror.RemoteCalls;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerOnline : Player
{
   protected override void ApplyMovement()
   {
      if (!isLocalPlayer){return;}

      base.ApplyMovement();
   }

   protected override void ListenForSwapping()
   {
   }
   
   protected override void Update()
   {
      if (isLocalPlayer && Input.GetKeyDown("y"))
      {
         CmdYahallo();
      }
      base.Update();
   }

   //test
   [Command]
   void CmdYahallo()
   {
      Debug.Log("Cmd: Yahallo");
      RpcYahallo();
   }

   [ClientRpc]
   void RpcYahallo()
   {
      Debug.Log("C Rpc: Yahallo");
   }
}
