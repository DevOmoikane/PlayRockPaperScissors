using Leap;
using UnityEngine;
using UnityEngine.Serialization;

public class GestureDetector : MonoBehaviour {
    
    [SerializeField] private float handVelocityThreshold = 1f;
    
    public HandPose DetectPose(Hand hand) {
        if (IsRock(hand)) return HandPose.Rock;
        if (IsPaper(hand)) return HandPose.Paper;
        if (IsScissors(hand)) return HandPose.Scissors;
        return HandPose.Unknown;
    }

    public bool IsHandMoving(Hand hand) {
        float palmVelocity = hand.PalmVelocity.magnitude;
        return palmVelocity > handVelocityThreshold;
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