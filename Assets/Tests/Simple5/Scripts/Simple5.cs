using FlatBuffers.Attributes;

namespace Worldreaver.Idle.User
{
    public class UserProfile
    {
        public string Id { get; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int CurrentExp { get; set; }
        public string SoftCurrency { get; set; }
        public string HardCurrency { get; set; }
        public string MilkCurrency { get; set; }
        public int Revolution { get; set; }
    }

    public class UserWaveProgress
    {
        public int CurrentLevel { get; private set; }
        public int CurrentWave { get; private set; }
    }
}