using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class CubeAgentRays : Agent
{
    public Transform Target;
    public float speedMultiplier = 0.5f;
    public float rotationMultiplier = 5;
    private bool targetTouched = false;

    public override void OnEpisodeBegin()
    {
        // reset de positie en orientatie als de agent gevallen is
        if (this.transform.localPosition.y < 0)
        {
            this.transform.localPosition = new Vector3(0.6f, 0.6f, 0);
            this.transform.localRotation = Quaternion.identity;
        }

        targetTouched = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent positie
        sensor.AddObservation(this.transform.localPosition);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");
        continuousActionsOut[1] = Input.GetAxis("Horizontal");
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Acties, 
        Vector3 controlSignal = Vector3.zero;
        controlSignal.z = actionBuffers.ContinuousActions[0];
        transform.Translate(controlSignal * speedMultiplier);
        transform.Rotate(0.0f, rotationMultiplier * actionBuffers.ContinuousActions[1], 0.0f);

        // Beloningen
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        // target bereikt
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);
            EndEpisode();
            targetTouched = true;
           // Debug.Log("Distance smaller than...rewarded");

        }
        if (targetTouched)
        {
            Collider[] colliders = Physics.OverlapSphere(Target.position, 0.5f);
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.CompareTag("GreenFloor") || this.transform.localPosition.x < 0)
                {
                    //Debug.Log("GreenFloor COLLISION");
                    SetReward(1.0f);
                    Target.localPosition = new Vector3((Random.value * 3 + 1), 0.5f, Random.value * 8 - 4);
                    targetTouched = false;
                    break;
                }
                else
                {
                    Target.localPosition = new Vector3(100, 0.5f, Random.value * 8 - 4);
                }
            }
            if (this.transform.localPosition.y < 0)
            {
               // SetReward(-0.25f);
                this.transform.localPosition = new Vector3(0.6f, 0.6f, 0);
               // this.transform.localPosition = new Vector3((Random.value * 3 + 1), 0.5f, Random.value * 8 - 4);
                this.transform.localRotation = Quaternion.identity;
                //Debug.Log("Punished for falling");
            }
        }
    

        // Van het platform gevallen?
        else if (this.transform.localPosition.y < 0)
        {
            //Debug.Log("Fell down");
            EndEpisode();
        }

    }

}