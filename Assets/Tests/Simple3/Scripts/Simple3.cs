

using FlatBuffers.Attributes;

namespace Simple3.Database
{
    public enum EColor
    {
        Red,
        Green,
        Blue
    }

    public struct Vec3
    {
        public float x;
        public float y;
        public float z;
    
        public Vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public class Sword
    {
        public int Damage { get; set; }
        public int Distance { get; set; }
    }

    public class Gun
    {
        public int Damage { get; set; }
        public int ReloadSpeed { get; set; }
    }

    [FlatBuffersUnion]
    public enum WeaponUnion
    {
        [FlatBuffersUnionMember(typeof(Sword))]
        Sword,

        [FlatBuffersUnionMember(typeof(Gun))] Gun,
    }


    public class GameData
    {
        public Vec3 Position { get; set; }
        public int Mana { get; set; }
        public int Hp { get; set; }
        public string Name { get; set; }
        public byte[] inventory;
        public EColor Color { get; set; }
        [FlatBuffersField(UnionType = typeof(WeaponUnion))]
        public WeaponUnion Weapon { get; set; }
    }
}