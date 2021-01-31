using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace GenshinbotCsharp
{
    class ScreenConfig
    {
        
        /// <summary>
        /// config for a single setting
        /// </summary>
        public class Configuration:IEnumerable<KeyValuePair<string, object>>
        {
            private Dictionary<string, object> config;
            public bool Has<T>(string key) => config.ContainsKey(key) && (config[key] is T);

            public T Get<T>(string key)
            {
                if (!config.ContainsKey(key))
                    throw new Exception("key doesn't exist");
                if (config is T obj)
                    return obj;
                else
                    throw new Exception(key + " is not type "+typeof(T).Name);
            }

            public void Set(string key, object value) => config[key] = value;

            public Dictionary<string, object>.KeyCollection GetKeys() => config.Keys;

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => config.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => config.GetEnumerator();
        }

       
        private Dictionary<Size, Configuration> configs;

        public Configuration DefaultConfig => configs[Size.Zero];

        /*public bool Has<T>(string key, Size size)
        {

        }

        public T Get<T>(string key, Size size)
        {
            
        }*/


    }
}
