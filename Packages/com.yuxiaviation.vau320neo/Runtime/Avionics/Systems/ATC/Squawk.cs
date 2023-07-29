using System;
using UdonSharp;
using UnityEngine;
using VirtualAviationJapan;
using VirtualBridge;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;
using YuxiFlightInstruments.BasicFlightData;

namespace A320VAU.ATC {
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class Squawk : UdonSharpBehaviour {
        public VirtualBridgeClient Client;
        public YFI_FlightDataInterface FlightDataInterface;

        private const int UPDATE_INTERVAL = 1;
        private float _lastUpdate = 0;

        private readonly DataDictionary _dictionary = new DataDictionary() {
            { "callsign", "" },
            { "typeCode", "" },
            { "registration", "" },
            { "instanceId", "" },
            { "worldId", "" },
            { "latitude", 0f },
            { "longitude", 0f },
            { "altitude", 0 },
            { "heading", 0 },
            { "groundspeed", 0 },
            { "verticalSpeed", 0 },
            { "squawkCode", 0 },
            { "track", 0 }
        };

        private void LateUpdate() {
            if (Time.time - _lastUpdate < UPDATE_INTERVAL) return;
            _lastUpdate = Time.time;

            _dictionary["callsign"] = "VAU320";
            _dictionary["typeCode"] = "V20N";
            _dictionary["registration"] = "B-0320";
            _dictionary["instanceId"] = "test";
            _dictionary["worldId"] = "test";

            var position = transform.position;
            _dictionary["longitude"] = position.z;
            _dictionary["latitude"] = position.y;

            _dictionary["altitude"] = (int)FlightDataInterface.altitude;
            _dictionary["groundspeed"] = (int)FlightDataInterface.groundSpeed;
            _dictionary["verticalSpeed"] = (int)FlightDataInterface.verticalSpeed;
            _dictionary["squawkCode"] = 2000;
            _dictionary["track"] = (int)FlightDataInterface.heading;
            _dictionary["heading"] = (int)FlightDataInterface.heading;

            Client.SendData("virtualaware.trackdata", _dictionary);
        }
    }
}