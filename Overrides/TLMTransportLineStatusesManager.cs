using ColossalFramework;
using ColossalFramework.IO;
using Klyte.Commons.Extensors;
using Klyte.Commons.Interfaces;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Klyte.TransportLinesManager.Overrides
{
    public class TLMTransportLineStatusesManager : Redirector, IRedirectable
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

        public static TLMTransportLineStatusesManager Instance { get; private set; }
        public Redirector RedirectorInstance => this;

        public void Awake()
        {
            AddRedirect(typeof(StatisticsManager).GetMethod("SimulationStepImpl", RedirectorUtils.allFlags), null, GetType().GetMethod("SimulationStepImpl", RedirectorUtils.allFlags));
            AddRedirect(typeof(StatisticsManager).GetMethod("UpdateData", RedirectorUtils.allFlags), null, GetType().GetMethod("UpdateData", RedirectorUtils.allFlags));

            Instance = this;
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

        public void AddToLine(ushort lineId, long income, long expense, ref Citizen citizenData) => IncrementInArray(lineId, ref m_linesData, (int) LineData.INCOME, (int) LineData.EXPENSE, (int) LineData.TOTAL_PASSENGERS, (int) LineData.TOURIST_PASSENGERS, (int) LineData.STUDENT_PASSENGERS, income, expense, ref citizenData);

        public void AddToVehicle(ushort vehicleId, long income, long expense, ref Citizen citizenData) => IncrementInArray(vehicleId, ref m_vehiclesData, (int) VehicleData.INCOME, (int) VehicleData.EXPENSE, (int) VehicleData.TOTAL_PASSENGERS, (int) VehicleData.TOURIST_PASSENGERS, (int) VehicleData.STUDENT_PASSENGERS, income, expense, ref citizenData);
        public void AddToStop(ushort stopId, long income, ref Citizen citizenData) => IncrementInArray(stopId, ref m_stopData, (int) StopData.INCOME, null, (int) StopData.TOTAL_PASSENGERS, (int) StopData.TOURIST_PASSENGERS, (int) StopData.STUDENT_PASSENGERS, income, 0, ref citizenData);


        private void IncrementInArray(ushort id, ref long[][] arrayRef, int incomeIdx, int? expenseIdx, int totalPassIdx, int tourPassIdx, int studPassIdx, long income, long expense, ref Citizen citizenData)
        {
            arrayRef[(id * CYCLES_HISTORY_ARRAY_SIZE) + CYCLES_CURRENT_DATA_IDX][incomeIdx] += income;
            if (expenseIdx is int idx)
            {
                arrayRef[(id * CYCLES_HISTORY_ARRAY_SIZE) + CYCLES_CURRENT_DATA_IDX][idx] += expense;
            }
            if (!citizenData.Equals(default))
            {
                arrayRef[(id * CYCLES_HISTORY_ARRAY_SIZE) + CYCLES_CURRENT_DATA_IDX][totalPassIdx]++;
                if ((citizenData.m_flags & Citizen.Flags.Tourist) != 0)
                {
                    arrayRef[(id * CYCLES_HISTORY_ARRAY_SIZE) + CYCLES_CURRENT_DATA_IDX][tourPassIdx]++;
                }

                if ((citizenData.m_flags & Citizen.Flags.Student) != 0)
                {
                    arrayRef[(id * CYCLES_HISTORY_ARRAY_SIZE) + CYCLES_CURRENT_DATA_IDX][studPassIdx]++;
                }
            }
        }

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

        public static void SimulationStepImpl(int subStep)
        {
            if (subStep != 0 && subStep != 1000)
            {
                uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
                uint frameCounterCycle = currentFrameIndex & FRAMES_PER_CYCLE_MASK;
                if (frameCounterCycle == FRAMES_PER_CYCLE_MASK)
                {
                    uint idxEnum = (currentFrameIndex >> BYTES_PER_CYCLE) & 15u;
                    LogUtils.DoLog($"Stroring data for frame {(currentFrameIndex & ~FRAMES_PER_CYCLE_MASK).ToString("X8")} into idx {idxEnum.ToString("X1")}");

                    FinishCycle(idxEnum, ref Instance.m_linesData, TransportManager.MAX_LINE_COUNT);
                    FinishCycle(idxEnum, ref Instance.m_vehiclesData, VehicleManager.MAX_VEHICLE_COUNT);
                    FinishCycle(idxEnum, ref Instance.m_stopData, NetManager.MAX_NODE_COUNT);
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


        public static void UpdateData(SimulationManager.UpdateMode mode)
        {
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginLoading("UVMTransportLineEconomyManager.UpdateData");
            if (mode == SimulationManager.UpdateMode.NewMap || mode == SimulationManager.UpdateMode.NewGameFromMap || mode == SimulationManager.UpdateMode.NewScenarioFromMap || mode == SimulationManager.UpdateMode.UpdateScenarioFromMap || mode == SimulationManager.UpdateMode.NewAsset)
            {
                ClearArray(ref Instance.m_linesData);
                ClearArray(ref Instance.m_vehiclesData);
                ClearArray(ref Instance.m_stopData);
            }
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndLoading();
        }

        private long[][] m_linesData;
        private long[][] m_vehiclesData;
        private long[][] m_stopData;

        private enum LineData
        {
            EXPENSE,
            INCOME,
            TOTAL_PASSENGERS,
            TOURIST_PASSENGERS,
            STUDENT_PASSENGERS
        }
        private enum VehicleData
        {
            EXPENSE,
            INCOME,
            TOTAL_PASSENGERS,
            TOURIST_PASSENGERS,
            STUDENT_PASSENGERS
        }
        private enum StopData
        {
            INCOME,
            TOTAL_PASSENGERS,
            TOURIST_PASSENGERS,
            STUDENT_PASSENGERS
        }



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
                        case LineData.INCOME:
                            return 0;
                        case LineData.TOTAL_PASSENGERS:
                        case LineData.TOURIST_PASSENGERS:
                        case LineData.STUDENT_PASSENGERS:
                            return 1;
                    }
                    break;
                case VehicleData v:
                    switch (v)
                    {
                        case VehicleData.EXPENSE:
                        case VehicleData.INCOME:
                            return 0;
                        case VehicleData.TOTAL_PASSENGERS:
                        case VehicleData.TOURIST_PASSENGERS:
                        case VehicleData.STUDENT_PASSENGERS:
                            return 1;
                    }
                    break;
                case StopData s:
                    switch (s)
                    {
                        case StopData.INCOME:
                            return 0;
                        case StopData.TOTAL_PASSENGERS:
                        case StopData.TOURIST_PASSENGERS:
                        case StopData.STUDENT_PASSENGERS:
                            return 1;
                    }
                    break;
            }
            return 99999999;
        }


        public const long CURRENT_VERSION = 1;

        public class Data : IDataContainer
        {
            public void Serialize(DataSerializer s)
            {
            }
            public void Deserialize(DataSerializer s)
            {
            }
            public void AfterDeserialize(DataSerializer s)
            {
            }
        }

        public abstract class TransportLineStorageBasicData : IDataExtensor
        {
            public abstract string SaveId { get; }

            protected abstract Enum[] LoadOrder { get; }

            public IDataExtensor Deserialize(Type type, byte[] data)
            {
                using var s = new MemoryStream(data);
                long version = ReadLong(s);
                foreach (Enum e in LoadOrder)
                {
                    if (version >= GetMinVersion(e))
                    {

                        Instance.DoWithArray(e, (ref long[][] arrayRef) =>
                        {
                            int idx = Instance.GetIdxFor(e);

                            for (int i = 0; i < arrayRef.Length; i++)
                            {
                                arrayRef[i][idx] = DeserializeFunction(s);
                            }
                        });
                    }
                }
                return this;
            }

            public byte[] Serialize()
            {
                using var s = new MemoryStream();

                WriteLong(s, CURRENT_VERSION);

                TLMTransportLineStatusesManager instance = Singleton<TLMTransportLineStatusesManager>.instance;

                foreach (Enum e in LoadOrder)
                {
                    instance.DoWithArray(e, (ref long[][] arrayRef) =>
                    {
                        int idx = instance.GetIdxFor(e);
                        for (int i = 0; i < arrayRef.Length; i++)
                        {
                            SerializeFunction(s, arrayRef[i][idx]);
                        }
                        LogUtils.DoErrorLog($"size: {s.Length} ({e.GetType()} {e})");
                    });

                }
                return s.ToArray();
            }
            protected static void WriteLong(Stream s, long value)
            {
                s.WriteByte((byte) ((value >> 56) & 255L));
                s.WriteByte((byte) ((value >> 48) & 255L));
                s.WriteByte((byte) ((value >> 40) & 255L));
                s.WriteByte((byte) ((value >> 32) & 255L));
                s.WriteByte((byte) ((value >> 24) & 255L));
                s.WriteByte((byte) ((value >> 16) & 255L));
                s.WriteByte((byte) ((value >> 8) & 255L));
                s.WriteByte((byte) (value & 255L));
            }

            protected static long ReadLong(Stream s)
            {
                long num = (long) (s.ReadByte() & 255) << 56;
                num |= (long) (s.ReadByte() & 255) << 48;
                num |= (long) (s.ReadByte() & 255) << 40;
                num |= (long) (s.ReadByte() & 255) << 32;
                num |= (long) (s.ReadByte() & 255) << 24;
                num |= (long) (s.ReadByte() & 255) << 16;
                num |= (long) (s.ReadByte() & 255) << 8;
                return num | (s.ReadByte() & 255 & 255L);
            }
            protected static void WriteInt32(Stream s, long value)
            {
                s.WriteByte((byte) ((value >> 24) & 255L));
                s.WriteByte((byte) ((value >> 16) & 255L));
                s.WriteByte((byte) ((value >> 8) & 255L));
                s.WriteByte((byte) (value & 255L));
            }

            protected static long ReadInt32(Stream s)
            {
                long num = (long) (s.ReadByte() & 255) << 24;
                num |= (long) (s.ReadByte() & 255) << 16;
                num |= (long) (s.ReadByte() & 255) << 8;
                return num | (s.ReadByte() & 255 & 255L);
            }
            protected static void WriteInt24(Stream s, long value)
            {
                s.WriteByte((byte) ((value >> 16) & 255L));
                s.WriteByte((byte) ((value >> 8) & 255L));
                s.WriteByte((byte) (value & 255L));
            }

            protected static long ReadInt24(Stream s)
            {
                long num = (long) (s.ReadByte() & 255) << 16;
                num |= (long) (s.ReadByte() & 255) << 8;
                return num | (s.ReadByte() & 255 & 255L);
            }

            public bool CanDeserialize(Type type, byte[] data)
            {
                using var s = new MemoryStream();
                SerializeFunction(s, 0);
                long bytesPerEntry = s.Length;

                long count = 0;
                foreach (Enum e in LoadOrder)
                {
                    Instance.DoWithArray(e, (ref long[][] arrayRef) =>
                    {
                        count += arrayRef.Select(x => x.Length).Sum();
                    });
                }
                return count * bytesPerEntry == data.Length;
            }


            protected virtual Action<Stream, long> SerializeFunction { get; } = WriteLong;
            protected virtual Func<Stream, long> DeserializeFunction { get; } = ReadLong;

        }
        public class TLMTransportLineStorageEconomyData : TransportLineStorageBasicData
        {
            public override string SaveId => "K45_TLM_TLMTransportLineStorageEconomyData";

            protected override Enum[] LoadOrder { get; } = new Enum[]
                                                            {
                                                                LineData.EXPENSE,
                                                                LineData.INCOME,
                                                                StopData.INCOME,
                                                                VehicleData.EXPENSE,
                                                                VehicleData.INCOME,
                                                            };
        }
        public class TLMTransportLineStoragePassengerData : TransportLineStorageBasicData
        {
            public override string SaveId => "K45_TLM_TLMTransportLineStoragePassengerData";

            protected override Enum[] LoadOrder { get; } = new Enum[]
                                                            {
                                                                 VehicleData.TOTAL_PASSENGERS,
                                                                 VehicleData.TOURIST_PASSENGERS,
                                                                 VehicleData.STUDENT_PASSENGERS,
                                                                 StopData.TOTAL_PASSENGERS,
                                                                 StopData.TOURIST_PASSENGERS,
                                                                 StopData.STUDENT_PASSENGERS,
                                                                 LineData.TOTAL_PASSENGERS,
                                                                 LineData.TOURIST_PASSENGERS,
                                                                 LineData.STUDENT_PASSENGERS
                                                            };
            protected override Action<Stream, long> SerializeFunction { get; } = WriteInt24;
            protected override Func<Stream, long> DeserializeFunction { get; } = ReadInt24;
        }
    }
}