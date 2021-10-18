using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace NeuroGen
{
    public class Checkpoint : MonoBehaviour
    {
        public LayerMask layerMask;
        public bool isFinish = false;
        public bool isFitnessPoint = false;
        public float fitnessWhenTouched = 5000;

        public bool isTeleport = false;
        public Transform teleportPosition;

        public List<CarController> crossedBy = new List<CarController>();

        public void Teleport(Transform target)
        {
            target.transform.position = teleportPosition.position;
            target.transform.rotation = teleportPosition.rotation;
            target.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }
}