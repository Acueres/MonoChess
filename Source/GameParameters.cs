using System.IO;
using System.Reflection;

using Newtonsoft.Json;

using MonoChess.Algorithms;

namespace MonoChess
{
    public class GameParameters
    {
        public Sides PlayerSide { get; set; } = Sides.White;
        public Sides CurrentSide { get; set; } = Sides.White;
        public bool SinglePlayer { get; set; }
        public bool ShowGrid { get; set; }
        public Algorithm AlgorithmType { get; set; }
        public int Depth { get; set; } = 3;
        public int[] PiecesData { get; set; }
        public bool[] CastlingData { get; set; }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText("save_data.json", json);
        }

        public bool Load()
        {
            if (!File.Exists(@"save_data.json")) return false;

            string data = File.ReadAllText("save_data.json");
            var loaded = JsonConvert.DeserializeObject<GameParameters>(data);

            if (loaded.PiecesData == null) return false;

            foreach (var property in loaded.GetType().GetProperties())
            {
                var value = property.GetValue(loaded);
                PropertyInfo propertyInfo = GetType().GetProperty(property.Name);
                propertyInfo.SetValue(this, value);
            }

            return true;
        }
    }
}
