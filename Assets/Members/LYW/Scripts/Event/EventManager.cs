using System.Collections;
using System.Collections.Generic;
using Members.LYW.Scripts.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

public class EventManager : MonoBehaviour
{
    [SerializeField] private List<EventDataSO> _eventDatas = new();
    [SerializeField] private EventDataSO _eventData;

    [Header("Items")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image exImage;
    [SerializeField] private TextMeshProUGUI eventTitle;
    [SerializeField] private TextExplainer eventExplain;

    [SerializeField] private Eventer eventer;
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
        
        {
            exImage.color = _eventData.exImageColor;
            exImage.material = _eventData.exImageMaterial;
            backgroundImage.color = _eventData.backGroundImageColor;
            exImage.rectTransform.localScale = new Vector3(_eventData.exImageScale, _eventData.exImageScale, _eventData.exImageScale);
        }
        
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
            int index = i;
            
            buttons[i].GetComponentInChildren<TextMeshProUGUI>().SetText(_eventData.choiceText[i]);
            buttons[i].onClick.AddListener(HideButtons);
            switch (_eventData.choiceEvent[i])
            {
                case Members.LYW.Scripts.Event.EventType.Say:
                    buttons[i].onClick.AddListener(eventer.Say);
                    break;
                case Members.LYW.Scripts.Event.EventType.MoveNextEvent:
                    buttons[i].onClick.AddListener(eventer.MoveNextEvent);
                    break;
                case  Members.LYW.Scripts.Event.EventType.DefaultComplete:
                    buttons[i].onClick.AddListener(()=>
                    {
                        eventer.EndEvent(_eventData.resultText[index]);
                    });
                    break;
                case  Members.LYW.Scripts.Event.EventType.ChangePlayerStatus:
                    buttons[i].onClick.AddListener(()=>
                    {
                        eventer.EndEvent(_eventData.resultText[index]);
                        eventer.SetPlayerStatus(_eventData.changeStatusDatas[index]);
                    });
                    break;
            }
        }
        
        for (int i = 0; i < _eventData.choices; i++)
        {
            buttons[i].GetComponent<SetButton>().Set(i);
        }
    }

    public void HideButtons()
    {
        foreach (var button in buttons)
        {
            button.onClick.RemoveListener(HideButtons);
            button.GetComponent<SetButton>().Hide();
        }
    }
}
