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

    [SerializeField] private GestureDetector gestureDetector;
    [SerializeField] private LeapProvider leapProvider;
    [SerializeField] private TextMeshPro playerChoiceText;
    [SerializeField] private TextMeshPro currentPlayerChoiceText;
    [SerializeField] private TextMeshPro currentStateText;
    [SerializeField] private TextMeshPro countdownText;
    
    private GameState currentState = GameState.Waiting;
    private float countdownTimer = 3f;
    private HandPose playerChoice;
    private HandPose computerChoice;

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
        if (frame.Hands.Count > 0) {
            Hand playerHand = frame.Hands[0];
            currentPose = gestureDetector.DetectPose(playerHand);
            isHandMoving = gestureDetector.IsHandMoving(playerHand);
            currentPlayerChoiceText.text = currentPose.ToString();
        }

        switch (currentState) {
            case GameState.Waiting:
                if (isHandMoving) {
                    currentState = GameState.CountingDown;
                    countdownTimer = 3f;
                    countdownText.enabled = true;
                }
                break;
            case GameState.CountingDown:
                countdownTimer -= Time.deltaTime;
                countdownText.text = Mathf.CeilToInt(countdownTimer).ToString();
                if (countdownTimer <= 0f) {
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