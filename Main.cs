using System;
using System.IO;
using PaintDotNet;
using BflimFileType;
using System.Drawing.Imaging;
using Wiiu;
using BCnEncoder.Decoder;
using BCnEncoder.Shared;
using CommunityToolkit.HighPerformance;
using System.Linq;
using BCnEncoder.Encoder;

namespace BflimFileType
{
    public class BflimFileTypeFactory : IFileTypeFactory
    {
        public FileType[] GetFileTypeInstances()
        {
            return new FileType[] { new BflimFileType() };
        }
    }

    public class BflimFileType : FileType
    {
        public BflimFileType()
            : base("BFLIM", new FileTypeOptions
            {
                LoadExtensions = new[] { ".bflim" },
                SaveExtensions = new[] { ".bflim" },
            })
        {}

        private bool isValidFile(Stream input)
        {
            if(input.Length < 0x28)
            {
                return false;
            }

            headerOffset = input.Length - 0x28;

            input.Seek(headerOffset, SeekOrigin.Begin);
            byte[] magic = new byte[0x14];
            input.Read(magic, 0, 0x14);

            // FLIM in Ascii
            if (magic[0] == 0x46 && magic[1] == 0x4C && magic[2] == 0x49 && magic[3] == 0x4D)
            {
                if(magic[4] == 0xFE && magic[5] == 0xFF)
                {
                    fileVersion = BinaryUtils.read32(magic, 0x08);
                    return true;
                }
                throw new FormatException("Little Endian is not supported");
            }
            throw new FormatException("Invalid Magic");
            
        }

        private Document parseFile(Stream input)
        {
            long imagOffset = input.Length - 0x28;

            byte[] buffer = new byte[0x50];
            input.Seek(input.Length-0x50, SeekOrigin.Begin);

            input.Read(buffer,0,0x50);

            for(long i = 0x4C; i >= 0; i--)
            {
                //imag in Ascii 
                if(buffer[i] == 0x69 && buffer[i+1] == 0x6D && buffer[i+2] == 0x61 && buffer[i+3] == 0x67)
                {
                    imagOffset = input.Length - 0x50 + i;
                }
            }

            long bufferOffset = imagOffset - (input.Length - 0x50);

            imageWidth = BinaryUtils.Read16(buffer, (int)bufferOffset + 0x08);
            imageHeight = BinaryUtils.Read16(buffer, (int)bufferOffset + 0x0A);
            fileAlign = BinaryUtils.Read16(buffer, (int)bufferOffset + 0x0C);

            byte FormatID = buffer[(int)bufferOffset + 0x0E];

            for(int i=0; i<BflimConstants.SupportedFormats.Count; i++)
            {
                if(BflimConstants.SupportedFormats[i].ID == FormatID)
                {
                    format = BflimConstants.SupportedFormats[i];
                    break;
                }
            }

            byte tileModeSwizzle = buffer[(int)bufferOffset + 0x0F];

            tileMode = (uint)tileModeSwizzle & 0x1F;
            swizzle = (uint)(tileModeSwizzle >> 5) & 0x07;

            pipeSwizzle = (int)(swizzle >> 0) & 0x01;  
            bankSwizzle = (int)(swizzle >> 1) & 0x03;  

            byte[] textureData = new byte[input.Length - 0x28];
            input.Seek(0, SeekOrigin.Begin);
            input.Read(textureData, 0, (int)input.Length - 0x28);

            GX2.GX2Surface surface = new GX2.GX2Surface();
            surface.bpp = format.BPP;
            surface.height = imageHeight;
            surface.width = imageWidth;
            surface.aa = (uint)GX2.GX2AAMode.GX2_AA_MODE_1X;
            surface.depth = 1;
            surface.dim = (uint)GX2.GX2SurfaceDimension.DIM_2D;
            surface.format = (uint)BflimToGX2(format.ID);
            surface.use = (uint)GX2.GX2SurfaceUse.USE_COLOR_BUFFER;
            surface.pitch = 0;
            surface.data = textureData;
            surface.numMips = 1;
            surface.mipOffset = new uint[0];
            surface.mipData = textureData;
            surface.tileMode = tileMode;
            surface.swizzle = swizzle << 8;
            surface.numArray = 1;

            byte[] decodedData = GX2.Decode(surface, 0, 0);

            if(format.ID == 0x0E) // BC3
            {
                BcDecoder decoder = new BcDecoder();
                Memory2D<ColorRgba32> pixels = decoder.DecodeRaw2D(decodedData, (int)imageWidth, (int)imageHeight, CompressionFormat.Bc3);

                Document file = new Document(imageWidth,imageHeight);
                BitmapLayer layer = Layer.CreateBackgroundLayer(imageWidth, imageHeight);
                Surface layerSurface = layer.Surface;
                Span2D<ColorRgba32> pixelSpan = pixels.Span;

                for (int y = 0; y < imageHeight; y++)
                {
                    for (int x = 0; x < imageWidth; x++)
                    {
                        ColorRgba32 px = pixelSpan[y, x];
                        layerSurface[x, y] = ColorBgra.FromBgra(px.b, px.g, px.r, px.a);
                    }
                }

                file.Layers.Add(layer);
                return file;
            }
            else if(format.ID == 0x09) // RGBA8
            {
                BcDecoder decoder = new BcDecoder();
                Memory2D<ColorRgba32> pixels = decoder.DecodeRaw2D(decodedData, (int)imageWidth, (int)imageHeight, CompressionFormat.Rgba);

                Document file = new Document(imageWidth, imageHeight);
                BitmapLayer layer = Layer.CreateBackgroundLayer(imageWidth, imageHeight);
                Surface layerSurface = layer.Surface;
                Span2D<ColorRgba32> pixelSpan = pixels.Span;

                for (int y = 0; y < imageHeight; y++)
                {
                    for (int x = 0; x < imageWidth; x++)
                    {
                        ColorRgba32 px = pixelSpan[y, x];
                        layerSurface[x, y] = ColorBgra.FromBgra(px.b, px.g, px.r, px.a);
                    }
                }

                file.Layers.Add(layer);
                return file;
            }
            return new Document(0,0);
        }

        private GX2.GX2SurfaceFormat BflimToGX2(byte bflimformat)
        {
            switch(bflimformat)
            {
                case 0x00: return GX2.GX2SurfaceFormat.TC_R8_UNORM;
                case 0x09: return GX2.GX2SurfaceFormat.TCS_R8_G8_B8_A8_UNORM;
                case 0x0E: return GX2.GX2SurfaceFormat.T_BC3_UNORM;
            }
            throw new FormatException("Format not supported yet");
        }

        private byte[] writeHeader(uint filesize, ushort width, ushort height)
        {
            byte[] header = new byte[0x14];
            header[0x00] = 0x46; // F
            header[0x01] = 0x4C; // L
            header[0x02] = 0x49; // I
            header[0x03] = 0x4D; // M

            header[0x04] = 0xFE;
            header[0x05] = 0xFF;

            BinaryUtils.write16(header, 0x06, 0x14);
            BinaryUtils.write32(header, 0x08, fileVersion);
            BinaryUtils.write32(header, 0x0C, filesize);

            BinaryUtils.write16(header, 0x10, 0x1);
            BinaryUtils.write16(header, 0x12, 0x00); // padding

            // Image Information:
            byte[] imageInfo = new byte[0x14];
            imageInfo[0x00] = 0x69; // i
            imageInfo[0x01] = 0x6D; // m
            imageInfo[0x02] = 0x61; // a
            imageInfo[0x03] = 0x67; // g

            BinaryUtils.write32(imageInfo, 0x04, 0x10);
            BinaryUtils.write16(imageInfo, 0x08, width);
            BinaryUtils.write16(imageInfo, 0x0A, height);
            BinaryUtils.write16(imageInfo, 0x0C, fileAlign);
            imageInfo[0x0E] = format.ID;

            uint swizzle = ((uint)pipeSwizzle & 0x01) | (((uint)bankSwizzle & 0x03) << 1);
            byte tileModeSwizzle = (byte)(((swizzle & 0x07) << 5) | (tileMode & 0x1F));

            imageInfo[0x0F] = tileModeSwizzle;

            BinaryUtils.write32(imageInfo, 0x10, filesize - 0x28);

            byte[] whole = header.Concat(imageInfo).ToArray();

            return whole;
        }

        private byte[] constructFileData(Surface scratchSurface)
        {
            int realWidth = scratchSurface.Width;
            int realHeight = scratchSurface.Height;

            var surfInfo = GX2.getSurfaceInfo(
                BflimToGX2(format.ID),
                (uint)realWidth,
                (uint)realHeight,
                1,
                (uint)GX2.GX2SurfaceDimension.DIM_2D,
                tileMode,
                (uint)GX2.GX2AAMode.GX2_AA_MODE_1X,
                0);

            int paddedWidth = (int)surfInfo.pitch * 4;
            int paddedHeight = (int)surfInfo.height * 4;

            byte[] rgbaBytes = new byte[paddedWidth * paddedHeight * 4];

            for (int y = 0; y < realHeight; y++)
            {
                for (int x = 0; x < realWidth; x++)
                {
                    ColorBgra px = scratchSurface[x, y];
                    int index = (y * paddedWidth + x) * 4;

                    rgbaBytes[index + 0] = px.R;
                    rgbaBytes[index + 1] = px.G;
                    rgbaBytes[index + 2] = px.B;
                    rgbaBytes[index + 3] = px.A;
                }
            }

            BcEncoder encoder = new BcEncoder();
            encoder.OutputOptions.Format = CompressionFormat.Bc3;
            byte[][] mips = encoder.EncodeToRawBytes(rgbaBytes, paddedWidth, paddedHeight, BCnEncoder.Encoder.PixelFormat.Rgba32);
            byte[] encodedBc3 = mips[0];

            byte[] swizzledData = GX2.swizzle(
                (uint)paddedWidth, (uint)paddedHeight,
                surfInfo.depth, surfInfo.height,
                (uint)BflimToGX2(format.ID), 0,
                (uint)GX2.GX2SurfaceUse.USE_COLOR_BUFFER,
                surfInfo.tileMode, swizzle << 8,
                surfInfo.pitch, surfInfo.bpp,
                0, 0, encodedBc3);

            byte[] header = writeHeader((uint)swizzledData.Length + 0x28, (ushort)realWidth, (ushort)realHeight);

            byte[] finalFile = swizzledData.Concat(header).ToArray();
            return finalFile;
        }

        private ushort imageHeight;
        private ushort imageWidth;
        private uint tileMode;
        private uint swizzle;
        private int pipeSwizzle; 
        private int bankSwizzle; 
        private long headerOffset;
        private uint fileVersion;
        private ushort fileAlign;

        private FormatTemplate format;

        protected override Document OnLoad(Stream input)
        {
            if(isValidFile(input))
            {
                return parseFile(input);
            }
            
            throw new FormatException("Invalid Bflim File");
        }

        protected override void OnSave(
            Document input,
            Stream output,
            SaveConfigToken token,
            Surface scratchSurface,
            ProgressEventHandler progressCallback)
        {
            input.Flatten(scratchSurface);

            uint blockWidth = ((uint)scratchSurface.Width + 3) / 4;
            uint blockHeight = ((uint)scratchSurface.Height + 3) / 4;
            long rawDataSize = blockWidth * blockHeight * 16;

            byte[] file = constructFileData(scratchSurface);

            output.Write(file, 0, file.Length);

            progressCallback?.Invoke(this, new ProgressEventArgs(100.0));
        }
    };
}
