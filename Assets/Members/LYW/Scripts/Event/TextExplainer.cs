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
            StopAllCoroutines();
            this.text.text = string.Empty;
            StartCoroutine(Texting(text.Replace("\\n", "\n")));
        }

        public float GetPrintDuration(string value) => value.Replace("\\n", "\n").Length * printTerm;
        
        IEnumerator Texting(string text)
        {
            foreach (char letter in text)
            {
                this.text.text += letter;
                yield return new WaitForSeconds(printTerm);
            }
        }
    }
}
