using System;
using Members.CJY.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [SerializeField] NodeEvent nodeEvent;

    private void Awake()
    {
        nodeEvent.OnNodeSelected += HandleNodeSelected;
    }
    
    private void HandleNodeSelected(NodeConnect node)
    {
        switch (node.info.type)
        {
            case NodeType.Battle:
                break;
            
            case NodeType.Elite:
                break;
            
            case NodeType.Boss:
                break;
            
            // ------------------------
            
            case NodeType.Event:
                SceneManager.LoadScene("EventScene");
                break;
            
            case NodeType.Rest:
                SceneManager.LoadScene("RestScene");
                break;
            
            case NodeType.Shop:
                break;
            
            // ------------------------
            
            case NodeType.Start:
                break;
            
            default:
                Debug.Log("뭔노드여");
                break;
        }
    }
}
