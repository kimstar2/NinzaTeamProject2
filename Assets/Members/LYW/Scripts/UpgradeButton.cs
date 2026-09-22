using System;
using System.Collections;
using System.Collections.Generic;
using Members.LYW.Scripts;
using Members.LYW.Scripts.MySystem.Events;
using Members.LYW.Scripts.System;
using Members.LYW.Scripts.System.Events;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UpgradeButton : MonoBehaviour
{
    [SerializeField] private DiceFragmentSlotSetter diceFragmentSlotSetter;
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        
        EventBus.Subscribe<RegisterFragmentEvent>(SetButtonState);
        EventBus.Subscribe<UnRegisterFragmentEvent>(SetButtonState);
        button.onClick.AddListener(SetButtonState);
    }

    void OnDisable()
    {
        EventBus.UnSubscribe<RegisterFragmentEvent>(SetButtonState);
        EventBus.UnSubscribe<UnRegisterFragmentEvent>(SetButtonState);
        button.onClick.RemoveListener(SetButtonState);
    }

    private void Start()
    {
        var colors = button.colors;
        colors.normalColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        button.colors = colors;
            
        button.interactable = false;
    }

    private void SetButtonState()
    {
        StartCoroutine(UpdateButton());
    }
    
    private void SetButtonState(RegisterFragmentEvent registerFragmentEvent)
    {
        StartCoroutine(UpdateButton());
    }

    private void SetButtonState(UnRegisterFragmentEvent registerFragmentEvent)
    {
        StartCoroutine(UpdateButton());
    }
    
    private IEnumerator UpdateButton()
    {
        yield return new WaitForSeconds(0.001f);
        
        if (diceFragmentSlotSetter.CanUpgrade)
        {
            var colors = button.colors;
            colors.normalColor = Color.white;
            button.colors = colors;
            
            button.interactable = true;
        }
        else
        {
            var colors = button.colors;
            colors.normalColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            button.colors = colors;
            
            button.interactable = false;
        }
    }
}
