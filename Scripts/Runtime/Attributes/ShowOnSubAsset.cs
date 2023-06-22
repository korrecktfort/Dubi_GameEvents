using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Show this property if the event is used as a sub asset
/// </summary>
namespace Dubi.GameEvents
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ShowOnSubAsset : PropertyAttribute
    {
    
    }
}