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

        private float _previousRollAngle;
        private float _previousTime;

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);
            _adiru = _injector.adiru;
            _saccAirVehicle = _injector.saccAirVehicle;
        }

        private void LateUpdate() {
            var time = Time.time;

            var bank = _adiru.irs.bank;
            var rollRate = (bank - _previousRollAngle) / (time - _previousTime);

            _previousTime = time;
            _previousRollAngle = bank;

            Debug.Log(
                $"G Load: {_saccAirVehicle.VertGs}G | Roll Rate: {rollRate}\n" +
                $"Pilot Input: pitch {_saccAirVehicle.RotationInputs.x} | roll {_saccAirVehicle.RotationInputs.z} | yaw {_saccAirVehicle.RotationInputs.y} / " +
                $"Override: pitch {_saccAirVehicle.JoystickOverride.x} | roll {_saccAirVehicle.JoystickOverride.z} | yaw {_saccAirVehicle.JoystickOverride.y}\n"
            );
        }
    }
}