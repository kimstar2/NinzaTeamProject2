using System.Collections;
using TMPro;
using UnityEngine;

namespace Members.LYW.Scripts.Event
{
    public class TextExplainer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private float printTerm = 0.05f;
        
        public void StartTexting(string text)
        {
            StartCoroutine(Texting(text));
        }
        
        IEnumerator Texting(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i]=='\\' && text[i+1] == 'n')
                {
                    i += 1;
                    this.text.text += "\n";
                    continue;
                }
                else
                {
                    this.text.text += text[i];
                }
                yield return new WaitForSeconds(printTerm);
            }
        }
    }
}