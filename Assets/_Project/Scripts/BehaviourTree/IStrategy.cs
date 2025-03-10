using System;
using System.Collections.Generic;
using BlackboardSystem;
using UnityEngine;
using UnityEngine.AI;

public interface IStrategy
{
    Node.Status Process();

    void Reset() {
        // Noop
    }
}

public class ActionStrategy : IStrategy
{
    readonly Action doSomething;

    public ActionStrategy(Action doSomething) {
        this.doSomething = doSomething;
    }

    public Node.Status Process() {
        doSomething();
        return Node.Status.Success;
    }
}

public class Condition : IStrategy
{
    readonly Func<bool> predicate;

    public Condition(Func<bool> predicate) {
        this.predicate = predicate;
    }

    public Node.Status Process() => predicate() ? Node.Status.Success : Node.Status.Failure;
}

public class MoveToTarget : IStrategy
{
    readonly Transform entity;
    readonly NavMeshAgent agent;
    readonly Transform target;
    readonly float speed;
    readonly float reachDistance;
    bool hasLookedAtTarget;
    // bool isPathCalculated;

    public MoveToTarget(Transform entity, NavMeshAgent agent, Transform target, float speed = 2,
        float                     reachDistance = 0f) {
        this.entity = entity;
        this.agent = agent;
        this.target = target;
        this.speed = speed;
        this.reachDistance = reachDistance;
    }

    public Node.Status Process() {
        if (Vector3.Distance(entity.position, target.position) < reachDistance) {
            agent.ResetPath();
            agent.isStopped = true;

            // Check, ob LookAt bereits einmal ausgeführt wurde
            if (!hasLookedAtTarget) {
                entity.LookAt(target);
                hasLookedAtTarget = true; // Flag setzen, um weitere Aufrufe zu verhindern
            }

            return Node.Status.Success;
        }

        agent.speed = speed;
        agent.stoppingDistance = reachDistance;
        agent.SetDestination(target.position);
        entity.LookAt(target);

        if (agent.pathPending) {
            // isPathCalculated = true;
            hasLookedAtTarget = false; // Setze das Flag zurück, wenn die Distanz groß genug ist
        }

        return Node.Status.Running;
    }

    // public void Reset() => isPathCalculated = false;
}

public class ExploreStrategy : IStrategy
{
    readonly Transform entity;
    readonly NavMeshAgent agent;
    readonly Animator animator;
    readonly List<ObjectDetector> detectors;
    private readonly float exploreSpeed;
    readonly float exploreRadius;
    readonly float pauseDuration;
    readonly float intervalBetweenPauses;
    
    private bool isWaiting = false; // Status für Wartezeit
    private float waitStartTime;    // Startzeit der aktuellen Wartezeit
    private float waitDuration;     // Dauer der Wartezeit

    private float elapsedTimeSinceLastPause;
    private float pauseEndTime;
    private float detectionRadiusMultiplier;
    private bool isPausing;
    private Blackboard blackboard;
    private BlackboardKey foodBowlInReachKey;
    private BlackboardKey waterBowlInReachKey;
    
    protected static readonly int IdleHash = Animator.StringToHash("Idle_2");
    protected static readonly int WalkHash = Animator.StringToHash("Walk_Fwd");
    protected static readonly int TrotHash = Animator.StringToHash("Trot_Fwd");
    protected static readonly int RunHash = Animator.StringToHash("Run_Fwd");
    protected static readonly int RunFastHash = Animator.StringToHash("RunFast_Fwd");
    protected static readonly int PickUp = Animator.StringToHash("PickUp");
    
    protected const float crossFadeDuration = 0.1f;

    private readonly Dictionary<ObjectDetector, float> originalRadius = new();

    public ExploreStrategy(Transform entity, NavMeshAgent agent, Animator animator, Blackboard blackboard, List<ObjectDetector> detectors, float exploreSpeed,
        float exploreRadius, float intervalBetweenPauses = 15f, float pauseDuration = 3f, float detectionRadiusMultiplier = 2f) {
        this.entity = entity;
        this.agent = agent;
        this.animator = animator;
        this.blackboard = blackboard;
        this.detectors = detectors;
        this.exploreSpeed = exploreSpeed;
        agent.speed = exploreSpeed;
        this.exploreRadius = exploreRadius;
        this.pauseDuration = pauseDuration;
        this.intervalBetweenPauses = intervalBetweenPauses;
        this.detectionRadiusMultiplier = detectionRadiusMultiplier;

        elapsedTimeSinceLastPause = 0f;
        isPausing = false;

        // Speichert die Ursprungsradien der Detektoren
        foreach (var detector in detectors) {
            originalRadius[detector] = detector.GetRadius();
        }
    }

    public Node.Status Process() {
        elapsedTimeSinceLastPause += Time.deltaTime;
        animator.CrossFade(WalkHash, crossFadeDuration);

        // Checkt, ob es Zeit für eine Pause ist
        if (elapsedTimeSinceLastPause >= intervalBetweenPauses) {
            if (!isPausing) {
                StartPause();
            }

            // Während der Pause nicht weiterlaufen
            if (Time.time >= pauseEndTime) {
                EndPause();
            }
            else {
                return Node.Status.Running;
            }
        }

        // Exploration-Logik, wenn nicht pausiert wird
        if (!isPausing && !agent.pathPending && HasReachedDestination()) {
            SetNewDestination();
        }

        return Node.Status.Running;
    }

    private void StartPause() {
        Debug.Log("ExploreStrategy: Starting pause for detector scan.");
        isPausing = true;
        pauseEndTime = Time.time + pauseDuration;
        agent.speed = 0f;
        animator.CrossFade(IdleHash, crossFadeDuration);

        // Verdoppelt den Radius aller Detektoren
        foreach (var detector in detectors) {
            detector.SetRadius(originalRadius[detector] * detectionRadiusMultiplier);
            // if (detector.CanDetectObject()) {
            //     if (blackboard.TryGetValue(foodBowlInReachKey, out bool foodInReach)) {
            //         blackboard.SetValue(foodBowlInReachKey, !foodInReach);
            //         Debug.Log($"FoodInReach: {foodInReach}");
            //     }
            //     
            //     isPausing = false;
            //     agent.speed = exploreSpeed;
            //     elapsedTimeSinceLastPause = 0f;
            //     agent.SetDestination(detector.ObjectToDetect.position);
            //     detector.SetRadius(originalRadius[detector]);
            // }
        }
    }

    private void EndPause() {
            Debug.Log("ExploreStrategy: Ending pause, resetting detector radii.");
            isPausing = false;
            agent.speed = exploreSpeed;
            elapsedTimeSinceLastPause = 0f;
            animator.CrossFade(WalkHash, crossFadeDuration);

            // Setze die Radien der Detektoren auf ihre Ursprungswerte zurück
            foreach (var detector in detectors) {
                if (detector.CanDetectObject()) agent.SetDestination(detector.ObjectToDetect.position);
                else SetNewDestination();
                detector.SetRadius(originalRadius[detector]);
            }
            //
            // // Setze eine neue Destination für die weitere Exploration
            // SetNewDestination();
        }

        private void SetNewDestination() {
            var randomDirection = UnityEngine.Random.insideUnitSphere * exploreRadius;
            randomDirection += entity.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, exploreRadius, NavMesh.AllAreas)) {
                agent.SetDestination(hit.position);
            }
        }

        private bool HasReachedDestination() {
            return agent.remainingDistance <= agent.stoppingDistance &&
                   (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f);
        }
        
        private void StartRandomWait()
        {
            waitDuration = UnityEngine.Random.Range(0.5f, 2f); // Generiere Zufallszeit zwischen 0.5 und 2 Sekunden
            waitStartTime = Time.time;                         // Starte die Uhr
            isWaiting = true;                                  // Aktiviere den Wartezustand
            animator.CrossFade(IdleHash, crossFadeDuration);   // Animation für die Pause
            Debug.Log($"ExploreStrategy: Start random wait for {waitDuration} seconds.");
        }
        
        private bool IsWaitTimeOver()
        {
            return Time.time >= waitStartTime + waitDuration;
        }

        public void Reset() {
            isPausing = false;
            agent.isStopped = false;
            elapsedTimeSinceLastPause = 0f;

            // Setze die Detektor-Radien ebenfalls zurück
            foreach (var detector in detectors) {
                detector.SetRadius(originalRadius[detector]);
            }

            agent.ResetPath();
        }
    }
    
       public class FetchBallStrategy : IStrategy
   {
       private readonly Transform dogTransform;
       private readonly Transform playerTransform;
       private readonly Transform objectGrabPoint;
       private readonly NavMeshAgent dogAgent;
       private readonly Animator animator;
       private readonly GameObject ball;
       private readonly Blackboard blackboard;
       private readonly BlackboardKey ballThrownKey;
       private readonly BlackboardKey calledDogKey;
       private readonly float pickupRange;
       private readonly float dropRange;
       
       protected static readonly int IdleHash = Animator.StringToHash("Idle_2");
       protected static readonly int WalkHash = Animator.StringToHash("Walk_Fwd");
       protected static readonly int TrotHash = Animator.StringToHash("Trot_Fwd");
       protected static readonly int RunHash = Animator.StringToHash("Run_Fwd");
       protected static readonly int RunFastHash = Animator.StringToHash("RunFast_Fwd");
       protected static readonly int PickUp = Animator.StringToHash("PickUp");
       protected static readonly int Drop = Animator.StringToHash("PutDown");
    
       protected const float crossFadeDuration = 0.1f;

       private enum State
       {
           WaitingForThrow,
           MovingToBall,
           PickingUpBall,
           ReturningToPlayer,
           DroppingBall
       }

       private State currentState;

       public FetchBallStrategy(Transform dogTransform, Transform playerTransform, NavMeshAgent dogAgent, Animator animator, Transform objectGrabPoint, GameObject ball, Blackboard blackboard, BlackboardKey ballThrownKey, BlackboardKey calledDogKey, float pickupRange = 0.1f, float dropRange = 2f)
       {
           this.dogTransform = dogTransform;
           this.playerTransform = playerTransform;
           this.dogAgent = dogAgent;
           this.animator = animator;
           this.objectGrabPoint = objectGrabPoint;
           this.ball = ball;
           this.blackboard = blackboard;
           this.ballThrownKey = ballThrownKey;
           this.calledDogKey = calledDogKey;
           this.pickupRange = pickupRange;
           this.dropRange = dropRange;

           currentState = State.MovingToBall;
       }

       public Node.Status Process()
       {
           switch (currentState)
           {
               case State.WaitingForThrow:
                   return WaitForBallThrow();

               case State.MovingToBall:
                   return MoveToBall();

               case State.PickingUpBall:
                   return PickUpBall();

               case State.ReturningToPlayer:
                   return ReturnToPlayer();

               case State.DroppingBall:
                   return DropBall();

               default:
                   return Node.Status.Failure;
           }
       }

       private Node.Status WaitForBallThrow()
       {
           if (blackboard.TryGetValue(calledDogKey, out bool calledDog) && !calledDog)
           {
               Debug.Log("CalledDog ist auf false gesetzt. Hund verlässt die Sequenz.");
               return Node.Status.Failure; // Hund verlässt die Sequenz
           }
           
           Debug.Log("Hund wartet auf den nächsten Ballwurf...");
           dogAgent.stoppingDistance = 4f;
           dogAgent.SetDestination(playerTransform.position);
           // dogAgent.isStopped = true;
           animator.CrossFade(IdleHash, crossFadeDuration);
           
           // Prüfe den Ballwurf-Status aus dem Blackboard
           if (blackboard.TryGetValue(ballThrownKey, out bool ballThrown) && ballThrown)
           {
               // dogAgent.isStopped = false;
               currentState = State.MovingToBall;
               blackboard.SetValue(ballThrownKey, false);
               Debug.Log("Ball wurde geworfen! Hund startet mit der Suche...");
               return Node.Status.Running;
           }

           return Node.Status.Running; // Hund bleibt im Wartezustand
       }

       private Node.Status MoveToBall()
       {
           dogAgent.SetDestination(ball.transform.position);
           dogAgent.stoppingDistance = pickupRange - 0.1f;
           animator.CrossFade(WalkHash, crossFadeDuration);

           if (Vector3.Distance(dogTransform.position, ball.transform.position) <= pickupRange)
           {
               Debug.Log("Hund ist nun am Ball.");
               currentState = State.PickingUpBall;
           }

           return Node.Status.Running;
       }

       private Node.Status PickUpBall()
       {
           // Simuliere das Aufheben des Balls
           Debug.Log("Hund hebt den Ball auf.");
       
           if (ball.TryGetComponent(out GrabbableObject grabbableObject))
           {
               animator.CrossFade(PickUp, crossFadeDuration);
               grabbableObject.Grab(objectGrabPoint);
               currentState = State.ReturningToPlayer;
           }

           return Node.Status.Running;
       }

       private Node.Status ReturnToPlayer()
       {
           dogAgent.SetDestination(playerTransform.position);
           dogAgent.stoppingDistance = dropRange - 0.2f;
           animator.CrossFade(WalkHash, crossFadeDuration);

           if (Vector3.Distance(dogTransform.position, playerTransform.position) <= dropRange)
           {
               currentState = State.DroppingBall;
           }

           return Node.Status.Running;
       }

       private Node.Status DropBall()
       {
           Debug.Log("Hund lässt den Ball fallen.");
           if (ball.TryGetComponent(out GrabbableObject grabbableObject))
           {
               animator.CrossFade(Drop, crossFadeDuration);
               grabbableObject.Drop();
               currentState = State.WaitingForThrow;
           }
           else
           {
               Debug.LogError("GrabbableObject-Komponente fehlt! Ball konnte nicht abgelegt werden.");
           }
           
           // Setze den Blackboard-Wert zurück
           blackboard.SetValue(ballThrownKey, false);

           return Node.Status.Running;
       }
   }