using Members.PDY.Scripts.Node;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "New Rest Event", menuName = "Map/Events/Event")]
public class EventSO : NodeEventSO
{
    public override void ExecuteEvent(Node nodeData)
    {
        SceneManager.LoadScene("EventScene");
    }
}