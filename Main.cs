using System;
using System.IO;
using PaintDotNet;
using BflimFileType;
using System.Drawing.Imaging;

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
            byte[] magic = new byte[6];
            input.Read(magic, 0, 6);

            // FLIM in Ascii
            if (magic[0] == 0x46 && magic[1] == 0x4C && magic[2] == 0x49 && magic[3] == 0x4D)
            {
                if(magic[4] == 0xFE && magic[5] == 0xFF)
                {
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

            tileMode = tileModeSwizzle & 0x1F;
            System.Windows.Forms.MessageBox.Show($"tileMode: {tileMode}");
            swizzle = (tileModeSwizzle >> 5) & 0x07;

            pipeSwizzle = (swizzle >> 0) & 0x01;  
            bankSwizzle = (swizzle >> 1) & 0x03;  

            Document file = new Document(imageWidth,imageHeight);
            BitmapLayer dummyLayer = Layer.CreateBackgroundLayer(imageWidth, imageHeight);
            file.Layers.Add(dummyLayer);
            return file;
        }

        private ushort imageHeight;
        private ushort imageWidth;
        private int tileMode;
        private int swizzle;
        private int pipeSwizzle; 
        private int bankSwizzle; 
        private long headerOffset;

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
            
        }
    };
}
