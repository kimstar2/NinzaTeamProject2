using Members.CJY.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

public class NodeMapLoader : MonoBehaviour
{ 
    [SerializeField] private NodeMaker nodeMaker;

    void Awake()
    {
        nodeMaker.OpenNode();
    }
}
