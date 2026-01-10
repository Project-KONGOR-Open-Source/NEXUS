namespace ZORGATH.WebPortal.API.Internals;

/// <summary>
///     Used in the unit/integration tests project as a marker for the ZORGATH.WebPortal.API assembly.
///     It acts as the type parameter for WebApplicationFactory, in order to point it to this compilation unit.
///     Any type in this project would also work, however using this pattern is the preferred practice.
/// </summary>
public interface ZORGATHAssemblyMarker
{
}