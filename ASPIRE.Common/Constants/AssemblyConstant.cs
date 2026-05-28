namespace ASPIRE.Common.Constants;

public static class AssemblyConstant
{
    /// <summary>
    ///     Used to identify the KONGOR master server test assembly.
    ///     Primarily used to expose internal symbols to the test project.
    /// </summary>
    /// <remarks>
    ///     <code>
    ///         [assembly: InternalsVisibleTo(AssemblyConstant.MasterServerTestAssembly)]
    ///     </code>
    /// </remarks>
    public const string MasterServerTestAssembly = "ASPIRE.Tests.KONGOR.MasterServer";

    /// <summary>
    ///     Used to identify the TRANSMUTANSTEIN chat server test assembly.
    ///     Primarily used to expose internal symbols to the test project.
    /// </summary>
    /// <remarks>
    ///     <code>
    ///         [assembly: InternalsVisibleTo(AssemblyConstant.ChatServerTestAssembly)]
    ///     </code>
    /// </remarks>
    public const string ChatServerTestAssembly = "ASPIRE.Tests.TRANSMUTANSTEIN.ChatServer";

    /// <summary>
    ///     Used to identify the ZORGATH web portal API test assembly.
    ///     Primarily used to expose internal symbols to the test project.
    /// </summary>
    /// <remarks>
    ///     <code>
    ///         [assembly: InternalsVisibleTo(AssemblyConstant.WebPortalAPITestAssembly)]
    ///     </code>
    /// </remarks>
    public const string WebPortalAPITestAssembly = "ASPIRE.Tests.ZORGATH.WebPortal.API";
}
