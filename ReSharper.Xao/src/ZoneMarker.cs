using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Feature.Services.Navigation;

namespace ReSharper.Xao
{
    [ZoneMarker]
    public class ZoneMarker : IRequire<NavigationZone>
    {
    }
}