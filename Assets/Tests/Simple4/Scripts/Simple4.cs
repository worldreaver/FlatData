

using FlatBuffers.Attributes;

namespace Simple4.Database
{
    public enum EProfitFoodType : byte
    {
        TypeA = 1,
        TypeB = 2,
        TypeC = 3,
        TypeD = 4,
    }

    public enum ECountry : byte
    {
        NewYork = 1,
        VietNam = 2
    }

    public enum ERestaurant : byte
    {
        Donut = 1,
        Burger = 2,
        Coffee = 3,
    }

    public class FoodData
    {
        [FlatBuffersDefaultValue(EFoodType.Donut)]
        public EFoodType Type { get; set; }

        public string Name { get; set; }

        [FlatBuffersDefaultValue(ECountry.NewYork)]
        public ECountry Country { get; set; }

        [FlatBuffersDefaultValue(ERestaurant.Donut)]
        public ERestaurant Restaurant { get; set; }

        public int LevelUnlock { get; set; }

        [FlatBuffersDefaultValue(EProfitFoodType.TypeA)]
        public EProfitFoodType ProfitType { get; set; }
    }

    public class FoodProfit
    {
        [FlatBuffersDefaultValue(EProfitFoodType.TypeA)]
        public EProfitFoodType Type { get; set; }

        public int[] Price { get; set; }
        public int[] Profit { get; set; }
    }

    public enum EFoodType : byte
    {
        Donut = 1,
        ChocolateIcing = 2,
        MilkShake = 3,
        StrawberryIcing = 4,
        AlmondSlice = 5,
        Sprinkles = 6,
    }

    public class MasterFoodTable
    {
        public FoodProfit[] FoodProfitCollection { get; set; }
        public FoodData[] FoodDataCollection { get; set; }
    }
}
