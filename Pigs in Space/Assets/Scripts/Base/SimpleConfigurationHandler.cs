﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Base
{
    public enum FileAccessor { TerrainGeneration }

    /// <summary>
    /// This class handles simple key:value predefined configuration files, for known primitives.
    /// </summary>
    public static class SimpleConfigurationHandler
    {
        #region static dictionaries

        private static readonly Dictionary<string, Dictionary<string, string>> sr_navigator = new Dictionary<string, Dictionary<string, string>>();

        #endregion static dictionaries

        #region public methods

        public static UInt16 GetUintProperty(string str, FileAccessor access)
        {
            return Convert.ToUInt16(GetStringProperty(str, access));
        }

        public static object GetStringProperty(string str, FileAccessor access)
        {
            CheckAndInit();
            return sr_navigator.Get(access.ToString(), "File Handler navigator").Get(str.ToLower(), "{0} dictionary".FormatWith(access));
        }

        public static Int32 GetIntProperty(string str, FileAccessor access)
        {
            return Convert.ToInt32(GetStringProperty(str, access));
        }

        public static float GetFloatProperty(string str, FileAccessor access)
        {
            return Convert.ToSingle(GetStringProperty(str, access));
        }

        #endregion public methods

        #region private methods

        private static void ReadFromFile(string str)
        {
            var dict = new Dictionary<string, string>();
            sr_navigator.Add(str, dict);
            char[] delimiters = { '=' };
            Debug.Log("Config/{0}".FormatWith(str));
            TextAsset targetFile = Resources.Load<TextAsset>("Config/{0}".FormatWith(str));
            string[] text = targetFile.text.Split('\n');
            foreach (string entry in text)
            {
                string[] temp = entry.Split(delimiters);
                dict.Add(temp[0].Trim().ToLower(), temp[1].Trim());
            }
        }

        private static void ReadFiles(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                ReadFromFile(file);
            }
        }

        private static void CheckAndInit()
        {
            lock (sr_navigator)
            {
                if (sr_navigator.Count == 0)
                {
                    ReadFiles(Enum.GetNames(typeof(FileAccessor)));
                }
            }
        }

        #endregion private methods
    }
}