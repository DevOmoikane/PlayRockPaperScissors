using System;
using Leap;
using UnityEngine;

public class GameController : MonoBehaviour {

    public enum GameState {
        Waiting,
        CountingDown,
        ShowingResult
    }

    [SerializeField] private GestureDetector gestureDetector;
    [SerializeField] private LeapProvider leapProvider;
    
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

    private void Update() {
        Frame frame = leapProvider.CurrentFrame;

        if (frame.Hands.Count > 0) {
            Hand playerHand = frame.Hands[0];
            HandPose currentPose = gestureDetector.DetectPose(playerHand);
            Debug.Log(currentPose);

            switch (currentState) {
                case GameState.Waiting:
                    if (currentPose == HandPose.Moving) {
                        currentState = GameState.CountingDown;
                    }
                    break;
                case GameState.CountingDown:
                    countdownTimer -= Time.deltaTime;
                    if (countdownTimer <= 0f) {
                        currentState = GameState.ShowingResult;
                    }
                    break;
                case GameState.ShowingResult:
                    break;
            }
        }
    }
}