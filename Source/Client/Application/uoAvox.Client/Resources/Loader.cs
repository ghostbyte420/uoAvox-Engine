using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uoAvox.Resources
{
    partial class Loader
    {
        [EmbedResourceCSharp.FileEmbed("uoalogo.png")]
        public static partial ReadOnlySpan<byte> GetCuoLogo();

        [EmbedResourceCSharp.FileEmbed("game-background.png")]
        public static partial ReadOnlySpan<byte> GetBackgroundImage();
    }
}
