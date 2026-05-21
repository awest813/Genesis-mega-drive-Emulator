using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MDTracer
{
    internal sealed class Form_Main_AviRecorder : IDisposable
    {
        private readonly FileStream g_stream;
        private readonly BinaryWriter g_writer;
        private readonly List<AviIndexEntry> g_indexEntries = new List<AviIndexEntry>();
        private readonly int g_width;
        private readonly int g_height;
        private readonly int g_fps;
        private readonly int g_rowStride;
        private readonly int g_frameSize;
        private readonly byte[] g_frameRow;
        private readonly byte[] g_sourceRow;
        private const int AUDIO_SAMPLES_PER_SECOND = 44100;
        private const int AUDIO_BITS_PER_SAMPLE = 16;
        private const int AUDIO_CHANNELS = 2;
        private const int AUDIO_BLOCK_ALIGN = AUDIO_CHANNELS * AUDIO_BITS_PER_SAMPLE / 8;
        private const int AUDIO_AVG_BYTES_PER_SECOND = AUDIO_SAMPLES_PER_SECOND * AUDIO_BLOCK_ALIGN;
        private readonly long g_riffSizePosition;
        private long g_avihFramesPosition;
        private long g_videoStrhFramesPosition;
        private long g_audioStrhSamplesPosition;
        private readonly long g_moviListSizePosition;
        private readonly long g_moviDataStart;
        private readonly object g_lock = new object();
        private int g_videoFrameCount;
        private long g_audioBytes;
        private bool g_disposed;

        public Form_Main_AviRecorder(string filePath, int width, int height, int fps)
        {
            g_width = width;
            g_height = height;
            g_fps = fps;
            g_rowStride = ((width * 3 + 3) / 4) * 4;
            g_frameSize = g_rowStride * height;
            g_frameRow = new byte[g_rowStride];
            g_sourceRow = new byte[width * 4];
            g_stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
            g_writer = new BinaryWriter(g_stream, Encoding.ASCII);

            WriteFourCC("RIFF");
            g_riffSizePosition = g_stream.Position;
            WriteUInt32(0);
            WriteFourCC("AVI ");

            WriteHeaderList();

            WriteFourCC("LIST");
            g_moviListSizePosition = g_stream.Position;
            WriteUInt32(0);
            WriteFourCC("movi");
            g_moviDataStart = g_stream.Position;
        }

        public int FrameCount
        {
            get
            {
                lock (g_lock)
                {
                    return g_videoFrameCount;
                }
            }
        }

        public void AddFrame(Bitmap source)
        {
            lock (g_lock)
            {
                ThrowIfDisposed();
                if (source.Width != g_width || source.Height != g_height)
                {
                    throw new InvalidOperationException("AVI frame size changed.");
                }

                long w_chunkStart = g_stream.Position;
                WriteFourCC("00db");
                WriteUInt32((uint)g_frameSize);

                Rectangle w_rect = new Rectangle(0, 0, g_width, g_height);
                Bitmap? w_tempBitmap = null;
                Bitmap w_bitmap = source;
                if (source.PixelFormat != PixelFormat.Format32bppArgb)
                {
                    w_tempBitmap = source.Clone(w_rect, PixelFormat.Format32bppArgb);
                    w_bitmap = w_tempBitmap;
                }

                BitmapData? w_data = null;
                try
                {
                    w_data = w_bitmap.LockBits(w_rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    int w_sourceStride = Math.Abs(w_data.Stride);
                    int w_pixelBytes = g_width * 3;
                    int w_paddingBytes = g_rowStride - w_pixelBytes;
                    for (int y = g_height - 1; y >= 0; y--)
                    {
                        if (w_paddingBytes > 0) Array.Clear(g_frameRow, w_pixelBytes, w_paddingBytes);
                        IntPtr w_source = w_data.Scan0 + y * w_data.Stride;
                        Marshal.Copy(w_source, g_sourceRow, 0, w_sourceStride);
                        for (int x = 0; x < g_width; x++)
                        {
                            int w_sourceOffset = x * 4;
                            int w_destOffset = x * 3;
                            g_frameRow[w_destOffset + 2] = g_sourceRow[w_sourceOffset + 0];
                            g_frameRow[w_destOffset + 0] = g_sourceRow[w_sourceOffset + 1];
                            g_frameRow[w_destOffset + 1] = g_sourceRow[w_sourceOffset + 2];
                        }
                        g_writer.Write(g_frameRow);
                    }
                }
                finally
                {
                    if (w_data != null) w_bitmap.UnlockBits(w_data);
                    w_tempBitmap?.Dispose();
                }

                if ((g_frameSize & 1) != 0) g_writer.Write((byte)0);

                g_indexEntries.Add(new AviIndexEntry
                {
                    ChunkId = "00db",
                    Flags = 0x10,
                    Offset = (uint)(w_chunkStart - g_moviDataStart),
                    Size = (uint)g_frameSize
                });
                g_videoFrameCount++;
            }
        }

        public void AddAudioSamples(byte[] source, int offset, int count)
        {
            lock (g_lock)
            {
                ThrowIfDisposed();
                if (source == null) throw new ArgumentNullException(nameof(source));
                if (offset < 0 || count < 0 || offset + count > source.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(count));
                }
                if (count <= 0) return;

                long w_chunkStart = g_stream.Position;
                WriteFourCC("01wb");
                WriteUInt32((uint)count);
                g_writer.Write(source, offset, count);
                if ((count & 1) != 0) g_writer.Write((byte)0);

                g_indexEntries.Add(new AviIndexEntry
                {
                    ChunkId = "01wb",
                    Flags = 0,
                    Offset = (uint)(w_chunkStart - g_moviDataStart),
                    Size = (uint)count
                });
                g_audioBytes += count;
            }
        }

        public void Dispose()
        {
            lock (g_lock)
            {
                if (g_disposed == true) return;

                g_disposed = true;
                long w_idx1Start = g_stream.Position;
                WriteFourCC("idx1");
                WriteUInt32((uint)(g_indexEntries.Count * 16));
                foreach (AviIndexEntry w_entry in g_indexEntries)
                {
                    WriteFourCC(w_entry.ChunkId);
                    WriteUInt32(w_entry.Flags);
                    WriteUInt32(w_entry.Offset);
                    WriteUInt32(w_entry.Size);
                }

                long w_fileEnd = g_stream.Position;

                g_stream.Position = g_riffSizePosition;
                WriteUInt32((uint)(w_fileEnd - 8));

                g_stream.Position = g_avihFramesPosition;
                WriteUInt32((uint)g_videoFrameCount);

                g_stream.Position = g_videoStrhFramesPosition;
                WriteUInt32((uint)g_videoFrameCount);

                g_stream.Position = g_audioStrhSamplesPosition;
                WriteUInt32((uint)(g_audioBytes / AUDIO_BLOCK_ALIGN));

                g_stream.Position = g_moviListSizePosition;
                WriteUInt32((uint)(w_idx1Start - g_moviListSizePosition - 4));

                g_stream.Position = w_fileEnd;
                g_writer.Dispose();
                g_stream.Dispose();
            }
        }

        private void WriteHeaderList()
        {
            long w_hdrlStart = StartList("hdrl");

            WriteFourCC("avih");
            WriteUInt32(56);
            WriteUInt32((uint)(1000000 / g_fps));
            WriteUInt32((uint)(g_frameSize * g_fps + AUDIO_AVG_BYTES_PER_SECOND));
            WriteUInt32(0);
            WriteUInt32(0x10);
            g_avihFramesPosition = g_stream.Position;
            WriteUInt32(0);
            WriteUInt32(0);
            WriteUInt32(2);
            WriteUInt32((uint)g_frameSize);
            WriteUInt32((uint)g_width);
            WriteUInt32((uint)g_height);
            WriteUInt32(0);
            WriteUInt32(0);
            WriteUInt32(0);
            WriteUInt32(0);

            long w_strlStart = StartList("strl");

            WriteFourCC("strh");
            WriteUInt32(56);
            WriteFourCC("vids");
            WriteFourCC("DIB ");
            WriteUInt32(0);
            WriteUInt16(0);
            WriteUInt16(0);
            WriteUInt32(0);
            WriteUInt32(1);
            WriteUInt32((uint)g_fps);
            WriteUInt32(0);
            g_videoStrhFramesPosition = g_stream.Position;
            WriteUInt32(0);
            WriteUInt32((uint)g_frameSize);
            WriteUInt32(0xffffffff);
            WriteUInt32(0);
            WriteInt16(0);
            WriteInt16(0);
            WriteInt16((short)g_width);
            WriteInt16((short)g_height);

            WriteFourCC("strf");
            WriteUInt32(40);
            WriteUInt32(40);
            WriteInt32(g_width);
            WriteInt32(g_height);
            WriteUInt16(1);
            WriteUInt16(24);
            WriteUInt32(0);
            WriteUInt32((uint)g_frameSize);
            WriteInt32(0);
            WriteInt32(0);
            WriteUInt32(0);
            WriteUInt32(0);

            EndList(w_strlStart);

            long w_audioStrlStart = StartList("strl");

            WriteFourCC("strh");
            WriteUInt32(56);
            WriteFourCC("auds");
            WriteUInt32(0);
            WriteUInt32(0);
            WriteUInt16(0);
            WriteUInt16(0);
            WriteUInt32(0);
            WriteUInt32(AUDIO_BLOCK_ALIGN);
            WriteUInt32(AUDIO_AVG_BYTES_PER_SECOND);
            WriteUInt32(0);
            g_audioStrhSamplesPosition = g_stream.Position;
            WriteUInt32(0);
            WriteUInt32((uint)AUDIO_BLOCK_ALIGN);
            WriteUInt32(0xffffffff);
            WriteUInt32((uint)AUDIO_BLOCK_ALIGN);
            WriteInt16(0);
            WriteInt16(0);
            WriteInt16(0);
            WriteInt16(0);

            WriteFourCC("strf");
            WriteUInt32(16);
            WriteUInt16(1);
            WriteUInt16(AUDIO_CHANNELS);
            WriteUInt32(AUDIO_SAMPLES_PER_SECOND);
            WriteUInt32(AUDIO_AVG_BYTES_PER_SECOND);
            WriteUInt16(AUDIO_BLOCK_ALIGN);
            WriteUInt16(AUDIO_BITS_PER_SAMPLE);

            EndList(w_audioStrlStart);
            EndList(w_hdrlStart);
        }

        private long StartList(string listType)
        {
            WriteFourCC("LIST");
            long w_sizePosition = g_stream.Position;
            WriteUInt32(0);
            WriteFourCC(listType);
            return w_sizePosition;
        }

        private void EndList(long sizePosition)
        {
            long w_end = g_stream.Position;
            g_stream.Position = sizePosition;
            WriteUInt32((uint)(w_end - sizePosition - 4));
            g_stream.Position = w_end;
        }

        private void ThrowIfDisposed()
        {
            if (g_disposed == true) throw new ObjectDisposedException(nameof(Form_Main_AviRecorder));
        }

        private void WriteFourCC(string value)
        {
            byte[] w_bytes = Encoding.ASCII.GetBytes(value);
            if (w_bytes.Length != 4) throw new ArgumentException("FourCC must be 4 bytes.", nameof(value));
            g_writer.Write(w_bytes);
        }

        private void WriteUInt16(ushort value) => g_writer.Write(value);
        private void WriteInt16(short value) => g_writer.Write(value);
        private void WriteUInt32(uint value) => g_writer.Write(value);
        private void WriteInt32(int value) => g_writer.Write(value);

        private struct AviIndexEntry
        {
            public string ChunkId;
            public uint Flags;
            public uint Offset;
            public uint Size;
        }
    }
}
