using PngFix;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Force.Crc32;

static (uint, uint, int) DetectPNGSize(string fileName)
{
    using var hash = new Crc32Algorithm();

    using var br = new BinaryReader(File.OpenRead(fileName));
    var signature = br.ReadUInt64();

    if (signature != 0xA1A0A0D474E5089)
    {
        Console.WriteLine("File is not vaild PNG file");
    }

    unsafe
    {
        while (true)
        {
            var size = BinaryPrimitives.ReverseEndianness(br.ReadInt32());
            var hdrOffset = br.BaseStream.Position;
            var hdr = br.ReadBytes(size + 4);
            var targetCrc32 = br.ReadUInt32();

            fixed (byte* p = hdr)
            {
                var header = (PngHeader*)p;
                if (header->signature != 0x52444849) continue;

                var realCrc32 = hash.ComputeHash(hdr).u32();
                Console.WriteLine($"Target CRC32: {targetCrc32:x}, Real CRC32: {realCrc32:x}");

                if (targetCrc32 == realCrc32)
                {
                    Console.WriteLine("Crc32 Match, PNG is Good.");
                    return (header->width.r(), header->height.r(), 0);
                }

                for (uint w = 0; w < 10000; w++)
                {
                    for (uint h = 0; h < 10000; h++)
                    {
                        header->width = w.r();
                        header->height = h.r();

                        var crc32 = hash.ComputeHash(hdr).u32();
                        if (crc32 == targetCrc32)
                        {
                            Console.WriteLine($"PNG size found: {w} {h}");
                            return (w, h, (int)hdrOffset);
                        }
                    }
                }

                break;
            }
        }
    }

    return (0, 0, 0);
}

foreach (var arg in args)
{
    if (File.Exists(arg))
    {
        var (w, h, offset) = DetectPNGSize(arg);

        if(offset != 0)
        {
            var bytes = File.ReadAllBytes(arg);
            using(var bw = new BinaryWriter(new MemoryStream(bytes)))
            {
                bw.Seek(offset + 4, SeekOrigin.Begin);
                bw.Write(w.r());
                bw.Write(h.r());
            }

            Console.WriteLine($"Writing Fixed File to {arg}.fixed.png...");
            File.WriteAllBytes(arg + ".fixed.png", bytes);
        }
    }
}

Console.ReadKey();

[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0xD + 4)]
struct PngHeader
{
    public uint signature;
    public uint width;
    public uint height;
    public byte depth;
    public byte color_type;
    public byte compression;
    public byte filter;
    public byte interlace;
}
