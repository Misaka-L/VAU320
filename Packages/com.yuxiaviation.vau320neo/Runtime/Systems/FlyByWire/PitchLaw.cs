using System;
using A320VAU.Common;
using A320VAU.DFUNC;
using Avionics.Systems.Common;
using EsnyaSFAddons.DFUNC;
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;

namespace A320VAU.Systems.FlyByWire {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PitchLaw : UdonSharpBehaviour {
        private DependenciesInjector _injector;
        private SaccAirVehicle _saccAirVehicle;

        private DFUNC_ElevatorTrim _elevatorTrim;
        private AircraftSystemData _aircraftSystemData;
        private RadioAltimeter.RadioAltimeter _radioAltimeter;
        private ADIRU.ADIRU _adiru;

        public float targetGLoad = 1f;

        private float _integral;
        private float _previousError;

        public float Kp = 1.5f;
        public float Ki = 0.00325f;
        public float Kd = 1.2f;

        public float autoTrimKp = 0.5f;
        public float autoTrimKi = 0f;
        public float autoTrimKd = 0f;

        public bool directLaw;

        private float _autoTrimIntegral;
        private float _autoTrimPreviousError;

        private float _previousPitch;

        public int VSWCONF0 = 145;
        public int VSWCONF1 = 113;
        public int VSWCONF2 = 107;
        public int VSWCONF3 = 104;
        public int VSWCONFFULL = 102;

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);
            _saccAirVehicle = _injector.saccAirVehicle;
            _elevatorTrim = _injector.elevatorTrim;
            _aircraftSystemData = _injector.equipmentData;
            _adiru = _injector.adiru;
            _radioAltimeter = _injector.radioAltimeter;
        }

        private void LateUpdate() {
            var Wi = Input.GetKey(KeyCode.W) ? 1 : 0; //inputs as ints
            var Si = Input.GetKey(KeyCode.S) ? -1 : 0;

            targetGLoad = 1f - Mathf.Clamp(Wi + Si, -1, 1) * 1.5f;
            // if ((RotationInputs.x < 0 && VertGs > 0) || (RotationInputs.x > 0 && VertGs < 0)) {
            //     RotationInputs.x *= Limits;
            // }

            _saccAirVehicle._JoystickOverridden = true;

            if (_aircraftSystemData.isAircraftGrounded || directLaw) {
                _saccAirVehicle.JoystickOverride.x = Mathf.Clamp(Wi + Si, -1, 1);
                return;
            }

            var gLoad = _saccAirVehicle.VertGs;

            // High Speed Protection
            var overSpeedError = _adiru.adr.instrumentAirSpeed - 350;
            if (overSpeedError > 0f) {
                targetGLoad += 0.1f * overSpeedError;
            }

            // Pitch Limit
            var pitchUpLimit = GetPitchUpLimit();
            var pitchDownLimit = -15f;

            var pitchUpLimitError = _adiru.irs.pitch - pitchUpLimit;
            if (pitchUpLimitError > 0) {
                targetGLoad += -0.5f * pitchUpLimitError;
            }

            var pitchDownLimitError = pitchDownLimit - _adiru.irs.pitch;
            if (pitchDownLimitError > 0) {
                targetGLoad += 0.5f * pitchDownLimitError;
            }

            // G Load Limit
            if (_aircraftSystemData.flapCurrentIndex == 0) {
                targetGLoad = Mathf.Clamp(targetGLoad, -1f, 2.5f);
            }
            else {
                targetGLoad = Mathf.Clamp(targetGLoad, -0.5f, 2f);
            }

            // PID
            var error = gLoad - targetGLoad;
            _integral += error * Time.deltaTime;
            var derivative = (error - _previousError) / Time.deltaTime;
            var pitchInput = Kp * error + Ki * _integral + Kd * derivative;

            pitchInput = Mathf.Clamp(pitchInput, -1f, 1f);

            if (!_aircraftSystemData.isAircraftGrounded && Mathf.Abs(targetGLoad - 1f) < 0.01f && gLoad > 0.5f) {
                UpdateAutoTrim();
            }
            else {
                _autoTrimPreviousError = 0f;
                _autoTrimIntegral = 0f;
            }

            _previousError = error;
            _previousPitch = _adiru.irs.pitch;

            _saccAirVehicle.JoystickOverride.x = pitchInput;
        }

        private void UpdateAutoTrim() {
            var error = (_adiru.irs.pitch - _previousPitch) / Time.deltaTime;
            _autoTrimIntegral += error * Time.deltaTime;
            var derivative = (error - _autoTrimPreviousError) / Time.deltaTime;
            var trim = autoTrimKp * error + autoTrimKi * _integral + autoTrimKd * derivative;

            _autoTrimIntegral += error * Time.deltaTime;
            _autoTrimPreviousError = error;

            _elevatorTrim.trim = trim;
        }

        private float GetPitchUpLimit() {
            var vsw = GetVSW();
            var lowSpeedError = vsw + 5f - _adiru.adr.instrumentAirSpeed;

            if (_aircraftSystemData.flapCurrentIndex != 4) {
                return Mathf.Clamp(30f - lowSpeedError, 25f, 30f);
            }

            return Mathf.Clamp(25f - lowSpeedError, 20f, 25f);
        }

        private float GetVSW() {
            var VSW = VSWCONF0;
            switch (_aircraftSystemData.flapCurrentIndex) {
                case 1:
                    VSW = VSWCONF1;
                    break;
                case 2:
                    VSW = VSWCONF2;
                    break;
                case 3:
                    VSW = VSWCONF3;
                    break;
                case 4:
                    VSW = VSWCONFFULL;
                    break;
            }

            return VSW;
        }

        private void OnDisable() {
            _saccAirVehicle._JoystickOverridden = false;
        }
    }
}