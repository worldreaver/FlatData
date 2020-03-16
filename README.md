# FlatData

![CI](https://github.com/worldreaver/FlatData/workflows/CI/badge.svg?branch=master)

## Requirements
[![Unity 2019.3+](https://img.shields.io/badge/unity-2019.3+-brightgreen.svg?style=flat&logo=unity&cacheSeconds=2592000)](https://unity3d.com/get-unity/download/archive)
[![.NET 4.x Scripting Runtime](https://img.shields.io/badge/.NET-4.x-blueviolet.svg?style=flat&cacheSeconds=2592000)](https://docs.unity3d.com/2019.1/Documentation/Manual/ScriptingRuntimeUpgrade.html)


## Reference
	-Newtonsoft.Json v12.0.3
	-Google.Apis.Auth v1.43.0
	-Google.Apis.Core v1.43.0
	-Google.Apis v1.43.0
	-Google.Apis.Sheets.v4 v1.43.0.1839
	-FlatBuffer v1.12.0


# Brenchmark
* Serialize and Deserialize array 2000 item

|                       |     Type    |     Size    |  Deserialize  | Deserialize GC |  Serialize  | Serialize GC |
|-----------------------|-------------|-------------|---------------|----------------|-------------|--------------|
| `FlatBuffer`          |    binary   |    2.846    |    0.0528     |     0.2485     |   0.2745    |    0.7279    |
| `MessagePack-Csharp`  |    binary   |    1.000    |    1.0000     |     1.0000     |   1.0000    |    1.0000    |
| `JsonUtility`         |    text     |    2.657    |    0.0793     |     0.6279     |   0.0341    |    0.3851    |

* Searching item `MemoryMaster` is faster than `FlatBuffer`

* find item have id 1746 in array 2000 item
```csharp
var item = Find($"Id_1746");
```

|                       |     Find     | Find  GC |
|-----------------------|--------------|----------|
| `FlatBuffer`          |    1.76ms    |   335B   |
| `MemoryMaster`        |    0.7ms     |   48B    |


* find each item once in array
```csharp
for (int i = 1; i < 2000; i++)
{
    var item = Find($"Id_{i}");
}
```

|                       |     Find     |  Find  GC   |
|-----------------------|--------------|-------------|
| `FlatBuffer`          |    34.4ms    |   254.5KB   |
| `MemoryMaster`        |    4.17ms    |   179.2KB   |


* JsonUtility has many limits as:
	- not support array (you need write class helper to support this)
	- not support dictionary (you need custom class which acts as a dictionary item or using other custom serializer)
	- the float value serialize is not accurate due to the encoding problem. You can change float to string to get an accurate value.
	- not support binnay search as part of itself


# Type mapping

This project uses the following type mappings between .NET base types and FlatBuffers.

|.NET Type		| FlatBuffers Type	| Size (bits)	|
|---------------|-------------------|---------------|
| `sbyte`		| `byte`			| 8				|
| `byte`		| `ubyte`			| 8				|
| `bool`		| `bool`			| 8				|
| `short`		| `short`			| 16			|
| `ushort`		| `ushort`			| 16			|
| `int`			| `int`				| 32			|
| `uint`		| `uint`			| 32			|
| `long`		| `long`			| 64			|
| `ulong`		| `ulong`			| 64			|
| `float`		| `float`			| 32			|
| `double`		| `double`			| 64			|
| `enum`		| `enum`			| default: 32   |
| `string`		| `string`			| Varies		|
| `struct`		| `struct`			| Varies		|
| `class`		| `table`			| Varies		|


# Spreadsheet

* Spreadsheet title is parent table contain all sheet (as name root table)
* Name of each sheet is name table
* Name binary file generate will coincides with name root table (`nameroottable_binary`)

![spreadsheet](https://drive.google.com/uc?export=view&id=1AX6syNUgyKI8lW0ETB2mgPhP9QqC_wsn)

# Usages

* `fbs file` ---generate--> `Result file code`.
* `Raw file code` ---generate--> `Schema file (fbs)` ---generate--> `Result file code`.

* Each namespace contain all related type. If type A and type B have a composition relationship => A and B are in the same namespace (same raw file code) or want to put them together

* If there are distinct types in the namespace without a composition relationship, then a table containing all those types is required. If type A and type B no relationship => need type C contain type A and type B, it same as save array data type A

```csharp
table Enemy{}
table Character{}
table Village{
	enemies:[Enemy];
	characters:[Character];
}
```

```csharp
table Enemy{}
table Village{
	enemies:[Enemy];
}
```

* `Raw file code` can be delete after generate (keep it for the purpose of changing if needed)
* Because type in `Raw file code` is same name in `Result file code` (diffirent namespace) so IDE can suggestions it so you should put `Raw file code` in another assembly definition individual file only avaiable editor platform (only exists on the editor). If you do not want to delete it.

* Value of field in enum need start form 0, if value start diffirent 0 field define by enum need define default value with attribute `FlatBuffersDefaultValue`

```csharp
public enum EnemyType : byte
{
    Warrios, 	// same as Warrios = 0
    Magician, 	// 1
    Pirate,		// 2
    Thief,		// 3	
    Beginner,	// 4
}

public class Enemy
{
    public int Id { get; set; }
    public string Name { get; set; }
    public EnemyType EnemyType { get; set; }
}
```

```csharp
public enum EnemyType : byte
{
    Warrios = 1,
    Magician = 2,
    Pirate = 3,
    Thief = 4,
    Beginner = 5,
}

public class Enemy
{
    public int Id { get; set; }
    public string Name { get; set; }
    [FlatBuffersDefaultValue(EnemyType.Warrios)] //attribute define default value
    public EnemyType EnemyType { get; set; }
}
```


# Test

* ClassReflectedAsAStruct
```csharp
using FlatBuffers.Attributes;

namespace FlatBuffers.Tests.TestTypes
{
    [FlatBuffersStruct]
    public class ClassReflectedAsAStruct
    {
        public int A { get; set; }
        public byte B { get; set; }
    }
}
```

* EnumWithUserMetadata
```csharp
using FlatBuffers.Attributes;

namespace FlatBuffers.Tests.TestTypes
{
    [FlatBuffersMetadata("magicEnum")]
    public enum EnumWithUserMetadata
    {
        Cat, Dog, Fish
    }
}
```

* StructReflectedAsATable
```csharp
using FlatBuffers.Attributes;

namespace FlatBuffers.Tests.TestTypes
{
    [FlatBuffersTable]
    public struct StructReflectedAsATable
    {
        public int A { get; set; }
        public byte B { get; set; }
    }
}
```

* TableWithAlternativeNameFields
```csharp
using FlatBuffers.Attributes;

namespace FlatBuffers.Tests.TestTypes
{
    public class TableWithAlternativeNameFields
    {
        [FlatBuffersField(Name = "AltIntProp")]
        public int IntProp { get; set; }

        [FlatBuffersField(Name = "AltStringProp")]
        public string StringProp { get; set; } 
    }
}
```

* TableWithRequiredFields
```csharp
using System.Collections.Generic;
using FlatBuffers.Attributes;

namespace FlatBuffers.Tests.TestTypes
{
    public class TableWithRequiredFields
    {
        [FlatBuffersField(Required = true)]
        public string StringProp { get; set; }

        [FlatBuffersField(Required = true)]
        public TestTable1 TableProp { get; set; }

        [FlatBuffersField(Required = true)]
        public List<int> VectorProp { get; set; }
    }
}
```

* TableWithUserMetadata
```csharp
using FlatBuffers.Attributes;

namespace FlatBuffers.Tests.TestTypes
{
    [FlatBuffersMetadata("types")]
    public class TableWithUserMetadata
    {
        [FlatBuffersMetadata("priority", 1)]
        public int PropA { get; set; }

        [FlatBuffersMetadata("toggle", 1)]
        public bool PropB { get; set; }

        [FlatBuffersMetadata("category", "tests")]
        public int PropC { get; set; }
    }
}
```

* TestEnum
```csharp
using System;
using FlatBuffers.Attributes;

namespace FlatBuffers.Tests.TestTypes
{
    public enum TestEnum : byte
    {
        Apple,
        Orange,
        Pear,
        Banana,
    };

    [Flags]
    public enum TestFlagsEnum : byte
    {
        None = 0,
        Apple = 1<<0,
        Orange = 1<<1,
        Pear = 1<<2,
        Banana = 1<<3,
    };

    public enum TestEnumWithNoDeclaredBaseType
    {
        Apple,
        Orange,
        Pear,
        Banana,
    };

    public enum TestIntBasedEnum : int
    {
        Apple,
        Orange,
        Pear,
        Banana = Int32.MaxValue,
    };

    [FlatBuffersEnum(AutoSizeEnum = true)]
    public enum TestEnumAutoSizedToByte
    {
        Apple,
        Orange,
        Pear,
        Banana = byte.MaxValue,
    };

    [FlatBuffersEnum(AutoSizeEnum = true)]
    public enum TestEnumAutoSizedToSByte
    {
        Apple,
        Orange,
        Pear,
        Banana = sbyte.MaxValue,
    };

    [FlatBuffersEnum(AutoSizeEnum = true)]
    public enum TestEnumAutoSizedToShort
    {
        Apple,
        Orange,
        Pear,
        Banana = short.MaxValue,
    };

    [FlatBuffersEnum(AutoSizeEnum = true)]
    public enum TestEnumAutoSizedToUShort
    {
        Apple,
        Orange,
        Pear,
        Banana = ushort.MaxValue,
    };

    [FlatBuffersEnum(AutoSizeEnum = true)]
    public enum TestEnumWithExplictSizeNotAutoSized : short
    {
        Apple,
        Orange,
        Pear,
        Banana = byte.MaxValue,
    };
}
```

* TestEnumWithExplicitNonContigValues
```csharp
namespace FlatBuffers.Tests.TestTypes
{
    public enum TestEnumWithExplicitNonContigValues : byte
    {
        Apple = 2,
        Orange = 1,
        Pear = 5,
        Banana = 9,
    };

    public enum TestEnumWithExplicitValues : byte
    {
        Apple = 2,
        Orange = 1,
        Pear = 5,
        Banana = 0,
    };
}
```

* TestStruct
```csharp
using System;

namespace FlatBuffers.Tests.TestTypes
{
    public struct TestStruct1 : IEquatable<TestStruct1>
    {
        public int IntProp { get; set; }
        public byte ByteProp { get; set; }
        public short ShortProp { get; set; }

        public bool Equals(TestStruct1 other)
        {
            return IntProp == other.IntProp && ByteProp == other.ByteProp && ShortProp == other.ShortProp;
        }
    }

    public struct TestStruct2
    {
        public int IntProp { get; set; }
        public TestStruct1 TestStructProp { get; set; }
    }
}
```

* TestStructWithEnum
```csharp
namespace FlatBuffers.Tests.TestTypes
{
    public struct TestStructWithEnum
    {
        public TestEnum EnumProp { get; set; }
    }
}
```

* TestStructWithForcedAlignment
```csharp
using FlatBuffers.Attributes;

namespace FlatBuffers.Tests.TestTypes
{
    [FlatBuffersStruct(ForceAlign = 16)]
    public struct TestStructWithForcedAlignment
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}

```

* TestTable1
```csharp
namespace FlatBuffers.Tests.TestTypes
{
    public class TestTable1
    {
        public int IntProp { get; set; }
        public byte ByteProp { get; set; }
        public short ShortProp { get; set; }
    }
}
```

* TestTable1UsingFields
```csharp
using FlatBuffers.Attributes;

namespace FlatBuffers.Tests.TestTypes
{
    public class TestTable1UsingFields
    {
        public int IntProp;
        public byte ByteProp;
        public short ShortProp;
    }

    public class TestTable1UsingFieldsAndDefaultFieldValues
    {
        [FlatBuffersDefaultValue(424242)]
        public int IntProp = 424242;

        [FlatBuffersDefaultValue(22)]
        public byte ByteProp = 22;

        [FlatBuffersDefaultValue(1024)]
        public short ShortProp = 1024;
    }
}
```


* TestTable2
```csharp
namespace FlatBuffers.Tests.TestTypes
{
    public class TestTable2
    {
        public string StringProp { get; set; }
    }
}
```

* TestTable3
```csharp
namespace FlatBuffers.Tests.TestTypes
{
    public class TestTable3
    {
        public bool BoolProp { get; set; }

        public long LongProp { get; set; }
        public sbyte SByteProp { get; set; }
    
        public ushort UShortProp { get; set; }

        public ulong ULongProp { get; set; }

        public TestEnum EnumProp { get; set; }

        public float FloatProp { get; set; }

        public double DoubleProp { get; set; }
    }
}
```

* TestTableWithArray
```csharp
using System.Collections.Generic;

namespace FlatBuffers.Tests.TestTypes
{
    public class TestTableWithArray
    {
        public int[] IntArray{ get; set; }
        public List<int> IntList { get; set; }
    }
}
```

* TestTableWithArrayOfBytes
```csharp
using System.Collections.Generic;

namespace FlatBuffers.Tests.TestTypes
{
    public class TestTableWithArrayOfBytes
    {
        public byte[] ByteArrayProp { get; set; }
        public List<byte> ByteListProp { get; set; }
    }
}
```

* TestTableWithArrayOfStrings
```csharp
using System.Collections.Generic;

namespace FlatBuffers.Tests.TestTypes
{
    public class TestTableWithArrayOfStrings
    {
        public string[] StringArrayProp { get; set; }
        public List<string> StringListProp { get; set; }
    }
}
```

* TestTableWithArrayOfStructs
```csharp
namespace FlatBuffers.Tests.TestTypes
{
    public class TestTableWithArrayOfStructs
    {
        public TestStruct1[] StructArray { get; set; }
    }
}
```

* TestTableWithArrayOfTables
```csharp
using System.Collections.Generic;

namespace FlatBuffers.Tests.TestTypes
{
    public class TestTableWithArrayOfTables
    {
        public TestTable1[] TableArrayProp { get; set; }
        public List<TestTable1> TableListProp { get; set; }
    }
}
```


* TestTableWithComments
```csharp
namespace FlatBuffers.Tests.TestTypes
{
    [FlatBuffersComment("This is a comment on a table")]
    public class TestTableWithComments
    {
        [FlatBuffersComment("Comment on an int field")]
        public int Field { get; set; }

        [FlatBuffersComment(0, "First comment of Multiple comments")]
        [FlatBuffersComment(1, "Second comment of Multiple comments")]
        public string StringField { get; set; }
        
        public int AnotherField { get; set; }
    }
}
```


* TestTableWithDefaults
```csharp
using FlatBuffers.Attributes;

namespace FlatBuffers.Tests.TestTypes
{
    public class TestTableWithDefaults
    {
        [FlatBuffersDefaultValue(123456)]
        public int IntProp { get; set; }

        [FlatBuffersDefaultValue(42)]
        public byte ByteProp { get; set; }

        [FlatBuffersDefaultValue(1024)]
        public short ShortProp { get; set; }
    }
}
```


* TestTableWithIdentifier
```csharp
using FlatBuffers.Attributes;

namespace FlatBuffers.Tests.TestTypes
{
    [FlatBuffersTable(Identifier = "TEST")]
    public class TestTableWithIdentifier
    {
        public int IntProp { get; set; }
    }
}
```


* TestTableWithDeprecatedField
```csharp
using FlatBuffers.Attributes;

namespace FlatBuffers.Tests.TestTypes
{
    public class TestTableWithDeprecatedField
    {
        public const byte DefaultBytePropValue = 127;

        public TestTableWithDeprecatedField()
        {
            ByteProp = DefaultBytePropValue;
        }

        public int IntProp { get; set; }

        [FlatBuffersField(Deprecated = true)]
        public byte ByteProp { get; set; }
        public short ShortProp { get; set; }
    }
}
```


* TestTableWithKey
```csharp
using FlatBuffers.Attributes;

namespace FlatBuffers.Tests.TestTypes
{
    public class TestTableWithKey
    {
        [FlatBuffersField(Key = true)]
        public int IntProp { get; set; }
        public int OtherProp { get; set; }
    }

    /// <summary>
    /// Example of a class that will throw an error on reflection - it has 2 key fields
    /// </summary>
    public class TestTableWith2Keys
    {
        [FlatBuffersField(Key = true)]
        public int IntProp { get; set; }

        [FlatBuffersField(Key = true)]
        public int OtherProp { get; set; }
    }

    public class TestTableWithKeyOnBadType
    {
        [FlatBuffersField(Key = true)]
        public TestTable1 OtherProp { get; set; }
    }
}
```


* TestTableWithNestedTestTable1
```csharp
using FlatBuffers.Attributes;

namespace FlatBuffers.Tests.TestTypes
{
    public class TestTableWithNestedTestTable1
    {
        public int IntProp { get; set; }

        [FlatBuffersField(NestedFlatBufferType = typeof(TestTable1))]
        public object Nested { get; set; }
    }
}
```

* TestTableWithOriginalOrdering
```csharp
using FlatBuffers.Attributes;

namespace FlatBuffers.Tests.TestTypes
{
    [FlatBuffersTable(OriginalOrdering = true)]
    public class TestTableWithOriginalOrdering
    {
        public int IntProp { get; set; }

        public byte ByteProp { get; set; }

        public short ShortProp { get; set; }
    }
}
```

* TestTableWithStruct
```csharp
namespace FlatBuffers.Tests.TestTypes
{
    public class TestTableWithStruct 
    {
        public TestStruct1 StructProp { get; set; }
        public int IntProp { get; set; }
    }
}
```

* TestTableWithTable
```csharp
namespace FlatBuffers.Tests.TestTypes
{
    public class TestTableWithTable
    {
        public TestTable1 TableProp { get; set; }
        public int IntProp { get; set; }
    }
}
```

* TestTableWithUnion
```csharp
using FlatBuffers.Attributes;

namespace FlatBuffers.Tests.TestTypes
{
    public class TestTableWithUnion
    {
        public int IntProp { get; set; }

        [FlatBuffersField(UnionType = typeof(TestUnion))]
        public object UnionProp { get; set; }
    }

    public class TestTableWithUnionAndMoreFields
    {
        public int IntProp { get; set; }

        [FlatBuffersField(UnionType = typeof(TestUnion))]
        public object UnionProp { get; set; }

        public string StringProp { get; set; }

        public float FloatProp { get; set; }

        public double DoubleProp { get; set; }
    }

    public class TestTableWithUnionAndCustomOrdering
    {
        [FlatBuffersField(Id = 1)]
        public int IntProp { get; set; }

        [FlatBuffersField(UnionType = typeof(TestUnion), Id = 0)]
        public object UnionProp { get; set; }
    }
}
```

* TestTableWithUserOrdering
```csharp
using FlatBuffers.Attributes;

namespace FlatBuffers.Tests.TestTypes
{
    public class TestTableWithUserOrdering
    {
        [FlatBuffersField(Id = 2)]
        public int IntProp { get; set; }

        [FlatBuffersField(Id = 0)]
        public byte ByteProp { get; set; }

        [FlatBuffersField(Id = 1)]
        public short ShortProp { get; set; }
    }
}
```

* TestUnion
```csharp
using FlatBuffers.Attributes;

namespace FlatBuffers.Tests.TestTypes
{
    [FlatBuffersUnion]
    public enum TestUnion
    {
        [FlatBuffersUnionMember(typeof(TestTable1))]
        TestTable1,

        [FlatBuffersUnionMember(typeof(TestTable2))]
        TestTable2
    }

    public class TestTableUsingUnion
    {
    	public TestUnion UnionProp{ get; set; }
    }
}

```