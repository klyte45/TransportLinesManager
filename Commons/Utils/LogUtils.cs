using System;
using System.Linq;
using UnityEngine;

namespace Klyte.Commons.Utils
{
    public class LogUtils
    {
        #region Log Utils

        public static void DoLog(string format, params object[] args)
        {
            try
            {
                if (CommonProperties.DebugMode)
                {
                    Console.WriteLine($"{CommonProperties.Acronym}v" + CommonProperties.Version + " " + format, args);
                }

            }
            catch
            {
                Debug.LogErrorFormat($"{CommonProperties.Acronym}: Erro ao fazer log: {{0}} (args = {{1}})", format, args == null ? "[]" : string.Join(",", args.Select(x => x != null ? x.ToString() : "--NULL--").ToArray()));
            }
        }
        public static void DoErrorLog(string format, params object[] args)
        {
            try
            {
                Console.WriteLine($"{CommonProperties.Acronym}v" + CommonProperties.Version + " " + format, args);
            }
            catch
            {
                Debug.LogErrorFormat($"{CommonProperties.Acronym}: Erro ao fazer log: {{0}} (args = {{1}})", format, args == null ? "[]" : string.Join(",", args.Select(x => x != null ? x.ToString() : "--NULL--").ToArray()));
            }
        }
        #endregion
    }
}
