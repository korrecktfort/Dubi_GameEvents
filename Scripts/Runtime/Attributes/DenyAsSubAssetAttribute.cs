using System;

/// <summary>
/// Will not appear as a possible sub asset type
/// </summary>
namespace Dubi.GameEvents
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DenyAsSubAssetAttribute : Attribute
    {    
    }
}
