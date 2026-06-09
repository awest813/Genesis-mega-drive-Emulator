using System;

namespace MDTracer
{
    public partial class Form_Code
    {
        //----------------------------------------------------------------
        private void Code_Paint_Code(PaintEventArgs e, int in_width, int in_height
            , int in_line_num, int in_top_line, int in_stop_line, int in_cursole_line, int in_hScrollBar)
        {
            ICodeAnalysisSession w_session = WinFormsDebugTools.g_codeAnalysis;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            e.Graphics.FillRectangle(Brushes.White, 0, 0, in_width, in_height);
            e.Graphics.FillRectangle(Brushes.DarkBlue, 0, 0, in_width, CODE_HEADER_HEIGHT);

            Font wfont = g_code_font;
            Font wfont_data = g_code_font_data;
            Brush wbrush = Brushes.White;
            Brush wbrush_red = Brushes.Red;
            e.Graphics.DrawString("addr", wfont, wbrush, new PointF(CODE_ADDRESS_X - in_hScrollBar, 1));
            e.Graphics.DrawString("dump", wfont, wbrush, new PointF(CODE_DUMP_X - in_hScrollBar, 1));
            e.Graphics.DrawString("mnemonic", wfont, wbrush, new PointF(CODE_MNEMONIC_HEADER_X - in_hScrollBar, 1));
            e.Graphics.DrawString("comment1", wfont, wbrush, new PointF(CODE_COMMENT_X - in_hScrollBar, 1));
            //-----------------------------------------------------------
            string w_text;
            int w_cur_line = in_top_line;
            int w_arrow_start_line = -1;
            int w_arrow_end_line = -1;
            for (int w_cur = 0; w_cur < in_line_num; w_cur++)
            {
                if (CodeAnalysisConstants.MemSize <= w_cur_line) break;
                if (w_session.ArrowStartLine == w_cur_line) w_arrow_start_line = w_cur;
                if (w_session.ArrowEndLine == w_cur_line) w_arrow_end_line = w_cur;

                CodeAnalysisTraceCode w_code = w_session.AnalyseCode[w_cur_line];
                int w_y = CODE_HEADER_HEIGHT + w_cur * CODE_LINE_HEIGHT;
                if (w_cur_line == in_stop_line)
                {
                    e.Graphics.FillRectangle(Brushes.LightBlue, 0, w_y, in_width, CODE_LINE_HEIGHT);
                }
                else if (w_code.type == CodeAnalysisTraceCode.Type.Non)
                {
                    e.Graphics.FillRectangle(Brushes.LightGray, 0, w_y, in_width, CODE_LINE_HEIGHT);
                }

                if (w_code.ret_line == true)
                {
                    e.Graphics.DrawLine(Pens.LightGray, 0, w_y + CODE_LINE_HEIGHT - 1, in_width, w_y + CODE_LINE_HEIGHT - 1);
                }
                if (w_cur_line == in_cursole_line)
                {
                    e.Graphics.DrawRectangle(Pens.Gray, 0, w_y, in_width, CODE_LINE_HEIGHT - 1);
                }

                //break mark
                if (w_code.break_static == true)
                {
                    w_text = "BK";
                    e.Graphics.DrawString(w_text, wfont, wbrush_red, new PointF(-in_hScrollBar, w_y));
                }

                //address
                w_text = w_code.address.ToString("X6");
                e.Graphics.DrawString(w_text, wfont, Brushes.Black, new PointF(CODE_ADDRESS_X - in_hScrollBar, w_y));

                //dump
                int w_dump_len = GetTraceCodeDumpLength(w_cur_line);
                for (int i = 0; i < w_dump_len; i++)
                {
                    w_text = w_session.AnalyseCode[w_cur_line + i].val.ToString("X4");
                    e.Graphics.DrawString(w_text, wfont_data, Brushes.Blue, new PointF(CODE_DUMP_X + i * 30 - in_hScrollBar, w_y + 2));
                }

                //View details
                if (w_code.type == CodeAnalysisTraceCode.Type.Opc)
                {
                    //mnemonic
                    w_text = w_code.operand ?? "";
                    e.Graphics.DrawString(w_text, wfont, Brushes.Black, new PointF(CODE_MNEMONIC_X - in_hScrollBar, w_y));
                }

                //comment1
                e.Graphics.DrawString(w_code.comment1 ?? "", wfont, Brushes.Black, new PointF(CODE_COMMENT_X - in_hScrollBar, w_y));
                w_cur_line += w_dump_len;
            }

            //arrow
            if ((w_session.ArrowStartLine != -1) && (w_session.ArrowEndLine != -1))
            {
                int w_first_visible_line = in_top_line;
                int w_after_visible_line = w_cur_line;
                int w_arrow_min_line = Math.Min(w_session.ArrowStartLine, w_session.ArrowEndLine);
                int w_arrow_max_line = Math.Max(w_session.ArrowStartLine, w_session.ArrowEndLine);

                if ((w_arrow_max_line < w_first_visible_line) || (w_after_visible_line <= w_arrow_min_line))
                {
                    return;
                }

                int GetArrowY(int in_line, int in_visible_row)
                {
                    if (in_visible_row >= 0)
                    {
                        return CODE_HEADER_HEIGHT + 8 + in_visible_row * CODE_LINE_HEIGHT;
                    }
                    return (in_line < w_first_visible_line)
                        ? CODE_HEADER_HEIGHT
                        : CODE_HEADER_HEIGHT + in_line_num * CODE_LINE_HEIGHT;
                }

                int w_start_y = GetArrowY(w_session.ArrowStartLine, w_arrow_start_line);
                int w_end_y = GetArrowY(w_session.ArrowEndLine, w_arrow_end_line);
                int w_x = CODE_JUMP_X - in_hScrollBar;
                int w_top = Math.Min(w_start_y, w_end_y);
                int w_bottom = Math.Max(w_start_y, w_end_y);

                if (w_bottom > w_top)
                {
                    e.Graphics.DrawLine(Pens.Red, w_x, w_top, w_x, w_bottom);
                }
                if (w_arrow_start_line != -1)
                {
                    e.Graphics.DrawLine(Pens.Red, w_x - 10, w_start_y, w_x, w_start_y);
                }
                if (w_arrow_end_line != -1)
                {
                    int w_direction = (w_session.ArrowStartLine <= w_session.ArrowEndLine) ? 1 : -1;
                    int w_wing_y = w_end_y - (4 * w_direction);
                    e.Graphics.DrawLine(Pens.Red, w_x, w_end_y, w_x - 4, w_wing_y);
                    e.Graphics.DrawLine(Pens.Red, w_x, w_end_y, w_x + 4, w_wing_y);
                }
            }
        }
    }
}
