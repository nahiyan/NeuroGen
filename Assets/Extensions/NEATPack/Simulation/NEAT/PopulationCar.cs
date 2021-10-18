using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NEAT;
using UnityEngine.Events;
using NeuroGen;

public class PopulationCar : PopulationProxy
{
    [SerializeField] private UnityEvent onGenerationChange = null;
    [SerializeField] private UnityEvent onGenomeStatusChange = null;
    public float maxVelocity = 55f;
    public float maxNegativeVelocity = -5f;

    private SmoothFollow cameraFollow = null;
    private List<GenomeCar> cars = null;
    private GenomeColorCtrl genomeColorCtrl = new GenomeColorCtrl();

    public bool EveryoneIsDead => cars.FirstOrDefault(x => !x.IsDone) == null;
    protected override void Awake()
    {
        base.Awake();

        if (Main.Instance.defaultTrackSystemInfo.spawnPoint == null)
        {
            Debug.LogError("Car spawn point can't be null");
            Debug.Break();
        }

        if (cameraFollow == null)
            cameraFollow = FindObjectOfType<SmoothFollow>();
    }

    private void FixedUpdate()
    {
        var cameraTarget = cars.OrderByDescending(x => x.GenomeProperty.Fitness).FirstOrDefault(x => x.gameObject.activeSelf);
        cameraFollow.Target = cameraTarget == null ? null : cameraTarget.transform;

        if (EveryoneIsDead)
            Evolve();

        onGenomeStatusChange.Invoke();
    }

    public void KillAll()
    {
        foreach (var car in cars)
            car.Die();
    }

    public void ReinitCars()
    {
        foreach (var car in cars)
            car.Reinit(Main.Instance.defaultTrackSystemInfo.spawnPoint);

        if (Main.Instance.defaultTrackSystemInfo.finishPoint != null)
            Main.Instance.defaultTrackSystemInfo.finishPoint.crossedBy.Clear();
    }

    public override void Evolve()
    {
        // Sort the cars based on fitness in descending order
        cars.Sort((x, y) => -x.GenomeProperty.Fitness.CompareTo(y.GenomeProperty.Fitness));

        // Display fitness values of the entire population
        string text = "Population fitnesses: ";
        foreach (var car in cars)
        {
            text += car.GenomeProperty.Fitness + " ";

            Database.Stage(JsonUtility.ToJson(new PackedGenome(car.GenomeProperty)), car.GenomeProperty.Fitness);
        }
        Debug.Log(text);

        using (StreamWriter sw = File.AppendText(Application.persistentDataPath + "/log.txt"))
            sw.WriteLine(text);

        // Trim the models table for the current extension
        Database.Commit(Configuration.Instance.saved_models_count);
        Database.SaveFile();

        // Evolution
        base.Evolve();

        ReinitCars();

        // genomeColorCtrl.UpdateSpeciesColor(Popl.SpeciesCtrl);
        // foreach (var car in cars)
        //     car.speciesColor.color = genomeColorCtrl.GetSpeciesColor(car.GenomeProperty.SpeciesId);
        onGenerationChange.Invoke();
    }

    public override void InitPopl()
    {
        base.InitPopl();
        cars = new List<GenomeCar>(Config.genomeCount);
    }

    protected override void InitGenomeProxyObj(GameObject genomeProxy)
    {
        var genomeCar = genomeProxy.GetComponent<GenomeCar>();
        if (cars.FirstOrDefault(x => x.Id == genomeCar.Id) == null)
        {
            cars.Add(genomeCar);
            genomeCar.Reinit(Main.Instance.defaultTrackSystemInfo.spawnPoint);
        }
    }
}
