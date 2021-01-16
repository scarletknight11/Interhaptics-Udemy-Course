using System.Runtime.InteropServices;

namespace Interhaptics.HapticRenderer.Core
{
    public class HARWrapper
    {
        const string DLL_NAME = "HAR2";

        [DllImport(DLL_NAME)]
        public static extern bool Init(int _rate_lib, int _rate_exe);
        [DllImport(DLL_NAME)]
        public static extern void Quit();
        [DllImport(DLL_NAME)]
        public static extern bool Reset(bool _erase_list);
        [DllImport(DLL_NAME)]
        public static extern void PlayVibration(int _id_hm);
        [DllImport(DLL_NAME)]
        public static extern void StopVibration(int _id_hm);
        [DllImport(DLL_NAME)]
        public static extern void ResetVibration(int _id_hm);
        [DllImport(DLL_NAME)]
        public static extern int GetPerceptionRow();
        [DllImport(DLL_NAME)]
        public static extern int GetPerceptionCol();
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetBPHapticsFeedback(float[,] _haptics_buffers, int _row_count, int _col_count);
        [DllImport(DLL_NAME)]
        private static extern void ComputeHaptics(int _id_hm, int _id_bp, float _x, float _y, float _z, float _time, bool _mute_tex = false, bool _mute_stiff = false, bool _mute_vib = false);
        [DllImport(DLL_NAME)]
        private static extern int AddHM(string _content);
        [DllImport(DLL_NAME)]
        private static extern bool UpdateHM(int _id, string _content);
        [DllImport(DLL_NAME)]
        public static extern float GetVibrationAmp(int _id, float _step);
        [DllImport(DLL_NAME)]
        public static extern float GetVibrationLength(int _id);
        [DllImport(DLL_NAME)]
        public static extern float GetTextureAmp(int _id, float _step);
        [DllImport(DLL_NAME)]
        public static extern float GetTextureLength(int _id);


        //Simple bytes to string parser
        private static string parseMaterial(UnityEngine.TextAsset _material)
        {
            if (_material == null)
                return "";

            string parse_string = "";

            for (int i = 0; i < _material.bytes.Length; i++)
            {
                //Convert one byte into on char and append to string
                parse_string += System.Convert.ToChar(_material.bytes[i]);
            }

            return parse_string;
        }

        public static void ComputeHaptics(int _id_hm, int _id_bp, UnityEngine.Vector3 _dists, bool _render_tex = true, bool _render_stiff = true, bool _render_vib = true)
        {
            ComputeHaptics(_id_hm, _id_bp, _dists.x, _dists.y, _dists.z, HapticManager.Instance.RealTime,
                           _render_tex && HapticManager.Instance.RenderTexture,
                           _render_stiff && HapticManager.Instance.RenderStiffness,
                           _render_vib && HapticManager.Instance.RenderVibration);
        }

        public static int AddHM(UnityEngine.TextAsset _material)
        {
            return AddHM(parseMaterial(_material));
        }

        public static bool UpdateHM(int _id, UnityEngine.TextAsset _material)
        {
            return UpdateHM(_id, parseMaterial(_material));
        }
    }
}
