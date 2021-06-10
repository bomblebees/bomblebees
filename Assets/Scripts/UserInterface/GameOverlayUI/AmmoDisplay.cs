using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class AmmoDisplay : MonoBehaviour
{
    [SerializeField] private InventoryStackItem[] invStackItems = new InventoryStackItem[4];
	[SerializeField] private Image selectedHighlight;

	public void UpdateInventoryQuantity(GameObject p)
    {
		Player player = p.GetComponent<Player>();

        SyncList<int> list = player.GetComponent<PlayerInventory>().inventoryList;

        for (int i = 0; i < list.Count; i++)
        {
            invStackItems[i].invSlotRadial.fillAmount = (float)list[i] / (float)player.GetComponent<PlayerInventory>().GetMaxInvSizes()[i];

            invStackItems[i].invCounter.text = list[i].ToString();
        }
    }

	public void UpdateInventorySize(GameObject p)
	{
		PlayerInventory inv = p.GetComponent<PlayerInventory>();

		SyncList<int> playerInventorySizes = inv.inventorySize;
		SyncList<int> list = inv.inventoryList;
		int selected = inv.selectedSlot;

		//Debug.Log("updating inventory size UI on " + gameObject.name + ", current inventory size in index 0: " + playerInventorySizes[0]);

		// for each radial frame container, deactivate each frame inside, and reactivate the correct one
		for (int i = 0; i < invStackItems.Length; i++)
		{
			InventoryRadialSlottedFrame frameImage = invStackItems[i].slottedFrame.GetComponent<InventoryRadialSlottedFrame>();

			frameImage.SwapFrame(playerInventorySizes[i]);
		}

		for (int i = 0; i < list.Count; i++)
		{
			invStackItems[i].invSlotRadial.fillAmount = (float)list[i] / (float)p.GetComponent<PlayerInventory>().GetMaxInvSizes()[i];
		}
	}

	public void UpdateInventorySelected(GameObject p)
	{
		PlayerInventory inv = p.GetComponent<PlayerInventory>();

		int selected = inv.selectedSlot;

		selectedHighlight.gameObject.transform.localPosition = invStackItems[selected].invSlotRadial.transform.parent.transform.localPosition;
	}
}
