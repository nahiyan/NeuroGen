using System.Collections.Generic;
using UnityEngine;
using NEAT;
using NeuroGen;

public class GenomeCar : GenomeProxy
{
    #region Fields
    public string json = "";
    private NeuroGen.CarController carController = null;
    public SpriteRenderer speciesColor;

    [Header("Raycast stuff")]
    [SerializeField] private LayerMask obstacleLayer = 0;
    private int finishCross = 0;
    private Vector3 lastPositionMark;
    private PopulationCar populationCar;

    private float currentMaxFitness = 0;
    private float lastMaxFitnessUpdate = 0;

    private List<NeuroGen.Checkpoint> checkpointPassed = new List<NeuroGen.Checkpoint>();

    public override bool IsDone
    { get { return !carController.isRunning; } set { carController.isRunning = !value; } }
    #endregion

    #region Monobehaviour
    private void Awake()
    {
        carController = gameObject.GetComponent<NeuroGen.CarController>();
        populationCar = FindObjectOfType<PopulationCar>();
    }

    private void FixedUpdate()
    {
        ActivateNeuralNet(carController.SensorValues);
        GenomeProperty.Fitness = carController.Fitness;
    }
    #endregion

    #region Override
    public override void Init(int id, Population popl)
    {
        base.Init(id, popl);
    }

    public override void ProcessNetworkOutput(float[] netOutputs)
    {
        base.ProcessNetworkOutput(netOutputs);
        carController.Step(netOutputs);
    }
    #endregion

    #region Public Methods
    public void Reinit(Transform targetPositionRotation)
    {
        json = JsonUtility.ToJson(new PackedGenome(GenomeProperty));

        carController.Reinit();
        transform.SetPositionAndRotation(
            targetPositionRotation.position,
            targetPositionRotation.rotation
        );

        GenomeProperty.Fitness = 0;

        finishCross = 0;
        checkpointPassed.Clear();

        currentMaxFitness = 0;
        lastMaxFitnessUpdate = Time.time;
    }

    public void Die()
    {
        carController.Stop();
    }
    #endregion

    #region Private methods

    #endregion
}
