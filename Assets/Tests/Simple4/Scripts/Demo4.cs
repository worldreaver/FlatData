

using System;
using FlatBufferGenerated.Simple4.Database;
using FlatBuffers;
using UnityEngine;

public class Demo4 : MonoBehaviour
{
    private void Start()
    {
        var byteBuffer = FlatHelper.Load(Application.dataPath + "/Tests/Simple4/Binary/test.wr");
        var master = MasterFoodTable.GetRootAsMasterFoodTable(byteBuffer);

        for (int i = 0; i < master.FoodDataCollectionLength; i++)
        {
            var p = master.FoodDataCollection(i);
            if (p != null)
            {
                Debug.Log(" Type:" + p.Value.Type + " Name:" + p.Value.Name + " Restaurant:" + p.Value.Restaurant + " LevelUnlock:" + p.Value.LevelUnlock + " ProfitType:" + p.Value.ProfitType);

                for (int j = 0; j < master.FoodProfitCollectionLength; j++)
                {
                    var profit = master.FoodProfitCollection(j);

                    if (profit != null && profit.Value.Type == p.Value.ProfitType)
                    {
                        foreach (var i1 in profit.Value.GetPriceArray())
                        {
                            Debug.Log("Price :" + i1);
                        }

                        foreach (var i1 in profit.Value.GetProfitArray())
                        {
                            Debug.Log("Profit :" + i1);
                        }

                        break;
                    }
                }
            }
        }
    }
}