using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public enum Axle
    {
        Front,
        Rear,
    }

    [Serializable]

    public struct Wheel
    {
        public WheelCollider wheelCollider;
        public Transform wheelMesh;
        public Axle axle;
    }

    [SerializeField] private Transform mastTransform;
    [SerializeField] private Transform forkTransform;
    [SerializeField] private Transform lifterTransform;

    public List<Wheel> wheels;

    public float motorForce = 6000;
    public float maxAcceleration = 50.0f;
    public float brakeAcceleration = 150.0f;


    [Header("Forklift Properties")]
    public float steeringSensitivity = 1.0f;
    public float maxSteeringAngle = 30.0f; 
    private float minMastAngle = -10f;
    private float maxMastAngle = 10f;
    private float currentRotation = 0f;

    private float forkSpeed = 0.1f;
    private float maxForkHeight = 0.07f;
    private float minForkHeight = 0f;

    private float moveInput;
    private float steerInput;
    private Rigidbody rb;
    private Vector3 centerOfMass;
    private Vector3 gravity;

    private Vector3 forkPos;
    private Vector3 lifterPos;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass;
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        DriveForce();
        Steering();
        Braking();
        WheelTransformUpdate();


        //set height
        forkPos = forkTransform.localPosition;
        lifterPos = lifterTransform.localPosition;

        if (Input.GetKey(KeyCode.E))
        {
            forkPos.y += forkSpeed * 0.5f * Time.deltaTime;
            forkPos.y = Mathf.Clamp(forkPos.y, minForkHeight, maxForkHeight);
            forkTransform.localPosition = forkPos;

            lifterPos.y += forkSpeed * 0.25f * Time.deltaTime;
            lifterPos.y = Mathf.Clamp(lifterPos.y, minForkHeight, maxForkHeight / 2);
            lifterTransform.localPosition = lifterPos;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            forkPos.y -= forkSpeed * 0.5f * Time.deltaTime;
            forkPos.y = Mathf.Clamp(forkPos.y, 0f, maxForkHeight);
            forkTransform.localPosition = forkPos;

            lifterPos.y -= forkSpeed * 0.25f * Time.deltaTime;
            lifterPos.y = Mathf.Clamp(lifterPos.y, minForkHeight, maxForkHeight / 2);
            lifterTransform.localPosition = lifterPos;
        }

        //rotate mast
        currentRotation = Mathf.Clamp(currentRotation, minMastAngle, maxMastAngle);

        if (Input.GetKey(KeyCode.C))
        {
            currentRotation += 3f * Time.deltaTime;
            mastTransform.localRotation = Quaternion.Euler(currentRotation,mastTransform.localRotation.eulerAngles.y, mastTransform.localRotation.eulerAngles.z);
        }
        if (Input.GetKey(KeyCode.V))
        {
            currentRotation -= 3f * Time.deltaTime;
            mastTransform.localRotation = Quaternion.Euler(currentRotation, mastTransform.localRotation.eulerAngles.y, mastTransform.localRotation.eulerAngles.z);
        }
    }

    private void Braking()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            foreach (Wheel wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 300 * brakeAcceleration * Time.deltaTime;
                Debug.Log("Braking");
            }
        }
        else
        {
            foreach (Wheel wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 0;
            }
        }
    }

    private void WheelTransformUpdate()
    {
        foreach (Wheel wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;

            wheel.wheelCollider.GetWorldPose(out pos, out rot);

            wheel.wheelMesh.position = pos;
            wheel.wheelMesh.rotation = rot;

        }
    }
    private void Steering()
    {
        foreach (Wheel wheel in wheels)
        {
            if (wheel.axle == Axle.Rear)
            {
                float _steerAngle = -steerInput * steeringSensitivity * maxSteeringAngle;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f);
            }
        }
    }

    private void GetInputs()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }

    private void DriveForce()
    {

        foreach (Wheel wheel in wheels)
        {
            if (wheel.axle == Axle.Front)
            {
                wheel.wheelCollider.motorTorque = moveInput * motorForce * maxAcceleration * Time.deltaTime;
            }
        }
    }
}
