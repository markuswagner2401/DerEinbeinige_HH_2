using System.Collections;
using System.Collections.Generic;
using ObliqueSenastions.VRRigSpace;
using Oculus.Interaction.Input;
using UnityEngine;

namespace ObliqueSenastions.Looping
{

    public class XRLoopingMoveWithArmSwing : MonoBehaviour
    {
        [SerializeField] bool armSwingRight = true;
        [SerializeField] bool armSwingLeft = true;
        [SerializeField] float speedFactor = 10f;
        [SerializeField] AnimationCurve speedChangeCurve;
        [SerializeField] float speedChangeTime = 5f;

        [SerializeField] SpeedChanger[] speedChangers;

        [System.Serializable]
        public struct SpeedChanger
        {
            public string name;

            public float targetValue;
            public float duration;
            public AnimationCurve curve;
        }

        [SerializeField] float threshold = .1f;

        [SerializeField] bool usingOVR = false;

        [SerializeField] XRVelocityTracker velocityTrackerRight = null;
        [SerializeField] XRVelocityTracker velocityTrackerLeft = null;

        [Tooltip("will be used on Handtracking")]
        [SerializeField] SimpleVelocityTracker simpleVelocityTrackerRight = null;

        [SerializeField] SimpleVelocityTracker simpleVelocityTrackerLeft = null;

        [Tooltip("used to get tracking confidece")]
        [SerializeField] OVRHand leftHand = null;

        [SerializeField] OVRHand rightHand = null;

        [SerializeField] LoopingControllerForwardVector forwardHead = null;
        [SerializeField] float smoothing;



        bool speedChangeInterrupted;

        XRLoopingMover loopingMover = null;

        float speedLeft;

        float prevSpeedLeft;
        float speedRight;

        float prevSpeedRight;

        void OnEnable()
        {
            if (loopingMover == null) loopingMover = GetComponent<XRLoopingMover>();

            prevSpeedLeft = speedLeft;
            prevSpeedRight = speedRight;
        }


        void FixedUpdate()
        {
            if (usingOVR)
            {
                OVRInput.FixedUpdate();

                if (OVRInput.GetActiveController() == OVRInput.Controller.Touch)
                {
                    print("hands not active");
                    speedLeft = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch).magnitude;
                    speedRight = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.RTouch).magnitude;

                }

                else
                {

                    //speedLeft = leftHand.IsDataHighConfidence ? simpleVelocityTrackerLeft.GetLocalSpeed() : Mathf.Lerp(prevSpeedLeft, 0, smoothing);
                    //speedRight = rightHand.IsDataHighConfidence ? simpleVelocityTrackerRight.GetLocalSpeed() : Mathf.Lerp(prevSpeedRight, 0, smoothing);

                    speedLeft = simpleVelocityTrackerLeft.GetLocalSpeed();
                    speedRight = simpleVelocityTrackerRight.GetLocalSpeed();

                }







            }

            else
            {
                speedLeft = velocityTrackerLeft.GetSpeed();
                speedRight = velocityTrackerRight.GetSpeed();

            }


            if (armSwingLeft && speedLeft > threshold)
            {
                speedLeft = Mathf.Lerp(prevSpeedLeft, speedLeft, smoothing);
                loopingMover.Move(forwardHead.GetControllerForward() * speedLeft * speedFactor * Time.deltaTime);
            }

            if (armSwingRight && speedRight > threshold)
            {
                speedRight = Mathf.Lerp(prevSpeedRight, speedRight, smoothing);
                loopingMover.Move(forwardHead.GetControllerForward() * speedRight * speedFactor * Time.deltaTime);
            }

            prevSpeedLeft = speedLeft;
            prevSpeedRight = speedRight;


        }

        //// speed change

        public void ChangeSpeed(string name)
        {
            int index = GetSpeedChangerIndexByName(name);
            if (index < 0) return;
            ChangeSpeed(index);
        }

        int GetSpeedChangerIndexByName(string name)
        {
            for (int i = 0; i < speedChangers.Length; i++)
            {
                if (speedChangers[i].name == name)
                {
                    return i;
                }
            }
            Debug.LogError("XRLoopingMoveWithArmSwing: No speed Changer found with name: " + name);
            return -1;
        }

        public void ChangeSpeed(int index)
        {
            StartCoroutine(InterruptAndChangeSpeed(index));
        }

        public IEnumerator InterruptAndChangeSpeed(int index)
        {
            speedChangeInterrupted = true;
            yield return new WaitForSeconds(0.1f);
            speedChangeInterrupted = false;
            StartCoroutine(FadeValue(speedFactor, speedChangers[index].targetValue, speedChangers[index].duration, speedChangers[index].curve));
            yield break;
        }


        ////

        public void ChangeSpeed(float newSpeed)
        {
            StartCoroutine(FadeValue(speedFactor, newSpeed, speedChangeTime, speedChangeCurve));
        }

        private IEnumerator FadeValue(float startValue, float targetValue, float duration, AnimationCurve speedChangeCurve)
        {
            float time = 0;
            float currentResult;
            while (time <= duration && !speedChangeInterrupted)
            {
                time += Time.deltaTime;
                currentResult = Mathf.Lerp(startValue, targetValue, speedChangeCurve.Evaluate(time / duration));
                speedFactor = currentResult;
                yield return null;
            }


            yield break;
        }


    }

}
