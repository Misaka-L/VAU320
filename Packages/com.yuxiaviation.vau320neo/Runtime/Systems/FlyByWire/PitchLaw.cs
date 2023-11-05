using A320VAU.Common;
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;

namespace A320VAU.Systems.FlyByWire {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PitchLaw : UdonSharpBehaviour {
        private DependenciesInjector _injector;
        private SaccAirVehicle _saccAirVehicle;

        public float targetGLoad = 1f;

        private float _integral;
        private float _previousError;

        public float Kp = 1.5f;
        public float Ki = 0.00325f;
        public float Kd = 1.2f;

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);
            _saccAirVehicle = _injector.saccAirVehicle;
        }

        private void LateUpdate() {
            var Wi = Input.GetKey(KeyCode.W) ? 1 : 0; //inputs as ints
            var Si = Input.GetKey(KeyCode.S) ? -1 : 0;

            targetGLoad = 1f - Mathf.Clamp(Wi + Si, -1, 1) * 1.5f;
            // if ((RotationInputs.x < 0 && VertGs > 0) || (RotationInputs.x > 0 && VertGs < 0)) {
            //     RotationInputs.x *= Limits;
            // }

            _saccAirVehicle._JoystickOverridden = true;

            var gLoad = _saccAirVehicle.VertGs;

            var error = gLoad - targetGLoad;
            _integral += error * Time.deltaTime;
            var derivative = (error - _previousError) / Time.deltaTime;
            var pitchInput = Kp * error + Ki * _integral + Kd * derivative;

            _previousError = error;

            _saccAirVehicle.JoystickOverride.x = Mathf.Clamp(pitchInput, -1f, 1f);
        }

        private void OnDisable() {
            _saccAirVehicle._JoystickOverridden = false;
        }
    }
}