using ColossalFramework;
using System;

namespace Klyte.Commons.Utils
{
    public static class PathUnitExtensions
    {
        public static bool GetLast2Positions(this PathUnit unit, out PathUnit.Position position1, out PathUnit.Position position2)
        {
            uint num = unit.m_nextPathUnit;
            if (num != 0u)
            {
                PathManager instance = Singleton<PathManager>.instance;
                int num2 = 0;
                uint nextPathUnit = instance.m_pathUnits.m_buffer[(int) (num)].m_nextPathUnit;
                while (nextPathUnit != 0u)
                {
                    num = nextPathUnit;
                    nextPathUnit = instance.m_pathUnits.m_buffer[(int) (num)].m_nextPathUnit;
                    if (++num2 >= 262144)
                    {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                        position1 = default;
                        position2 = default;
                        return false;
                    }
                }
                if (instance.m_pathUnits.m_buffer[(int) num].m_positionCount == 1)
                {
                    return unit.GetPosition(unit.m_positionCount - 1, out position1) & instance.m_pathUnits.m_buffer[(int) num].GetLastPosition(out position2);
                }
                return instance.m_pathUnits.m_buffer[(int) num].GetLast2Positions(out position1, out position2);
            }
            return unit.GetPosition(unit.m_positionCount - 2, out position1) & unit.GetPosition(unit.m_positionCount - 1, out position2);
        }
    }
}
