using Silk.NET.OpenAL;

namespace MDTracer.Platform.Portable
{
  internal sealed class OpenAlAudioOutputBackend : IAudioOutputBackend, IDisposable
  {
    private const int QueueBufferCount = 4;

    private AL? g_al;
    private ALContext? g_alc;
    private unsafe Device* g_device;
    private unsafe Context* g_context;
    private uint g_source;
    private uint[] g_buffers = Array.Empty<uint>();
    private int g_sampleRate;
    private int g_bufferChunkBytes;
    private int g_capacityBytes;
    private int g_queuedBytes;
    private readonly object g_lock = new();
    private bool g_disposed;

    public int BufferedBytes
    {
      get
      {
        lock (g_lock)
        {
          return g_queuedBytes;
        }
      }
    }

    public int BufferCapacity => g_capacityBytes;

    public void Initialize(int in_sampleRate, int in_bitsPerSample, int in_channels, int in_bufferSizeBytes)
    {
      g_sampleRate = in_sampleRate;
      g_bufferChunkBytes = in_bufferSizeBytes;
      g_capacityBytes = in_bufferSizeBytes * QueueBufferCount * 2;

      g_alc = ALContext.GetApi(true);
      g_al = AL.GetApi(true);

      unsafe
      {
        g_device = g_alc.OpenDevice("");
        if (g_device == null)
        {
          throw new InvalidOperationException("OpenAL device open failed.");
        }

        g_context = g_alc.CreateContext(g_device, null);
        if (g_context == null)
        {
          throw new InvalidOperationException("OpenAL context create failed.");
        }

        g_alc.MakeContextCurrent(g_context);
      }

      g_al.GetError();
      g_source = g_al.GenSource();
      g_buffers = g_al.GenBuffers(QueueBufferCount);
      g_queuedBytes = 0;

      byte[] w_silence = new byte[g_bufferChunkBytes];
      for (int i = 0; i < QueueBufferCount; i++)
      {
        UploadBuffer(g_buffers[i], w_silence);
        QueueBuffer(g_buffers[i]);
        g_queuedBytes += g_bufferChunkBytes;
      }
    }

    public void Play()
    {
      if (g_al == null) return;
      g_al.SourcePlay(g_source);
    }

    public void ClearBuffer()
    {
      if (g_al == null) return;

      lock (g_lock)
      {
        g_al.SourceStop(g_source);
        DrainProcessedBuffers();
        UnqueueAllBuffers();
        g_queuedBytes = 0;

        byte[] w_silence = new byte[g_bufferChunkBytes];
        for (int i = 0; i < QueueBufferCount; i++)
        {
          UploadBuffer(g_buffers[i], w_silence);
          QueueBuffer(g_buffers[i]);
          g_queuedBytes += g_bufferChunkBytes;
        }
      }
    }

    public bool TryEnqueueSamples(byte[] in_buffer, int in_offset, int in_count)
    {
      if (g_al == null) return true;

      lock (g_lock)
      {
        while (g_queuedBytes + in_count > g_capacityBytes)
        {
          if (DrainProcessedBuffers() == false)
          {
            Thread.Sleep(1);
          }
        }

        int w_remaining = in_count;
        int w_offset = in_offset;
        while (w_remaining > 0)
        {
          int w_chunk = Math.Min(w_remaining, g_bufferChunkBytes);
          uint w_buffer = AcquireBuffer();
          UploadBuffer(w_buffer, in_buffer, w_offset, w_chunk);
          QueueBuffer(w_buffer);
          w_offset += w_chunk;
          w_remaining -= w_chunk;
        }

        g_queuedBytes += in_count;
        EnsurePlaying();
        return true;
      }
    }

    public void Dispose()
    {
      if (g_disposed) return;
      g_disposed = true;

      if (g_al != null)
      {
        g_al.SourceStop(g_source);
        g_al.DeleteSource(g_source);
        if (g_buffers.Length > 0)
        {
          g_al.DeleteBuffers(g_buffers);
        }
      }

      unsafe
      {
        if (g_alc != null)
        {
          if (g_context != null)
          {
            g_alc.DestroyContext(g_context);
            g_context = null;
          }

          if (g_device != null)
          {
            g_alc.CloseDevice(g_device);
            g_device = null;
          }
        }
      }

      g_al?.Dispose();
      g_alc?.Dispose();
      g_al = null;
      g_alc = null;
    }

    private void UploadBuffer(uint in_buffer, byte[] in_data)
    {
      UploadBuffer(in_buffer, in_data, 0, in_data.Length);
    }

    private void UploadBuffer(uint in_buffer, byte[] in_data, int in_offset, int in_count)
    {
      if (g_al == null) return;

      byte[] w_chunk = new byte[in_count];
      Array.Copy(in_data, in_offset, w_chunk, 0, in_count);
      g_al.BufferData(in_buffer, BufferFormat.Stereo16, w_chunk, g_sampleRate);
    }

    private void QueueBuffer(uint in_buffer)
    {
      if (g_al == null) return;
      g_al.SourceQueueBuffers(g_source, new[] { in_buffer });
    }

    private bool DrainProcessedBuffers()
    {
      if (g_al == null) return false;

      int w_processed = QuerySourceProperty(GetSourceInteger.BuffersProcessed);
      if (w_processed <= 0) return false;

      uint[] w_unqueued = new uint[w_processed];
      g_al.SourceUnqueueBuffers(g_source, w_unqueued);
      foreach (uint w_buffer in w_unqueued)
      {
        int w_size = QueryBufferProperty(w_buffer, GetBufferInteger.Size);
        g_queuedBytes = Math.Max(0, g_queuedBytes - (w_size > 0 ? w_size : g_bufferChunkBytes));
      }

      return true;
    }

    private uint AcquireBuffer()
    {
      if (g_al == null) return 0;

      int w_processed = QuerySourceProperty(GetSourceInteger.BuffersProcessed);
      if (w_processed > 0)
      {
        uint[] w_unqueued = new uint[1];
        g_al.SourceUnqueueBuffers(g_source, w_unqueued);
        if (w_unqueued[0] != 0)
        {
          int w_size = QueryBufferProperty(w_unqueued[0], GetBufferInteger.Size);
          g_queuedBytes = Math.Max(0, g_queuedBytes - (w_size > 0 ? w_size : g_bufferChunkBytes));
          return w_unqueued[0];
        }
      }

      return g_buffers[0];
    }

    private void UnqueueAllBuffers()
    {
      if (g_al == null) return;

      int w_queued = QuerySourceProperty(GetSourceInteger.BuffersQueued);
      if (w_queued > 0)
      {
        g_al.SourceUnqueueBuffers(g_source, new uint[w_queued]);
      }
    }

    private void EnsurePlaying()
    {
      if (g_al == null) return;

      int w_state = QuerySourceProperty(GetSourceInteger.SourceState);
      if (w_state != (int)SourceState.Playing)
      {
        g_al.SourcePlay(g_source);
      }
    }

    private int QuerySourceProperty(GetSourceInteger in_property)
    {
      if (g_al == null) return 0;
      g_al.GetSourceProperty(g_source, in_property, out int w_value);
      return w_value;
    }

    private int QueryBufferProperty(uint in_buffer, GetBufferInteger in_property)
    {
      if (g_al == null) return 0;
      g_al.GetBufferProperty(in_buffer, in_property, out int w_value);
      return w_value;
    }
  }
}
