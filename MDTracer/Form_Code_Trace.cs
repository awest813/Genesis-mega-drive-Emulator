using System;
using System.Diagnostics;


namespace MDTracer
{
    public partial class Form_Code_Trace
    {
        public bool g_cpu_pause;
        private bool g_chk_enable; 
        private ManualResetEvent g_waitHandle;

        public enum STACK_LIST_TYPE : int
        {
            NON,
            TOP,
            JSR,
            BSR,
            TRAP,
            HINT,
            VINT,
            EXT
        }
        public string[] STACK_LIST_TYPE_STR = new string[] { "", "TOP", "JSR", "BSR", "TRAP", "HINT", "VINT", "EXT" };
        public const int STACK_LIST_NUM = 1024;
        public struct STACK_LIST
        {
            public STACK_LIST_TYPE type;
            public uint func_address;
            public uint caller_address;
            public int caller_num;
            public uint ret_address;
            public uint start_address;
            public uint end_address;
            public uint stack_address;
        }
        public STACK_LIST[] g_stack_list;
        public int g_stack_cur;
        public uint g_func_address;
        public uint g_caller_address;
        public int g_arrow_start_line;
        public int g_arrow_end_line;

        public Form_Code_Trace()
        {
            initialize();
        }
        //----------------------------------------------------------------
        //initialize
        //----------------------------------------------------------------
        public void initialize()
        {
            g_cpu_pause = false;
            g_waitHandle = new ManualResetEvent(false);
            g_stack_list = new STACK_LIST[STACK_LIST_NUM];
            g_func_address = md_main.g_md_m68k.g_initial_PC;
            g_analyse_code = new TRACECODE[MEMSIZE];
        }
        //----------------------------------------------------------------
        //trace event
        //----------------------------------------------------------------
        public void Trace_Start()
        {
            if (g_cpu_pause == true)
            {
                g_cpu_pause = false;
                g_waitHandle.Set();
            }
        }
        public void Trace_Stop()
        {
            g_cpu_pause = true;
        }
        public void Trace_StepIn()
        {
            if (g_cpu_pause == false)
            {
                g_cpu_pause = true;
            }
            else
            {
                g_waitHandle.Set();
            }
        }
        public void Trace_StepOver()
        {
            if (g_cpu_pause == false)
            {
                g_cpu_pause = true;
            }
            else
            {
                int w_line = get_code_from_addr(md_main.g_md_m68k.g_reg_PC);
                if ((w_line >= 0) && (g_analyse_code[w_line].operand_jsr == true))
                {
                    int w_line2 = get_code_from_addr((uint)(md_main.g_md_m68k.g_reg_PC + (g_analyse_code[w_line].leng2 * 2)));
                    if (w_line2 >= 0)
                    {
                        g_analyse_code[w_line2].break_flash = true;
                        g_cpu_pause = false;
                    }
                }
                g_waitHandle.Set();
            }
        }
        public void Trace_FirstStepBreak()
        {
            int w_line = get_code_from_addr(md_main.g_md_m68k.g_initial_PC);
            if (w_line >= 0)
            {
                g_analyse_code[w_line].break_flash = true;
            }
        }
        public void CPU_Trace_push(STACK_LIST_TYPE in_type, uint in_caller_address, uint in_start_address, uint in_ret_address, uint in_stack_address)
        {
            if (in_caller_address == 0) return;
            if (STACK_LIST_NUM <= g_stack_cur) return;
            in_caller_address &= 0xffffff;
            in_start_address &= 0xffffff;
            in_ret_address &= 0xffffff;

            uint w_func_address = (in_caller_address < 256) ? in_caller_address : g_func_address;
            int w_line = get_code_from_addr(in_caller_address);
            if (w_line < 0) return;
            int w_num = g_analyse_code[w_line].stack.FindIndex(x => x.start_address == in_start_address);
            if (w_num == -1)
            {
                w_num = g_analyse_code[w_line].stack.Count();
                g_analyse_code[w_line].stack.Add(new STACK_LIST
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
            g_stack_list[g_stack_cur].type = in_type;
            g_stack_list[g_stack_cur].caller_address = in_caller_address;
            g_stack_list[g_stack_cur].caller_num = w_num;
            g_stack_list[g_stack_cur].func_address = w_func_address;
            g_stack_list[g_stack_cur].ret_address = in_ret_address;
            g_stack_list[g_stack_cur].start_address = in_start_address;
            g_stack_list[g_stack_cur].stack_address = in_stack_address + 4;
            g_stack_cur += 1;
            g_func_address = in_start_address;
            g_caller_address = in_caller_address;
        }
        public void CPU_Trace_pop(uint in_pc, uint in_end_addres, uint in_stack_address)
        {
            if (g_stack_cur > 0)
            {
                if (in_stack_address != g_stack_list[g_stack_cur - 1].stack_address)
                {
                    int w_num = g_stack_cur - 1;
                    g_stack_cur = 0;
                    for (int i = w_num; i >= 0; i--)
                    {
                        if (in_stack_address == g_stack_list[i].stack_address)
                        {
                            g_stack_cur = i + 1;
                            break;
                        }
                    }
                }
            }
            if (g_stack_cur > 0)
            {
                in_pc &= 0xffffff;
                in_end_addres &= 0xffffff;
                STACK_LIST w_currentStack = g_stack_list[g_stack_cur - 1];
                int w_line = get_code_from_addr(w_currentStack.caller_address);
                if (w_line >= 0)
                {
                    TRACECODE w_trace = g_analyse_code[w_line];
                    if ((0 <= w_currentStack.caller_num) && (w_currentStack.caller_num < w_trace.stack.Count))
                    {
                        STACK_LIST w_stack = w_trace.stack[w_currentStack.caller_num];
                        w_stack.end_address = in_end_addres;
                        w_trace.stack[w_currentStack.caller_num] = w_stack;
                        g_analyse_code[w_line] = w_trace;
                    }
                }
                g_stack_cur -= 1;
                if (g_stack_cur > 0)
                {
                    g_func_address = g_stack_list[g_stack_cur - 1].start_address;
                    g_caller_address = g_stack_list[g_stack_cur - 1].caller_address;
                }
                else
                {
                    g_func_address = in_pc;
                    g_caller_address = 0;
                }
            }
        }
        public void CPU_Trace(uint in_addr)
        {
            int w_line = get_code_from_addr(in_addr);
            if (w_line < 0) return;
            if (g_analyse_code[w_line].type == TRACECODE.TYPE.NON)
            {
                g_analyse_code[w_line].type = TRACECODE.TYPE.CHK;
                g_chk_enable = true;
            }
            g_analyse_code[w_line].func_address = g_func_address;

            if (((g_cpu_pause == true)&&
                 ((md_main.g_trace_sip == false)||
                 ((md_main.g_md_m68k.g_interrupt_H_act == false)
                 &&(md_main.g_md_m68k.g_interrupt_V_act == false)
                 &&(md_main.g_md_m68k.g_interrupt_EXT_act == false))))
                || (g_analyse_code[w_line].break_static == true)
                || (g_analyse_code[w_line].break_flash == true))
            {
                g_cpu_pause = true;
                if (g_chk_enable == true)
                {
                    analyses();
                    g_chk_enable = false;
                }
                g_analyse_code[w_line].break_flash = false;
                md_main.g_form_code.RequestTraceBreakView(w_line);
                md_main.g_form_registry.RequestRegistryRefresh();
                md_main.g_form_flow.RequestFlowUpdate(g_func_address, g_caller_address);

                g_waitHandle.WaitOne(Timeout.Infinite);
                g_waitHandle.Reset();
            }
        }
    }
}
