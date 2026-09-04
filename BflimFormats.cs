using System.Collections.Generic;

namespace BflimFileType
{
    public struct FormatTemplate
    {
        public byte ID { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public byte BPP { get; set; }
        public string Suffix { get; set; }
    }

    public static class BflimConstants
    {
        public static readonly List<FormatTemplate> SupportedFormats = new List<FormatTemplate>
        {
            new FormatTemplate { ID = 0x00, Name = "L8_UNORM",               Type = "Luminance",            BPP = 8,  Suffix = "^c"  },
            new FormatTemplate { ID = 0x01, Name = "A8_UNORM",               Type = "Alpha",                BPP = 8,  Suffix = "^d"  },
            new FormatTemplate { ID = 0x02, Name = "LA4_UNORM",              Type = "Luminance Alpha",      BPP = 8,  Suffix = "^e"  },
            new FormatTemplate { ID = 0x03, Name = "LA8_UNORM",              Type = "Luminance Alpha",      BPP = 16, Suffix = "^f"  },
            new FormatTemplate { ID = 0x04, Name = "HILO8",                  Type = "High-Low",             BPP = 16, Suffix = "^g"  },
            new FormatTemplate { ID = 0x05, Name = "RGB565_UNORM",           Type = "Color",                BPP = 16, Suffix = "^h"  },
            new FormatTemplate { ID = 0x06, Name = "RGBX8_UNORM",            Type = "Color",                BPP = 32, Suffix = "^i"  },
            new FormatTemplate { ID = 0x07, Name = "RGB5A1_UNORM",           Type = "Color Alpha",          BPP = 16, Suffix = "^j"  },
            new FormatTemplate { ID = 0x08, Name = "RGBA4_UNORM",            Type = "Color Alpha",          BPP = 16, Suffix = "^k"  },
            new FormatTemplate { ID = 0x09, Name = "RGBA8_UNORM",            Type = "Color Alpha",          BPP = 32, Suffix = "^l"  },
            new FormatTemplate { ID = 0x0A, Name = "ETC1_UNORM",             Type = "ETC1",                 BPP = 4,  Suffix = "^m"  },
            new FormatTemplate { ID = 0x0B, Name = "ETC1A4_UNORM",           Type = "ETC1 Alpha 4",         BPP = 8,  Suffix = "^n"  },
            new FormatTemplate { ID = 0x0C, Name = "BC1_UNORM",              Type = "BC1 (DXT1)",           BPP = 4,  Suffix = "^o"  },
            new FormatTemplate { ID = 0x0D, Name = "BC2_UNORM",              Type = "BC2 (DXT3)",           BPP = 8,  Suffix = "^p"  },
            new FormatTemplate { ID = 0x0E, Name = "BC3_UNORM",              Type = "BC3 (DXT5)",           BPP = 8,  Suffix = "^q"  },
            new FormatTemplate { ID = 0x0F, Name = "BC4L_UNORM",             Type = "BC4 (Luminance)",      BPP = 4,  Suffix = "^r"  },
            new FormatTemplate { ID = 0x10, Name = "BC4A_UNORM",             Type = "BC4 (Alpha)",          BPP = 4,  Suffix = "^s"  },
            new FormatTemplate { ID = 0x11, Name = "BC5_UNORM",              Type = "BC5 (RG)",             BPP = 8,  Suffix = "^t"  },
            new FormatTemplate { ID = 0x12, Name = "L4_UNORM",               Type = "Luminance",            BPP = 4,  Suffix = "^u"  },
            new FormatTemplate { ID = 0x13, Name = "A4_UNORM",               Type = "Alpha",                BPP = 4,  Suffix = "^v"  },
            new FormatTemplate { ID = 0x14, Name = "RGBA8_SRGB",             Type = "Color Alpha (sRGB)",   BPP = 32, Suffix = "^w"  },
            new FormatTemplate { ID = 0x15, Name = "BC1_SRGB",               Type = "BC1 (sRGB)",           BPP = 4,  Suffix = "^x"  },
            new FormatTemplate { ID = 0x16, Name = "BC2_SRGB",               Type = "BC2 (sRGB)",           BPP = 8,  Suffix = "^y"  },
            new FormatTemplate { ID = 0x17, Name = "BC3_SRGB",               Type = "BC3 (sRGB)",           BPP = 8,  Suffix = "^z"  },
            new FormatTemplate { ID = 0x18, Name = "RGB10A2_UNORM",          Type = "Color Alpha (10-bit)", BPP = 32, Suffix = "unk" },
            new FormatTemplate { ID = 0x19, Name = "RGB565_INDIRECT_UNORM", Type = "Color (Indirect)",     BPP = 16, Suffix = "unk" }
        };
    }
}