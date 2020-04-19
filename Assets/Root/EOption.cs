#if UNITY_EDITOR
namespace FlatBuffers
{
    /// <summary>
    /// flatc [EOption] -o "ouput path" "input file fbs"
    /// </summary>
    public enum EOption : byte
    {
        [StringArgument("--csharp")] Csharp,
        [StringArgument("--gen-mutable")] Mutable,
        [StringArgument("--gen-onefile")] Onefile,
        [StringArgument("--gen-object-api")] Object,
    }
}
#endif