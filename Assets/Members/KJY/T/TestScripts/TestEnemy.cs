using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Members.KJY.T.TestScripts
{
    
    public class TestEnemy : MonoBehaviour
    {
        [SerializeField] private DiceData defaultDiceData;
        [SerializeField] private Image icon;
        [SerializeField] private Image diceImage;

        [ContextMenu("Kill")]
        public void Kill()
        {
            icon.color = Color.gray;
            StartCoroutine(RollDice());
        }

        public IEnumerator RollDice()
        {
            int r = 0;
            for (int i = 0; i < 20; i++)
            {
                r = Random.Range(0, defaultDiceData.diceSurface.Count);
                diceImage.sprite = defaultDiceData.diceSurface[r].diceSprite;
                yield return new WaitForSeconds(0.05f);
            }
            Debug.Log("이거 줌");
            DiceSystem.Instance.SetSurface(r,defaultDiceData.diceSurface[r]);
        }
    }
}