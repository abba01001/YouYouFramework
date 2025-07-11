using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Cashier : MonoBehaviour
{
    private Transform cashierPos;
    private NavMeshAgent agent;
    public Animator anim;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        cashierPos = FindObjectOfType<BillingDesk>().cashierPos;
        agent.SetDestination(cashierPos.position);
    }

    private void Update()
    {
        if (ReachedDestination())
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);

            Destroy(this);
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
            anim.SetBool("Run", false);
        else
            anim.SetBool("Run", true);
    }

    private bool ReachedDestination()
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    return true;
            }
        }

        return false;
    }
}
