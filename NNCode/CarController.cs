using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[RequireComponent(typeof(NNet))]
public class CarController : MonoBehaviour
{ 

    private Vector3 startPosition, startRotation;
    private NNet network;

    [Range(-1f,1f)]
    public float a,t;

    public float timeSinceStart = 0f;

    [Header("Fitness")]
    public float overallFitness;
    public float distanceMultipler = 1.4f;
    public float avgSpeedMultiplier = 0.2f;
    public float sensorMultiplier = 0.1f;

    [Header("Network Options")]
    public int LAYERS = 1;
    public int NEURONS = 10;

    private Vector3 lastPosition;
    private float totalDistanceTravelled;
    private float avgSpeed;
    private float distanceTravelled;
    private float currentDistance;
    private float startDistance;

    private float aSensor,bSensor,cSensor, dSensor, eSensor;


    [Header("Car Options")]
    public float maxSteerAngle = 45f;
    public float turnSpeed = 20f;
    public float maxMotorTorque = 500f;
    public float maxBrakeTorque = 150f;
    public float currentSpeed;
    public float maxSpeed = 10000f;
    public WheelCollider wheelFL;
    public WheelCollider wheelFR;
    public WheelCollider wheelRL;
    public WheelCollider wheelRR;
    public Vector3 centerOfMass;
    public bool isBraking = false;
    public Texture2D textureNormal;
    public Texture2D textureBraking;
    public Renderer carRenderer;

    private float targetSteerAngle = 0;

    public Rigidbody rb;

    private bool writeSpeeds = false;

    List<float> speedList;
    List<float> fitnessList;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        GetComponent<Rigidbody>().centerOfMass = centerOfMass;

        speedList = new List<float>();
        fitnessList = new List<float>();

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        startPosition = transform.position;
        startRotation = transform.eulerAngles;
        network = GetComponent<NNet>();

        wheelFL.steerAngle = 0f;
        wheelFR.steerAngle = 0f;

        wheelFL.motorTorque = 0f;
        wheelFR.motorTorque = 0f;
    }

    public void ResetWithNetwork (NNet net)
    {
        network = net;
        Reset();
    }

    

    public void Reset() {

        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        avgSpeed = 0f;
        lastPosition = startPosition;
        overallFitness = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
        currentSpeed = 0f;
        wheelFL.steerAngle = 0f;
        wheelFR.steerAngle = 0f;
        targetSteerAngle = 0f;
        wheelFL.motorTorque = 0f;
        wheelFR.motorTorque = 0f;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

    }

    private void OnCollisionEnter(Collision collision) {

           Death();
      

        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        avgSpeed = 0f;
        lastPosition = startPosition;
        overallFitness = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
        currentSpeed = 0f;
        wheelFL.steerAngle = 0f;
        wheelFR.steerAngle = 0f;
        targetSteerAngle = 0f;
        wheelFL.motorTorque = 0f;
        wheelFR.motorTorque = 0f;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private void FixedUpdate() {

        InputSensors();
        lastPosition = transform.position;


        (a, t) = network.RunNetwork(aSensor, bSensor, cSensor, dSensor, eSensor);


        MoveCar(a,t);
        //ApplySteer();
        //LerpToSteerAngle();

        timeSinceStart += Time.fixedDeltaTime;

        CalculateFitness();

        //a = 0;
        //t = 0;

        if (writeSpeeds == true) { 
        speedList.Add(currentSpeed);
        }

        if (Input.GetKey(KeyCode.S))
        {
            writeSpeeds = true;
        }
    }

    private void Death ()
    {
        fitnessList.Add(overallFitness);
        GameObject.FindObjectOfType<GeneticManager>().Death(overallFitness, network);
        
    }

    private void CalculateFitness() {

        currentDistance = Vector3.Magnitude(transform.position);
        startDistance = Vector3.Magnitude(lastPosition);
        distanceTravelled = startDistance + currentDistance;

        totalDistanceTravelled += Vector3.Distance(startPosition, transform.position)/10;
        avgSpeed = totalDistanceTravelled/timeSinceStart;

  
        overallFitness = (totalDistanceTravelled*distanceMultipler)+(avgSpeed*avgSpeedMultiplier)+(((aSensor+bSensor+cSensor+dSensor+eSensor)/5)*sensorMultiplier);



        if (timeSinceStart > 20 && overallFitness < 350) {
            Death();
        }

        if (overallFitness >= 100000) {
            Death();
        }



    }

    private void InputSensors() {

        Vector3 a = (transform.forward+transform.right);
        Vector3 b = (transform.forward);
        Vector3 c = (transform.forward-transform.right);
        Vector3 d = (-transform.right);
        Vector3 e = (transform.right);

        Ray r = new Ray(transform.position,a);
        RaycastHit hit;

       

        if (Physics.Raycast(r, out hit)) {
            aSensor = hit.distance/20;
            Debug.DrawLine(r.origin, hit.point, Color.red);
            
        }

        r.direction = b;

        if (Physics.Raycast(r, out hit)) {
            bSensor = hit.distance/20;
            Debug.DrawLine(r.origin, hit.point, Color.red);
            
        }

        r.direction = c;

        if (Physics.Raycast(r, out hit)) {
            cSensor = hit.distance/20;
            Debug.DrawLine(r.origin, hit.point, Color.red);
            
        }

        r.direction = d;

        if (Physics.Raycast(r, out hit))
        {
            dSensor = hit.distance / 20;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

        r.direction = e;

        if (Physics.Raycast(r, out hit))
        {
            eSensor = hit.distance / 20;
            Debug.DrawLine(r.origin, hit.point, Color.red);
        }

    }

    //private Vector3 inp;
    public void MoveCar (float v, float h) {
        // inp = Vector3.Lerp(Vector3.zero,new Vector3(0,0,v*11.4f),0.02f);
        // inp = transform.TransformDirection(inp);
        // transform.position += inp;

        // transform.eulerAngles += new Vector3(0, (h*90)*0.02f,0);

        // Vector3 futurePosition = transform.position + inp;

        currentSpeed = rb.velocity.magnitude;

        //if (currentSpeed < maxSpeed && !isBraking)
        //{
        wheelFL.motorTorque = v * maxMotorTorque;
        wheelFR.motorTorque = v * maxMotorTorque;
       // }
        //else
        //{
          //  wheelFL.motorTorque = 0;
            //wheelFR.motorTorque = 0;
        //}

        //if (avoiding) return;
        //Vector3 relativeVector = transform.InverseTransformPoint(nodes[currectNode].position);
        //float newSteer = (relativeVector.x / relativeVector.magnitude) * maxSteerAngle;
        //targetSteerAngle = newSteer;

        targetSteerAngle = h * maxSteerAngle;

        //wheelFL.steerAngle = Mathf.Lerp(wheelFL.steerAngle, targetSteerAngle, Time.deltaTime * turnSpeed);
        //wheelFR.steerAngle = Mathf.Lerp(wheelFR.steerAngle, targetSteerAngle, Time.deltaTime * turnSpeed);

        wheelFL.steerAngle = targetSteerAngle;
        wheelFR.steerAngle = targetSteerAngle;
    }

    private void OnApplicationQuit()
    {
        string textToWrite = "SPEED DATA" + "\r\n";
        foreach (float data in speedList)
        {
            textToWrite = textToWrite + data + "\n";
        }
        File.AppendAllText("speedGA.txt", textToWrite);


        string anotherTextToWrite = "FITNESS DATA" + "\r\n";
        foreach (float fitnes in fitnessList)
        {
            anotherTextToWrite = anotherTextToWrite + fitnes + "\n";
        }
        File.AppendAllText("fitnessGA.txt", anotherTextToWrite);
    }
}
