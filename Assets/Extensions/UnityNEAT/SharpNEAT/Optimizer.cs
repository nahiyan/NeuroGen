using UnityEngine;
using System.Collections;
using SharpNeat.Phenomes;
using System.Collections.Generic;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using System;
using System.Xml;
using System.IO;

public class Optimizer : MonoBehaviour
{
    public int NUM_INPUTS = 3;
    public int NUM_OUTPUTS = 2;

    public int Trials;
    bool EARunning;
    string popFileSavePath, champFileSavePath;

    SimpleExperiment experiment;
    static NeatEvolutionAlgorithm<NeatGenome> _ea;

    public GameObject Unit;

    public Dictionary<IBlackBox, UnitController> ControllerMap = new Dictionary<IBlackBox, UnitController>();
    private DateTime startTime;
    private float timeLeft;
    private float accum;
    private int frames;
    private float updateInterval = 12;

    private uint Generation;
    private double Fitness;
    public float evoSpeed = 25;

    // Use this for initialization
    void Start()
    {
        Utility.DebugLog = true;
        experiment = new SimpleExperiment();
        XmlDocument xmlConfig = new XmlDocument();
        TextAsset textAsset = (TextAsset)Resources.Load("experiment.config");
        xmlConfig.LoadXml(textAsset.text);
        experiment.SetOptimizer(this);

        experiment.Initialize("Car Experiment", xmlConfig.DocumentElement, NUM_INPUTS, NUM_OUTPUTS);

        champFileSavePath = Application.persistentDataPath + string.Format("/{0}.champ.xml", "car");
        popFileSavePath = Application.persistentDataPath + string.Format("/{0}.pop.xml", "car");
    }

    // Update is called once per frame
    // void Update()
    // {
    //     //  evaluationStartTime += Time.deltaTime;

    //     timeLeft -= Time.deltaTime;
    //     accum += Time.timeScale / Time.deltaTime;
    //     ++frames;

    //     if (timeLeft <= 0.0)
    //     {
    //         // var fps = accum / frames;
    //         timeLeft = updateInterval;
    //         accum = 0.0f;
    //         frames = 0;

    //         // if (fps < 10)
    //         //     print("Lowering time scale to " + --Time.timeScale);
    //     }
    // }

    public void StartEA()
    {
        Utility.DebugLog = true;
        Utility.Log("Starting experiment");
        // print("Loading: " + popFileLoadPath);
        _ea = experiment.CreateEvolutionAlgorithm(popFileSavePath);
        startTime = DateTime.Now;

        _ea.UpdateEvent += new EventHandler(ea_UpdateEvent);
        _ea.PausedEvent += new EventHandler(ea_PauseEvent);

        //   Time.fixedDeltaTime = 0.045f;
        Time.timeScale = evoSpeed;
        _ea.StartContinue();
        EARunning = true;
    }

    void ea_UpdateEvent(object sender, EventArgs e)
    {
        Utility.Log(string.Format("gen={0:N0} bestFitness={1:N6}",
            _ea.CurrentGeneration, _ea.Statistics._maxFitness));

        Fitness = _ea.Statistics._maxFitness;
        Generation = _ea.CurrentGeneration;

        //    Utility.Log(string.Format("Moving average: {0}, N: {1}", _ea.Statistics._bestFitnessMA.Mean, _ea.Statistics._bestFitnessMA.Length));
    }

    void ea_PauseEvent(object sender, EventArgs e)
    {
        Time.timeScale = 1;
        Utility.Log("Done ea'ing (and neat'ing)");

        XmlWriterSettings _xwSettings = new XmlWriterSettings();
        _xwSettings.Indent = true;

        // Save genomes to xml file.        
        DirectoryInfo dirInf = new DirectoryInfo(Application.persistentDataPath);
        if (!dirInf.Exists)
        {
            Debug.Log("Creating subdirectory");
            dirInf.Create();
        }
        using (XmlWriter xw = XmlWriter.Create(popFileSavePath, _xwSettings))
        {
            experiment.SavePopulation(xw, _ea.GenomeList);
        }

        // Also save the best genome
        using (XmlWriter xw = XmlWriter.Create(champFileSavePath, _xwSettings))
        {
            experiment.SavePopulation(xw, new NeatGenome[] { _ea.CurrentChampGenome });
        }

        DateTime endTime = DateTime.Now;
        Utility.Log("Total time elapsed: " + (endTime - startTime));

        System.IO.StreamReader stream = new System.IO.StreamReader(popFileSavePath);

        EARunning = false;
    }

    // public static void SaveChamp()
    // {
    //     var champFileSavePath = Application.persistentDataPath + string.Format("/{0}.champ.xml", "car");

    //     XmlWriterSettings _xwSettings = new XmlWriterSettings();
    //     _xwSettings.Indent = true;

    //     using (XmlWriter xw = XmlWriter.Create(champFileSavePath, _xwSettings))
    //     {
    //         experiment.SavePopulation(xw, new NeatGenome[] { _ea.CurrentChampGenome });
    //     }
    // }

    public void StopEA()
    {
        if (_ea != null && _ea.RunState == SharpNeat.Core.RunState.Running)
            _ea.Stop();
    }

    public void Evaluate(IBlackBox box)
    {
        GameObject obj = Instantiate(Unit, Unit.transform.position, Unit.transform.rotation) as GameObject;
        UnitController controller = obj.GetComponent<UnitController>();

        ControllerMap.Add(box, controller);

        controller.Activate(box);
    }

    public void StopEvaluation(IBlackBox box)
    {
        UnitController ct = ControllerMap[box];

        Destroy(ct.gameObject);
    }

    public void RunBest()
    {
        Time.timeScale = 1;

        NeatGenome genome = null;

        // Try to load the genome from the XML document.
        try
        {
            using (XmlReader xr = XmlReader.Create(champFileSavePath))
                genome = NeatGenomeXmlIO.ReadCompleteGenomeList(xr, false, (NeatGenomeFactory)experiment.CreateGenomeFactory())[0];
        }
        catch (Exception)
        {
            // print(champFileLoadPath + " Error loading genome from file!\nLoading aborted.\n"
            //						  + e1.Message + "\nJoe: " + champFileLoadPath);
            return;
        }

        // Get a genome decoder that can convert genomes to phenomes.
        var genomeDecoder = experiment.CreateGenomeDecoder();

        // Decode the genome into a phenome (neural network).
        var phenome = genomeDecoder.Decode(genome);

        GameObject obj = Instantiate(Unit, Unit.transform.position, Unit.transform.rotation) as GameObject;
        UnitController controller = obj.GetComponent<UnitController>();

        ControllerMap.Add(phenome, controller);

        controller.Activate(phenome);

        Coroutiner.StartCoroutine(MonitorBestPhenome(controller));
    }

    private IEnumerator MonitorBestPhenome(UnitController controller)
    {
        int i = 0;
        while (true)
        {
            if (controller.isRunning)
            {
                if (i % 100 == 0)
                    Debug.Log("Fitness: " + controller.Fitness);
            }
            else
            {
                ControllerMap.Clear();
                Debug.Log("Final fitness: " + controller.Fitness);
                break;
            }

            i++;

            yield return new WaitForFixedUpdate();
        }
    }

    public float GetFitness(IBlackBox box)
    {
        if (ControllerMap.ContainsKey(box))
        {
            var fitness = ControllerMap[box].Fitness;
            ControllerMap.Remove(box);
            return fitness;
        }

        return 0;
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 40), "Start"))
            StartEA();

        if (GUI.Button(new Rect(10, 60, 100, 40), "Stop"))
            StopEA();

        if (GUI.Button(new Rect(10, 110, 100, 40), "Run best"))
            RunBest();

        if (GUI.Button(new Rect(10, 160, 100, 40), "Inc. TS"))
            Time.timeScale++;

        if (GUI.Button(new Rect(10, 210, 100, 40), "Dec. TS"))
            Time.timeScale--;

        GUI.Button(new Rect(10, Screen.height - 70, 130, 60), string.Format("Generation: {0}\nFitness: {1:0.00}\nTime Scale: {2:0.0}", Generation, Fitness, Time.timeScale));
    }
}
