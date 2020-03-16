

using System;
using FlatBufferGenerated.Simple3.Database;
using FlatBuffers;
using UnityEngine;

public class Demo3 : MonoBehaviour
{
    private void Start()
    {
        var builder = new FlatBufferBuilder(1);
        
        // WeaponUnion weaponType = WeaponUnion.Sword;
        // Sword.StartSword(builder);
        // Sword.AddDamage(builder, 123);
        // Sword.AddDistance(builder, 999);
        // var offsetWeapon = Sword.EndSword(builder);
        
        WeaponUnion weaponType = WeaponUnion.Gun;
        Gun.StartGun(builder);
        Gun.AddDamage(builder, 1200);
        Gun.AddReloadSpeed(builder, 10);
        var offsetWeapon = Gun.EndGun(builder);
        
        var nameData = builder.CreateString("Test name! time:" + DateTime.Now);
        
        GameData.StartGameData(builder);
        GameData.AddName(builder, nameData);
        GameData.AddPosition(builder, Vec3.CreateVec3(builder, 1, 1, 1));
        GameData.AddColor(builder, EColor.Green);
        
        GameData.AddWeaponType(builder, weaponType);
        GameData.AddWeapon(builder, offsetWeapon.Value);
        
        var offset = GameData.EndGameData(builder);
        builder.Finish(offset.Value);
        
        FlatHelper.Save(builder, Application.persistentDataPath + "/simple3.wr");
        
        var byteBuffer = FlatHelper.Load(Application.persistentDataPath + "/simple3.wr");
        var game = GameData.GetRootAsGameData(byteBuffer);
        Debug.Log("name :" + game.Name);
        if (game.Position != null)
        {
            Debug.Log("postion : (" + game.Position.Value.X + "," + game.Position.Value.Y + "," + game.Position.Value.Z + ")");
        }
        
        Debug.Log("color :" + game.Color);
        Debug.Log("weapon type :" + game.WeaponType);
        
        switch (game.WeaponType)
        {
            case WeaponUnion.NONE:
                break;
            case WeaponUnion.Sword:
                var sword = game.Weapon<Sword>();
                if (sword != null)
                {
                    Debug.Log("sword damage :" + sword.Value.Damage);
                    Debug.Log("sword distance :" + sword.Value.Distance);
                }
        
                break;
            case WeaponUnion.Gun:
                var gun = game.Weapon<Gun>();
                if (gun != null)
                {
                    Debug.Log("gun damage :" + gun.Value.Damage);
                    Debug.Log("gun reload speed :" + gun.Value.ReloadSpeed);
                }
        
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        Debug.Log("----------------------------------------------------------");
    }
}