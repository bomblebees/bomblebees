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
      if (!hasAuthority)
      {
         return;
      }
      CmdApplyMovement();
   }
   
   protected override void ListenForSwapping()
   {
      if (!hasAuthority)
      {
         return;
      }
      CmdListenForSwapping();
      //base.ListenForSwapping();
   }

   protected override void Update()
   {
      base.Update();
      
      if (isServer && Input.GetKeyDown("2"))
      {
         RpcLinkAssets();
      }
   }

   [Command]
   private void CmdListenForSwapping()
   {
      RpcListenForSwapping(connectionToClient);
   }

   [TargetRpc]
   private void RpcListenForSwapping(NetworkConnection target)
   {
      base.ListenForSwapping();
   }

   [Command]
   private void CmdApplyMovement()
   {
      RpcApplyMovement(connectionToClient);
   }

   [TargetRpc]
   private void RpcApplyMovement(NetworkConnection target)
   {
      base.ApplyMovement();
   }

   [ClientRpc]
   private void RpcLinkAssets()
   {
      LinkAssets();
      UpdateHeldHex();
   }
}
