using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnvScript : MonoBehaviour
{
    [Tooltip("The agent inside the area")]
    public RLEngine carAgent;

    //public GameObject[] sideWall = GameObject.FindGameObjectsWithTag("wall");

    [Tooltip("The TextMeshPro text that shows the cumulative reward of the agent")]
    public TextMeshPro cumulativeRewardText;

    public void ResetArea()
    {
        PlaceCar();

        /*
        carAgent.transform.position = carAgent.startPosition;
        carAgent.transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(-50f, 120f), 0f);
        carAgent.currentSpeed = 0f;
        carAgent.wheelFL.steerAngle = 0f;
        carAgent.wheelFR.steerAngle = 0f;
        carAgent.targetSteerAngle = 0f;
        carAgent.wheelFL.motorTorque = 0f;
        carAgent.wheelFR.motorTorque = 0f;
        carAgent.distance = 0f;
        */
    }

    private void PlaceCar()
    {
        Rigidbody rigidbody = carAgent.GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        carAgent.transform.position = carAgent.startPosition;
        carAgent.transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 60f), 0f);
    }


    private void Start()
    {
        ResetArea();
        carAgent.startPosition = carAgent.transform.position;
    }

    private void Update()
    {
        cumulativeRewardText.text = carAgent.GetCumulativeReward().ToString("0.00");
    }



}

