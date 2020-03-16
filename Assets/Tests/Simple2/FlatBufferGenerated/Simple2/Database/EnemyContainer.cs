// <auto-generated>
//  automatically generated by the FlatBuffers compiler, do not modify
// </auto-generated>

namespace FlatBufferGenerated.Simple2.Database
{

using global::System;
using global::System.Collections.Generic;
using global::FlatBuffers;

public struct EnemyContainer : IFlatbufferObject
{
  private Table __p;
  public ByteBuffer ByteBuffer { get { return __p.bb; } }
  public static void ValidateVersion() { FlatBufferConstants.FLATBUFFERS_1_12_0(); }
  public static EnemyContainer GetRootAsEnemyContainer(ByteBuffer _bb) { return GetRootAsEnemyContainer(_bb, new EnemyContainer()); }
  public static EnemyContainer GetRootAsEnemyContainer(ByteBuffer _bb, EnemyContainer obj) { return (obj.__assign(_bb.GetInt(_bb.Position) + _bb.Position, _bb)); }
  public void __init(int _i, ByteBuffer _bb) { __p = new Table(_i, _bb); }
  public EnemyContainer __assign(int _i, ByteBuffer _bb) { __init(_i, _bb); return this; }

  public FlatBufferGenerated.Simple2.Database.Enemy? Enemies(int j) { int o = __p.__offset(4); return o != 0 ? (FlatBufferGenerated.Simple2.Database.Enemy?)(new FlatBufferGenerated.Simple2.Database.Enemy()).__assign(__p.__indirect(__p.__vector(o) + j * 4), __p.bb) : null; }
  public int EnemiesLength { get { int o = __p.__offset(4); return o != 0 ? __p.__vector_len(o) : 0; } }

  public static Offset<FlatBufferGenerated.Simple2.Database.EnemyContainer> CreateEnemyContainer(FlatBufferBuilder builder,
      VectorOffset enemiesOffset = default(VectorOffset)) {
    builder.StartTable(1);
    EnemyContainer.AddEnemies(builder, enemiesOffset);
    return EnemyContainer.EndEnemyContainer(builder);
  }

  public static void StartEnemyContainer(FlatBufferBuilder builder) { builder.StartTable(1); }
  public static void AddEnemies(FlatBufferBuilder builder, VectorOffset enemiesOffset) { builder.AddOffset(0, enemiesOffset.Value, 0); }
  public static VectorOffset CreateEnemiesVector(FlatBufferBuilder builder, Offset<FlatBufferGenerated.Simple2.Database.Enemy>[] data) { builder.StartVector(4, data.Length, 4); for (int i = data.Length - 1; i >= 0; i--) builder.AddOffset(data[i].Value); return builder.EndVector(); }
  public static VectorOffset CreateEnemiesVectorBlock(FlatBufferBuilder builder, Offset<FlatBufferGenerated.Simple2.Database.Enemy>[] data) { builder.StartVector(4, data.Length, 4); builder.Add(data); return builder.EndVector(); }
  public static void StartEnemiesVector(FlatBufferBuilder builder, int numElems) { builder.StartVector(4, numElems, 4); }
  public static Offset<FlatBufferGenerated.Simple2.Database.EnemyContainer> EndEnemyContainer(FlatBufferBuilder builder) {
    int o = builder.EndTable();
    return new Offset<FlatBufferGenerated.Simple2.Database.EnemyContainer>(o);
  }
};


}
