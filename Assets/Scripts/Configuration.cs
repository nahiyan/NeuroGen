using UnityEngine;

namespace NeuroGen
{
    public class Configuration
    {
        public int population_size = 50;
        public string models_file_path = Application.persistentDataPath + "/database.json";
        public int saved_models_count = 10;
        public float speed = 1;
        public bool start_from_saved_models = true;
        public bool top_down_mode = true;
        private static Configuration instance = null;
        public static Configuration Instance
        {
            get
            {
                if (instance == null)
                    instance = new Configuration();
                return instance;
            }
        }

        public static void LoadJson(string jsonString)
        {
            instance = JsonUtility.FromJson<Configuration>(jsonString);
        }
    }
}