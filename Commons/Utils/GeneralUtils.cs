using ColossalFramework.Globalization;
using Klyte.Commons.i18n;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Klyte.Commons.Utils
{
    public class GeneralUtils
    {
        public static void DoLocaleDump()
        {
            string localeDump = "LOCALE DUMP:\r\n";
            try
            {
                var locale = KlyteLocaleManager.m_localeStringsDictionary(KlyteLocaleManager.m_localeManagerLocale(LocaleManager.instance));
                foreach (Locale.Key k in locale.Keys)
                {
                    localeDump += string.Format("{0}  =>  {1}\n", k.ToString(), locale[k]);
                }
            }
            catch (Exception e)
            {

                LogUtils.DoErrorLog("LOCALE DUMP FAIL: {0}", e.ToString());
            }
            Debug.LogWarning(localeDump);
        }

        public static bool FindSimetry(int[] array, out int middle)
        {
            middle = -1;
            int size = array.Length;
            if (size == 0)
                return false;
            for (int j = -1; j < size / 2; j++)
            {
                int offsetL = (j + size) % size;
                int offsetH = (j + 2) % size;
                if (array[offsetL] == array[offsetH])
                {
                    middle = j + 1;
                    break;
                }
            }
            //			LogUtils.DoLog("middle="+middle);
            if (middle >= 0)
            {
                for (int k = 1; k <= size / 2; k++)
                {
                    int offsetL = (-k + middle + size) % size;
                    int offsetH = (k + middle) % size;
                    if (array[offsetL] != array[offsetH])
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}
