using System;

namespace Dubi.GameEvents
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RequireObjectComponent : Attribute
    {
        public Type type = null;

        public RequireObjectComponent(Type type)
        {
            this.type = type;
        }
    }
}