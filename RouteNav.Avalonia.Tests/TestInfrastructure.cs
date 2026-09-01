using Xunit;

namespace RouteNav.Avalonia.Tests;

/// <summary>Groups all test classes into a single collection so tests run sequentially (tests touch Navigation statics).</summary>
[CollectionDefinition("Sequential")]
public sealed class SequentialTestCollection
{
}
