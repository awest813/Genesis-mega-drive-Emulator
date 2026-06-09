namespace MDTracer
{
    //----------------------------------------------------------------
    // Production 68000 execution tracer (Phase 3).
    //
    // IM68kTracer previously lived on Form_Code_Trace, which kept CPU
    // trace wiring coupled to the WinForms disassembly window. The core
    // now depends only on this class through WinFormsDebugTools.g_cpuTracer.
    //----------------------------------------------------------------
    internal sealed class M68kExecutionTracer : IM68kTracer
    {
        private readonly ICodeAnalysisSession g_session;
        private readonly ManualResetEvent g_waitHandle = new ManualResetEvent(false);
        private bool g_chk_enable;

        public CodeAnalysisStackEntry[] StackList { get; }
        public int StackCur { get; private set; }
        public uint FuncAddress { get; private set; }
        public uint CallerAddress { get; private set; }
        public bool CpuPause { get; private set; }

        public M68kExecutionTracer(ICodeAnalysisSession in_session)
        {
            g_session = in_session;
            StackList = new CodeAnalysisStackEntry[CodeAnalysisConstants.StackListNum];
            FuncAddress = md_main.g_md_m68k.g_initial_PC;
        }

        public void Trace_Start()
        {
            if (CpuPause == true)
            {
                CpuPause = false;
                g_waitHandle.Set();
            }
        }

        public void Trace_Stop()
        {
            CpuPause = true;
        }

        public void Trace_StepIn()
        {
            if (CpuPause == false)
            {
                CpuPause = true;
            }
            else
            {
                g_waitHandle.Set();
            }
        }

        public void Trace_StepOver()
        {
            if (CpuPause == false)
            {
                CpuPause = true;
            }
            else
            {
                int w_line = g_session.GetCodeFromAddr(md_main.g_md_m68k.g_reg_PC);
                if ((w_line >= 0) && (g_session.AnalyseCode[w_line].operand_jsr == true))
                {
                    int w_line2 = g_session.GetCodeFromAddr(
                        (uint)(md_main.g_md_m68k.g_reg_PC + (g_session.AnalyseCode[w_line].leng2 * 2)));
                    if (w_line2 >= 0)
                    {
                        g_session.SetBreakFlash(w_line2, true);
                        CpuPause = false;
                    }
                }
                g_waitHandle.Set();
            }
        }

        public void Trace_FirstStepBreak()
        {
            int w_line = g_session.GetCodeFromAddr(md_main.g_md_m68k.g_initial_PC);
            if (w_line >= 0)
            {
                g_session.SetBreakFlash(w_line, true);
            }
        }

        public void CPU_Trace_push(
            M68kStackEntryType in_type,
            uint in_caller_address,
            uint in_start_address,
            uint in_ret_address,
            uint in_stack_address)
        {
            if (in_caller_address == 0) return;
            if (CodeAnalysisConstants.StackListNum <= StackCur) return;
            in_caller_address &= 0xffffff;
            in_start_address &= 0xffffff;
            in_ret_address &= 0xffffff;

            uint w_func_address = (in_caller_address < 256) ? in_caller_address : FuncAddress;
            int w_line = g_session.GetCodeFromAddr(in_caller_address);
            if (w_line < 0) return;
            int w_num = g_session.AnalyseCode[w_line].stack.FindIndex(x => x.start_address == in_start_address);
            if (w_num == -1)
            {
                w_num = g_session.AnalyseCode[w_line].stack.Count();
                g_session.AnalyseCode[w_line].stack.Add(new CodeAnalysisStackEntry
                {
                    type = in_type,
                    caller_address = in_caller_address,
                    caller_num = w_num,
                    func_address = w_func_address,
                    ret_address = in_ret_address,
                    start_address = in_start_address,
                    end_address = 0
                });
            }
            StackList[StackCur].type = in_type;
            StackList[StackCur].caller_address = in_caller_address;
            StackList[StackCur].caller_num = w_num;
            StackList[StackCur].func_address = w_func_address;
            StackList[StackCur].ret_address = in_ret_address;
            StackList[StackCur].start_address = in_start_address;
            StackList[StackCur].stack_address = in_stack_address + 4;
            StackCur += 1;
            FuncAddress = in_start_address;
            CallerAddress = in_caller_address;
        }

        public void CPU_Trace_pop(uint in_pc, uint in_end_addres, uint in_stack_address)
        {
            if (StackCur > 0)
            {
                if (in_stack_address != StackList[StackCur - 1].stack_address)
                {
                    int w_num = StackCur - 1;
                    StackCur = 0;
                    for (int i = w_num; i >= 0; i--)
                    {
                        if (in_stack_address == StackList[i].stack_address)
                        {
                            StackCur = i + 1;
                            break;
                        }
                    }
                }
            }
            if (StackCur > 0)
            {
                in_pc &= 0xffffff;
                in_end_addres &= 0xffffff;
                CodeAnalysisStackEntry w_currentStack = StackList[StackCur - 1];
                int w_line = g_session.GetCodeFromAddr(w_currentStack.caller_address);
                if (w_line >= 0)
                {
                    CodeAnalysisTraceCode w_trace = g_session.AnalyseCode[w_line];
                    if ((0 <= w_currentStack.caller_num) && (w_currentStack.caller_num < w_trace.stack.Count))
                    {
                        CodeAnalysisStackEntry w_stack = w_trace.stack[w_currentStack.caller_num];
                        w_stack.end_address = in_end_addres;
                        w_trace.stack[w_currentStack.caller_num] = w_stack;
                        g_session.AnalyseCode[w_line] = w_trace;
                    }
                }
                StackCur -= 1;
                if (StackCur > 0)
                {
                    FuncAddress = StackList[StackCur - 1].start_address;
                    CallerAddress = StackList[StackCur - 1].caller_address;
                }
                else
                {
                    FuncAddress = in_pc;
                    CallerAddress = 0;
                }
            }
        }

        public void CPU_Trace(uint in_addr)
        {
            int w_line = g_session.GetCodeFromAddr(in_addr);
            if (w_line < 0) return;
            if (g_session.AnalyseCode[w_line].type == CodeAnalysisTraceCode.Type.Non)
            {
                g_session.AnalyseCode[w_line].type = CodeAnalysisTraceCode.Type.Chk;
                g_chk_enable = true;
            }
            g_session.AnalyseCode[w_line].func_address = FuncAddress;

            if (((CpuPause == true) &&
                 ((md_main.g_debugView.trace_sip == false) ||
                 ((md_main.g_md_m68k.g_interrupt_H_act == false)
                 && (md_main.g_md_m68k.g_interrupt_V_act == false)
                 && (md_main.g_md_m68k.g_interrupt_EXT_act == false))))
                || (g_session.AnalyseCode[w_line].break_static == true)
                || (g_session.AnalyseCode[w_line].break_flash == true))
            {
                CpuPause = true;
                if (g_chk_enable == true)
                {
                    g_session.Analyses();
                    g_chk_enable = false;
                }
                g_session.AnalyseCode[w_line].break_flash = false;
                WinFormsDebugTools.g_coordinator.OnTraceBreak(w_line, FuncAddress, CallerAddress);

                g_waitHandle.WaitOne(Timeout.Infinite);
                g_waitHandle.Reset();
            }
        }
    }
}
