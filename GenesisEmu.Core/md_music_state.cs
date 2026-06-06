using System;
using System.IO;

namespace MDTracer
{
    internal partial class md_music
    {
        public void write_state(BinaryWriter in_writer)
        {
            write_array(in_writer, g_master_chk);
            write_array(in_writer, g_master_vol);
            write_array(in_writer, g_out_vol);
            write_array(in_writer, g_freq_out);
            write_array(in_writer, g_buffer);
            in_writer.Write(g_buffer_cur);
            in_writer.Write(g_clock_total);
            g_md_sn76489.write_state(in_writer);
            g_md_ym2612.write_state(in_writer);
        }

        public void restore_state(BinaryReader in_reader)
        {
            read_array(in_reader, g_master_chk);
            read_array(in_reader, g_master_vol);
            read_array(in_reader, g_out_vol);
            read_array(in_reader, g_freq_out);
            read_array(in_reader, g_buffer);
            g_buffer_cur = in_reader.ReadInt32();
            g_clock_total = in_reader.ReadSingle();
            g_md_sn76489.restore_state(in_reader);
            g_md_ym2612.restore_state(in_reader);
        }

        private static void write_array(BinaryWriter in_writer, bool[] in_array)
        {
            in_writer.Write(in_array.Length);
            for (int i = 0; i < in_array.Length; i++) in_writer.Write(in_array[i]);
        }

        private static void write_array(BinaryWriter in_writer, int[] in_array)
        {
            in_writer.Write(in_array.Length);
            for (int i = 0; i < in_array.Length; i++) in_writer.Write(in_array[i]);
        }

        private static void write_array(BinaryWriter in_writer, float[] in_array)
        {
            in_writer.Write(in_array.Length);
            for (int i = 0; i < in_array.Length; i++) in_writer.Write(in_array[i]);
        }

        private static void write_array(BinaryWriter in_writer, byte[] in_array)
        {
            in_writer.Write(in_array.Length);
            in_writer.Write(in_array);
        }

        private static void read_array(BinaryReader in_reader, bool[] in_array)
        {
            int w_length = in_reader.ReadInt32();
            if (w_length != in_array.Length) throw new InvalidDataException("music bool array size is invalid.");
            for (int i = 0; i < in_array.Length; i++) in_array[i] = in_reader.ReadBoolean();
        }

        private static void read_array(BinaryReader in_reader, int[] in_array)
        {
            int w_length = in_reader.ReadInt32();
            if (w_length != in_array.Length) throw new InvalidDataException("music int array size is invalid.");
            for (int i = 0; i < in_array.Length; i++) in_array[i] = in_reader.ReadInt32();
        }

        private static void read_array(BinaryReader in_reader, float[] in_array)
        {
            int w_length = in_reader.ReadInt32();
            if (w_length != in_array.Length) throw new InvalidDataException("music float array size is invalid.");
            for (int i = 0; i < in_array.Length; i++) in_array[i] = in_reader.ReadSingle();
        }

        private static void read_array(BinaryReader in_reader, byte[] in_array)
        {
            int w_length = in_reader.ReadInt32();
            if (w_length != in_array.Length) throw new InvalidDataException("music byte array size is invalid.");
            byte[] w_data = in_reader.ReadBytes(w_length);
            if (w_data.Length != w_length) throw new InvalidDataException("music byte array data is invalid.");
            Buffer.BlockCopy(w_data, 0, in_array, 0, w_length);
        }
    }

    internal partial class md_sn76489
    {
        public void write_state(BinaryWriter in_writer)
        {
            write_array(in_writer, g_psg_clock);
            write_array(in_writer, g_channel_out);
            write_array(in_writer, g_freq);
            write_array(in_writer, g_vol);
            write_array(in_writer, g_duty);
            in_writer.Write(g_noise_mode);
            in_writer.Write(g_write_num_bk);
            in_writer.Write(g_shift_reg);
            in_writer.Write(g_ch2_clock);
        }

        public void restore_state(BinaryReader in_reader)
        {
            read_array(in_reader, g_psg_clock);
            read_array(in_reader, g_channel_out);
            read_array(in_reader, g_freq);
            read_array(in_reader, g_vol);
            read_array(in_reader, g_duty);
            g_noise_mode = in_reader.ReadBoolean();
            g_write_num_bk = in_reader.ReadInt32();
            g_shift_reg = in_reader.ReadInt32();
            g_ch2_clock = in_reader.ReadSingle();
        }

        private static void write_array(BinaryWriter in_writer, float[] in_array)
        {
            in_writer.Write(in_array.Length);
            for (int i = 0; i < in_array.Length; i++) in_writer.Write(in_array[i]);
        }

        private static void write_array(BinaryWriter in_writer, int[] in_array)
        {
            in_writer.Write(in_array.Length);
            for (int i = 0; i < in_array.Length; i++) in_writer.Write(in_array[i]);
        }

        private static void write_array(BinaryWriter in_writer, bool[] in_array)
        {
            in_writer.Write(in_array.Length);
            for (int i = 0; i < in_array.Length; i++) in_writer.Write(in_array[i]);
        }

        private static void read_array(BinaryReader in_reader, float[] in_array)
        {
            int w_length = in_reader.ReadInt32();
            if (w_length != in_array.Length) throw new InvalidDataException("SN76489 float array size is invalid.");
            for (int i = 0; i < in_array.Length; i++) in_array[i] = in_reader.ReadSingle();
        }

        private static void read_array(BinaryReader in_reader, int[] in_array)
        {
            int w_length = in_reader.ReadInt32();
            if (w_length != in_array.Length) throw new InvalidDataException("SN76489 int array size is invalid.");
            for (int i = 0; i < in_array.Length; i++) in_array[i] = in_reader.ReadInt32();
        }

        private static void read_array(BinaryReader in_reader, bool[] in_array)
        {
            int w_length = in_reader.ReadInt32();
            if (w_length != in_array.Length) throw new InvalidDataException("SN76489 bool array size is invalid.");
            for (int i = 0; i < in_array.Length; i++) in_array[i] = in_reader.ReadBoolean();
        }
    }

    internal partial class md_ym2612
    {
        public void write_state(BinaryWriter in_writer)
        {
            in_writer.Write(g_reg_22_lfo_enable);
            in_writer.Write(g_reg_22_lfo_inc);
            in_writer.Write(g_reg_24_timerA);
            in_writer.Write(g_reg_26_timerB);
            in_writer.Write(g_reg_27_mode);
            in_writer.Write(g_reg_27_enable_A);
            in_writer.Write(g_reg_27_enable_B);
            in_writer.Write(g_reg_27_load_B);
            in_writer.Write(g_reg_27_load_A);
            in_writer.Write(g_reg_2a_dac_data);
            in_writer.Write(g_reg_2b_dac);

            write_array(in_writer, g_reg_30_multi);
            write_array(in_writer, g_reg_30_dt);
            write_array(in_writer, g_reg_40_tl);
            write_array(in_writer, g_reg_50_key_scale);
            write_array(in_writer, g_reg_60_ams_enable);
            write_array(in_writer, g_reg_80_sl);
            write_array(in_writer, g_reg_90_ssg);
            write_array(in_writer, g_reg_a0_fnum);
            write_array(in_writer, g_reg_a4_fnum);
            write_array(in_writer, g_reg_a4_block);
            write_array(in_writer, g_reg_b0_fb);
            write_array(in_writer, g_reg_b0_algo);
            write_array(in_writer, g_reg_b4_l);
            write_array(in_writer, g_reg_b4_r);
            write_array(in_writer, g_reg_b4_ams);
            write_array(in_writer, g_reg_b4_pms);

            in_writer.Write(g_reg_addr1);
            in_writer.Write(g_reg_addr2);
            write_array(in_writer, g_reg);
            in_writer.Write(g_dac_high_level);
            in_writer.Write(g_com_lfo_cnt);
            in_writer.Write(g_com_lfo_env_cnt);
            in_writer.Write(g_com_lfo_freq_cnt);
            in_writer.Write(g_com_timerA);
            in_writer.Write(g_com_timerB);
            in_writer.Write(g_com_timerA_cnt);
            in_writer.Write(g_com_timerB_cnt);
            in_writer.Write(g_com_status);

            write_array(in_writer, g_ch_out);
            write_array(in_writer, g_ch_reg_reflesh);
            write_array(in_writer, g_ch_pms_cnt);
            write_array(in_writer, g_slot_key_scale);
            write_array(in_writer, g_slot_fnum);
            write_array(in_writer, g_slot_keycode);
            write_array(in_writer, g_slot_freq_cnt);
            write_array(in_writer, g_slot_op_calc);
            write_array(in_writer, g_slot_phase_out);
            write_array(in_writer, g_slot_phase_inc);
            write_env_cond_array(in_writer, g_slot_env_cond);
            write_array(in_writer, g_slot_env_incA);
            write_array(in_writer, g_slot_env_incD);
            write_array(in_writer, g_slot_env_incS);
            write_array(in_writer, g_slot_env_incR);
            write_array(in_writer, g_slot_env_cnt);
            write_array(in_writer, g_slot_env_cmp);
            write_array(in_writer, g_slot_env_out);
            write_array(in_writer, g_slot_env_indexA);
            write_array(in_writer, g_slot_env_indexD);
            write_array(in_writer, g_slot_env_indexS);
            write_array(in_writer, g_slot_env_indexR);
            write_array(in_writer, g_slot_ams);
            write_array(in_writer, g_slot_CNT_MASK);
        }

        public void restore_state(BinaryReader in_reader)
        {
            g_reg_22_lfo_enable = in_reader.ReadBoolean();
            g_reg_22_lfo_inc = in_reader.ReadInt32();
            g_reg_24_timerA = in_reader.ReadInt32();
            g_reg_26_timerB = in_reader.ReadInt32();
            g_reg_27_mode = in_reader.ReadByte();
            g_reg_27_enable_A = in_reader.ReadBoolean();
            g_reg_27_enable_B = in_reader.ReadBoolean();
            g_reg_27_load_B = in_reader.ReadBoolean();
            g_reg_27_load_A = in_reader.ReadBoolean();
            g_reg_2a_dac_data = in_reader.ReadInt32();
            g_reg_2b_dac = in_reader.ReadInt32();

            read_array(in_reader, g_reg_30_multi);
            read_array(in_reader, g_reg_30_dt);
            read_array(in_reader, g_reg_40_tl);
            read_array(in_reader, g_reg_50_key_scale);
            read_array(in_reader, g_reg_60_ams_enable);
            read_array(in_reader, g_reg_80_sl);
            read_array(in_reader, g_reg_90_ssg);
            read_array(in_reader, g_reg_a0_fnum);
            read_array(in_reader, g_reg_a4_fnum);
            read_array(in_reader, g_reg_a4_block);
            read_array(in_reader, g_reg_b0_fb);
            read_array(in_reader, g_reg_b0_algo);
            read_array(in_reader, g_reg_b4_l);
            read_array(in_reader, g_reg_b4_r);
            read_array(in_reader, g_reg_b4_ams);
            read_array(in_reader, g_reg_b4_pms);

            g_reg_addr1 = in_reader.ReadByte();
            g_reg_addr2 = in_reader.ReadByte();
            read_array(in_reader, g_reg);
            g_dac_high_level = in_reader.ReadInt32();
            g_com_lfo_cnt = in_reader.ReadInt32();
            g_com_lfo_env_cnt = in_reader.ReadInt32();
            g_com_lfo_freq_cnt = in_reader.ReadInt32();
            g_com_timerA = in_reader.ReadInt32();
            g_com_timerB = in_reader.ReadInt32();
            g_com_timerA_cnt = in_reader.ReadInt32();
            g_com_timerB_cnt = in_reader.ReadInt32();
            g_com_status = in_reader.ReadByte();

            read_array(in_reader, g_ch_out);
            read_array(in_reader, g_ch_reg_reflesh);
            read_array(in_reader, g_ch_pms_cnt);
            read_array(in_reader, g_slot_key_scale);
            read_array(in_reader, g_slot_fnum);
            read_array(in_reader, g_slot_keycode);
            read_array(in_reader, g_slot_freq_cnt);
            read_array(in_reader, g_slot_op_calc);
            read_array(in_reader, g_slot_phase_out);
            read_array(in_reader, g_slot_phase_inc);
            read_env_cond_array(in_reader, g_slot_env_cond);
            read_array(in_reader, g_slot_env_incA);
            read_array(in_reader, g_slot_env_incD);
            read_array(in_reader, g_slot_env_incS);
            read_array(in_reader, g_slot_env_incR);
            read_array(in_reader, g_slot_env_cnt);
            read_array(in_reader, g_slot_env_cmp);
            read_array(in_reader, g_slot_env_out);
            read_array(in_reader, g_slot_env_indexA);
            read_array(in_reader, g_slot_env_indexD);
            read_array(in_reader, g_slot_env_indexS);
            read_array(in_reader, g_slot_env_indexR);
            read_array(in_reader, g_slot_ams);
            read_array(in_reader, g_slot_CNT_MASK);
        }

        private static void write_array(BinaryWriter in_writer, byte[] in_array)
        {
            in_writer.Write(in_array.Length);
            in_writer.Write(in_array);
        }

        private static void write_array(BinaryWriter in_writer, int[] in_array)
        {
            in_writer.Write(in_array.Length);
            for (int i = 0; i < in_array.Length; i++) in_writer.Write(in_array[i]);
        }

        private static void write_array(BinaryWriter in_writer, bool[] in_array)
        {
            in_writer.Write(in_array.Length);
            for (int i = 0; i < in_array.Length; i++) in_writer.Write(in_array[i]);
        }

        private static void write_array(BinaryWriter in_writer, double[] in_array)
        {
            in_writer.Write(in_array.Length);
            for (int i = 0; i < in_array.Length; i++) in_writer.Write(in_array[i]);
        }

        private static void write_array(BinaryWriter in_writer, byte[,] in_array)
        {
            in_writer.Write(in_array.GetLength(0));
            in_writer.Write(in_array.GetLength(1));
            for (int y = 0; y < in_array.GetLength(0); y++)
            {
                for (int x = 0; x < in_array.GetLength(1); x++) in_writer.Write(in_array[y, x]);
            }
        }

        private static void write_array(BinaryWriter in_writer, int[,] in_array)
        {
            in_writer.Write(in_array.GetLength(0));
            in_writer.Write(in_array.GetLength(1));
            for (int y = 0; y < in_array.GetLength(0); y++)
            {
                for (int x = 0; x < in_array.GetLength(1); x++) in_writer.Write(in_array[y, x]);
            }
        }

        private static void write_array(BinaryWriter in_writer, bool[,] in_array)
        {
            in_writer.Write(in_array.GetLength(0));
            in_writer.Write(in_array.GetLength(1));
            for (int y = 0; y < in_array.GetLength(0); y++)
            {
                for (int x = 0; x < in_array.GetLength(1); x++) in_writer.Write(in_array[y, x]);
            }
        }

        private static void write_array(BinaryWriter in_writer, double[,] in_array)
        {
            in_writer.Write(in_array.GetLength(0));
            in_writer.Write(in_array.GetLength(1));
            for (int y = 0; y < in_array.GetLength(0); y++)
            {
                for (int x = 0; x < in_array.GetLength(1); x++) in_writer.Write(in_array[y, x]);
            }
        }

        private static void write_env_cond_array(BinaryWriter in_writer, ENV_COND[,] in_array)
        {
            in_writer.Write(in_array.GetLength(0));
            in_writer.Write(in_array.GetLength(1));
            for (int y = 0; y < in_array.GetLength(0); y++)
            {
                for (int x = 0; x < in_array.GetLength(1); x++) in_writer.Write((int)in_array[y, x]);
            }
        }

        private static void read_array(BinaryReader in_reader, byte[] in_array)
        {
            int w_length = in_reader.ReadInt32();
            if (w_length != in_array.Length) throw new InvalidDataException("YM2612 byte array size is invalid.");
            byte[] w_data = in_reader.ReadBytes(w_length);
            if (w_data.Length != w_length) throw new InvalidDataException("YM2612 byte array data is invalid.");
            Buffer.BlockCopy(w_data, 0, in_array, 0, w_length);
        }

        private static void read_array(BinaryReader in_reader, int[] in_array)
        {
            int w_length = in_reader.ReadInt32();
            if (w_length != in_array.Length) throw new InvalidDataException("YM2612 int array size is invalid.");
            for (int i = 0; i < in_array.Length; i++) in_array[i] = in_reader.ReadInt32();
        }

        private static void read_array(BinaryReader in_reader, bool[] in_array)
        {
            int w_length = in_reader.ReadInt32();
            if (w_length != in_array.Length) throw new InvalidDataException("YM2612 bool array size is invalid.");
            for (int i = 0; i < in_array.Length; i++) in_array[i] = in_reader.ReadBoolean();
        }

        private static void read_array(BinaryReader in_reader, double[] in_array)
        {
            int w_length = in_reader.ReadInt32();
            if (w_length != in_array.Length) throw new InvalidDataException("YM2612 double array size is invalid.");
            for (int i = 0; i < in_array.Length; i++) in_array[i] = in_reader.ReadDouble();
        }

        private static void read_array(BinaryReader in_reader, byte[,] in_array)
        {
            check_rank(in_reader, in_array.GetLength(0), in_array.GetLength(1), "YM2612 byte[,] array size is invalid.");
            for (int y = 0; y < in_array.GetLength(0); y++)
            {
                for (int x = 0; x < in_array.GetLength(1); x++) in_array[y, x] = in_reader.ReadByte();
            }
        }

        private static void read_array(BinaryReader in_reader, int[,] in_array)
        {
            check_rank(in_reader, in_array.GetLength(0), in_array.GetLength(1), "YM2612 int[,] array size is invalid.");
            for (int y = 0; y < in_array.GetLength(0); y++)
            {
                for (int x = 0; x < in_array.GetLength(1); x++) in_array[y, x] = in_reader.ReadInt32();
            }
        }

        private static void read_array(BinaryReader in_reader, bool[,] in_array)
        {
            check_rank(in_reader, in_array.GetLength(0), in_array.GetLength(1), "YM2612 bool[,] array size is invalid.");
            for (int y = 0; y < in_array.GetLength(0); y++)
            {
                for (int x = 0; x < in_array.GetLength(1); x++) in_array[y, x] = in_reader.ReadBoolean();
            }
        }

        private static void read_array(BinaryReader in_reader, double[,] in_array)
        {
            check_rank(in_reader, in_array.GetLength(0), in_array.GetLength(1), "YM2612 double[,] array size is invalid.");
            for (int y = 0; y < in_array.GetLength(0); y++)
            {
                for (int x = 0; x < in_array.GetLength(1); x++) in_array[y, x] = in_reader.ReadDouble();
            }
        }

        private static void read_env_cond_array(BinaryReader in_reader, ENV_COND[,] in_array)
        {
            check_rank(in_reader, in_array.GetLength(0), in_array.GetLength(1), "YM2612 ENV_COND array size is invalid.");
            for (int y = 0; y < in_array.GetLength(0); y++)
            {
                for (int x = 0; x < in_array.GetLength(1); x++) in_array[y, x] = (ENV_COND)in_reader.ReadInt32();
            }
        }

        private static void check_rank(BinaryReader in_reader, int in_y, int in_x, string in_message)
        {
            int w_y = in_reader.ReadInt32();
            int w_x = in_reader.ReadInt32();
            if ((w_y != in_y) || (w_x != in_x)) throw new InvalidDataException(in_message);
        }
    }
}
