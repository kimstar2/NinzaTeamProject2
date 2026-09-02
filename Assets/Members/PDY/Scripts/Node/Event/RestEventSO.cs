using Members.PDY.Scripts.Node;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "New Rest Event", menuName = "Map/Events/Rest Event")]
public class RestEventSO : NodeEventSO
{
    public override void ExecuteEvent(Node nodeData)
    {
        SceneManager.LoadScene("RestScene");
    }
}