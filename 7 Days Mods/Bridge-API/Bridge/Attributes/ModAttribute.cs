using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bridge.Attributes
{
    /// <summary>
    /// An assembly level attribute indicating the attributed assembly contains a mod.
    /// Any assembly which is intended to be loaded and properly injected at game run time must contain this attribute.
    /// </summary>
    /// <remarks>
    /// The information passed into this attribute will be displayed in the global mods list, with additional
    /// details such as a list of injected methods.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class ModAttribute : Attribute
    {
        public String name;
        public String author;
        public String version;
        public String description;

        public ModAttribute()
        {
            name = author = version = description = "";
        }

        public override string ToString()
        {
            return String.Format(
                    "Mod=[{0}] Author=[{1}] Version=[{2}] Description=[{3}]",
                    new object[] {
                            name,
                            author,
                            version,
                            description
                    });
        }

        public override bool Equals(object obj)
        {
            ModAttribute other = obj as ModAttribute;
            if (other == null)
            {
                return false;
            }
            return Object.Equals(name, other.name) &&
                Object.Equals(author, other.author) &&
                Object.Equals(version, other.version) &&
                Object.Equals(description, other.description);
        }

        public override int GetHashCode()
        {
            return Tuple.Create(name, author, version, description).GetHashCode();
        }
    }
}
