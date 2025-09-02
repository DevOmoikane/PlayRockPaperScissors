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

    [Header("Gameplay Variables")]
    [SerializeField] private int selectMovementAtCount = 3;
    
    private GameState currentState = GameState.Waiting;
    private int countMovement = 0;
    private HandPose playerChoice;
    private HandPose computerChoice;
    private bool captureMovement = false;

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
            Vector3 direction = handMovement.velocity.normalized;
            float palmVelocity = handMovement.velocity.magnitude;
            handVectorText.text = $"{palmVelocity:00.00}\n{direction.x:0.00}\n{direction.y:0.00}\n{direction.z:0.00}";
            currentPlayerChoiceText.text = currentPose.ToString();
        }

        switch (currentState) {
            case GameState.Waiting:
                if (isHandMoving) {
                    currentState = GameState.CountingDown;
                    countdownText.enabled = true;
                    countMovement = 0;
                    captureMovement = true;
                }
                break;
            case GameState.CountingDown:
                countdownText.text = Mathf.CeilToInt(countMovement).ToString();
                if (captureMovement && handMovement != null) {
                    Vector3 dirVector = handMovement.velocity.normalized;
                    if (dirVector.x >= 0.5 && dirVector.y <= -0.5) {
                        captureMovement = false;
                        countMovement++;
                        FunctionTimer.Create(() => { captureMovement = true;}, 0.5f);
                    }
                }
                if (countMovement >= 3) {
                    currentState = GameState.ShowingResult;
                    countdownText.enabled = false;
                    playerChoice = currentPose;
                    FunctionTimer.Create(() => { currentState = GameState.Waiting; }, 5f);
                }
                break;
            case GameState.ShowingResult:
                playerChoiceText.text = playerChoice.ToString();
                break;
        }
    }
}