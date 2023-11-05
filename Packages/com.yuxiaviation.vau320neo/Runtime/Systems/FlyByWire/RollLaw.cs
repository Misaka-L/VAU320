
using System;
using A320VAU.Common;
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace A320VAU.Systems.FlyByWire {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RollLaw : UdonSharpBehaviour {
        private DependenciesInjector _injector;
        private SaccAirVehicle _saccAirVehicle;
        private SaccEntity _saccEntity;

        private Rigidbody _rigidbody;

        public float Kp = 0.05f;
        public float Ki = 0.01f;
        public float Kd = 0.05f;

        public float targetRollRate = 0f;

        public bool directLaw;

        private float _integral;
        private float _previousError;

        private float _previousRollAngle;
        private float _previousTime;

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);
            _saccAirVehicle = _injector.saccAirVehicle;
            _saccEntity = _injector.saccEntity;

            _rigidbody = _saccEntity.GetComponent<Rigidbody>();
        }

        private void LateUpdate() {
            var time = Time.time;

            var Ai = Input.GetKey(KeyCode.A) ? -1 : 0;
            var Di = Input.GetKey(KeyCode.D) ? 1 : 0;
            var Qi = Input.GetKey(KeyCode.Q) ? -1 : 0;
            var Ei = Input.GetKey(KeyCode.E) ? 1 : 0;

            if (directLaw) {
                _saccAirVehicle.JoystickOverride.z = Ai + Di;
                return;
            }

            targetRollRate = (Ai + Di) * 15f;
            // var rollRate = (_rigidbody.angularVelocity.z - _previousRollAngle) / (time - _previousTime);
            var localRotation = _saccEntity.transform.localRotation;
            var rollRate = (localRotation.z - _previousRollAngle) / (time - _previousTime);

            // _saccAirVehicle.JoystickOverride.z = Mathf.Clamp((Ai + Di) * -1, -1, 1);
            _saccAirVehicle.JoystickOverride.y = Mathf.Clamp(Qi + Ei, -1, 1);

            var error = rollRate - targetRollRate;
            _integral += error * Time.deltaTime;
            var derivative = (error - _previousError) / Time.deltaTime;
            var rollInput = Kp * error + Ki * _integral + Kd * derivative;

            _previousError = error;
            _previousTime = time;
            // _previousRollAngle = _rigidbody.transform.localRotation.z;
            _previousRollAngle = localRotation.z;

            _saccAirVehicle.JoystickOverride.z = Mathf.Clamp(rollInput, -1f, 1f);
        }
    }
}