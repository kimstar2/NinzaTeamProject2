using System.Collections;
using System.Collections.Generic;
using Members.LYW.Scripts.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventManager : MonoBehaviour
{
    [SerializeField] private List<EventDataSO> _eventDatas = new();
    [SerializeField] private EventDataSO _eventData;

    [Header("Items")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image exImage;
    [SerializeField] private TextMeshProUGUI eventTitle;
    [SerializeField] private TextExplainer eventExplain;

    [SerializeField] private List<Button> buttons;

    void Awake()
    {
        _eventData = _eventDatas[Random.Range(0, _eventDatas.Count)];
        
        foreach (var button in buttons)
        {
            button.GetComponent<CanvasGroup>().alpha = 0;
        }

        exImage.sprite = _eventData.EXImage;
        backgroundImage.sprite = _eventData.backgroundImage;
        
        exImage.color = Color.white;
        backgroundImage.color = Color.gray2;
        
        eventTitle.SetText(_eventData.EventTitle);
    }
    
    void Start()
    {
        StartCoroutine(StartRoutine());
        eventExplain.StartTexting(_eventData.EventExplain);
    }

    private IEnumerator StartRoutine()
    {
        yield return new WaitForSeconds(0.1f);
        StartEvent();
    }
    
    private void StartEvent()
    {
        for (int i = 0; i < _eventData.choices; i++)
        {
            buttons[i].GetComponentInChildren<TextMeshProUGUI>().SetText(_eventData.choiceText[i]);
            buttons[i].onClick = _eventData.choiceEvent[i];
            buttons[i].onClick.AddListener(() =>
            {
                HideButtons();
            });
        }
        
        for (int i = 0; i < _eventData.choices; i++)
        {
            buttons[i].GetComponent<SetButton>().Set(i);
        }
    }

    public void HideButtons()
    {
        foreach (var button in buttons)
            button.GetComponent<SetButton>().Hide();
    }
}
