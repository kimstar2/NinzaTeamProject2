using System.Collections.Generic;
using UnityEngine;

namespace Members.KJY._01.Scripts.Dice.Data
{
    // 한 번이라도 얻은 면을 기억한다. 런이 끝나도 남아야 해서 PlayerPrefs에 저장.
    public static class DiceCatalogProgress
    {
        private const string Key = "DiceCatalog.Discovered.v2";
        private const char Separator = '|';
        private static HashSet<string> _discovered;

        private static HashSet<string> Discovered
        {
            get
            {
                if (_discovered != null) return _discovered;
                _discovered = new HashSet<string>();
                string saved = PlayerPrefs.GetString(Key, string.Empty);
                foreach (string id in saved.Split(Separator))
                    if (!string.IsNullOrEmpty(id)) _discovered.Add(id);
                return _discovered;
            }
        }

        // 에셋 이름이 같은 면이 있어서(예: 처형 / 마무리의 면 둘 다 Execution Face) 표시 이름까지 붙여 구분한다
        private static string Id(DiceDataSO face) => face.name + "#" + face.MainName;

        public static bool IsDiscovered(DiceDataSO face) => face != null && Discovered.Contains(Id(face));

        public static void Discover(DiceDataSO face)
        {
            if (face == null || !Discovered.Add(Id(face))) return;
            PlayerPrefs.SetString(Key, string.Join(Separator.ToString(), Discovered));
            PlayerPrefs.Save();
        }

        public static void Discover(DiceDataListSO list)
        {
            if (list == null) return;
            for (int i = 0; i < 6; i++) Discover(list.GetDiceData((DiceFaceType)i));
        }

        public static void ResetAll()
        {
            Discovered.Clear();
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("KJY/주사위 도감 기록 초기화")]
        private static void ResetFromMenu() => ResetAll();
#endif
    }
}
