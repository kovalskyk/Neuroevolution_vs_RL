using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.IO;


public class RLEngine : Agent
{ 
    public Vector3 startPosition, startRotation;

    public float maxSteerAngle = 45f;
    public float turnSpeed = 100f;
    public float maxMotorTorque = 500f;
    public float maxBrakeTorque = 5000f;
    public float currentSpeed;
    public float maxSpeed = 1000f;
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;
    public Vector3 centerOfMass;
    public bool isBraking = false;
    public Texture2D textureNormal;
    public Texture2D textureBraking;
    public Renderer carRenderer;

    public float targetSteerAngle = 0f;

    private EnvScript circuitArea;
    new private Rigidbody rigidbody;
    //private GameObject[] walls;
    private List<GameObject> walls;

    public float distance = 0f;
    private Vector3 lastPosition;

    public float diff;

    public Rigidbody rb;

    public Vector3 localVel;

    List<float> speedList;

    private bool writeSpeeds = false;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        //base.Initialize();
        GetComponent<Rigidbody>().centerOfMass = centerOfMass;
        //walls = new List<GameObject>(GameObject.FindGameObjectsWithTag("wall"));
        circuitArea = GetComponentInParent<EnvScript>();
        rigidbody = GetComponent<Rigidbody>();
        

        startPosition = transform.position;
        startRotation = transform.eulerAngles;

        speedList = new List<float>();
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        float forwardAmount = Mathf.Clamp(vectorAction[0], 0, 1);

        float turnAmount = Mathf.Clamp(vectorAction[1], -1, 1);

        //float braking = Mathf.Clamp(vectorAction[2], 0, 1);

        //float forwardAmount = vectorAction[0];
        //float turnAmount = 0f;

        //if (vectorAction[1] == 1f)
        //{
        //    turnAmount = -1f;
        //}
        //else if (vectorAction[1] == 2f)
        //{
        //    turnAmount = 1f;
        //}


        if (localVel.z < 0.2f && forwardAmount < 0.1f)
        {
            forwardAmount = 1f;
        }

        wheelFL.motorTorque = forwardAmount * maxMotorTorque * 5f;
        wheelFR.motorTorque = forwardAmount * maxMotorTorque * 5f;

        targetSteerAngle = turnAmount * maxSteerAngle;

        //wheelFL.steerAngle = Mathf.Lerp(wheelFL.steerAngle, targetSteerAngle, Time.fixedDeltaTime * turnSpeed);
        //wheelFR.steerAngle = Mathf.Lerp(wheelFR.steerAngle, targetSteerAngle, Time.fixedDeltaTime * turnSpeed);

        wheelFL.steerAngle = targetSteerAngle;
        wheelFR.steerAngle = targetSteerAngle;

        //if (braking > 0.5f)
        //{
        //    carRenderer.material.mainTexture = textureBraking;
        //    wheelFL.brakeTorque = maxBrakeTorque;
        //    wheelFR.brakeTorque = maxBrakeTorque;
        //}
        //else
        //{
        //    carRenderer.material.mainTexture = textureNormal;
        //    wheelFL.brakeTorque = 0;
        //    wheelFR.brakeTorque = 0;
        //}


        if (MaxStep > 0) AddReward(-1f / MaxStep);

        AddReward(localVel.z/2000f);
    }

    public override void OnEpisodeBegin()
    {
        //base.OnEpisodeBegin();
        circuitArea.ResetArea();
        lastPosition = transform.position;
        distance = 0f;
        diff = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {   
        sensor.AddObservation(localVel);
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(wheelFL.steerAngle);
        sensor.AddObservation(wheelFR.steerAngle);
    }


    private void FixedUpdate()
    {
        if (StepCount % 4 == 0)
        {
            RequestDecision();
        } else
        {
            RequestAction();
        }

        currentSpeed = rb.velocity.magnitude;


        diff = Vector3.Distance(transform.position, lastPosition);
        distance += Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;

        localVel = transform.InverseTransformDirection(rigidbody.velocity);

        if (writeSpeeds == true)
        {
            speedList.Add(currentSpeed);
        }

        if (Input.GetKey(KeyCode.S))
        {
            writeSpeeds = true;
        }
    }


    public void WallHit()
    {
        AddReward(-1f);
        EndEpisode();
        distance = 0f;
        diff = 0f;
    }

    public void CheckpointHit()
    {
        AddReward(0.2f); // Increase Fitness/Score
    }


    public override void Heuristic(float[] actionsOut)
    {
        actionsOut[0] = 0f;
        actionsOut[1] = 0f;
       // actionsOut[2] = 0f;

        if (Input.GetKey(KeyCode.W))
        {
            actionsOut[0] = 1;
        } else if (Input.GetKey(KeyCode.S))
        {
            actionsOut[0] = -1;
        }

        if (Input.GetKey(KeyCode.A))
        { 
            actionsOut[1] = -1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            actionsOut[1] = 1;
        }
    }

    private void OnApplicationQuit()
    {
        string textToWrite = "SPEED DATA" + "\r\n";
        foreach (float data in speedList)
        {
            textToWrite = textToWrite + data + "\n";
        }
        File.AppendAllText("speed.txt", textToWrite);
    }
}

