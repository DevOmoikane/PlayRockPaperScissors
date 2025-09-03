using System;
using Leap;
using TMPro;
using UnityEngine;
using CodeMonkey.Utils;

public class GameController : MonoBehaviour {

    public enum GameState {
        Waiting,
        CountingDown,
        ShowingResult
    }

    [Header("Visuals")]
    [SerializeField] private GestureDetector gestureDetector;
    [SerializeField] private LeapProvider leapProvider;
    [SerializeField] private TextMeshPro playerChoiceText;
    [SerializeField] private TextMeshPro currentPlayerChoiceText;
    [SerializeField] private TextMeshPro currentStateText;
    [SerializeField] private TextMeshPro countdownText;
    [SerializeField] private TextMeshPro handVectorText;
    [SerializeField] private Transform PCFist;

    [Header("Gameplay Variables")]
    [SerializeField] private int selectMovementAtCount = 3;

    // Added: robust "down" detection parameters
    [Header("Down Detection")]
    [Tooltip("Transform of the sensor or a GameObject matching the sensor orientation")]
    [SerializeField] private Transform sensorTransform;
    [Tooltip("Down direction in SENSOR LOCAL SPACE (normalized internally). For 45° right-down, use (1,-1,0)")]
    [SerializeField] private Vector3 sensorLocalDownDirection = new Vector3(1f, -1f, 0f);
    [Tooltip("Smoothed projected down-speed needed to trigger a 'down' event (m/s)")]
    [SerializeField] private float downTriggerSpeed = 0.6f;
    [Tooltip("Speed below which the 'down' state is released (hysteresis) (m/s)")]
    [SerializeField] private float downReleaseSpeed = 0.3f;
    [Tooltip("Exponential smoothing time constant for velocity (seconds)")]
    [SerializeField] private float velocitySmoothingTime = 0.1f;
    [Tooltip("Cooldown after counting a 'down' edge to avoid multi-count from noise (seconds)")]
    [SerializeField] private float downDebounceTime = 0.25f;
    
    private GameState currentState = GameState.Waiting;
    private int countMovement = 0;
    private HandPose playerChoice;
    private HandPose computerChoice;
    private bool captureMovement = false;

    // Added: runtime state for robust detection
    private float smoothedDownSpeed = 0f;
    private bool downActive = false;
    private bool upActive = false;
    private float recaptureTimer = 0f;
    private Vector3 downDirWorld = new Vector3(1f, -1f, 0f);

    private void OnEnable() {
        leapProvider.OnUpdateFrame += OnUpdateFrame;
    }

    private void OnDisable() {
        leapProvider.OnUpdateFrame -= OnUpdateFrame;
    }

    void OnUpdateFrame(Frame frame) {
        if (frame.Hands.Count > 0) {
            var hand = frame.Hands[0];
        }
    }
    
    private void Start() {
        currentState = GameState.Waiting;
        countdownText.enabled = false;
        // Compute world-space down direction based on sensor orientation
        Vector3 localDown = sensorLocalDownDirection.sqrMagnitude > 0f ? sensorLocalDownDirection.normalized : new Vector3(1f, -1f, 0f).normalized;
        downDirWorld = sensorTransform != null ? sensorTransform.TransformDirection(localDown).normalized : localDown;
    }

    private void Update() {
        currentStateText.text = currentState.ToString();
        
        HandPose currentPose = HandPose.Unknown;
        bool isHandMoving = false;
        Frame frame = leapProvider.CurrentFrame;
        GestureDetector.HandMovement handMovement = null;
        if (frame.Hands.Count > 0) {
            Hand playerHand = frame.Hands[0];
            currentPose = gestureDetector.DetectPose(playerHand);
            handMovement = gestureDetector.GetHandMovement(playerHand);
            isHandMoving = handMovement.isMoving;

            // Project palm velocity onto the down direction and smooth it
            float projectedDownSpeed = Vector3.Dot(handMovement.velocity, downDirWorld); // m/s along 'down'
            float dt = Mathf.Max(Time.deltaTime, 0.0001f);
            float alpha = 1f - Mathf.Exp(-dt / Mathf.Max(velocitySmoothingTime, 0.0001f)); // exponential smoothing
            smoothedDownSpeed = Mathf.Lerp(smoothedDownSpeed, projectedDownSpeed, alpha);

            // Vector3 direction = handMovement.velocity.normalized;
            // float palmVelocity = handMovement.velocity.magnitude;
            // handVectorText.text = $"{palmVelocity:00.00}\n{direction.x:0.00}\n{direction.y:0.00}\n{direction.z:0.00}\n↓:{smoothedDownSpeed:0.00}";
            // currentPlayerChoiceText.text = currentPose.ToString();

            float handDeltaPosition = -projectedDownSpeed * 10 * Time.deltaTime;
            // TODO: protect the position of the fist so it doesn't go over the boundaries of the screen or playable area
            PCFist.Translate(0, handDeltaPosition, 0);
        }

        switch (currentState) {
            case GameState.Waiting:
                if (isHandMoving) {
                    currentState = GameState.CountingDown;
                    countdownText.enabled = true;
                    countMovement = 0;
                    captureMovement = true;

                    // Reset detection state
                    smoothedDownSpeed = 0f;
                    downActive = false;
                    upActive = false;
                    recaptureTimer = 0f;
                }
                break;
            case GameState.CountingDown:
                countdownText.text = Mathf.CeilToInt(countMovement).ToString();

                // Debounced, hysteretic edge detection of a single clear "down" movement
                if (captureMovement && handMovement != null) {
                    recaptureTimer -= Time.deltaTime;

                    // Only consider counting when not in cooldown
                    if (recaptureTimer <= 0f) {
                        // Enter 'down' when we cross trigger threshold from below
                        if (!downActive && smoothedDownSpeed >= downTriggerSpeed) {
                            downActive = true;
                            countMovement++;
                            // Start cooldown so noise/spikes don't double count
                            recaptureTimer = downDebounceTime;
                        }else if (!upActive && smoothedDownSpeed <= -downTriggerSpeed) {
                            upActive = true;
                            recaptureTimer = downDebounceTime;
                        }

                        // Exit 'down' when we fall below release threshold
                        if (downActive && smoothedDownSpeed <= downReleaseSpeed) {
                            downActive = false;
                        }
                    }
                }

                if (countMovement >= selectMovementAtCount) {
                    currentState = GameState.ShowingResult;
                    countdownText.enabled = false;
                    // playerChoice = currentPose;
                    FunctionTimer.Create(() => { playerChoice = currentPose; }, 0.2f);
                    FunctionTimer.Create(() => { currentState = GameState.Waiting; }, 5f);
                }
                break;
            case GameState.ShowingResult:
                playerChoiceText.text = playerChoice.ToString();
                break;
        }
    }
}