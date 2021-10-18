using UnityEngine;
using SharpNeat.Phenomes;

namespace NeuroGen
{
    public class CarController : MonoBehaviour
    {
        [SerializeField] private TextMesh fitnessText;
        [SerializeField] private GameObject[] sensors;
        private LineRenderer[] sensorLineRenderers;

        [SerializeField] private WheelCollider[] wheelColliders;
        [SerializeField] private Transform[] wheelTransforms;
        [SerializeField] private float maxSteeringAngle = 30f;
        [SerializeField] private float motorForce = 1100f;
        [SerializeField] private float brakeForce = 3000f;
        private bool showSensors = true;
        private float distanceTravelled;
        private float idleTime = 0;
        public bool humanControlled = false;
        public bool isRunning = false;
        public float Fitness { get { return distanceTravelled; } }
        public float[] SensorValues
        {
            get
            {
                float[] sensorValues = new float[10];

                for (int i = 0; i < sensors.Length; i++)
                {
                    var lineRenderer = sensors[i].GetComponent<LineRenderer>();
                    var startingPosition = lineRenderer.transform.TransformPoint(lineRenderer.GetPosition(0));
                    var endingPosition = lineRenderer.transform.TransformPoint(lineRenderer.GetPosition(1));
                    var sensorLength = Vector3.Distance(startingPosition, endingPosition);
                    var sensorDirection = lineRenderer.transform.TransformDirection(lineRenderer.GetPosition(1));

                    RaycastHit hit;
                    if (Physics.Raycast(startingPosition, sensorDirection.normalized, out hit, sensorLength, Physics.DefaultRaycastLayers))
                    {
                        lineRenderer.startColor = Color.red;
                        lineRenderer.endColor = Color.red;
                        sensorValues[i] = (sensorLength - hit.distance) / sensorLength;
                    }
                    else
                    {
                        lineRenderer.startColor = Color.green;
                        lineRenderer.endColor = Color.green;
                        sensorValues[i] = 0;
                    }
                }

                sensorValues[9] = transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity).z;

                return sensorValues;
            }
        }

        // Start is called before the first frame update
        void Awake()
        {
            distanceTravelled = 0;

            var extents = transform.GetChild(0).gameObject.GetComponent<Renderer>().bounds.extents;

            if (showSensors)
            {
                foreach (var sensor in sensors)
                {
                    sensor.GetComponent<LineRenderer>().startColor = Color.green;
                    sensor.GetComponent<LineRenderer>().endColor = Color.green;
                }
            }
        }

        public void Step(float[] output)
        {
            if (humanControlled || isRunning)
            {
                // Controls
                float horizontalControl = 0, verticalControl = 0;
                float brakeControl = 0;
                var localVelocity = transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity);

                if (humanControlled)
                {
                    horizontalControl = Input.GetAxis("Horizontal");
                    verticalControl = Input.GetAxis("Vertical");
                    brakeControl = Input.GetKey(KeyCode.Space) ? 1 : 0;
                }
                else if (isRunning)
                {
                    horizontalControl = Mathf.Lerp(-1, 1, output[0]);
                    verticalControl = Mathf.Lerp(-1, 1, output[1]);

                    if ((verticalControl < 0 && localVelocity.z > 1) || (verticalControl > 0 && localVelocity.z < -1))
                        brakeControl = 1;

                    // Accumulate distance
                    distanceTravelled += localVelocity.z * Time.fixedDeltaTime;

                    // Idle timer
                    if (localVelocity.z <= .5)
                    {
                        idleTime += Time.fixedDeltaTime;
                        if (idleTime > 2)
                            Stop();
                    }
                    else
                    {
                        idleTime = 0;
                    }
                }

                // Physics
                for (int i = 0; i < 2; i++)
                {
                    // Motor
                    wheelColliders[i].motorTorque = localVelocity.z <= 20 ? verticalControl * motorForce : 0;

                    // Steering
                    wheelColliders[i].steerAngle = maxSteeringAngle * horizontalControl;
                }

                // Braking
                foreach (var wheelCollider in wheelColliders)
                    wheelCollider.brakeTorque = brakeControl * brakeForce;

                // Wheel Poses
                for (int i = 0; i < 4; i++)
                {
                    Vector3 position;
                    Quaternion rotation;

                    wheelColliders[i].GetWorldPose(out position, out rotation);
                    wheelTransforms[i].rotation = rotation;
                    wheelTransforms[i].position = position;
                }

                // Fitness text
                fitnessText.text = Fitness.ToString("0.00");
                if (Fitness > Main.Instance.highestFitness)
                    Main.Instance.highestFitness = Fitness;
            }
        }

        void OnCollisionEnter(Collision collider)
        {
            if (collider.gameObject.layer == 9)
                Stop();
        }

        public void Reinit()
        {
            distanceTravelled = 0;

            isRunning = true;
            gameObject.SetActive(true);
            idleTime = 0;

            Main.Instance.survivors++;
        }

        public void Stop()
        {
            isRunning = false;
            gameObject.SetActive(false);
            Main.Instance.survivors--;
        }
    }
}