using System;

namespace H3MP.Networking
{
    public static class VersionExtensions
    {
        public static bool CompatibleWith(this Version @this, Version other)
        {
            // Allow different builds/patches.
            return @this.Major == other.Major && @this.Minor == other.Minor;
        }
    }
}