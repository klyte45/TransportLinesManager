using System;
using System.Text;

namespace Klyte.Commons.Utils
{
    public class NumberingUtils
    {
        #region Numbering Utils
        public static string ToRomanNumeral(ushort value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException("Please use a positive integer greater than zero.");
            }

            var sb = new StringBuilder();
            if (value >= 4000)
            {
                RomanizeCore(sb, (ushort) (value / 1000));
                sb.Append("·");
                value %= 1000;
            }
            RomanizeCore(sb, value);

            return sb.ToString();
        }
        private static ushort RomanizeCore(StringBuilder sb, ushort remain)
        {
            while (remain > 0)
            {
                if (remain >= 1000)
                {
                    sb.Append("M");
                    remain -= 1000;
                }
                else if (remain >= 900)
                {
                    sb.Append("CM");
                    remain -= 900;
                }
                else if (remain >= 500)
                {
                    sb.Append("D");
                    remain -= 500;
                }
                else if (remain >= 400)
                {
                    sb.Append("CD");
                    remain -= 400;
                }
                else if (remain >= 100)
                {
                    sb.Append("C");
                    remain -= 100;
                }
                else if (remain >= 90)
                {
                    sb.Append("XC");
                    remain -= 90;
                }
                else if (remain >= 50)
                {
                    sb.Append("L");
                    remain -= 50;
                }
                else if (remain >= 40)
                {
                    sb.Append("XL");
                    remain -= 40;
                }
                else if (remain >= 10)
                {
                    sb.Append("X");
                    remain -= 10;
                }
                else
                {
                    switch (remain)
                    {
                        case 9:
                            sb.Append("IX");
                            break;
                        case 8:
                            sb.Append("VIII");
                            break;
                        case 7:
                            sb.Append("VII");
                            break;
                        case 6:
                            sb.Append("VI");
                            break;
                        case 5:
                            sb.Append("V");
                            break;
                        case 4:
                            sb.Append("IV");
                            break;
                        case 3:
                            sb.Append("III");
                            break;
                        case 2:
                            sb.Append("II");
                            break;
                        case 1:
                            sb.Append("I");
                            break;
                    }
                    remain = 0;
                }
            }

            return remain;
        }
        public static string GetStringFromNumber(string[] array, int number)
        {
            int arraySize = array.Length;
            string saida = "";
            while (number > 0)
            {
                int idx = (number - 1) % arraySize;
                saida = "" + array[idx] + saida;
                if (number % arraySize == 0)
                {
                    number /= arraySize;
                    number--;
                }
                else
                {
                    number /= arraySize;
                }

            }
            return saida;
        }
        #endregion
    }
}
