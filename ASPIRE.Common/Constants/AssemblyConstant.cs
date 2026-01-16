namespace ASPIRE.Common.Constants;

public static class AssemblyConstant
{
    /// <summary>
    ///     Used to identify the test collection assembly.
    ///     Primarily used to expose internal symbols to the test project.
    /// </summary>
    /// <remarks>
    ///     <code>
    ///         [assembly: InternalsVisibleTo(AssemblyConstant.TestCollectionAssembly)]
    ///     </code>
    /// </remarks>
    public const string TestCollectionAssembly = "ASPIRE.Tests";
}
