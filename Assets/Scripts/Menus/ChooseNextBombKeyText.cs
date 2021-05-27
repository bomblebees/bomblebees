using InControl;
using TMPro;
using UnityEngine;

public class ChooseNextBombKeyText : MonoBehaviour
{
    private TMP_Text _tmpText;
    private PlayerAction _playerAction;
    
    private void Awake()
    {
        _tmpText = GetComponent<TMP_Text>();
        _playerAction = FindObjectOfType<MenuManager>().GameActions.ChooseNextBomb;

        UpdateText();
    }

    private void OnEnable()
    {
        _playerAction.OnBindingsChanged += OnBindingChanged;
    }
    
    private void OnBindingChanged()
    {
        UpdateText();
    }
    
    private void UpdateText()
    {
        _tmpText.text = _playerAction.Bindings.Count > 0 ? _playerAction.Bindings[0].Name : "N/A";
    }
}
