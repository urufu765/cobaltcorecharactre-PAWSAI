using Nanoray.PluginManager;
using Nickel;

namespace JollyJolly;

internal interface IRegisterable
{
    static abstract void Register(IPluginPackage<IModManifest> package, IModHelper helper);
}