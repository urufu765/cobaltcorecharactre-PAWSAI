using System;
using System.Reflection;
using Nanoray.PluginManager;
using Nickel;

namespace Starhunters;

public class SpriteLoading : Attribute
{
    public SpriteLoading(string basic, string owner)
    {
        this.basic = basic;
        this.owner = owner;
    }
    public SpriteLoading(string basic)
    {
        this.basic = basic;
        this.owner = null;
    }
    public string? owner;
    public string basic;

    public static void RegisterSprites(Type type, IPluginPackage<IModManifest> package)
    {
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
        {
            var attr = prop.GetCustomAttribute<SpriteLoading>();
            if (attr != null)
            {
                Spr sprite;
                if (attr.owner is string s)
                {
                    sprite = ModEntry.RegisterSprite(package, $"assets/{attr.basic}/{s}/{prop.Name}.png").Sprite;
                }
                else
                {
                    sprite = ModEntry.RegisterSprite(package, $"assets/{attr.basic}/{prop.Name}.png").Sprite;
                }
                if (prop.CanWrite) prop.SetValue(ModEntry.Instance, sprite);
            }
        }
    }
}