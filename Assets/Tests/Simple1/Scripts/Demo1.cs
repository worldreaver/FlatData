using FlatBufferGenerated.Simple1.Database;
using FlatBuffers;
using UnityEngine;

public class Demo1 : MonoBehaviour
{
    private void Start()
    {
        var builder = new FlatBufferBuilder(1);

        //string need create before Start Reward
        var nameReward = builder.CreateString("Coin");
        var rd = Reward.CreateReward(builder, 0, nameReward, 110);
        builder.Finish(rd.Value);

        //save
        FlatHelper.Save(builder, Application.persistentDataPath + "/simple1.wr");

        //load
        var byteBuffer = FlatHelper.Load(Application.persistentDataPath + "/simple1.wr");
        var r = Reward.GetRootAsReward(byteBuffer);

        Debug.Log(" id:" + r.Id + " name:" + r.Name + " value:" + r.Value);
    }
}