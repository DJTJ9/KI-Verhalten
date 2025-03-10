using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BlackboardSystem;
using UnityEngine.Serialization;


public class Dog : MonoBehaviour
{
    [SerializeField] private GameObject foodBowl;
    [SerializeField] private GameObject waterBowl;
    [SerializeField] private GameObject player;
    private PlayerController playerController;
    [SerializeField] private List<ObjectDetector> detectors = new();
    [SerializeField] private ObjectDetector foodDetector;
    [SerializeField] private ObjectDetector waterDetector;
    [SerializeField] private Transform objectGrabPoint;
    [SerializeField] private GameObject grabbableObject;

    [SerializeField] private Animator animator;
    [SerializeField] BlackboardData blackboardData;
    
    protected static readonly int IdleHash = Animator.StringToHash("Idle_2");
    protected static readonly int WalkHash = Animator.StringToHash("Walk_Fwd");
    protected static readonly int TrotHash = Animator.StringToHash("Trot_Fwd");
    protected static readonly int RunHash = Animator.StringToHash("Run_Fwd");
    protected static readonly int RunFastHash = Animator.StringToHash("RunFast_Fwd");
    protected static readonly int PickUp = Animator.StringToHash("PickUp");
    
    protected const float crossFadeDuration = 0.1f;

    private NavMeshAgent agent;
    private BehaviourTree tree;

    private Blackboard blackboard;
    private BlackboardKey calledDogKey;
    private BlackboardKey ballInHandKey;
    private BlackboardKey ballThrownKey;
    private BlackboardKey foodBowlInReachKey;
    private BlackboardKey waterBowlInReachKey;

    public bool BallThrown = false;

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        playerController = player.GetComponent<PlayerController>();

        blackboard = BlackboardManager.SharedBlackboard;
        blackboardData.SetValuesOnBlackboard(blackboard);
        calledDogKey = blackboard.GetOrRegisterKey("CalledDog");
        ballInHandKey = blackboard.GetOrRegisterKey("BallInHand");
        ballThrownKey = blackboard.GetOrRegisterKey("BallThrown");
        blackboard.SetValue(ballThrownKey, false);
        foodBowlInReachKey = blackboard.GetOrRegisterKey("FoodBowlInReach");
        waterBowlInReachKey = blackboard.GetOrRegisterKey("WaterBowlInReach");

        tree = new BehaviourTree("Dog");

        PrioritySelector dogActions = new PrioritySelector("Dog Logic");

        Sequence runToOwner = new Sequence("RunToOwner", 100);
        bool CalledDog() {
            if (blackboard.TryGetValue(calledDogKey, out bool calledDog)) {
                if (!calledDog) {
                    runToOwner.Reset();
                    return false;
                }
            }

            return true;
        }
        runToOwner.AddChild(new Leaf("CalledDog", new Condition(CalledDog)));
        runToOwner.AddChild(new Leaf("RunToOwner", new MoveToTarget(transform, agent, player.transform, 5f, 4f)));
        dogActions.AddChild(runToOwner);

        // Ergänze in der BehaviourTree-Initialisierung:
        Sequence fetchBallSequence = new Sequence("FetchBall", 200);
        // fetchBallSequence.AddChild(new Leaf("CalledDog", new Condition(CalledDog)));
        bool BallInHand() {
            if (blackboard.TryGetValue(ballInHandKey, out bool ballInHand)) {
                if (!ballInHand) {
                    fetchBallSequence.Reset();
                    return false;
                }
            }
        
            return true;
        }
        bool BallThrown() {
            if (blackboard.TryGetValue(ballThrownKey, out bool ballThrown)) {
                if (!ballThrown) {
                    fetchBallSequence.Reset();
                    return false;
                }
            }
        
            return true;
        }
        fetchBallSequence.AddChild(new Leaf("BallThrownCondition", new Condition(BallThrown)));
        fetchBallSequence.AddChild(new Leaf("FetchBallProcess", new FetchBallStrategy(transform, player.transform, agent, animator, objectGrabPoint, grabbableObject, blackboard, ballThrownKey, calledDogKey,0.5f, 2f)));
        dogActions.AddChild(fetchBallSequence);
        
        Selector getFoodOrWater = new PrioritySelector("GetFoodOrWater", 50);

        Sequence goToFoodBowl = new Sequence("GoToFoodBowl", 20);
        bool foodInReach() {
            if (blackboard.TryGetValue(foodBowlInReachKey, out bool foodBowlInReach)) {
                if (foodBowlInReach) {
                    goToFoodBowl.Reset();
                    return true;
                }
            }

            return false;
        }
        goToFoodBowl.AddChild(new Leaf("IsFoodBowlAvailable", new Condition(() => foodInReach() && foodBowl.activeSelf)));
        goToFoodBowl.AddChild(new Leaf("MoveToFoodBowl", new MoveToTarget(transform, agent, foodBowl.transform, 5f, 0.2f)));
        goToFoodBowl.AddChild(new Leaf("EatFood", new ActionStrategy(() => foodBowl.SetActive(false))));
        getFoodOrWater.AddChild(goToFoodBowl);

        Sequence goToWaterBowl = new Sequence("GoToWaterBowl", 10);
        bool waterInReach() {
            if (blackboard.TryGetValue(waterBowlInReachKey, out bool waterBowlInReach)) {
                if (waterBowlInReach) {
                    goToWaterBowl.Reset();
                    return true;
                }
            }

            return false;
        }
        goToWaterBowl.AddChild(new Leaf("IsWaterBowlAvailable", new Condition(() => waterInReach() && waterBowl.activeSelf)));
        goToWaterBowl.AddChild(new Leaf("MoveToWaterBowl", new MoveToTarget(transform, agent, waterBowl.transform, 3f, 0.2f)));
        goToWaterBowl.AddChild(new Leaf("DrinkWater", new ActionStrategy(() => waterBowl.SetActive(false))));
        getFoodOrWater.AddChild(goToWaterBowl);

        dogActions.AddChild(getFoodOrWater);

        Selector explore = new PrioritySelector("Explore", 1);
        explore.AddChild(goToFoodBowl);
        explore.AddChild(goToWaterBowl);
        explore.AddChild(new Leaf("Explore", new ExploreStrategy(transform, agent, animator, blackboard, detectors, 1.5f, 25f, 5f)));
        dogActions.AddChild(explore);


        tree.AddChild(dogActions);
    }

    private void Update() {
        tree.Process();
        UpdateBlackboardValues();

        CallDog();
    }

    private void CallDog() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (blackboard.TryGetValue(calledDogKey, out bool calledDog)) {
                blackboard.SetValue(calledDogKey, !calledDog);
                Debug.Log($"CalledDog: {calledDog}");
            }
        }
    }

    private void UpdateBlackboardValues() {
        blackboard.SetValue(ballThrownKey, BallThrown);
        blackboard.SetValue(foodBowlInReachKey, foodDetector.CanDetectObject());
        blackboard.SetValue(waterBowlInReachKey, waterDetector.CanDetectObject());
    }
}