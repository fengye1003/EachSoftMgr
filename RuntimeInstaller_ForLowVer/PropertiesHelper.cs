﻿using System;
using System.Collections;
using System.IO;
using System.Text;

/*
 * @author : RoadToTheExpert
 * @date : 2019.07
 * @file : Properties.cs
 */

//Re-edited by Fengye1003
//Updated by Each. Tech.
//Completed at 2022/07/23 12:15


namespace RuntimeInstaller_ForLowVer
{
    public class PropertiesHelper
    {
        public static Hashtable Load(string file)
        {
            Hashtable ht = new Hashtable(16);
            string content;
            try
            {
                content = File.ReadAllText(file, Encoding.UTF8);
            }
            catch
            {
                return null;
            }
            string[] rows = content.Split('\n');
            foreach (string c in rows)
            {
                if (c.Trim().Length == 0)
                    continue;
                string[] kv = c.Split('=');
                if (kv.Length == 1)
                {
                    ht[kv[0].Trim()] = "";
                }
                else if (kv.Length == 2)
                {
                    ht[kv[0].Trim()] = kv[1].Trim();
                }
            }
            return ht;
        }

        public static bool Save(string file, Hashtable ht)
        {
            if (ht == null || ht.Count == 0)
                return false;
            StringBuilder sb = new StringBuilder(ht.Count * 12);
            foreach (string k in ht.Keys)
            {
                sb.Append(k).Append('=').Append(ht[k]).Append(Environment.NewLine);
            }
            try
            {
                File.WriteAllText(file, sb.ToString(), Encoding.UTF8);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Hashtable FixProperties(Hashtable htStandard, string path)
        {

            Hashtable ht = Load(path);
            if (ht == null)
            {
                ht = new();
            }
            foreach (string key in htStandard.Keys)
            {
                if (!ht.Contains(key)) ht.Add(key, htStandard[key] as string);
            }
            Save(path, ht);
            Log.SaveLog("Hashtable fixed.");
            return ht;
        }

        public static Hashtable AutoCheck(Hashtable htStandard, string path)
        {
            bool isFixed = true;
            Hashtable ht = Load(path);
            if (ht == null)
            {
                isFixed = false;
            }
            else
            {
                foreach (string key in htStandard.Keys)
                {
                    if (!ht.Contains(key)) isFixed = false;
                }
            }

            if (!isFixed)
                return FixProperties(htStandard, path);
            else
                return ht;
        }

    }
}