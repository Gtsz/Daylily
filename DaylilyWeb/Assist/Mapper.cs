﻿using DaylilyWeb.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DaylilyWeb.Assist
{
    public static class Mapper
    {
        static Dictionary<string, string> dClassName = new Dictionary<string, string>();
        static JsonSettings Plugins = new JsonSettings();
        static string pluginDir = Path.Combine(Environment.CurrentDirectory, "plugins");
        public static void Init()
        {
            dClassName.Add("roll", "Roll");
            dClassName.Add("ping", "Ping");
            if (!Directory.Exists(pluginDir))
                Directory.CreateDirectory(pluginDir);

            if (!File.Exists(Path.Combine(pluginDir, "plugins.json")))
            {
                JsonSettings js = new JsonSettings();
                var ok = new Dictionary<string, string>();
                ok.Add("echo", "Just");
                ok.Add("for", "For");
                var ok2 = new Dictionary<string, string>();
                ok2.Add("templet", "Templet");
                ok2.Add("something", "Something");
                js.Plugins.Add("DaylilyPlugins.dll", ok);
                js.Plugins.Add("Test.dll", ok2);
                string contents = ConvertJsonString(JsonConvert.SerializeObject(js));

                File.WriteAllText(Path.Combine(pluginDir, "plugins.json"), contents);
            }
            else
            {
                string settings = File.ReadAllText(Path.Combine(pluginDir, "plugins.json"));
                Plugins = JsonConvert.DeserializeObject<JsonSettings>(settings);
            }
        }
        public static string GetClassName(string name, out string fileName)
        {
            foreach (KeyValuePair<string, Dictionary<string, string>> kvp in Plugins.Plugins)
            {
                if (kvp.Value.Keys.Contains(name))
                {
                    fileName = Path.Combine(pluginDir, kvp.Key);
                    return kvp.Value[name];
                }
            }
            fileName = null;
            if (!dClassName.Keys.Contains(name))
                return null;
            return dClassName[name];
        }
        private static string ConvertJsonString(string str)
        {
            //格式化json字符串  
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = serializer.Deserialize(jtr);
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }
    }
}