using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPhysics : MonoBehaviour {
    [SerializeField] private List<Transform> suspensionPointList;
    [SerializeField] private Transform carBase;

    [SerializeField] private float suspensionRestDist = 0.7f;
    [SerializeField] private float springStrength = 100f;
    [SerializeField] private float damping = 10f;

    [SerializeField] private float tireMass = 1f;

    [SerializeField] private float spinVelocity = 10f;
    [SerializeField] private float moveVelocity = 10f;

    [SerializeField] private float maxMoveVelocity = 12f;
    [SerializeField] private float maxSpinVelocity =0.6f;


    private Rigidbody carRigidBody;

    private RaycastHit rayHit;
    private bool didRayHit = false;

    private void Start() {
        carRigidBody = carBase.GetComponent<Rigidbody>();

    }

    private void FixedUpdate() {
        for (int i = 0; i < suspensionPointList.Count; i ++) {
            HandleSuspension(suspensionPointList[i]);
        }

        HandleSteering(suspensionPointList[0], 0.5f);
        HandleSteering(suspensionPointList[1], 0.5f);
        HandleSteering(suspensionPointList[2], 0.2f);
        HandleSteering(suspensionPointList[3], 0.2f);

        HandleMovement();
    }

    private void HandleSuspension(Transform suspensionPoint) {
        Ray ray = new Ray(suspensionPoint.position, carBase.TransformDirection(Vector3.down));
        didRayHit = Physics.Raycast(ray, out rayHit, suspensionRestDist);

        if (didRayHit) {
            Debug.DrawRay(suspensionPoint.position, carBase.TransformDirection(Vector3.down) * suspensionRestDist, Color.green);

            Vector3 springDirection = suspensionPoint.up;

            Vector3 tireWorldVel = carRigidBody.GetPointVelocity(suspensionPoint.position);

            float offset = suspensionRestDist - rayHit.distance;

            float velocity = Vector3.Dot(springDirection, tireWorldVel);

            float force = (offset * springStrength) - (velocity * damping) * carRigidBody.mass;

            carRigidBody.AddForceAtPosition(springDirection * force, suspensionPoint.position);
        }
    }

    private void HandleSteering(Transform suspensionPoint, float tireGripFactor) {
        Ray ray = new Ray(suspensionPoint.position, carBase.TransformDirection(Vector3.down));
        didRayHit = Physics.Raycast(ray, out rayHit, suspensionRestDist);

        if (didRayHit) {
            Vector3 steeringDirection = suspensionPoint.TransformDirection(Vector3.right);

            Vector3 tireWorldVel = carRigidBody.GetPointVelocity(suspensionPoint.position);

            float steeringVelocity = Vector3.Dot(steeringDirection, tireWorldVel);

            float desiredChange = -steeringVelocity * tireGripFactor;

            float desiredAcceleration = desiredChange / Time.fixedDeltaTime;

            carRigidBody.AddForceAtPosition(steeringDirection * tireMass * desiredAcceleration, suspensionPoint.position);
        }
    }

    private void HandleMovement() {
        Vector2 inputVector = InputManager.Instance.GetInputVectorNormalized();

        if (inputVector.y < 0) {
            Debug.Log("goingBack");
            Steering(suspensionPointList[2]);
            Steering(suspensionPointList[3]);
        } else {
            Steering(suspensionPointList[0]);
            Steering(suspensionPointList[1]);
        }

        Movement(suspensionPointList[2]);
        Movement(suspensionPointList[3]);
        
    }

    private void Steering(Transform suspensionPoint) {
        Vector2 inputVector = InputManager.Instance.GetInputVectorNormalized();

        Vector3 steeringDirection = Vector3.zero;
        if (inputVector.x < 0) {
            steeringDirection = suspensionPoint.TransformDirection(Vector3.left); // use transform direction
        } else if (inputVector.x > 0) {
            steeringDirection = suspensionPoint.TransformDirection(Vector3.right);
        }

        float spinSpeed = Vector3.Dot(steeringDirection, carRigidBody.velocity);

        float spin = spinVelocity;

        if (spinSpeed > maxSpinVelocity) {
            spin = 0f;
        } else {
            spin = spinVelocity;
        }

        if (DidRayHit()) {
            carRigidBody.AddForceAtPosition(steeringDirection * tireMass * spin, suspensionPoint.position);
        }
    }

    private void Movement(Transform suspensionPoint) {
        Vector2 inputVector = InputManager.Instance.GetInputVectorNormalized();

        Vector3 moveDirection = Vector3.zero;

        if (inputVector.y < 0) {
            moveDirection = suspensionPoint.TransformDirection(Vector3.back);
        } else if (inputVector.y > 0) {
            moveDirection = suspensionPoint.TransformDirection(Vector3.forward);
        }

        float carSpeed = Vector3.Dot(moveDirection, carRigidBody.velocity);

        float move = moveVelocity;

        if (carSpeed > maxMoveVelocity) {
            move = 0;
        } else {
            move = moveVelocity;
        }

        if (DidRayHit()) {
            carRigidBody.AddForceAtPosition(moveDirection * tireMass * move, suspensionPoint.position);
        }

    }

    private bool DidRayHit() {
        for (int i = 0; i < suspensionPointList.Count; i++) {
            Ray ray = new Ray(suspensionPointList[i].position, carBase.TransformDirection(Vector3.down));
            didRayHit = Physics.Raycast(ray, out rayHit, suspensionRestDist);

            if (didRayHit) {
                return true;
            }
        }
        return false;
    }

}
