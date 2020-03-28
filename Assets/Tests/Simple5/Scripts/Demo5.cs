using FlatBufferGenerated.Worldreaver.Idle.User;
using FlatBuffers;
using UnityEngine;
using UnityEngine.Profiling;

public class Demo5 : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        var builder = new FlatBufferBuilder(1);

        // var id = builder.CreateString("12345");
        // var userName = builder.CreateString("Hero");
        // var coin = builder.CreateString("0");
        // var gem = builder.CreateString("0");
        // var milk = builder.CreateString("0");
        //
        // UserProfile.StartUserProfile(builder);
        // UserProfile.AddId(builder, id);
        // UserProfile.AddLevel(builder, 1);
        // UserProfile.AddName(builder, userName);
        // UserProfile.AddRevolution(builder, 0);
        // UserProfile.AddCurrentExp(builder, 0);
        // UserProfile.AddHardCurrency(builder, gem);
        // UserProfile.AddMilkCurrency(builder, milk);
        // UserProfile.AddSoftCurrency(builder, coin);
        //
        // var userOffset = UserProfile.EndUserProfile(builder);
        // builder.Finish(userOffset.Value);
        //
        // FlatHelper.Save(builder, Application.persistentDataPath + "/user.wr");

        var byteBuffer = FlatHelper.Load(Application.persistentDataPath + "/user.wr");
        var user = UserProfile.GetRootAsUserProfile(byteBuffer);
        Debug.Log("id:" + user.Id);
        Debug.Log("level:" + user.Level);
        Debug.Log("name:" + user.Name);
        Debug.Log("revolution:" + user.Revolution);
        Debug.Log("coin:" + user.SoftCurrency);
        Debug.Log("gem:" + user.HardCurrency);
        Debug.Log("milk:" + user.MilkCurrency);

        var monster2 = user.UnPack();
        Profiler.BeginSample("track unpack");
        for (int i = 0; i < 100000; i++)
        {
            var monster3 = user.UnPack();
        }

        Profiler.EndSample();

        Profiler.BeginSample("track pack");
        for (int i = 0; i < 100000; i++)
        {
            UserProfile.Pack(builder, monster2);
        }

        Profiler.EndSample();


        // monster2.Name = "darknight";
        // Debug.Log(monster2.Id);
        // Debug.Log(monster2.Name);
        //     
        // var fbb = new FlatBufferBuilder(1);
        // fbb.Finish(UserProfile.Pack(fbb, monster2).Value);
        // FlatHelper.Save(fbb, Application.persistentDataPath + "/user.wr");
        //
        // byteBuffer = FlatHelper.Load(Application.persistentDataPath + "/user.wr");
        // user = UserProfile.GetRootAsUserProfile(byteBuffer);
        // Debug.Log("id:" + user.Id);
        // Debug.Log("level:" + user.Level);
        // Debug.Log("name:" + user.Name);
        // Debug.Log("revolution:" + user.Revolution);
        // Debug.Log("coin:" + user.SoftCurrency);
        // Debug.Log("gem:" + user.HardCurrency);
        // Debug.Log("milk:" + user.MilkCurrency);
    }
}