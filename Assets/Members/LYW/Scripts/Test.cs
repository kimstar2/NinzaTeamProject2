using UnityEngine;

public class Test : MonoBehaviour
{
    private GameObject firstEvent;
    private GameObject secondEvent;

    private void Awake()
    {
        firstEvent = GameObject.Find("Event");
        secondEvent = GameObject.Find("Event2");
    }
    
    public void StartSecondEvent()
    {
        firstEvent.SetActive(false);
        secondEvent.SetActive(true);
    }
    
    public void Say(int num)
    {
        Debug.Log(num);
    }
}
