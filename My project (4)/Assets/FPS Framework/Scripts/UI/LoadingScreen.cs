using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Threading.Tasks;

namespace Akila.FPSFramework
{
    /// <summary>
    /// Controls the loading screen UI, showing tips during scene loading.
    /// </summary>
    [AddComponentMenu("Akila/FPS Framework/UI/Loading Screen")]
    public class LoadingScreen : MonoBehaviour
    {
        /// <summary>
        /// List of tips to display on the loading screen.
        /// </summary>
        [Tooltip("List of tips displayed randomly on the loading screen.")]
        public List<string> loadingTips = new List<string>();

        /// <summary>
        /// Text element used to display the tips.
        /// </summary>
        [Tooltip("TextMeshProUGUI component to show the tip.")]
        public TextMeshProUGUI tipsTextUI;

        /// <summary>
        /// Time interval between displaying different tips, in seconds.
        /// </summary>
        [Tooltip("Duration in seconds between tip changes.")]
        public float tipDisplayInterval = 4f;

        /// <summary>
        /// Delay in seconds before hiding the loading screen after loading is complete.
        /// </summary>
        [Tooltip("Delay in seconds before hiding the loading screen after the scene is loaded.")]
        public float postLoadDelay = 1f;

        /// <summary>
        /// Singleton instance of the LoadingScreen.
        /// </summary>
        public static LoadingScreen Instance
        {
            get
            {
                if(instance == null)
                {
                    Debug.LogError("Loading Scene is null, inizalizing one.");

                    Initialize();

                    return null;
                }

                return instance;
            }
        }

        private static LoadingScreen instance;

        /// <summary>
        /// The current state of the loading screen. If true the loading scene is loaded and ready to load other scenes.
        /// </summary>
        public static bool Initialized;

        // Index of the previously displayed tip, to avoid showing the same tip consecutively.
        private int previousTipIndex;

        private void Awake()
        {
            // Ensure a single instance exists
            if (instance == null)
                instance = this;
            else
            {
                Debug.LogError("Multiple instances of LoadingScreen detected. Destroying duplicate.", gameObject);
                Destroy(gameObject);
 
                return;
            }

            // Disable the loading screen on start
            Disable();

            // Ensure tipsTextUI is set up
            if (tipsTextUI == null)
            {
                Debug.LogError("Tips Text UI component is not assigned in the inspector.", gameObject);

                return;
            }


            // Prevent destruction of the loading screen when switching scenes
            DontDestroyOnLoad(gameObject);

            // Start showing tips at regular intervals
            InvokeRepeating(nameof(DisplayRandomTip), 0, tipDisplayInterval);
        }

        /// <summary>
        /// Displays a random tip from the list, ensuring the same tip is not displayed consecutively.
        /// </summary>
        private void DisplayRandomTip()
        {
            if (loadingTips.Count == 0)
            {
                Debug.LogError("Loading tips list is empty.", gameObject);
                return;
            }

            int index;
            do
            {
                index = Random.Range(0, loadingTips.Count);
            }
            // Ensure the new tip is different from the previous one
            while (index == previousTipIndex && loadingTips.Count > 1);

            previousTipIndex = index;
            tipsTextUI.text = loadingTips[index];
        }

        /// <summary>
        /// Activates or deactivates all child GameObjects of the loading screen.
        /// </summary>
        /// <param name="state">True to activate, false to deactivate.</param>
        public void SetActive(bool state)
        {
            if (transform.childCount == 0)
            {
                Debug.LogError("No child objects found under the loading screen.", gameObject);
            }

            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(state);
            }
        }

        /// <summary>
        /// Enables the loading screen.
        /// </summary>
        public void Enable()
        {
            SetActive(true);
        }

        /// <summary>
        /// Disables the loading screen.
        /// </summary>
        public void Disable()
        {
            SetActive(false);
        }

        /// <summary>
        /// Loads the specified scene asynchronously, displaying the loading screen during the load.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load.</param>
        /// <returns>An IEnumerator for coroutine handling.</returns>
        protected void Load(string sceneName)
        {
            StartCoroutine(LoadAsync(sceneName));
        }

        /// <summary>
        /// Loads the specified scene asynchronously, displaying the loading screen during the load.
        /// </summary>
        /// <param name="sceneName">The name of the scene to load.</param>
        /// <returns>An IEnumerator for coroutine handling.</returns>
        protected IEnumerator LoadAsync(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("Scene name is null or empty.", gameObject);
                yield break;
            }

            // Show the loading screen
            Enable();

            // Small delay before starting the async operation
            yield return new WaitForSeconds(0.2f);

            // Start loading the scene asynchronously
            AsyncOperation loadOperation;
            try
            {
                loadOperation = SceneManager.LoadSceneAsync(sceneName);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to load scene '{sceneName}': {ex.Message}", gameObject);
                yield break;
            }

            // Wait until the scene is nearly fully loaded (90% progress)
            yield return new WaitUntil(() => loadOperation.progress >= 0.90f);

            // Hide the loading screen after a delay once the scene is loaded
            Invoke(nameof(Disable), postLoadDelay);
        }

        /// <summary>
        /// Initializes the Loading Screen by loading the "Loading" scene asynchronously in an additive mode.
        /// If the Loading Screen is already initialized, it logs an error and skips initialization.
        /// </summary>
        public static void Initialize()
        {
            // Check if the instance is null, meaning it hasn't been initialized yet.
            if (instance == null)
            {
                // Load the "Loading" scene asynchronously without unloading the current scene.
                SceneManager.LoadSceneAsync("Loading");

                Initialized = true;
            }
            else
            {
                // If the instance already exists, log an error indicating initialization has already occurred.
                Debug.LogError("Initialization aborted: LoadingScreen has already been initialized. Further initialization is not allowed.", instance);
            }
        }

        public static async void LoadScene(string sceneName)
        {
            bool called = false;

            // Continue looping until the current time exceeds the highest event time.
            while (instance == null)
            {
                if(called == false)
                {
                    Initialize();
                }

                called = true;

                // Yield to allow other operations to run, making this method asynchronous.
                await Task.Yield();
            }

            if (instance)
                instance.Load(sceneName);
        }
    }
}
