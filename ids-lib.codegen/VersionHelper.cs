using Xbim.Common;

namespace IdsLib.Generator;

internal static class VersionHelper
{
    public static string GetFileVersion(Type type)
    {
        var info = new XbimAssemblyInfo(type);
        return info.FileVersion;
    }
}
