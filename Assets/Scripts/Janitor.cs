using UnityEngine;
using UnityEngine.AI;

public class Janitor : MonoBehaviour
{
    [SerializeField] Transform storePoint;

    private NavMeshAgent agent;
    private Animator anim;

    private WorkerState currentState;
    private StockBoxController currentBox;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
    }

    void Start()
    {
        currentState = WorkerState.WalkingToStore;
        agent.SetDestination(storePoint.position);
    }

    void Update()
    {
        anim.SetFloat("Speed", agent.velocity.magnitude);

        switch(currentState)
        {
            case WorkerState.WalkingToStore: 
                if(!ReachedDestination())
                    break;

                break;

            case WorkerState.Working: 
                

                break;
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
        // currentState = CashierState.WalkingToStore;

        // agent.Warp(CustomerManager.instance.GetRandomSpawnPoint().position);
        // agent.SetDestination(checkout.GetCashierPoint().position);
    }

    private void EndDay()
    {
        // currentState = CashierState.WalkingToStore;
        // agent.SetDestination(CustomerManager.instance.GetRandomSpawnPoint().position);
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
