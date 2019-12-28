using ColossalFramework;
using ColossalFramework.IO;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;

namespace Klyte.TransportLinesManager.Extensors
{
    public class UVMTransportLineEconomyManager : SimulationManagerBase<UVMTransportLineEconomyManager, UVMTransportLineEconomyProperties>, ISimulationManager
    {
        public const int BYTES_PER_CYCLE = 12;
        public const int FRAMES_PER_CYCLE = 1 << BYTES_PER_CYCLE;
        public const int FRAMES_PER_CYCLE_MASK = FRAMES_PER_CYCLE - 1;
        public const int TOTAL_STORAGE_CAPACITY = (1 << (BYTES_PER_CYCLE + 4));
        public const int INDEX_AND_FRAMES_MASK = TOTAL_STORAGE_CAPACITY - 1;
        public const int CYCLES_HISTORY_SIZE = 16;
        public const int CYCLES_HISTORY_MASK = CYCLES_HISTORY_SIZE - 1;
        public const int CYCLES_HISTORY_ARRAY_SIZE = CYCLES_HISTORY_SIZE + 1;
        public const int CYCLES_CURRENT_DATA_IDX = CYCLES_HISTORY_SIZE;


        protected override void Awake()
        {
            base.Awake();
            m_linesData = new long[CYCLES_HISTORY_ARRAY_SIZE * TransportManager.MAX_LINE_COUNT][];
            for (int k = 0; k < m_linesData.Length; k++)
            {
                m_linesData[k] = new long[Enum.GetValues(typeof(LineData)).Length];
            }


            m_vehiclesData = new long[CYCLES_HISTORY_ARRAY_SIZE * VehicleManager.MAX_VEHICLE_COUNT][];
            for (int k = 0; k < m_vehiclesData.Length; k++)
            {
                m_vehiclesData[k] = new long[Enum.GetValues(typeof(VehicleData)).Length];
            }

            m_stopData = new long[CYCLES_HISTORY_ARRAY_SIZE * NetManager.MAX_NODE_COUNT][];
            for (int k = 0; k < m_stopData.Length; k++)
            {
                m_stopData[k] = new long[Enum.GetValues(typeof(StopData)).Length];
            }
        }

        public override void InitializeProperties(UVMTransportLineEconomyProperties properties) => base.InitializeProperties(properties);

        public void AddToLine(ushort lineId, long income, long expense)
        {
            m_linesData[(lineId * CYCLES_HISTORY_ARRAY_SIZE) + CYCLES_CURRENT_DATA_IDX][(int) LineData.INCOME] += income;
            m_linesData[(lineId * CYCLES_HISTORY_ARRAY_SIZE) + CYCLES_CURRENT_DATA_IDX][(int) LineData.EXPENSE] += expense;
        }

        public void AddToVehicle(ushort vehicleId, long income, long expense)
        {
            m_vehiclesData[(vehicleId * CYCLES_HISTORY_ARRAY_SIZE) + CYCLES_CURRENT_DATA_IDX][(int) VehicleData.INCOME] += income;
            m_vehiclesData[(vehicleId * CYCLES_HISTORY_ARRAY_SIZE) + CYCLES_CURRENT_DATA_IDX][(int) VehicleData.EXPENSE] += expense;
        }
        public void AddToStop(ushort stopId, long income) => m_stopData[(stopId * CYCLES_HISTORY_ARRAY_SIZE) + CYCLES_CURRENT_DATA_IDX][(int) StopData.INCOME] += income;


        public void GetIncomeAndExpensesForLine(ushort lineId, out long income, out long expenses) => GetGenericIncomeExpense(lineId, out income, out expenses, ref m_linesData, (int) LineData.INCOME, (int) LineData.EXPENSE);

        private void GetGenericIncomeExpense(ushort id, out long income, out long expenses, ref long[][] arrayData, int incomeEntry, int expenseEntry)
        {
            income = 0L;
            expenses = 0L;
            for (int j = 0; j <= 16; j++)
            {
                income += GetAtArray(id, ref arrayData, incomeEntry, j);
                expenses += GetAtArray(id, ref arrayData, expenseEntry, j);
            }
        }

        private static long GetAtArray(ushort id, ref long[][] arrayData, int entryIdx, int dataIdx) => arrayData[(id * 17) + dataIdx][entryIdx];

        private void GetGenericIncome(ushort id, out long income, ref long[][] arrayData, int incomeEntry)
        {
            income = 0L;
            for (int j = 0; j <= 16; j++)
            {
                income += arrayData[(id * 17) + j][incomeEntry];
            }
        }

        public void GetIncomeAndExpensesForVehicle(ushort vehicleId, out long income, out long expenses) => GetGenericIncomeExpense(vehicleId, out income, out expenses, ref m_vehiclesData, (int) VehicleData.INCOME, (int) VehicleData.EXPENSE);
        public void GetStopIncome(ushort stopId, out long income) => GetGenericIncome(stopId, out income, ref m_stopData, (int) StopData.INCOME);

        public void GetCurrentIncomeAndExpensesForLine(ushort lineId, out long income, out long expenses)
        {
            income = GetAtArray(lineId, ref m_linesData, (int) LineData.INCOME, CYCLES_CURRENT_DATA_IDX);
            expenses = GetAtArray(lineId, ref m_linesData, (int) LineData.EXPENSE, CYCLES_CURRENT_DATA_IDX);
        }
        public void GetCurrentIncomeAndExpensesForVehicles(ushort vehicleId, out long income, out long expenses)
        {
            income = GetAtArray(vehicleId, ref m_vehiclesData, (int) VehicleData.INCOME, CYCLES_CURRENT_DATA_IDX);
            expenses = GetAtArray(vehicleId, ref m_vehiclesData, (int) VehicleData.EXPENSE, CYCLES_CURRENT_DATA_IDX);
        }
        public void GetCurrentStopIncome(ushort stopId, out long income) => income = GetAtArray(stopId, ref m_stopData, (int) StopData.INCOME, CYCLES_CURRENT_DATA_IDX);

        public void GetLastWeekIncomeAndExpensesForLine(ushort lineId, out long income, out long expenses)
        {
            int lastIdx = ((int) CurrentArrayEntryIdx + CYCLES_HISTORY_SIZE - 1) & CYCLES_HISTORY_MASK;
            income = GetAtArray(lineId, ref m_linesData, (int) LineData.INCOME, lastIdx);
            expenses = GetAtArray(lineId, ref m_linesData, (int) LineData.EXPENSE, lastIdx);
        }
        public void GetLastWeekIncomeAndExpensesForVehicles(ushort vehicleId, out long income, out long expenses)
        {
            int lastIdx = ((int) CurrentArrayEntryIdx + CYCLES_HISTORY_SIZE - 1) & CYCLES_HISTORY_MASK;
            income = GetAtArray(vehicleId, ref m_vehiclesData, (int) VehicleData.INCOME, lastIdx);
            expenses = GetAtArray(vehicleId, ref m_vehiclesData, (int) VehicleData.EXPENSE, lastIdx);
        }
        public void GetLastWeekStopIncome(ushort stopId, out long income)
        {
            int lastIdx = ((int) CurrentArrayEntryIdx + CYCLES_HISTORY_SIZE - 1) & CYCLES_HISTORY_MASK;
            income = GetAtArray(stopId, ref m_stopData, (int) StopData.INCOME, lastIdx);
        }

        public List<IncomeExpense> GetLineReport(ushort lineId)
        {
            var result = new List<IncomeExpense>();
            for (int j = 0; j < 16; j++)
            {
                result.Add(new IncomeExpense
                {
                    Income = GetAtArray(lineId, ref m_linesData, (int) LineData.INCOME, j),
                    Expense = GetAtArray(lineId, ref m_linesData, (int) LineData.EXPENSE, j),
                    RefFrame = GetStartFrameForArrayIdx(j)
                });

            }
            result.Add(new IncomeExpense
            {
                Income = GetAtArray(lineId, ref m_linesData, (int) LineData.INCOME, CYCLES_CURRENT_DATA_IDX),
                Expense = GetAtArray(lineId, ref m_linesData, (int) LineData.EXPENSE, CYCLES_CURRENT_DATA_IDX),
                RefFrame = Singleton<SimulationManager>.instance.m_currentFrameIndex & ~FRAMES_PER_CYCLE_MASK
            });
            result.Sort((a, b) => a.RefFrame.CompareTo(b.RefFrame));
            return result;
        }
        public struct IncomeExpense
        {
            public long RefFrame { get; set; }
            public long Income { get; set; }
            public long Expense { get; set; }

            public DateTime StartDate => SimulationManager.instance.FrameToTime((uint) RefFrame);
            public DateTime EndDate => SimulationManager.instance.FrameToTime((uint) RefFrame + FRAMES_PER_CYCLE_MASK);
            public float StartDayTime => FrameToDaytime(RefFrame);
            public float EndDayTime => FrameToDaytime(RefFrame + FRAMES_PER_CYCLE_MASK);

            private static float FrameToDaytime(long refFrame)
            {
                float num = (refFrame + SimulationManager.instance.m_dayTimeOffsetFrames) & (SimulationManager.DAYTIME_FRAMES - 1u);
                num *= SimulationManager.DAYTIME_FRAME_TO_HOUR;
                if (num >= 24f)
                {
                    num -= 24f;
                }
                return num;
            }
        }

        private uint CurrentArrayEntryIdx => (Singleton<SimulationManager>.instance.m_currentFrameIndex >> BYTES_PER_CYCLE) & CYCLES_HISTORY_MASK;
        private long GetStartFrameForArrayIdx(int idx) => (Singleton<SimulationManager>.instance.m_currentFrameIndex & ~INDEX_AND_FRAMES_MASK) + (idx << BYTES_PER_CYCLE) - (idx >= CurrentArrayEntryIdx ? TOTAL_STORAGE_CAPACITY : 0);

        protected override void SimulationStepImpl(int subStep)
        {
            if (subStep != 0 && subStep != 1000)
            {
                uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
                uint frameCounterCycle = currentFrameIndex & FRAMES_PER_CYCLE_MASK;
                if (frameCounterCycle == FRAMES_PER_CYCLE_MASK)
                {
                    uint idxEnum = (currentFrameIndex >> BYTES_PER_CYCLE) & 15u;
                    LogUtils.DoLog($"Stroring data for frame {(currentFrameIndex & ~FRAMES_PER_CYCLE_MASK).ToString("X8")} into idx {idxEnum.ToString("X1")}");

                    FinishCycle(idxEnum, ref m_linesData, TransportManager.MAX_LINE_COUNT);
                    FinishCycle(idxEnum, ref m_vehiclesData, VehicleManager.MAX_VEHICLE_COUNT);
                    FinishCycle(idxEnum, ref m_stopData, NetManager.MAX_NODE_COUNT);
                }
            }
        }

        private static void FinishCycle(uint idxEnum, ref long[][] arrayRef, int loopSize)
        {
            for (int k = 0; k < loopSize; k++)
            {
                int kIdx = (k * CYCLES_HISTORY_ARRAY_SIZE);
                for (int l = 0; l < arrayRef[kIdx].Length; l++)
                {
                    arrayRef[kIdx + idxEnum][l] = arrayRef[kIdx + CYCLES_CURRENT_DATA_IDX][l];
                    arrayRef[kIdx + CYCLES_CURRENT_DATA_IDX][l] = 0;
                }
            }
        }

        private static void ClearArray(ref long[][] arrayRef)
        {
            for (int k = 0; k < arrayRef.Length; k++)
            {
                for (int l = 0; l < arrayRef[k].Length; l++)
                {
                    arrayRef[k][l] = 0;
                }
            }
        }

        public override void GetData(FastList<IDataContainer> data)
        {
            base.GetData(data);
            data.Add(new Data());
        }

        public override void UpdateData(SimulationManager.UpdateMode mode)
        {
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginLoading("UVMTransportLineEconomyManager.UpdateData");
            base.UpdateData(mode);
            if (mode == SimulationManager.UpdateMode.NewMap || mode == SimulationManager.UpdateMode.NewGameFromMap || mode == SimulationManager.UpdateMode.NewScenarioFromMap || mode == SimulationManager.UpdateMode.UpdateScenarioFromMap || mode == SimulationManager.UpdateMode.NewAsset)
            {
                ClearArray(ref m_linesData);
                ClearArray(ref m_vehiclesData);
                ClearArray(ref m_stopData);
            }
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndLoading();
        }

        private long[][] m_linesData;
        private long[][] m_vehiclesData;
        private long[][] m_stopData;

        private enum LineData
        {
            EXPENSE = 0,
            INCOME = 1
        }
        private enum VehicleData
        {
            EXPENSE = 0,
            INCOME = 1
        }
        private enum StopData
        {
            INCOME = 0
        }

        private static readonly Enum[] m_loadOrder = new Enum[]
        {
            LineData.EXPENSE,
            LineData.INCOME,
            StopData.INCOME,
            VehicleData.EXPENSE,
            VehicleData.INCOME,
        };

        private void DoWithArray(Enum e, DoWithArrayRef action)
        {
            switch (e)
            {
                case LineData _:
                    action(ref m_linesData);
                    break;
                case VehicleData _:
                    action(ref m_vehiclesData);
                    break;
                case StopData _:
                    action(ref m_stopData);
                    break;
            }
        }

        private delegate void DoWithArrayRef(ref long[][] arrayRef);

        private int GetIdxFor(Enum e)
        {
            switch (e)
            {
                case LineData l:
                    return (int) l;
                case VehicleData l:
                    return (int) l;
                case StopData l:
                    return (int) l;
                default:
                    e.GetType();
                    throw new Exception("Invalid data in array deserialize!");
            }
        }

        private static int GetMinVersion(Enum e)
        {
            switch (e)
            {
                case LineData l:
                    switch (l)
                    {
                        case LineData.EXPENSE:
                            return 0;
                        case LineData.INCOME:
                            return 0;
                    }
                    break;
                case VehicleData v:
                    switch (v)
                    {
                        case VehicleData.EXPENSE:
                            return 0;
                        case VehicleData.INCOME:
                            return 0;
                    }
                    break;
                case StopData s:
                    switch (s)
                    {
                        case StopData.INCOME:
                            return 0;
                    }
                    break;
            }
            return 99999999;
        }


        public class Data : IDataContainer
        {
            public const int CURRENT_VERSION = 1;
            public void Serialize(DataSerializer s)
            {
                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginSerialize(s, "UVMTransportLineEconomyManager");
                UVMTransportLineEconomyManager instance = Singleton<UVMTransportLineEconomyManager>.instance;

                var longArraysSerial = EncodedArray.Long.BeginWrite(s);
                foreach (Enum e in m_loadOrder)
                {
                    instance.DoWithArray(e, (ref long[][] arrayRef) =>
                     {
                         int idx = instance.GetIdxFor(e);
                         for (int i = 0; i < arrayRef.Length; i++)
                         {
                             longArraysSerial.Write(arrayRef[i][idx]);
                         }
                     });

                }
                longArraysSerial.EndWrite();
                s.version = CURRENT_VERSION;
                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndSerialize(s, "UVMTransportLineEconomyManager");
            }

            public void Deserialize(DataSerializer s)
            {
                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginDeserialize(s, "UVMTransportLineEconomyManager");
                UVMTransportLineEconomyManager instance = Singleton<UVMTransportLineEconomyManager>.instance;
                var long2 = EncodedArray.Long.BeginRead(s);
                //LogUtils.DoErrorLog($"Deserialize  long2.Read(): { long2.Read()}");
                int totalCount = 0;
                foreach (Enum e in m_loadOrder)
                {
                    if (s.version >= GetMinVersion(e))
                    {

                        instance.DoWithArray(e, (ref long[][] arrayRef) =>
                        {
                            int idx = instance.GetIdxFor(e);

                            for (int i = 0; i < arrayRef.Length; i++)
                            {
                                arrayRef[i][idx] = long2.Read();
                                totalCount++;
                            }
                        });
                    }
                }
                long2.EndRead();

                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndDeserialize(s, "UVMTransportLineEconomyManager");
            }
            public void AfterDeserialize(DataSerializer s)
            {
                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginAfterDeserialize(s, "UVMTransportLineEconomyManager");
                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndAfterDeserialize(s, "UVMTransportLineEconomyManager");
            }
        }
    }
}