using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Interhaptics.HapticRenderer.Core
{
    public class HapticManager : Interhaptics.Utils.Singleton<HapticManager>
    {
        private float m_rate_exe = 60;
        private float m_real_time = 0;
        private float m_old_frame = 0;
        private Devices.HapticDeviceManager m_haptic_device_manager = new Devices.HapticDeviceManager();

        public bool RenderTexture = true;
        public bool RenderStiffness = true;
        public bool RenderVibration = true;

        public Devices.HapticDeviceManager DeviceManager
        {
            get => m_haptic_device_manager;
        }

        public float RealTime
        {
            get => Time.realtimeSinceStartup;
        }

        override protected void OnAwake()
        {
            HARWrapper.Init(1000, 30);
            m_haptic_device_manager.Init();
        }

        protected override void OnStart()
        {

        }

        protected override void OnUpdate()
        {
            
        }

        override protected void OnFixedUpdate()
        {
            m_haptic_device_manager.SendHaptics();
        }

        override protected void OnOnApplicationQuit()
        {
            HARWrapper.Quit();
        }

        protected override void OnOnDestroy()
        {
            HARWrapper.Quit();
        }
    }
}
