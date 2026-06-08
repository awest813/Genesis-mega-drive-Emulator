using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D12;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using MDTracer;

namespace MDTracer.Platform.Windows
{
    internal sealed class DirectX12VdpGpuRenderer : IVdpGpuRenderer
    {
        private Device g_dx_device;
        private CommandQueue g_dx_CommandQueue;
        private DescriptorHeap g_dx_Heap;
        private CpuDescriptorHandle g_dx_HeapHandle;
        private int g_dx_HeapHandle_offset;
        private CommandAllocator g_dx_CommandAllocator;
        private GraphicsCommandList g_dx_CommandList;
        private int g_dx_FenceNum;
        private Fence g_dx_Fence;
        private AutoResetEvent g_dx_FenceEvent;
        private RootSignature g_dx_RootSignature;
        private PipelineState g_dx_PipelineState_screenb;
        private PipelineState g_dx_PipelineState_screena;
        private PipelineState g_dx_PipelineState_sprite;
        private PipelineState g_dx_PipelineState_window;
        private PipelineState g_dx_PipelineState_final;
        private Resource g_dx_vram_buffers;
        private Resource g_dx_color_buffers;
        private Resource g_dx_colorshadow_buffers;
        private Resource g_dx_color_highlight_buffers;
        private Resource g_dx_line_snap_buffers;
        private Resource g_dx_screen_buffers;
        private Resource g_dx_cmap_buffers;
        private Resource g_dx_primap_buffers;
        private Resource g_dx_shadowmap_buffers;
        private Resource g_dx_register_buffers;
        private Resource g_dx_vram_update_buffers;
        private Resource g_dx_color_update_buffers;
        private Resource g_dx_color_shadow_update_buffers;
        private Resource g_dx_color_highlight_update_buffers;
        private Resource g_dx_line_snap_update_buffers;
        private Resource g_dx_screen_download_buffers;
        private IntPtr g_dx_vram_update_buffers_ptr;
        private IntPtr g_dx_color_update_buffers_ptr;
        private IntPtr g_dx_color_shadow_update_buffers_ptr;
        private IntPtr g_dx_color_highlight_update_buffers_ptr;
        private IntPtr g_dx_line_snap_update_buffers_ptr;
        private IntPtr g_dx_screen_download_buffers_ptr;
        private IntPtr g_dx_register_buffers_ptr;
        private bool g_dx_rendering_initialized;

        public void InitializeIfNeeded()
        {
            if (g_dx_rendering_initialized == true) return;

            g_dx_device = new Device(null, SharpDX.Direct3D.FeatureLevel.Level_11_0);
            g_dx_CommandQueue = g_dx_device.CreateCommandQueue(new CommandQueueDescription(CommandListType.Compute));
            g_dx_Heap = g_dx_device.CreateDescriptorHeap(new DescriptorHeapDescription()
            {
                DescriptorCount = 10,
                Type = DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView,
                Flags = DescriptorHeapFlags.ShaderVisible,
            });
            g_dx_HeapHandle = g_dx_Heap.CPUDescriptorHandleForHeapStart;
            g_dx_HeapHandle_offset = g_dx_device.GetDescriptorHandleIncrementSize(DescriptorHeapType.ConstantBufferViewShaderResourceViewUnorderedAccessView);
            g_dx_CommandAllocator = g_dx_device.CreateCommandAllocator(CommandListType.Compute);
            g_dx_CommandList = g_dx_device.CreateCommandList(CommandListType.Compute, g_dx_CommandAllocator, null);
            g_dx_CommandList.Close();
            g_dx_Fence = g_dx_device.CreateFence(0, FenceFlags.None);
            g_dx_FenceNum = 1;
            g_dx_FenceEvent = new AutoResetEvent(false);

            var w_rootsignatureDesc = new RootSignatureDescription(RootSignatureFlags.None,
                new[]{ new RootParameter(ShaderVisibility.All,
                        new DescriptorRange()
                        {
                            RangeType = DescriptorRangeType.ShaderResourceView,
                            BaseShaderRegister = 0,
                            OffsetInDescriptorsFromTableStart = 0,
                            DescriptorCount = 5,
                        },
                        new DescriptorRange()
                        {
                            RangeType = DescriptorRangeType.UnorderedAccessView,
                            BaseShaderRegister = 0,
                            OffsetInDescriptorsFromTableStart = 5,
                            DescriptorCount = 4,
                        },
                        new DescriptorRange()
                        {
                            RangeType = DescriptorRangeType.ConstantBufferView,
                            BaseShaderRegister = 0,
                            OffsetInDescriptorsFromTableStart = 9,
                            DescriptorCount = 1,
                        }),
            });
            g_dx_RootSignature = g_dx_device.CreateRootSignature(w_rootsignatureDesc.Serialize());

            string w_hlsl_string;
            var w_assembly = Assembly.GetExecutingAssembly();
            using (Stream w_stream = w_assembly.GetManifestResourceStream("MDTracer.Platform.Windows.md_vdp_renderer_directx_update.hlsl"))
            {
                using (StreamReader w_reader = new StreamReader(w_stream))
                {
                    w_hlsl_string = w_reader.ReadToEnd();
                }
            }
            g_dx_PipelineState_screenb = CreatePipelineState(w_hlsl_string, "CS_SCREENB");
            g_dx_PipelineState_screena = CreatePipelineState(w_hlsl_string, "CS_SCREENA");
            g_dx_PipelineState_sprite = CreatePipelineState(w_hlsl_string, "CS_SPRITE");
            g_dx_PipelineState_window = CreatePipelineState(w_hlsl_string, "CS_WINDOW");
            g_dx_PipelineState_final = CreatePipelineState(w_hlsl_string, "CS_FINAL");

            g_dx_vram_buffers = CreateBufferResource_view_srv(VdpGpuConstants.VramBufferElements, Utilities.SizeOf<uint>());
            g_dx_color_buffers = CreateBufferResource_view_srv(VdpGpuConstants.ColorMax, Utilities.SizeOf<uint>());
            g_dx_colorshadow_buffers = CreateBufferResource_view_srv(VdpGpuConstants.ColorMax, Utilities.SizeOf<uint>());
            g_dx_color_highlight_buffers = CreateBufferResource_view_srv(VdpGpuConstants.ColorMax, Utilities.SizeOf<uint>());
            g_dx_line_snap_buffers = CreateBufferResource_view_srv(VdpGpuConstants.DisplayYSize, Marshal.SizeOf(typeof(VDP_LINE_SNAP)));
            g_dx_screen_buffers = CreateBufferResource_view_uav(VdpGpuConstants.DisplayBufSize, Utilities.SizeOf<uint>());
            g_dx_cmap_buffers = CreateBufferResource_view_uav(VdpGpuConstants.DisplayBufSize, Utilities.SizeOf<uint>());
            g_dx_primap_buffers = CreateBufferResource_view_uav(VdpGpuConstants.DisplayBufSize, Utilities.SizeOf<uint>());
            g_dx_shadowmap_buffers = CreateBufferResource_view_uav(VdpGpuConstants.DisplayBufSize, Utilities.SizeOf<uint>());
            g_dx_register_buffers = CreateBufferResource_view_cbv(Marshal.SizeOf(typeof(VDP_REGISTER)));
            g_dx_register_buffers_ptr = g_dx_register_buffers.Map(0);

            g_dx_vram_update_buffers = CreateBufferResource_update(VdpGpuConstants.VramBufferElements, Utilities.SizeOf<uint>());
            g_dx_color_update_buffers = CreateBufferResource_update(VdpGpuConstants.ColorMax, Utilities.SizeOf<uint>());
            g_dx_color_shadow_update_buffers = CreateBufferResource_update(VdpGpuConstants.ColorMax, Utilities.SizeOf<uint>());
            g_dx_color_highlight_update_buffers = CreateBufferResource_update(VdpGpuConstants.ColorMax, Utilities.SizeOf<uint>());
            g_dx_line_snap_update_buffers = CreateBufferResource_update(VdpGpuConstants.DisplayYSize, Marshal.SizeOf(typeof(VDP_LINE_SNAP)));
            g_dx_screen_download_buffers = CreateBufferResource_update(VdpGpuConstants.DisplayBufSize, Utilities.SizeOf<uint>());
            g_dx_vram_update_buffers_ptr = g_dx_vram_update_buffers.Map(0);
            g_dx_color_update_buffers_ptr = g_dx_color_update_buffers.Map(0);
            g_dx_color_shadow_update_buffers_ptr = g_dx_color_shadow_update_buffers.Map(0);
            g_dx_color_highlight_update_buffers_ptr = g_dx_color_highlight_update_buffers.Map(0);
            g_dx_line_snap_update_buffers_ptr = g_dx_line_snap_update_buffers.Map(0);
            g_dx_screen_download_buffers_ptr = g_dx_screen_download_buffers.Map(0);
            g_dx_rendering_initialized = true;
        }

        public void StageFrameData(
            uint[] in_rendererVram,
            uint[] in_colors,
            uint[] in_colorShadow,
            uint[] in_colorHighlight,
            VDP_LINE_SNAP[] in_lineSnap)
        {
            var w_cur_ptr1 = g_dx_vram_update_buffers_ptr;
            for (var i = 0; i < VdpGpuConstants.VramBufferElements; i++)
            {
                w_cur_ptr1 = Utilities.WriteAndPosition(w_cur_ptr1, ref in_rendererVram[i]);
            }
            var w_cur_ptr2 = g_dx_color_update_buffers_ptr;
            for (var i = 0; i < VdpGpuConstants.ColorMax; i++)
            {
                w_cur_ptr2 = Utilities.WriteAndPosition(w_cur_ptr2, ref in_colors[i]);
            }
            var w_cur_ptr3 = g_dx_color_shadow_update_buffers_ptr;
            for (var i = 0; i < VdpGpuConstants.ColorMax; i++)
            {
                w_cur_ptr3 = Utilities.WriteAndPosition(w_cur_ptr3, ref in_colorShadow[i]);
            }
            var w_cur_ptr4 = g_dx_color_highlight_update_buffers_ptr;
            for (var i = 0; i < VdpGpuConstants.ColorMax; i++)
            {
                w_cur_ptr4 = Utilities.WriteAndPosition(w_cur_ptr4, ref in_colorHighlight[i]);
            }

            var w_cur_ptr5 = g_dx_line_snap_update_buffers_ptr;
            for (var i = 0; i < VdpGpuConstants.DisplayYSize; i++)
            {
                w_cur_ptr5 = Utilities.WriteAndPosition(w_cur_ptr5, ref in_lineSnap[i].hscrollA);
                w_cur_ptr5 = Utilities.WriteAndPosition(w_cur_ptr5, ref in_lineSnap[i].hscrollB);
                for (int j = 0; j < VdpGpuConstants.VsramDataSize; j++) w_cur_ptr5 = Utilities.WriteAndPosition(w_cur_ptr5, ref in_lineSnap[i].vscrollA[j]);
                for (int j = 0; j < VdpGpuConstants.VsramDataSize; j++) w_cur_ptr5 = Utilities.WriteAndPosition(w_cur_ptr5, ref in_lineSnap[i].vscrollB[j]);
                w_cur_ptr5 = Utilities.WriteAndPosition(w_cur_ptr5, ref in_lineSnap[i].window_x_st);
                w_cur_ptr5 = Utilities.WriteAndPosition(w_cur_ptr5, ref in_lineSnap[i].window_x_ed);
                w_cur_ptr5 = Utilities.WriteAndPosition(w_cur_ptr5, ref in_lineSnap[i].sprite_rendrere_num);
                for (int j = 0; j < VdpGpuConstants.MaxSprite; j++) w_cur_ptr5 = Utilities.WriteAndPosition(w_cur_ptr5, ref in_lineSnap[i].sprite_left[j]);
                for (int j = 0; j < VdpGpuConstants.MaxSprite; j++) w_cur_ptr5 = Utilities.WriteAndPosition(w_cur_ptr5, ref in_lineSnap[i].sprite_right[j]);
                for (int j = 0; j < VdpGpuConstants.MaxSprite; j++) w_cur_ptr5 = Utilities.WriteAndPosition(w_cur_ptr5, ref in_lineSnap[i].sprite_top[j]);
                for (int j = 0; j < VdpGpuConstants.MaxSprite; j++) w_cur_ptr5 = Utilities.WriteAndPosition(w_cur_ptr5, ref in_lineSnap[i].sprite_bottom[j]);
                for (int j = 0; j < VdpGpuConstants.MaxSprite; j++) w_cur_ptr5 = Utilities.WriteAndPosition(w_cur_ptr5, ref in_lineSnap[i].sprite_xcell_size[j]);
                for (int j = 0; j < VdpGpuConstants.MaxSprite; j++) w_cur_ptr5 = Utilities.WriteAndPosition(w_cur_ptr5, ref in_lineSnap[i].sprite_ycell_size[j]);
                for (int j = 0; j < VdpGpuConstants.MaxSprite; j++) w_cur_ptr5 = Utilities.WriteAndPosition(w_cur_ptr5, ref in_lineSnap[i].sprite_priority[j]);
                for (int j = 0; j < VdpGpuConstants.MaxSprite; j++) w_cur_ptr5 = Utilities.WriteAndPosition(w_cur_ptr5, ref in_lineSnap[i].sprite_palette[j]);
                for (int j = 0; j < VdpGpuConstants.MaxSprite; j++) w_cur_ptr5 = Utilities.WriteAndPosition(w_cur_ptr5, ref in_lineSnap[i].sprite_reverse[j]);
                for (int j = 0; j < VdpGpuConstants.MaxSprite; j++) w_cur_ptr5 = Utilities.WriteAndPosition(w_cur_ptr5, ref in_lineSnap[i].sprite_char[j]);
            }
        }

        public void Render(in VDP_REGISTER in_register, int in_displayYSize)
        {
            VDP_REGISTER w_register = in_register;
            Utilities.Write(g_dx_register_buffers_ptr, ref w_register);

            g_dx_CommandAllocator.Reset();
            g_dx_CommandList.Reset(g_dx_CommandAllocator, null);
            g_dx_CommandList.SetComputeRootSignature(g_dx_RootSignature);
            g_dx_CommandList.SetDescriptorHeaps(1, new[] { g_dx_Heap });
            g_dx_CommandList.SetComputeRootDescriptorTable(0, g_dx_Heap.GPUDescriptorHandleForHeapStart);
            g_dx_CommandList.CopyBufferRegion(g_dx_vram_buffers, 0, g_dx_vram_update_buffers, 0, VdpGpuConstants.VramBufferElements * Utilities.SizeOf<uint>());
            g_dx_CommandList.CopyBufferRegion(g_dx_color_buffers, 0, g_dx_color_update_buffers, 0, VdpGpuConstants.ColorMax * Utilities.SizeOf<uint>());
            g_dx_CommandList.CopyBufferRegion(g_dx_colorshadow_buffers, 0, g_dx_color_shadow_update_buffers, 0, VdpGpuConstants.ColorMax * Utilities.SizeOf<uint>());
            g_dx_CommandList.CopyBufferRegion(g_dx_color_highlight_buffers, 0, g_dx_color_highlight_update_buffers, 0, VdpGpuConstants.ColorMax * Utilities.SizeOf<uint>());
            g_dx_CommandList.CopyBufferRegion(g_dx_line_snap_buffers, 0, g_dx_line_snap_update_buffers, 0, VdpGpuConstants.DisplayYSize * Marshal.SizeOf(typeof(VDP_LINE_SNAP)));
            g_dx_CommandList.PipelineState = g_dx_PipelineState_screenb;
            g_dx_CommandList.Dispatch(in_displayYSize, 1, 1);
            g_dx_CommandList.ResourceBarrier(new ResourceTransitionBarrier(g_dx_cmap_buffers, ResourceStates.UnorderedAccess, ResourceStates.NonPixelShaderResource));
            g_dx_CommandList.ResourceBarrier(new ResourceTransitionBarrier(g_dx_cmap_buffers, ResourceStates.NonPixelShaderResource, ResourceStates.UnorderedAccess));
            g_dx_CommandList.PipelineState = g_dx_PipelineState_screena;
            g_dx_CommandList.Dispatch(in_displayYSize, 1, 1);
            g_dx_CommandList.ResourceBarrier(new ResourceTransitionBarrier(g_dx_cmap_buffers, ResourceStates.UnorderedAccess, ResourceStates.NonPixelShaderResource));
            g_dx_CommandList.ResourceBarrier(new ResourceTransitionBarrier(g_dx_cmap_buffers, ResourceStates.NonPixelShaderResource, ResourceStates.UnorderedAccess));
            g_dx_CommandList.PipelineState = g_dx_PipelineState_sprite;
            g_dx_CommandList.Dispatch(in_displayYSize, 1, 1);
            g_dx_CommandList.ResourceBarrier(new ResourceTransitionBarrier(g_dx_cmap_buffers, ResourceStates.UnorderedAccess, ResourceStates.NonPixelShaderResource));
            g_dx_CommandList.ResourceBarrier(new ResourceTransitionBarrier(g_dx_cmap_buffers, ResourceStates.NonPixelShaderResource, ResourceStates.UnorderedAccess));
            g_dx_CommandList.PipelineState = g_dx_PipelineState_window;
            g_dx_CommandList.Dispatch(in_displayYSize, 1, 1);
            g_dx_CommandList.ResourceBarrier(new ResourceTransitionBarrier(g_dx_cmap_buffers, ResourceStates.UnorderedAccess, ResourceStates.NonPixelShaderResource));
            g_dx_CommandList.ResourceBarrier(new ResourceTransitionBarrier(g_dx_cmap_buffers, ResourceStates.NonPixelShaderResource, ResourceStates.UnorderedAccess));
            g_dx_CommandList.PipelineState = g_dx_PipelineState_final;
            g_dx_CommandList.Dispatch(in_displayYSize, 1, 1);
            g_dx_CommandList.CopyBufferRegion(g_dx_screen_download_buffers, 0, g_dx_screen_buffers, 0, VdpGpuConstants.DisplayBufSize * Utilities.SizeOf<uint>());
            g_dx_CommandList.Close();
            g_dx_CommandQueue.ExecuteCommandList(g_dx_CommandList);
            int fence = g_dx_FenceNum;
            g_dx_FenceNum++;
            g_dx_CommandQueue.Signal(g_dx_Fence, fence);

            if (g_dx_Fence.CompletedValue < fence)
            {
                g_dx_Fence.SetEventOnCompletion(fence, g_dx_FenceEvent.SafeWaitHandle.DangerousGetHandle());
                g_dx_FenceEvent.WaitOne();
            }
        }

        public void DownloadScreen(uint[] in_destination)
        {
            var currentComputeOutputBufferPtr = g_dx_screen_download_buffers_ptr;
            for (var i = 0; i < VdpGpuConstants.DisplayBufSize; i++)
            {
                currentComputeOutputBufferPtr = Utilities.ReadAndPosition(currentComputeOutputBufferPtr, ref in_destination[i]);
            }
        }

        public void Dispose()
        {
            if (g_dx_rendering_initialized == false) return;

            g_dx_rendering_initialized = false;
            unmap_resource(g_dx_register_buffers, ref g_dx_register_buffers_ptr);
            unmap_resource(g_dx_vram_update_buffers, ref g_dx_vram_update_buffers_ptr);
            unmap_resource(g_dx_color_update_buffers, ref g_dx_color_update_buffers_ptr);
            unmap_resource(g_dx_color_shadow_update_buffers, ref g_dx_color_shadow_update_buffers_ptr);
            unmap_resource(g_dx_color_highlight_update_buffers, ref g_dx_color_highlight_update_buffers_ptr);
            unmap_resource(g_dx_line_snap_update_buffers, ref g_dx_line_snap_update_buffers_ptr);
            unmap_resource(g_dx_screen_download_buffers, ref g_dx_screen_download_buffers_ptr);

            dispose_dx(g_dx_screen_download_buffers);
            dispose_dx(g_dx_line_snap_update_buffers);
            dispose_dx(g_dx_color_highlight_update_buffers);
            dispose_dx(g_dx_color_shadow_update_buffers);
            dispose_dx(g_dx_color_update_buffers);
            dispose_dx(g_dx_vram_update_buffers);
            dispose_dx(g_dx_register_buffers);
            dispose_dx(g_dx_shadowmap_buffers);
            dispose_dx(g_dx_primap_buffers);
            dispose_dx(g_dx_cmap_buffers);
            dispose_dx(g_dx_screen_buffers);
            dispose_dx(g_dx_line_snap_buffers);
            dispose_dx(g_dx_color_highlight_buffers);
            dispose_dx(g_dx_colorshadow_buffers);
            dispose_dx(g_dx_color_buffers);
            dispose_dx(g_dx_vram_buffers);
            dispose_dx(g_dx_PipelineState_final);
            dispose_dx(g_dx_PipelineState_window);
            dispose_dx(g_dx_PipelineState_sprite);
            dispose_dx(g_dx_PipelineState_screena);
            dispose_dx(g_dx_PipelineState_screenb);
            dispose_dx(g_dx_RootSignature);
            dispose_dx(g_dx_FenceEvent);
            dispose_dx(g_dx_Fence);
            dispose_dx(g_dx_CommandList);
            dispose_dx(g_dx_CommandAllocator);
            dispose_dx(g_dx_Heap);
            dispose_dx(g_dx_CommandQueue);
            dispose_dx(g_dx_device);
        }

        private Resource CreateBufferResource_view_srv(int in_bufsize, int in_struct_size)
        {
            Resource w_buffer = g_dx_device.CreateCommittedResource(
                new HeapProperties(HeapType.Default),
                HeapFlags.None,
                ResourceDescription.Buffer(in_bufsize * in_struct_size),
                ResourceStates.NonPixelShaderResource
            );
            var w_desc = new ShaderResourceViewDescription()
            {
                Format = SharpDX.DXGI.Format.Unknown,
                Dimension = ShaderResourceViewDimension.Buffer,
                Shader4ComponentMapping = 0x1688,
                Buffer =
                    {
                        FirstElement = 0,
                        ElementCount = in_bufsize,
                        StructureByteStride = in_struct_size,
                        Flags = BufferShaderResourceViewFlags.None,
                    },
            };
            g_dx_device.CreateShaderResourceView(w_buffer, w_desc, g_dx_HeapHandle);
            g_dx_HeapHandle += g_dx_HeapHandle_offset;
            return w_buffer;
        }

        private Resource CreateBufferResource_view_uav(int in_bufsize, int in_struct_size)
        {
            Resource w_buffer = g_dx_device.CreateCommittedResource(
                new HeapProperties(HeapType.Default),
                HeapFlags.None,
                ResourceDescription.Buffer(in_bufsize * in_struct_size, ResourceFlags.AllowUnorderedAccess),
                ResourceStates.UnorderedAccess
            );
            var w_desc = new UnorderedAccessViewDescription()
            {
                Format = SharpDX.DXGI.Format.Unknown,
                Dimension = UnorderedAccessViewDimension.Buffer,
                Buffer =
                    {
                        FirstElement = 0,
                        ElementCount = in_bufsize,
                        StructureByteStride = in_struct_size,
                        CounterOffsetInBytes = 0,
                        Flags = BufferUnorderedAccessViewFlags.None,
                    },
            };
            g_dx_device.CreateUnorderedAccessView(w_buffer, null, w_desc, g_dx_HeapHandle);
            g_dx_HeapHandle += g_dx_HeapHandle_offset;
            return w_buffer;
        }

        private Resource CreateBufferResource_view_cbv(int in_bufsize)
        {
            Resource w_buffer = g_dx_device.CreateCommittedResource(
                new HeapProperties(HeapType.Upload),
                HeapFlags.None,
                ResourceDescription.Buffer(in_bufsize),
                ResourceStates.GenericRead
                );
            var w_desc = new ConstantBufferViewDescription()
            {
                BufferLocation = w_buffer.GPUVirtualAddress,
                SizeInBytes = in_bufsize,
            };

            g_dx_device.CreateConstantBufferView(w_desc, g_dx_HeapHandle);
            g_dx_HeapHandle += g_dx_HeapHandle_offset;
            return w_buffer;
        }

        private Resource CreateBufferResource_update(int in_bufsize, int in_struct_size)
        {
            return g_dx_device.CreateCommittedResource(
                new HeapProperties(HeapType.Upload),
                HeapFlags.None,
                ResourceDescription.Buffer(in_bufsize * in_struct_size),
                ResourceStates.GenericRead
            );
        }

        private PipelineState CreatePipelineState(string in_hlsl_string, string in_entrypoint)
        {
            SharpDX.Direct3D12.ShaderBytecode w_ShaderBytecode = new SharpDX.Direct3D12.ShaderBytecode(
                        SharpDX.D3DCompiler.ShaderBytecode.Compile(in_hlsl_string, in_entrypoint, "cs_5_0", ShaderFlags.OptimizationLevel3));
            var w_cpsDesc = new ComputePipelineStateDescription()
            {
                RootSignaturePointer = g_dx_RootSignature,
                ComputeShader = w_ShaderBytecode,
            };
            return g_dx_device.CreateComputePipelineState(w_cpsDesc);
        }

        private static void unmap_resource(Resource in_resource, ref IntPtr in_ptr)
        {
            if ((in_resource == null) || (in_ptr == IntPtr.Zero)) return;

            try
            {
                in_resource.Unmap(0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[VDP] DirectX unmap failed: " + ex.Message);
            }
            in_ptr = IntPtr.Zero;
        }

        private static void dispose_dx(IDisposable in_disposable)
        {
            try
            {
                in_disposable?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[VDP] DirectX dispose failed: " + ex.Message);
            }
        }
    }
}
