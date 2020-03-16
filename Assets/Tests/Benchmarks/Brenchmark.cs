using FlatBufferGenerated.BrenchmarkFlatBuffer;
using FlatBuffers;
using FlatBuffers.Attributes;
using UnityEngine;
using UnityEngine.Profiling;

public class Brenchmark : MonoBehaviour
{
    private const string PATH = @"D:\yenmoc\Workspace\yenmoc\worldreaver\FlatData\Assets\Tests\Benchmarks\BrenchmarkMasterTable_binary.wr";

    private void Start()
    {
        Profiler.BeginSample("track load flat-buffer");
        var byteBuffer = FlatHelper.Load(PATH);
        var master = BrenchmarkMasterTable.GetRootAsBrenchmarkMasterTable(byteBuffer);
        Profiler.EndSample();

        for (int i = 0; i < master.DataLength; i++)
        {
            var item = master.Data(i);
            if (item != null)
            {
                Debug.Log("Id:" + item.Value.Id + "  Name:" + item.Value.Name + "  Hp:" + item.Value.Hp);
            }
        }

        Debug.Log("===========~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~=========");

        for (int i = 0; i < master.TestArrayLength; i++)
        {
            var item = master.TestArray(i);
            if (item != null)
            {
                Debug.Log("Id:" + item.Value.Id + "  Name:" + item.Value.Name);
                for (int j = 0; j < item.Value.MissionNamesLength; j++)
                {
                    Debug.Log(item.Value.MissionNames(j));
                }
            }

            Debug.Log("----------------------------------------------------");
        }
    }
}


namespace BrenchmarkFlatBuffer
{
    public class ItemA
    {
        [FlatBuffersField(Key = true)] public string Id { get; set; }
        public string Name { get; set; }
        public int Hp { get; set; }
    }

    public class ItemB
    {
        [FlatBuffersField(Key = true)] public int Id { get; set; }
        public string Name { get; set; }
        public string[] MissionNames { get; set; }
        public int[] MissionValues { get; set; }
    }

    public class BrenchmarkMasterTable
    {
        public ItemA[] Data { get; set; }
        public ItemB[] TestArray { get; set; }
    }
}