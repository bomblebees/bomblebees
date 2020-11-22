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
      if (!isLocalPlayer)
      {
         return;
      }
      base.ApplyMovement();
   }

   protected override void ListenForSwapping()
   {
      if (!isLocalPlayer)
      {
         return;
      }
      base.ListenForSwapping();
   }
   
   protected override void Update()
   {
      base.Update();

      if (Input.GetKeyDown("2"))
      { 
         RpcLinkAssets();
      }
   }

   [ClientRpc]
   private void RpcListenForSwapping()
   {
      base.ListenForSwapping();
   }

   [ClientRpc]
   private void RpcLinkAssets()
   {
      LinkAssets();
      UpdateHeldHex();
   }
}
