using Nanoray.PluginManager;
using Nickel;

namespace Starhunters;

internal interface IRegisterable
{
    static abstract void Register(IPluginPackage<IModManifest> package, IModHelper helper);
}