using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MessageFeed : MonoBehaviour
{
    [SerializeField] private GameObject messageFeedPrefab;
    [SerializeField] private GameObject messageFeedCanvas;
    [SerializeField] private GameObject messsageFeedAnchor;
    [SerializeField] private int maxMessages = 10;
    [SerializeField] private float fadeDelay = 4f;

    [SerializeField] GameUIManager gameUIManager = null;

    private List<GameObject> feedUIs = new List<GameObject>();

    public void CreateMessage(string messageText, int charCode = -1)
    {
        // Do not create more messages than max messages
        if (feedUIs.Count >= maxMessages) return;

        // Create the killfeed object
        GameObject message = Instantiate(
            messageFeedPrefab,
            new Vector3(0, 0, 0),
            Quaternion.identity,
            messageFeedCanvas.transform);

        // Set the text
        message.GetComponent<TMP_Text>().text = messageText;

        // Set the character image, if applicable
        if (charCode != -1)
        {
            message.GetComponentInChildren<Image>().sprite = gameUIManager.GetComponent<CharacterHelper>().GetCharImage(charCode);
        }

        // Get initial anchor position
        Vector2 pos = messsageFeedAnchor.GetComponent<RectTransform>().anchoredPosition;

        // Start the height above where it will endup for a nice drop in transition
        pos.y += 50;

        //killfeed.GetComponent<RectTransform>().anchoredPosition = new Vector3(-180, 50 + (feedUIs.Count * 50), 0);

        // Set the position of the message
        message.GetComponent<RectTransform>().anchoredPosition = pos;

        // Add killfeed to list
        feedUIs.Insert(0, message);

        UpdateMessagefeed();

        // Start the fade tween animation
        message.GetComponent<TMP_Text>().LeanAlphaText(0, 1).setDelay(fadeDelay).setOnComplete(() => OnMessageFadeComplete(message));
    }

    public void OnMessageFadeComplete(GameObject killfeed)
    {
        // Remove from the list
        feedUIs.Remove(killfeed);

        // Destroy the object
        Destroy(killfeed);

        // Update the new killfeed
        UpdateMessagefeed();
    }

    public void UpdateMessagefeed()
    {
        Vector2 ancPos = messsageFeedAnchor.GetComponent<RectTransform>().anchoredPosition;

        for (int i = 0; i < feedUIs.Count; i++)
        {
            LeanTween.moveLocalY(feedUIs[i], ancPos.y - (i * 50), 0.5f).setEase(LeanTweenType.easeOutExpo);
        }
    }

    #region Message Types

    public void OnKillEvent(GameObject bomb, GameObject player)
    {
        string killtext = " died to " + GetBombText(bomb);

        CreateMessage(killtext, player.GetComponent<Player>().characterCode);
    }

    public void OnSwapEvent(char comboKey, GameObject player, int numBombsAwarded)
    {
        string killtext = GetComboText(comboKey) + GetComboMultiplier(numBombsAwarded);

        CreateMessage(killtext, player.GetComponent<Player>().characterCode);
    }

    #endregion


    #region Helpers

    private string GetComboMultiplier(int combos)
    {
        switch (combos)
        {
            case 1: return "<size=100%> x1";
            case 2: return "<color=#ADFF00><size=125%> x2</color>";
            case 3: return "<color=#E9FF00><size=150%> x3</color>";
            case 4: return "<color=#FFB700><size=175%> x4</color>";
            default: return "<color=#FF3100><size=200%> x" + combos.ToString() + "</color>";
        }
    }

    //private IEnumerator StartFade(int idx)
    //{
    //    yield return new WaitForSeconds(fadeDelay);

    //    Debug.Log("start tween");
    //    //feedUIs[0].GetComponent<TMP_Text>().color = Color.clear;

    //    //feedUIs[idx].GetComponent<TMP_Text>().LeanAlphaText(0, 1).setOnComplete(OnFeedFaded);

    //    //LeanTween.alphaText(feedUIs[0].GetComponent<TMP_Text>(), )
    //    //LeanTween.alphaText(feedUIs[0].GetComponent<TMP_Text>(), 0f, 3f);
    //}

    //private void OnFeedFaded()
    //{

    //}


    private string GetPlayerText(GameObject player)
    {
        Player p = player.GetComponent<Player>();
        return "<b><color=#" + ColorUtility.ToHtmlStringRGB(p.playerColor) + ">" + p.steamName + "</color></b>";
    }

    private string GetBombText(GameObject bomb)
    {
        if (bomb.GetComponent<BombObject>() != null)
        {
            return "<color=#B2B2B2>Default Bomb</color>";
        }
        else if (bomb.GetComponent<LaserObject>() != null)
        {
            return "<color=#F9FF23>Laser Bomb</color>";
        }
        else if (bomb.GetComponent<PlasmaObject>() != null)
        {
            return "<color=#17E575>Plasma Bomb</color>";
        }
        else if (bomb.GetComponent<BlinkObject>() != null)
        {
            return "<color=#00D9FF>Blink Bomb</color>";
        }
        else if (bomb.GetComponent<SludgeObject>() != null)
        {
            return "<color=#F153FF>Sludge Bomb</color>";
        }
        else
        {
            Debug.LogError("Could not get bomb type!");
            return "";
        }
    }

    private string GetComboText(char key)
    {
        switch (key)
        {
            case 'b': return "<color=#00D9FF>Blink Combo</color>";
            case 'g': return "<color=#17E575>Plasma Combo</color>";
            case 'y': return "<color=#F9FF23>Laser Combo</color>";
            case 'r': return "<color=#E21616>Big Bomb</color>";
            case 'p': return "<color=#F153FF>Sludge Combo</color>";
            case 'w': return "<color=#B2B2B2>Queen Bee Combo</color>";
            default: return "Error: Key " + key + " not found";
        }
    }

    #endregion
}
