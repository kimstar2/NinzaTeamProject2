using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Members.PSW.Code.SettingSystem
{
    // This owner is intentionally unique across scenes; no gameplay service locator is exposed.
    [DefaultExecutionOrder(-10000)]
    [DisallowMultipleComponent]
    public sealed class SettingsRuntime : MonoBehaviour
    {
        private const string PrefabResourcePath = "SettingsWindow";

        [SerializeField] private EventSystem settingsEventSystem;

        private static SettingsRuntime _instance;
        private readonly List<Behaviour> _disabledSceneInputs = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession()
        {
            // Also supports entering Play mode without reloading the domain.
            if (_instance != null)
            {
                SceneManager.sceneLoaded -= _instance.HandleSceneLoaded;
                _instance.RestoreSceneInputs();
            }
            _instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureSettingsExist()
        {
            if (_instance == null)
            {
                SettingsRuntime existing = FindFirstObjectByType<SettingsRuntime>(FindObjectsInactive.Include);
                if (existing != null)
                {
                    existing.ClaimOwnership();
                    existing.gameObject.SetActive(true);
                }
                else
                {
                    GameObject prefab = Resources.Load<GameObject>(PrefabResourcePath);
                    if (prefab == null)
                    {
                        Debug.LogError("The SettingsWindow prefab is missing from Resources.");
                        return;
                    }
                    Instantiate(prefab);
                }
            }

            if (_instance != null)
                _instance.UseSingleEventSystem();
        }

        private void Awake()
        {
            ClaimOwnership();
        }

        private void ClaimOwnership()
        {
            if (_instance != null && _instance != this)
            {
                // Disable immediately, before duplicate input and persistence components run.
                gameObject.SetActive(false);
                Destroy(gameObject);
                return;
            }

            _instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UseSingleEventSystem();
        }

        private void UseSingleEventSystem()
        {
            if (settingsEventSystem == null)
            {
                Debug.LogError("Assign the persistent settings EventSystem.", this);
                return;
            }

            settingsEventSystem.gameObject.SetActive(true);
            settingsEventSystem.enabled = true;
            _disabledSceneInputs.RemoveAll(component => component == null);

            foreach (EventSystem other in FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
            {
                if (other == settingsEventSystem)
                    continue;

                // Keep authored scene objects and unrelated components intact.
                foreach (BaseInputModule module in other.GetComponents<BaseInputModule>())
                    DisableSceneInput(module);
                DisableSceneInput(other);
            }

            EventSystem.current = settingsEventSystem;
        }

        private void DisableSceneInput(Behaviour component)
        {
            if (!component.enabled)
                return;

            _disabledSceneInputs.Add(component);
            component.enabled = false;
        }

        private void RestoreSceneInputs()
        {
            foreach (Behaviour component in _disabledSceneInputs)
            {
                if (component != null)
                    component.enabled = true;
            }
            _disabledSceneInputs.Clear();
        }

        private void OnDestroy()
        {
            if (_instance != this)
                return;

            SceneManager.sceneLoaded -= HandleSceneLoaded;
            _instance = null;
            RestoreSceneInputs();
        }
    }
}
