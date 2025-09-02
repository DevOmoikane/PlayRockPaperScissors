using Leap;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class GestureDetector : MonoBehaviour {
    
    [SerializeField] private float handVelocityThreshold = 1f;

    public class HandMovement {
        public bool isMoving;
        public Vector3 velocity;
    }
    
    public HandPose DetectPose(Hand hand) {
        if (IsRock(hand)) return HandPose.Rock;
        if (IsPaper(hand)) return HandPose.Paper;
        if (IsScissors(hand)) return HandPose.Scissors;
        return HandPose.Unknown;
    }

    public HandMovement GetHandMovement(Hand hand) {
        HandMovement handMovement = new HandMovement();
        handMovement.isMoving = hand.PalmVelocity.magnitude > handVelocityThreshold;
        handMovement.velocity = hand.PalmVelocity;
        return handMovement;
    }

    private bool IsRock(Hand hand) {
        foreach (Finger finger in hand.fingers) {
            if (finger.IsExtended) return false;
        }
        return true;
    }

    private bool IsPaper(Hand hand) {
        foreach (Finger finger in hand.fingers) {
            if (!finger.IsExtended) return false;
        }
        return true;
    }
    
    private bool IsScissors(Hand hand) {
        bool indexExtended = hand.fingers[1].IsExtended;
        bool middleExtended = hand.fingers[2].IsExtended;
        bool othersClosed = //!hand.fingers[0].IsExtended && 
                            !hand.fingers[3].IsExtended && 
                            !hand.fingers[4].IsExtended;
        return /*indexExtended &&*/ middleExtended && othersClosed;
    }
}