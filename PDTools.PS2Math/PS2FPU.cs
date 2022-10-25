using System;

namespace PDTools.PS2Math
{
    public class PS2FPU
    {
        public static float pcsx2_float(double in_dbl)
        {
            //xMOVAPS(xRegisterSSE(absreg), xRegisterSSE(reg));
            ulong in_ulong = BitConverter.ToUInt64(BitConverter.GetBytes(in_dbl), 0);
            //xAND.PD(xRegisterSSE(absreg), ptr[&s_const.dbl_s_pos]);
            ulong abs_ulong = in_ulong & 0x7fffffffffffffffUL;

            //xUCOMI.SD(xRegisterSSE(absreg), ptr[&s_const.dbl_ps2_overflow]); u8* to_overflow = JAE8(0);
            if (abs_ulong >= 0x4800000000000000UL)
            {
                //xCVTSD2SS(xRegisterSSE(reg), xRegisterSSE(reg));
                float overflow_flt = (float)in_dbl;
                //xOR.PS(xRegisterSSE(reg), ptr[&s_const.pos]); //clamp
                uint overflow_uint = BitConverter.ToUInt32(BitConverter.GetBytes(overflow_flt), 0) | 0x7fffffffU;
                return BitConverter.ToSingle(BitConverter.GetBytes(overflow_uint), 0);
            }

            //xUCOMI.SD(xRegisterSSE(absreg), ptr[&s_const.dbl_cvt_overflow]); u8* to_complex = JAE8(0);
            if (abs_ulong >= 0x47f0000000000000UL)
            {
                //xPSUB.Q(xRegisterSSE(reg), ptr[&s_const.dbl_one_exp]);
                ulong complex_ulong = in_ulong - 0x10000000000000UL;
                //xCVTSD2SS(xRegisterSSE(reg), xRegisterSSE(reg));
                float complex_flt = (float)BitConverter.ToDouble(BitConverter.GetBytes(complex_ulong), 0);
                //xPADD.D(xRegisterSSE(reg), ptr[s_const.one_exp]);
                uint complex_uint = BitConverter.ToUInt32(BitConverter.GetBytes(complex_flt), 0) + 0x800000U;
                return BitConverter.ToSingle(BitConverter.GetBytes(complex_uint), 0);
            }

            //xUCOMI.SD(xRegisterSSE(absreg), ptr[&s_const.dbl_underflow]); u8* to_underflow = JB8(0);
            if (abs_ulong < 0x3810000000000000UL)
            {
                //xCVTSD2SS(xRegisterSSE(reg), xRegisterSSE(reg));
                float underflow_flt = (float)in_dbl;
                //xAND.PS(xRegisterSSE(reg), ptr[s_const.neg]); //flush to zero
                uint underflow_uint = BitConverter.ToUInt32(BitConverter.GetBytes(underflow_flt), 0) & 0x80000000U;
                return BitConverter.ToSingle(BitConverter.GetBytes(underflow_uint), 0);
            }

            //xCVTSD2SS(xRegisterSSE(reg), xRegisterSSE(reg)); //simply convert
            return (float)in_dbl;
        }

        public static float pcsx2_mul(float a, float b)
        {
            double da = (double)a;
            double db = (double)b;

            var dc = da * db;

            return pcsx2_float(dc);
        }
    }
}
