using System;
using FlatBufferGenerated.Simple2.Database;
using FlatBuffers;
using UnityEngine;

public class Demo2 : MonoBehaviour
{
    private void Start()
    {
        var builder = new FlatBufferBuilder(1);
        var nameOffset1 = builder.CreateString("Hero");
        var nameOffset2 = builder.CreateString("Hero2");
        var persons = new Offset<Enemy>[2];
        //persons[0] = Enemy.CreateEnemy(builder, 1, nameOffset1, EnemyType.Beginner);
        //persons[1] = Enemy.CreateEnemy(builder, 2, nameOffset2, EnemyType.Warrios);


        Enemy.StartEnemy(builder);
        Enemy.AddId(builder, 1);
        Enemy.AddName(builder, nameOffset1);
        Enemy.AddEnemyType(builder, EnemyType.Beginner);
        persons[0] = Enemy.EndEnemy(builder);

        Enemy.StartEnemy(builder);
        Enemy.AddId(builder, 2);
        Enemy.AddName(builder, nameOffset2);
        Enemy.AddEnemyType(builder, EnemyType.Warrios);
        persons[1] = Enemy.EndEnemy(builder);

        var enemyOffset = EnemyContainer.CreateEnemiesVector(builder, persons);

        EnemyContainer.StartEnemyContainer(builder);
        EnemyContainer.AddEnemies(builder, enemyOffset);
        var root = EnemyContainer.EndEnemyContainer(builder);
        builder.Finish(root.Value);

        FlatHelper.Save(builder, Application.persistentDataPath + "/simple2.wr");

        var byteBuffer = FlatHelper.Load(Application.persistentDataPath + "/simple2.wr");
        var enemyContainer = EnemyContainer.GetRootAsEnemyContainer(byteBuffer);

        // Debug.Log("id:" + person.Id + " name:" + person.Name + " gender:" + person.Gender + " age:" + person.Age);
        for (int i = 0; i < enemyContainer.EnemiesLength; i++)
        {
            var p = enemyContainer.Enemies(i);
            if (p != null)
            {
                Debug.Log(" id:" + p.Value.Id + " name:" + p.Value.Name + " type:" + p.Value.EnemyType);
            }
        }

        Debug.Log("----------------------------------------------------------");
    }
}