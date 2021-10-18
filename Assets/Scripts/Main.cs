using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace NeuroGen
{
    public enum StartMode
    {
        RandomModels,
        BestModels
    }
    public enum Status
    {
        Running,
        Paused,
        Stopped
    }
    public class Main : MonoBehaviour
    {
        public int inputCount;
        public int outputCount;
        public int bestModelsCount;
        public StartMode startMode = StartMode.RandomModels;
        public Status status = Status.Stopped;
        public GameObject carPrefab;
        public GameObject[] extensions;
        public GameObject topDownCamera;
        public GameObject thirdPersonCamera;
        public GameObject startMenu;
        public HUD hud;
        public TrackSystemInfo defaultTrackSystemInfo;
        public int selectedExtensionIndex;
        public float timeScale;
        private CarController[] carControllers;
        public Text speedText;
        private static Main instance = null;
        public static Main Instance { get { return instance; } }
        public int survivors;
        public float highestFitness;

        void Start()
        {
            // Prepare log file
            if (!File.Exists(Application.persistentDataPath + "/log.txt"))
                File.CreateText(Application.persistentDataPath + "/log.txt");

            instance = this;

            var arguments = System.Environment.GetCommandLineArgs();
            if (arguments.Length == 2)
            {
                Configuration.LoadJson(arguments[1].Replace("\\\"", "\""));
            }
            else
            {
                startMenu.SetActive(true);

                // Create models file if it doesn't exist
                if (!File.Exists(Configuration.Instance.models_file_path))
                    using (StreamWriter sw = File.CreateText(Configuration.Instance.models_file_path))
                        sw.WriteLine("{\"models\": []}");
            }

            Database.LoadFile(Configuration.Instance.models_file_path);

            if (Configuration.Instance.start_from_saved_models)
                startMode = StartMode.BestModels;
            else
                startMode = StartMode.RandomModels;

            if (Configuration.Instance.top_down_mode)
                topDownCamera.SetActive(true);
            else
                thirdPersonCamera.SetActive(true);

            timeScale = Configuration.Instance.speed;
            hud.speedSlider.value = timeScale;
            defaultTrackSystemInfo.gameObject.SetActive(true);

            if (arguments.Length == 2)
                StartSimulation();
        }

        void ManageSelectedExtension(bool activate = true)
        {
            if (selectedExtensionIndex >= 0 && selectedExtensionIndex < extensions.Length)
            {
                var extensionMainObject = extensions[selectedExtensionIndex];

                survivors = 0;
                highestFitness = 0;

                extensionMainObject.SetActive(activate);
                status = activate ? Status.Running : Status.Stopped;
            }
        }
        void ActivateExtension()
        {
            ManageSelectedExtension(true);
        }
        void DeactivateExtension()
        {
            ManageSelectedExtension(false);
        }

        void FixedUpdate()
        {
            Time.timeScale = hud.speedSlider.value;
            hud.speedSliderValue.text = Time.timeScale.ToString("0");
            hud.survivorsLabel.text = "Survivors: " + survivors + "/" + Configuration.Instance.population_size;
            hud.highestFitnessLabel.text = "Highest Fitness: " + highestFitness.ToString("0");
        }

        public void UpdateStartMode(Dropdown startModeDropdown)
        {
            switch (startModeDropdown.value)
            {
                case 0:
                    startMode = StartMode.RandomModels;
                    break;
                default:
                    startMode = StartMode.BestModels;
                    break;
            }
        }

        public void StartSimulation()
        {
            startMenu.SetActive(false);

            ActivateExtension();

            hud.gameObject.SetActive(true);
        }

        public void StopSimulation()
        {
            DeactivateExtension();

            hud.gameObject.SetActive(false);
            startMenu.SetActive(true);
        }
        public void TogglePauseResume()
        {
            if (Time.timeScale > 0)
            {
                Time.timeScale = 0;
                hud.pauseResumeToggle.GetComponentInChildren<Text>().text = "Resume";
            }
            else
            {
                Time.timeScale = hud.speedSlider.value;
                hud.pauseResumeToggle.GetComponentInChildren<Text>().text = "Pause";
            }
        }
        public void Quit()
        {
            Application.Quit();
        }
    }
}