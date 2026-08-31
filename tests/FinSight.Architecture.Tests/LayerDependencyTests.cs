using System.Reflection;
using FinSight.Application;
using FinSight.Domain;
using FinSight.Infrastructure;
using Xunit;

namespace FinSight.Architecture.Tests;

/// <summary>
/// Contains architecture unit tests verifying clean architecture layer dependency constraints.
/// </summary>
public sealed class LayerDependencyTests
{
    /// <summary>
    /// Verifies that the Domain assembly does not reference the Infrastructure assembly.
    /// </summary>
    [Fact]
    public void DomainMustNotReferenceInfrastructure()
    {
        Assert.DoesNotContain(
            GetReferencedAssemblyNames(
                typeof(DomainAssemblyMarker).Assembly),
            name => name == "FinSight.Infrastructure");
    }

    /// <summary>
    /// Verifies that the Domain assembly does not reference the Application assembly.
    /// </summary>
    [Fact]
    public void DomainMustNotReferenceApplication()
    {
        Assert.DoesNotContain(
            GetReferencedAssemblyNames(
                typeof(DomainAssemblyMarker).Assembly),
            name => name == "FinSight.Application");
    }

    /// <summary>
    /// Verifies that the Application assembly does not reference the Infrastructure assembly.
    /// </summary>
    [Fact]
    public void ApplicationMustNotReferenceInfrastructure()
    {
        Assert.DoesNotContain(
            GetReferencedAssemblyNames(
                typeof(ApplicationAssemblyMarker).Assembly),
            name => name == "FinSight.Infrastructure");
    }

    /// <summary>
    /// Verifies that the Application assembly directly references the Domain assembly.
    /// </summary>
    [Fact]
    public void ApplicationMustReferenceDomain()
    {
        Assert.Contains(
            GetReferencedAssemblyNames(
                typeof(ApplicationAssemblyMarker).Assembly),
            name => name == "FinSight.Domain");
    }

    private static IEnumerable<string> GetReferencedAssemblyNames(
        Assembly assembly)
    {
        return assembly
            .GetReferencedAssemblies()
            .Select(x => x.Name!)
            .Where(x => x is not null);
    }
}
