namespace FinSight.Application;

/// <summary>
/// Marker class used for assembly scanning and architecture testing.
/// </summary>
public static class ApplicationAssemblyMarker
{
    /// <summary>
    /// Explicit reference to Domain assembly marker to prevent compiler reference stripping.
    /// </summary>
    public static readonly Type DomainAssemblyMarkerType = typeof(Domain.DomainAssemblyMarker);
}
