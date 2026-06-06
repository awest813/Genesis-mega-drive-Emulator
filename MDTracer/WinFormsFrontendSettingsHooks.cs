namespace MDTracer
{
    internal sealed class WinFormsFrontendSettingsHooks : IFrontendSettingsHooks
    {
        public bool TryApplySetting(string in_name, string in_value)
        {
            string[] w_val = in_value.Split(':');
            switch (in_name)
            {
                case "screen_main":
                    Form_Main.g_screen_xpos = int.Parse(w_val[0]);
                    Form_Main.g_screen_ypos = int.Parse(w_val[1]);
                    Form_Main.g_screen_size_x = int.Parse(w_val[2]);
                    Form_Main.g_screen_size_y = int.Parse(w_val[3]);
                    return true;
                case "screenA":
                    md_main.g_debugView.screenA_enable = w_val[0] == "1";
                    WinFormsDebugTools.g_form_screenA.g_screen_xpos = int.Parse(w_val[1]);
                    WinFormsDebugTools.g_form_screenA.g_screen_ypos = int.Parse(w_val[2]);
                    read_vdp_screen_menu_setting(WinFormsDebugTools.g_form_screenA, w_val);
                    return true;
                case "screenB":
                    md_main.g_debugView.screenB_enable = w_val[0] == "1";
                    WinFormsDebugTools.g_form_screenB.g_screen_xpos = int.Parse(w_val[1]);
                    WinFormsDebugTools.g_form_screenB.g_screen_ypos = int.Parse(w_val[2]);
                    read_vdp_screen_menu_setting(WinFormsDebugTools.g_form_screenB, w_val);
                    return true;
                case "screenW":
                    md_main.g_debugView.screenW_enable = w_val[0] == "1";
                    WinFormsDebugTools.g_form_screenW.g_screen_xpos = int.Parse(w_val[1]);
                    WinFormsDebugTools.g_form_screenW.g_screen_ypos = int.Parse(w_val[2]);
                    read_vdp_screen_menu_setting(WinFormsDebugTools.g_form_screenW, w_val);
                    return true;
                case "screenS":
                    md_main.g_debugView.screenS_enable = w_val[0] == "1";
                    WinFormsDebugTools.g_form_screenS.g_screen_xpos = int.Parse(w_val[1]);
                    WinFormsDebugTools.g_form_screenS.g_screen_ypos = int.Parse(w_val[2]);
                    read_vdp_screen_menu_setting(WinFormsDebugTools.g_form_screenS, w_val);
                    return true;
                case "pattern":
                    md_main.g_debugView.pattern_enable = w_val[0] == "1";
                    WinFormsDebugTools.g_form_pattern.g_screen_xpos = int.Parse(w_val[1]);
                    WinFormsDebugTools.g_form_pattern.g_screen_ypos = int.Parse(w_val[2]);
                    return true;
                case "pallete":
                    md_main.g_debugView.pallete_enable = w_val[0] == "1";
                    WinFormsDebugTools.g_form_pallete.g_screen_xpos = int.Parse(w_val[1]);
                    WinFormsDebugTools.g_form_pallete.g_screen_ypos = int.Parse(w_val[2]);
                    return true;
                case "code":
                    md_main.g_debugView.code_enable = w_val[0] == "1";
                    WinFormsDebugTools.g_form_code.g_screen_xpos = int.Parse(w_val[1]);
                    WinFormsDebugTools.g_form_code.g_screen_ypos = int.Parse(w_val[2]);
                    return true;
                case "code_tools":
                    WinFormsDebugTools.g_form_code.SetCodeToolLayoutText(in_value);
                    return true;
                case "io":
                    md_main.g_debugView.io_enable = w_val[0] == "1";
                    WinFormsDebugTools.g_form_io.g_screen_xpos = int.Parse(w_val[1]);
                    WinFormsDebugTools.g_form_io.g_screen_ypos = int.Parse(w_val[2]);
                    return true;
                case "music":
                    md_main.g_debugView.music_enable = w_val[0] == "1";
                    WinFormsDebugTools.g_form_music.g_screen_xpos = int.Parse(w_val[1]);
                    WinFormsDebugTools.g_form_music.g_screen_ypos = int.Parse(w_val[2]);
                    return true;
                case "registry":
                    md_main.g_debugView.registry_enable = w_val[0] == "1";
                    WinFormsDebugTools.g_form_registry.g_screen_xpos = int.Parse(w_val[1]);
                    WinFormsDebugTools.g_form_registry.g_screen_ypos = int.Parse(w_val[2]);
                    return true;
                case "flow":
                    md_main.g_debugView.flow_enable = w_val[0] == "1";
                    WinFormsDebugTools.g_form_flow.g_screen_xpos = int.Parse(w_val[1]);
                    WinFormsDebugTools.g_form_flow.g_screen_ypos = int.Parse(w_val[2]);
                    return true;
                case "setting_view":
                    apply_overlay_view_setting(w_val);
                    return true;
                case "file0": Form_Main.g_file_name[0] = in_value; return true;
                case "file1": Form_Main.g_file_name[1] = in_value; return true;
                case "file2": Form_Main.g_file_name[2] = in_value; return true;
                case "file3": Form_Main.g_file_name[3] = in_value; return true;
                case "file4": Form_Main.g_file_name[4] = in_value; return true;
                case "file5": Form_Main.g_file_name[5] = in_value; return true;
                case "file6": Form_Main.g_file_name[6] = in_value; return true;
                case "file7": Form_Main.g_file_name[7] = in_value; return true;
                case "file8": Form_Main.g_file_name[8] = in_value; return true;
                default:
                    return false;
            }
        }

        public void CaptureSettings(Action<string, string> settingAdd)
        {
            string w_val = Form_Main.g_screen_xpos
                + ":" + Form_Main.g_screen_ypos
                + ":" + Form_Main.g_screen_size_x
                + ":" + Form_Main.g_screen_size_y;
            settingAdd("screen_main", w_val);

            w_val = ((md_main.g_debugView.screenA_enable == true) ? "1" : "0")
                + ":" + WinFormsDebugTools.g_form_screenA.g_screen_xpos
                + ":" + WinFormsDebugTools.g_form_screenA.g_screen_ypos
                + ":" + WinFormsDebugTools.g_form_screenA.GetMenuSettingText();
            settingAdd("screenA", w_val);

            w_val = ((md_main.g_debugView.screenB_enable == true) ? "1" : "0")
                + ":" + WinFormsDebugTools.g_form_screenB.g_screen_xpos
                + ":" + WinFormsDebugTools.g_form_screenB.g_screen_ypos
                + ":" + WinFormsDebugTools.g_form_screenB.GetMenuSettingText();
            settingAdd("screenB", w_val);

            w_val = ((md_main.g_debugView.screenW_enable == true) ? "1" : "0")
                + ":" + WinFormsDebugTools.g_form_screenW.g_screen_xpos
                + ":" + WinFormsDebugTools.g_form_screenW.g_screen_ypos
                + ":" + WinFormsDebugTools.g_form_screenW.GetMenuSettingText();
            settingAdd("screenW", w_val);

            w_val = ((md_main.g_debugView.screenS_enable == true) ? "1" : "0")
                + ":" + WinFormsDebugTools.g_form_screenS.g_screen_xpos
                + ":" + WinFormsDebugTools.g_form_screenS.g_screen_ypos
                + ":" + WinFormsDebugTools.g_form_screenS.GetMenuSettingText();
            settingAdd("screenS", w_val);

            w_val = ((md_main.g_debugView.pattern_enable == true) ? "1" : "0")
                + ":" + WinFormsDebugTools.g_form_pattern.g_screen_xpos
                + ":" + WinFormsDebugTools.g_form_pattern.g_screen_ypos;
            settingAdd("pattern", w_val);

            w_val = ((md_main.g_debugView.pallete_enable == true) ? "1" : "0")
                + ":" + WinFormsDebugTools.g_form_pallete.g_screen_xpos
                + ":" + WinFormsDebugTools.g_form_pallete.g_screen_ypos;
            settingAdd("pallete", w_val);

            w_val = ((md_main.g_debugView.code_enable == true) ? "1" : "0")
                + ":" + WinFormsDebugTools.g_form_code.g_screen_xpos
                + ":" + WinFormsDebugTools.g_form_code.g_screen_ypos;
            settingAdd("code", w_val);
            settingAdd("code_tools", WinFormsDebugTools.g_form_code.GetCodeToolLayoutText());

            w_val = ((md_main.g_debugView.io_enable == true) ? "1" : "0")
                + ":" + WinFormsDebugTools.g_form_io.g_screen_xpos
                + ":" + WinFormsDebugTools.g_form_io.g_screen_ypos;
            settingAdd("io", w_val);

            w_val = ((md_main.g_debugView.music_enable == true) ? "1" : "0")
                + ":" + WinFormsDebugTools.g_form_music.g_screen_xpos
                + ":" + WinFormsDebugTools.g_form_music.g_screen_ypos;
            settingAdd("music", w_val);

            w_val = ((md_main.g_debugView.registry_enable == true) ? "1" : "0")
                + ":" + WinFormsDebugTools.g_form_registry.g_screen_xpos
                + ":" + WinFormsDebugTools.g_form_registry.g_screen_ypos;
            settingAdd("registry", w_val);

            w_val = ((md_main.g_debugView.flow_enable == true) ? "1" : "0")
                + ":" + WinFormsDebugTools.g_form_flow.g_screen_xpos
                + ":" + WinFormsDebugTools.g_form_flow.g_screen_ypos;
            settingAdd("flow", w_val);

            settingAdd("setting_view", capture_overlay_view_setting());

            for (int i = 0; i < 9; i++)
            {
                settingAdd("file" + i, Form_Main.g_file_name[i]);
            }
        }

        public void EnsureCodeToolLayoutVisible()
        {
            WinFormsDebugTools.g_form_code.EnsureCodeToolLayoutVisible();
        }

        private static void read_vdp_screen_menu_setting(Form_VDP_Screen in_form, string[] in_val)
        {
            if (in_val.Length < 6) return;

            in_form.SetMenuSetting(
                in_val[3] == "1",
                in_val[4] == "1",
                in_val[5] == "1");
        }

        private static void apply_overlay_view_setting(string[] in_val)
        {
            md_vdp w_vdp = md_main.g_md_vdp;
            if (w_vdp == null) return;

            if (in_val.Length > 0) w_vdp.g_overlay_view_screenA = in_val[0] == "1";
            if (in_val.Length > 1) w_vdp.g_overlay_view_screenB = in_val[1] == "1";
            if (in_val.Length > 2) w_vdp.g_overlay_view_screenW = in_val[2] == "1";
            if (in_val.Length > 3) w_vdp.g_overlay_view_screenS = in_val[3] == "1";
            if (in_val.Length > 4) w_vdp.g_overlay_screenA_High = in_val[4] == "1";
            if (in_val.Length > 5) w_vdp.g_overlay_screenA_Low = in_val[5] == "1";
            if (in_val.Length > 6) w_vdp.g_overlay_screenB_High = in_val[6] == "1";
            if (in_val.Length > 7) w_vdp.g_overlay_screenB_Low = in_val[7] == "1";
            if (in_val.Length > 8) w_vdp.g_overlay_screenW_High = in_val[8] == "1";
            if (in_val.Length > 9) w_vdp.g_overlay_screenW_Low = in_val[9] == "1";
            if (in_val.Length > 10) w_vdp.g_overlay_screenS_High = in_val[10] == "1";
            if (in_val.Length > 11) w_vdp.g_overlay_screenS_Low = in_val[11] == "1";
        }

        private static string capture_overlay_view_setting()
        {
            md_vdp w_vdp = md_main.g_md_vdp;
            return ((w_vdp.g_overlay_view_screenA == true) ? "1" : "0")
                + ":" + ((w_vdp.g_overlay_view_screenB == true) ? "1" : "0")
                + ":" + ((w_vdp.g_overlay_view_screenW == true) ? "1" : "0")
                + ":" + ((w_vdp.g_overlay_view_screenS == true) ? "1" : "0")
                + ":" + ((w_vdp.g_overlay_screenA_High == true) ? "1" : "0")
                + ":" + ((w_vdp.g_overlay_screenA_Low == true) ? "1" : "0")
                + ":" + ((w_vdp.g_overlay_screenB_High == true) ? "1" : "0")
                + ":" + ((w_vdp.g_overlay_screenB_Low == true) ? "1" : "0")
                + ":" + ((w_vdp.g_overlay_screenW_High == true) ? "1" : "0")
                + ":" + ((w_vdp.g_overlay_screenW_Low == true) ? "1" : "0")
                + ":" + ((w_vdp.g_overlay_screenS_High == true) ? "1" : "0")
                + ":" + ((w_vdp.g_overlay_screenS_Low == true) ? "1" : "0");
        }
    }
}
