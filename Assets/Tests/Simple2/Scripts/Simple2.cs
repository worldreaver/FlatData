

namespace Simple2.Database
{
    public enum EnemyType : byte
    {
        Warrios,
        Magician,
        Pirate,
        Thief,
        Beginner,
    }

    public class Enemy
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public EnemyType EnemyType { get; set; }
    }

    public class EnemyContainer
    {
        public Enemy[] enemies;
    }
}