// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace FlatBufferGenerated.Simple3.Database
{

using global::System;
using global::System.Collections.Generic;
using global::FlatBuffers;

public struct Sword : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
  public static Sword GetRootAsSword(ByteBuffer _bb) { return GetRootAsSword(_bb, new Sword()); }
  public static Sword GetRootAsSword(ByteBuffer _bb, Sword obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public Sword __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public int Damage { get { int o = __p.__offset(4); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public bool MutateDamage(int Damage) { int o = __p.__offset(4); if (o != 0) { __p.bb.PutInt(o + __p.bb_pos, Damage); return true; } else { return false; } }
  public int Distance { get { int o = __p.__offset(6); return o != 0 ? __p.bb.GetInt(o + __p.bb_pos) : (int)0; } }
  public bool MutateDistance(int Distance) { int o = __p.__offset(6); if (o != 0) { __p.bb.PutInt(o + __p.bb_pos, Distance); return true; } else { return false; } }

  public static Offset<FlatBufferGenerated.Simple3.Database.Sword> CreateSword(FlatBufferBuilder builder,
      int Damage = 0,
      int Distance = 0) {
    builder.StartTable(2);
    Sword.AddDistance(builder, Distance);
    Sword.AddDamage(builder, Damage);
    return Sword.EndSword(builder);
  }

  public static void StartSword(FlatBufferBuilder builder) { builder.StartTable(2); }
  public static void AddDamage(FlatBufferBuilder builder, int Damage) { builder.AddInt(0, Damage, 0); }
  public static void AddDistance(FlatBufferBuilder builder, int Distance) { builder.AddInt(1, Distance, 0); }
  public static Offset<FlatBufferGenerated.Simple3.Database.Sword> EndSword(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<FlatBufferGenerated.Simple3.Database.Sword>(o);
  }
};


}
