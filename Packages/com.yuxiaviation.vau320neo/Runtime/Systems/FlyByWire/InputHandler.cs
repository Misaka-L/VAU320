using System;
using A320VAU.Common;
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;

namespace A320VAU.Systems.FlyByWire {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InputHandler : UdonSharpBehaviour {
        private DependenciesInjector _injector;
        private SaccAirVehicle _saccAirVehicle;

        public float pitchInput { get; private set; }
        public float rollInput { get; private set; }
        public float yawInput { get; private set; }

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);
            _saccAirVehicle = _injector.saccAirVehicle;
        }

        private void LateUpdate() {
            _saccAirVehicle._JoystickOverridden = true;

            // # Keyboard
            // [W][S]/[Up Arrow][Down Arrow] for pitch input
            var wKeyInput = Input.GetKey(KeyCode.W) ? 1 : 0;
            var sKeyInput = Input.GetKey(KeyCode.S) ? -1 : 0;

            var upKeyInput = Input.GetKey(KeyCode.UpArrow) ? 1 : 0;
            var downKeyInput = Input.GetKey(KeyCode.DownArrow) ? -1 : 0;

            // [A][D]/[Left Arrow][Right Arrow] for roll input
            var aKeyInput = Input.GetKey(KeyCode.A) ? -1 : 0;
            var dKeyInput = Input.GetKey(KeyCode.D) ? 1 : 0;

            var leftKeyInput = Input.GetKey(KeyCode.LeftArrow) ? -1 : 0;
            var rightKeyInput = Input.GetKey(KeyCode.RightArrow) ? 1 : 0;

            // [Q]/[E] for yaw input
            var qKeyInput = Input.GetKey(KeyCode.Q) ? -1 : 0;
            var eKeyInput = Input.GetKey(KeyCode.E) ? 1 : 0;

            // # Joystick
            var joystickXInput = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryThumbstickHorizontal");
            var joystickYInput = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryThumbstickVertical");

            // Square Joy Input (For Game Controller)
            if (_saccAirVehicle.SquareJoyInput) {
                var squareJoystickInput = SquareJoyInput(joystickXInput, joystickYInput);

                joystickXInput = squareJoystickInput.x;
                joystickYInput = squareJoystickInput.y;
            }

            rollInput = Mathf.Clamp(aKeyInput + dKeyInput + leftKeyInput + rightKeyInput + joystickXInput, -1f, 1f);
            yawInput = Mathf.Clamp(qKeyInput + eKeyInput + joystickXInput, -1f, 1f);
            pitchInput = Mathf.Clamp(wKeyInput + sKeyInput + upKeyInput + downKeyInput + joystickYInput, -1f, 1f);
        }

        private static Vector2 SquareJoyInput(float joystickXInput, float joystickYInput) {
            var joystickInputTemp = new Vector2(joystickXInput, joystickYInput);

            if (Mathf.Abs(joystickInputTemp.x) > Mathf.Abs(joystickInputTemp.y))
            {
                if (Mathf.Abs(joystickInputTemp.x) <= 0)
                    return joystickInputTemp;

                var temp = joystickInputTemp.magnitude / Mathf.Abs(joystickInputTemp.x);
                joystickInputTemp *= temp;

                return joystickInputTemp;
            }

            if (Mathf.Abs(joystickInputTemp.y) > 0)
            {
                var temp = joystickInputTemp.magnitude / Mathf.Abs(joystickInputTemp.y);
                joystickInputTemp *= temp;
            }

            return joystickInputTemp;
        }
    }
}