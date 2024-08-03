#if NETSTANDARD2_0

// These Annotations are Part of .NET Standard 2.1 and .NET Core 3.0+. Defining them here allows their usage to support
// developers using this library with nullable-reference-type-warnings enabled.
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
    internal sealed class MaybeNullWhenAttribute : Attribute
    {
        public MaybeNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;
        public bool ReturnValue { get; }
    }
}

#endif
