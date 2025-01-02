using System.Collections.Immutable;

namespace ProtonDrive.Client.Authentication.Contracts;

internal sealed record ScopesResponse : ApiResponse
{
    public IImmutableList<string> Scopes { get; init; } = ImmutableList<string>.Empty;
}
