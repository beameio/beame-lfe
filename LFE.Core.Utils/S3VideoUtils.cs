using System;
using System.Collections.Generic;
using System.Linq;

namespace LFE.Core.Utils
{
    public static class S3VideoUtils
    {
        public enum ES3Presets
        {
            HLS_TWO_M
            ,HLS_ONE_HALF_M
            ,HLS_ONE_M
            ,HLS_600_K
            ,HLS_400_K
            ,GEN_1080P
            ,GEN_720P
            ,GEN_480P
            ,GEN_360P
        }

        public static Dictionary<ES3Presets,string> S3PresetsDictionary = new Dictionary<ES3Presets, string>
        {
             {ES3Presets.HLS_TWO_M, "1351620000001-200010"}
            ,{ES3Presets.HLS_ONE_HALF_M, "1351620000001-200020"}
            ,{ES3Presets.HLS_ONE_M, "1351620000001-200030"}
            ,{ES3Presets.HLS_600_K, "1351620000001-200040"}
            ,{ES3Presets.HLS_400_K, "1351620000001-200050"}
            ,{ES3Presets.GEN_1080P, "1435913540637-adauu0"}
            ,{ES3Presets.GEN_720P, "1435913635120-0n5cf3"}
            ,{ES3Presets.GEN_480P, "1351620000001-000020"}
            ,{ES3Presets.GEN_360P, "1351620000001-000040"}
        };

        public static string GetPresetIdByType(this ES3Presets type)
        {
            string preset;

            return S3PresetsDictionary.TryGetValue(type, out preset) ? preset : string.Empty;
        }

        public static ES3Presets? GetTypeByPresetId(this string preset)
        {
            try
            {
                return S3PresetsDictionary.FirstOrDefault(x => x.Value == preset).Key;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string CombineVideoiPlayListFileUrl(this string name, string bucket, string prefix)
        {
            return String.Format("{0}{1}/{2}{3}.m3u8", Constants.S3_ROOT_URL, bucket, prefix, name);
        }
    }
}
