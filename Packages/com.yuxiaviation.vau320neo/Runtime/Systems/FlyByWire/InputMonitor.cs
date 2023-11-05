using System;
using A320VAU.Common;
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;

namespace A320VAU.Systems.FlyByWire {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class InputMonitor : UdonSharpBehaviour {
        private DependenciesInjector _injector;
        private ADIRU.ADIRU _adiru;
        private SaccAirVehicle _saccAirVehicle;
        private SaccEntity _saccEntity;

        private Rigidbody _rigidbody;

        private float _previousRollAngle;
        private float _previousTime;

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);
            _adiru = _injector.adiru;
            _saccAirVehicle = _injector.saccAirVehicle;
            _saccEntity = _injector.saccEntity;

            _rigidbody = _saccEntity.GetComponent<Rigidbody>();
        }

        private void LateUpdate() {
            var time = Time.time;
            // var angularVelocity = _rigidbody.angularVelocity;

            var rotation = _saccEntity.transform.rotation;
            var rollRate = (rotation.z - _previousRollAngle) / (time - _previousTime);

            _previousTime = time;
            _previousRollAngle = rotation.z;

            // {(_saccAirVehicle.transform.rotation * angularVelocity).z} / {angularVelocity.z}
            Debug.Log(
                $"G Load: {_saccAirVehicle.VertGs}G | Roll Rate: {rollRate}\n" +
                $"Pilot Input: pitch {_saccAirVehicle.RotationInputs.x} | roll {_saccAirVehicle.RotationInputs.z} | yaw {_saccAirVehicle.RotationInputs.y} / " +
                $"Override: pitch {_saccAirVehicle.JoystickOverride.x} | roll {_saccAirVehicle.JoystickOverride.z} | yaw {_saccAirVehicle.JoystickOverride.y}\n"
            );
        }
    }
}