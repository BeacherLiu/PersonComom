using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PersonalCommon
{
    /// <summary>
    /// 数据标准化校验
    /// </summary>
    public static class DataFormat
    {
        #region 验证身份证  
        
        /// <summary>
        /// 身份证号码校验
        /// </summary>
        /// <param name="idcard">身份证号码</param>
        /// <returns>true 符合规范 false 不是有效身份证号码</returns>
        public static bool IDCardFormat(string idcard)
        {
            if (idcard.Length == 18)
            {
                bool check = CheckIDCard18(idcard);
                return check;
            }
            else if (idcard.Length == 15)
            {
                bool check = CheckIDCard15(idcard);
                return check;
            }
            else
            {
                return false;
            }
        }


        private static bool CheckIDCard18(string Id)
        {
            
            long n = 0;
            if (long.TryParse(Id.Remove(17), out n) == false || n < Math.Pow(10, 16) || long.TryParse(Id.Replace('x', '0').Replace('X', '0'), out n) == false)
            {
                return false;//数字验证  
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(Id.Remove(2)) == -1)
            {
                return false;//省份验证  
            }

            string birth = Id.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证  
            }

            string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            char[] Ai = Id.Remove(17).ToCharArray();
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
            }
            int y = -1;
            DivRem(sum, 11, out y);
            if (arrVarifyCode[y] != Id.Substring(17, 1).ToLower())
            {
                return false;//校验码验证  
            }
            return true;//符合GB11643-1999标准  
        }

        private static int DivRem(int a, int b, out int result)
        {
            result = a % b;
            return (a / b);
        }

        private static bool CheckIDCard15(string Id)
        {
            long n = 0;
            if (long.TryParse(Id, out n) == false || n < Math.Pow(10, 14))
            {
                return false;//数字验证  
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(Id.Remove(2)) == -1)
            {
                return false;//省份验证  
            }
            string birth = Id.Substring(6, 6).Insert(4, "-").Insert(2, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证  
            }
            return true;//符合15位身份证标准  
        }
        #endregion

        #region 验证手机号码
        /// <summary>
        /// 效验手机号码、座机号码
        /// </summary>
        /// <param name="phone">手机号或座机号</param>
        /// <returns>true 符合规范 false 不符合规范</returns>
        public static bool PhoneFormat(string phone)
        {
            Regex regex = new Regex("^(0?(13[0-9]|15[012356789]|17[013678]|18[0-9]|14[57])[0-9]{8})|(400|800)([0-9\\-]{7,10})|(([0-9]{4}|[0-9]{3})(-| )?)?([0-9]{7,8})((-| |转)*([0-9]{1,4}))?$");
            return regex.IsMatch(phone);
        }
        #endregion


    }
}