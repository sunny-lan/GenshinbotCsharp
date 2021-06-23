using OpenCvSharp;
using System;
using System.Text.Json.Serialization;

namespace genshinbot.data
{
    public class SavableMatSubset
    {
        public string Path { get; set; }
        public ImreadModes ImreadMode { get; set; } = ImreadModes.Unchanged;
    }
    public class SavableMat : SavableMatSubset
    {
        private Mat? _value;
        public Mat Value
        {
            get
            {
                if (_value is Mat m) return _value;
                else
                {
                    _value = Data.Imread(this.Path, this.ImreadMode);
                    return _value;
                }
            }
            set
            {
                _value?.Dispose();
                _value = value;
            }
        }
        ~SavableMat()
        {
            Value?.Dispose();
        }
    }
}