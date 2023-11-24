using A320VAU.Common;
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;

namespace A320VAU.Systems.FlyByWire {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RollLaw : UdonSharpBehaviour {
        public InputHandler inputHandler;

        private DependenciesInjector _injector;
        private SaccAirVehicle _saccAirVehicle;

        private ADIRU.ADIRU _adiru;

        public float Kp = 0.1f;
        public float Ki = 0f;
        public float Kd = 0f;

        public float targetRollRate = 0f;

        public bool directLaw;

        private float _integral;
        private float _previousError;

        private float _previousRollAngle;
        private float _previousTime;

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);
            _saccAirVehicle = _injector.saccAirVehicle;
            _adiru = _injector.adiru;
        }

        private void LateUpdate() {
            var time = Time.time;

            var bank = _adiru.irs.bank;
            var rollRate = (bank - _previousRollAngle) / (time - _previousTime);

            if (directLaw) {
                _saccAirVehicle.JoystickOverride.z = inputHandler.rollInput;
                return;
            }

            targetRollRate = inputHandler.rollInput * 15f;

            // Limit
            var maxRollLimit = 67f;
            var normalRollLimit = 33f;

            if (Mathf.Approximately(targetRollRate, 0f) && Mathf.Abs(bank) > normalRollLimit) {
                var normalRollLimitError = bank >= 0f ? normalRollLimit - bank : -normalRollLimit - bank;
                targetRollRate += normalRollLimitError * 5f;
            }

            if (Mathf.Abs(bank) > maxRollLimit) {
                var maxRollLimitError = bank >= 0f ? maxRollLimit - bank : -maxRollLimit - bank;
                targetRollRate += maxRollLimitError * 5f;
            }

            // Roll Rate Limit
            targetRollRate = Mathf.Clamp(targetRollRate, -15f, 15f);

            // PID
            var error = rollRate - targetRollRate;
            _integral += error * Time.deltaTime;
            var derivative = (error - _previousError) / Time.deltaTime;
            var rollInput = Kp * error + Ki * _integral + Kd * derivative;

            _previousError = error;
            _previousTime = time;
            _previousRollAngle = bank;

            _saccAirVehicle.JoystickOverride.z = Mathf.Clamp(rollInput, -1f, 1f);

            // Rudder
            _saccAirVehicle.JoystickOverride.y = Mathf.Clamp(inputHandler.yawInput, -1, 1);
        }
    }
}