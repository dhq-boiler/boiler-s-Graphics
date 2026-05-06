using boilersGraphics.Controls;
using boilersGraphics.Extensions;
using boilersGraphics.Models;
using boilersGraphics.UserControls;
using boilersGraphics.ViewModels;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace boilersGraphics.Helpers;

public static class AnimationGifExporter
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public static void Export(string filePath, MainWindowViewModel mainWindowVM)
    {
        var pages = mainWindowVM.CanvasPages;
        if (pages.Count == 0) return;

        var diagramVM = mainWindowVM.DiagramViewModel;
        var designerCanvas = Application.Current.MainWindow.GetChildOfType<DesignerCanvas>();

        // Save current canvas state
        int originalIndex = mainWindowVM.ActiveCanvasIndex.Value;
        mainWindowVM.SaveCurrentCanvasState();

        var frames = new List<(BitmapSource bitmap, int delayMs)>();

        try
        {
            for (int i = 0; i < pages.Count; i++)
            {
                // Switch to page
                if (i != originalIndex)
                {
                    var page = pages[i];
                    if (page.SerializedData != null)
                        diagramVM.RestoreCanvasState(page.SerializedData);
                    else
                    {
                        diagramVM.Layers.Clear();
                        diagramVM.Layers.Add(new Layer());
                    }
                }

                // Render current canvas
                var background = diagramVM.BackgroundItem.Value;
                var renderer = diagramVM.Renderer;
                var rtb = renderer.Render(null, designerCanvas, diagramVM, background, background);

                frames.Add((rtb, pages[i].DurationMs));
            }

            // Restore original canvas
            if (originalIndex < pages.Count)
            {
                var originalPage = pages[originalIndex];
                if (originalPage.SerializedData != null)
                    diagramVM.RestoreCanvasState(originalPage.SerializedData);
            }
            mainWindowVM.ActiveCanvasIndex.Value = originalIndex;
            mainWindowVM.UpdateActiveStates();

            // Write animated GIF
            WriteAnimatedGif(filePath, frames);

            Logger.Info($"Animation GIF exported: {filePath}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to export animation GIF");

            // Restore original canvas on error
            try
            {
                var originalPage = pages[originalIndex];
                if (originalPage.SerializedData != null)
                    diagramVM.RestoreCanvasState(originalPage.SerializedData);
                mainWindowVM.ActiveCanvasIndex.Value = originalIndex;
                mainWindowVM.UpdateActiveStates();
            }
            catch { }

            throw;
        }
    }

    private static void WriteAnimatedGif(string filePath, List<(BitmapSource bitmap, int delayMs)> frames)
    {
        if (frames.Count == 0) return;

        using var stream = File.Create(filePath);
        using var writer = new BinaryWriter(stream);

        int width = frames[0].bitmap.PixelWidth;
        int height = frames[0].bitmap.PixelHeight;

        // GIF Header
        writer.Write(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }); // "GIF89a"

        // Logical Screen Descriptor
        writer.Write((ushort)width);
        writer.Write((ushort)height);
        writer.Write((byte)0x70); // No global color table, 8 bits color resolution
        writer.Write((byte)0x00); // Background color index
        writer.Write((byte)0x00); // Pixel aspect ratio

        // Netscape Application Extension (for looping)
        writer.Write((byte)0x21); // Extension introducer
        writer.Write((byte)0xFF); // Application extension
        writer.Write((byte)0x0B); // Block size
        writer.Write(System.Text.Encoding.ASCII.GetBytes("NETSCAPE2.0"));
        writer.Write((byte)0x03); // Sub-block size
        writer.Write((byte)0x01); // Sub-block ID
        writer.Write((ushort)0);  // Loop count (0 = infinite)
        writer.Write((byte)0x00); // Block terminator

        foreach (var (bitmap, delayMs) in frames)
        {
            WriteGifFrame(writer, bitmap, delayMs);
        }

        // GIF Trailer
        writer.Write((byte)0x3B);
    }

    private static void WriteGifFrame(BinaryWriter writer, BitmapSource bitmap, int delayMs)
    {
        // Convert to 8-bit indexed color using FormatConvertedBitmap
        var quantized = new FormatConvertedBitmap(bitmap, PixelFormats.Indexed8, BitmapPalettes.Halftone256, 0);

        int width = quantized.PixelWidth;
        int height = quantized.PixelHeight;
        int stride = width;
        byte[] pixels = new byte[stride * height];
        quantized.CopyPixels(pixels, stride, 0);

        // Get palette colors
        var palette = quantized.Palette ?? BitmapPalettes.Halftone256;
        var colors = palette.Colors;

        // Graphic Control Extension
        writer.Write((byte)0x21); // Extension introducer
        writer.Write((byte)0xF9); // Graphic control label
        writer.Write((byte)0x04); // Block size
        writer.Write((byte)0x00); // Disposal method: none
        writer.Write((ushort)(delayMs / 10)); // Delay time in 1/100 seconds
        writer.Write((byte)0x00); // Transparent color index
        writer.Write((byte)0x00); // Block terminator

        // Image Descriptor
        writer.Write((byte)0x2C); // Image separator
        writer.Write((ushort)0);  // Left position
        writer.Write((ushort)0);  // Top position
        writer.Write((ushort)width);
        writer.Write((ushort)height);

        // Local Color Table flag + size
        int colorTableSize = 256;
        int colorTableBits = 7; // 2^(7+1) = 256
        writer.Write((byte)(0x80 | colorTableBits)); // Local color table present

        // Local Color Table
        for (int i = 0; i < colorTableSize; i++)
        {
            if (i < colors.Count)
            {
                writer.Write(colors[i].R);
                writer.Write(colors[i].G);
                writer.Write(colors[i].B);
            }
            else
            {
                writer.Write((byte)0);
                writer.Write((byte)0);
                writer.Write((byte)0);
            }
        }

        // Image Data (LZW compressed)
        WriteLzwCompressed(writer, pixels, 8); // 8 = min code size for 256 colors
    }

    private static void WriteLzwCompressed(BinaryWriter writer, byte[] pixels, int minCodeSize)
    {
        writer.Write((byte)minCodeSize);

        int clearCode = 1 << minCodeSize;
        int eoiCode = clearCode + 1;

        // Simple LZW encoding
        var codeTable = new Dictionary<string, int>();
        int nextCode = eoiCode + 1;
        int codeSize = minCodeSize + 1;
        int maxCode = (1 << codeSize) - 1;

        var buffer = new MemoryStream();
        int bitBuffer = 0;
        int bitsInBuffer = 0;

        void WriteBits(int code, int numBits)
        {
            bitBuffer |= code << bitsInBuffer;
            bitsInBuffer += numBits;
            while (bitsInBuffer >= 8)
            {
                buffer.WriteByte((byte)(bitBuffer & 0xFF));
                bitBuffer >>= 8;
                bitsInBuffer -= 8;
            }
        }

        void FlushBits()
        {
            if (bitsInBuffer > 0)
                buffer.WriteByte((byte)(bitBuffer & 0xFF));
            bitBuffer = 0;
            bitsInBuffer = 0;
        }

        // Initialize code table
        void ResetTable()
        {
            codeTable.Clear();
            for (int i = 0; i < clearCode; i++)
                codeTable[((char)i).ToString()] = i;
            nextCode = eoiCode + 1;
            codeSize = minCodeSize + 1;
            maxCode = (1 << codeSize) - 1;
        }

        ResetTable();
        WriteBits(clearCode, codeSize);

        if (pixels.Length == 0)
        {
            WriteBits(eoiCode, codeSize);
            FlushBits();
            WriteSubBlocks(writer, buffer.ToArray());
            return;
        }

        string current = ((char)pixels[0]).ToString();

        for (int i = 1; i < pixels.Length; i++)
        {
            string next = current + (char)pixels[i];
            if (codeTable.ContainsKey(next))
            {
                current = next;
            }
            else
            {
                WriteBits(codeTable[current], codeSize);
                if (nextCode <= 4095)
                {
                    codeTable[next] = nextCode++;
                    if (nextCode > maxCode + 1 && codeSize < 12)
                    {
                        codeSize++;
                        maxCode = (1 << codeSize) - 1;
                    }
                }
                else
                {
                    WriteBits(clearCode, codeSize);
                    ResetTable();
                }
                current = ((char)pixels[i]).ToString();
            }
        }

        WriteBits(codeTable[current], codeSize);
        WriteBits(eoiCode, codeSize);
        FlushBits();

        WriteSubBlocks(writer, buffer.ToArray());
    }

    private static void WriteSubBlocks(BinaryWriter writer, byte[] data)
    {
        int offset = 0;
        while (offset < data.Length)
        {
            int blockSize = Math.Min(255, data.Length - offset);
            writer.Write((byte)blockSize);
            writer.Write(data, offset, blockSize);
            offset += blockSize;
        }
        writer.Write((byte)0x00); // Block terminator
    }
}
