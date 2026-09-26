using UnityEngine;
using UnityEngine.AI;

public enum WorkerState
{
    WalkingToStore,
    Working,
    GoingHome
}

public class Cashier : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;

    private Checkout checkout;
    private WorkerState currentState;

    public float paymentProccesingTime;
    public float customerWaitTime;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        checkout = FindAnyObjectByType<Checkout>();
    }

    void Update()
    {
        anim.SetFloat("Speed", agent.velocity.magnitude);

        switch(currentState)
        {
            case WorkerState.WalkingToStore: 
                if(!ReachedDestination())
                    break;

                checkout.SetCashier(this);
                currentState = WorkerState.Working;
                break;

            case WorkerState.Working: break;
            case WorkerState.GoingHome: break;
        }
    }

    private bool ReachedDestination()
    {
        if (agent.pathPending)
            return false;

        return agent.remainingDistance <= agent.stoppingDistance;
    }

    private void StartDay()
    {
        currentState = WorkerState.WalkingToStore;

        agent.Warp(CustomerManager.instance.GetRandomSpawnPoint().position);
        agent.SetDestination(checkout.GetCashierPoint().position);
    }

    private void EndDay()
    {
        checkout.SetCashier(null);

        currentState = WorkerState.GoingHome;
        agent.SetDestination(CustomerManager.instance.GetRandomSpawnPoint().position);
    }

    private void OnEnable()
    {
        DayManager.OnWorkingDayStart += StartDay;
        DayManager.OnDayEnd += EndDay;
    }

    private void OnDisable()
    {
        DayManager.OnWorkingDayStart -= StartDay;
        DayManager.OnDayEnd -= EndDay;
    }
}
