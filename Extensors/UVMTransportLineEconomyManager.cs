using ColossalFramework;
using ColossalFramework.IO;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Klyte.TransportLinesManager.Extensors
{
    public class UVMTransportLineEconomyManager : SimulationManagerBase<UVMTransportLineEconomyManager, UVMTransportLineEconomyProperties>, ISimulationManager
    {
        public const int BYTES_PER_CYCLE = 12;
        public const int FRAMES_PER_CYCLE = 1 << BYTES_PER_CYCLE;
        public const int FRAMES_PER_CYCLE_MASK = FRAMES_PER_CYCLE - 1;
        public const int TOTAL_STORAGE_CAPACITY = (1 << (BYTES_PER_CYCLE + 4));
        public const int INDEX_AND_FRAMES_MASK = TOTAL_STORAGE_CAPACITY - 1;


        protected override void Awake()
        {
            base.Awake();
            m_linesExpense = new long[17 * TransportManager.MAX_LINE_COUNT];
            m_linesIncome = new long[17 * TransportManager.MAX_LINE_COUNT];

            m_vehiclesExpense = new long[17 * VehicleManager.MAX_VEHICLE_COUNT];
            m_vehiclesIncome = new long[17 * VehicleManager.MAX_VEHICLE_COUNT];

            m_stopIncome = new long[17 * NetManager.MAX_NODE_COUNT];
        }

        public override void InitializeProperties(UVMTransportLineEconomyProperties properties) => base.InitializeProperties(properties);

        public void AddToLine(ushort lineId, long income, long expense)
        {
            m_linesIncome[(lineId * 17) + 16] += income;
            m_linesExpense[(lineId * 17) + 16] += expense;
        }

        public void AddToVehicle(ushort vehicleId, long income, long expense)
        {
            m_vehiclesIncome[(vehicleId * 17) + 16] += income;
            m_vehiclesExpense[(vehicleId * 17) + 16] += expense;
        }
        public void AddToStop(ushort stopId, long income) => m_stopIncome[(stopId * 17) + 16] += income;


        public void GetIncomeAndExpensesForLine(ushort lineId, out long income, out long expenses)
        {
            income = 0L;
            expenses = 0L;
            for (int j = 0; j <= 16; j++)
            {
                income += m_linesIncome[(lineId * 17) + j];
                expenses += m_linesExpense[(lineId * 17) + j];
            }
        }
        public void GetIncomeAndExpensesForVehicle(ushort vehicleId, out long income, out long expenses)
        {
            income = 0L;
            expenses = 0L;
            for (int j = 0; j <= 16; j++)
            {
                income += m_vehiclesIncome[(vehicleId * 17) + j];
                expenses += m_vehiclesExpense[(vehicleId * 17) + j];
            }
        }
        public void GetStopIncome(ushort stopId, out long income)
        {
            income = 0L;
            for (int j = 0; j <= 16; j++)
            {
                income += m_stopIncome[(stopId * 17) + j];
            }
        }



        public void GetCurrentIncomeAndExpensesForLine(ushort lineId, out long income, out long expenses)
        {
            income = m_linesIncome[(lineId * 17) + 16];
            expenses = m_linesExpense[(lineId * 17) + 16];
        }
        public void GetCurrentIncomeAndExpensesForVehicles(ushort vehicleId, out long income, out long expenses)
        {
            income = m_vehiclesIncome[(vehicleId * 17) + 16];
            expenses = m_vehiclesExpense[(vehicleId * 17) + 16];
        }
        public void GetCurrentStopIncome(ushort stopId, out long income) => income = m_stopIncome[(stopId * 17) + 16];

        public void GetLastWeekIncomeAndExpensesForLine(ushort lineId, out long income, out long expenses)
        {
            income = m_linesIncome[(lineId * 17) + ((CurrentArrayEntryIdx + 0xF) & 0xF)];
            expenses = m_linesExpense[(lineId * 17) + ((CurrentArrayEntryIdx + 0xF) & 0xF)];
        }
        public void GetLastWeekIncomeAndExpensesForVehicles(ushort vehicleId, out long income, out long expenses)
        {
            income = m_vehiclesIncome[(vehicleId * 17) + ((CurrentArrayEntryIdx + 0xF) & 0xF)];
            expenses = m_vehiclesExpense[(vehicleId * 17) + ((CurrentArrayEntryIdx + 0xF) & 0xF)];
        }
        public void GetLastWeekStopIncome(ushort stopId, out long income) => income = m_stopIncome[(stopId * 17) + ((CurrentArrayEntryIdx + 0xF) & 0xF)];

        public List<IncomeExpense> GetLineReport(ushort lineId)
        {
            var result = new List<IncomeExpense>();
            for (int j = 0; j < 16; j++)
            {
                result.Add(new IncomeExpense
                {
                    Income = m_linesIncome[(lineId * 17) + j],
                    Expense = m_linesExpense[(lineId * 17) + j],
                    RefFrame = GetStartFrameForArrayIdx(j)
                });

            }
            result.Add(new IncomeExpense
            {
                Income = m_linesIncome[(lineId * 17) + 16],
                Expense = m_linesExpense[(lineId * 17) + 16],
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

        private uint CurrentArrayEntryIdx => (Singleton<SimulationManager>.instance.m_currentFrameIndex >> BYTES_PER_CYCLE) & 0xF;
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
                    for (int k = 0; k < TransportManager.MAX_LINE_COUNT; k++)
                    {
                        m_linesIncome[(int) ((k * 17) + idxEnum)] = m_linesIncome[(k * 17) + 16];
                        m_linesIncome[(k * 17) + 16] = 0L;
                        m_linesExpense[(int) ((k * 17) + idxEnum)] = m_linesExpense[(k * 17) + 16];
                        m_linesExpense[(k * 17) + 16] = 0L;
                    }
                    for (int k = 0; k < NetManager.MAX_NODE_COUNT; k++)
                    {
                        m_stopIncome[(int) (checked(unchecked((k * 17) + (long) ((ulong) idxEnum))))] = m_stopIncome[(k * 17) + 16];
                        m_stopIncome[(k * 17) + 16] = 0L;
                    }
                    for (int k = 0; k < VehicleManager.MAX_VEHICLE_COUNT; k++)
                    {
                        m_vehiclesIncome[(int) ((k * 17) + (idxEnum))] = m_vehiclesIncome[(k * 17) + 16];
                        m_vehiclesIncome[(k * 17) + 16] = 0L;
                        m_vehiclesExpense[(int) ((k * 17) + idxEnum)] = m_vehiclesExpense[(k * 17) + 16];
                        m_vehiclesExpense[(k * 17) + 16] = 0L;
                    }
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
                for (int num = 0; num < m_linesIncome.Length; num++)
                {
                    m_linesIncome[num] = 0L;
                }
                for (int num = 0; num < m_linesExpense.Length; num++)
                {
                    m_linesExpense[num] = 0L;
                }
                for (int num = 0; num < m_stopIncome.Length; num++)
                {
                    m_stopIncome[num] = 0L;
                }
                for (int num = 0; num < m_vehiclesIncome.Length; num++)
                {
                    m_vehiclesIncome[num] = 0L;
                }
                for (int num = 0; num < m_vehiclesExpense.Length; num++)
                {
                    m_vehiclesExpense[num] = 0L;
                }
            }
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndLoading();
        }

        private long[] m_linesExpense;

        private long[] m_linesIncome;
        private long[] m_vehiclesExpense;

        private long[] m_vehiclesIncome;

        private long[] m_stopIncome;



        public class Data : IDataContainer
        {
            public void Serialize(DataSerializer s)
            {
                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginSerialize(s, "UVMTransportLineEconomyManager");
                UVMTransportLineEconomyManager instance = Singleton<UVMTransportLineEconomyManager>.instance;
                int leSize = instance.m_linesExpense.Length;
                int liSize = instance.m_linesIncome.Length;
                int veSize = instance.m_vehiclesExpense.Length;
                int viSize = instance.m_vehiclesIncome.Length;
                int siSize = instance.m_stopIncome.Length;
                Version assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                if (assemblyVersion.CompareTo(new Version("11.2")) >= 0)
                {
                }

                var longArraysSerial = EncodedArray.Long.BeginWrite(s);
                for (int i = 0; i < leSize; i++)
                {
                    longArraysSerial.Write(instance.m_linesExpense[i]);
                }
                for (int i = 0; i < liSize; i++)
                {
                    longArraysSerial.Write(instance.m_linesIncome[i]);
                }
                for (int i = 0; i < siSize; i++)
                {
                    longArraysSerial.Write(instance.m_stopIncome[i]);
                }
                for (int i = 0; i < veSize; i++)
                {
                    longArraysSerial.Write(instance.m_vehiclesExpense[i]);
                }
                for (int i = 0; i < viSize; i++)
                {
                    longArraysSerial.Write(instance.m_vehiclesIncome[i]);
                }
                longArraysSerial.EndWrite();

                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndSerialize(s, "UVMTransportLineEconomyManager");
            }

            public void Deserialize(DataSerializer s)
            {
                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginDeserialize(s, "UVMTransportLineEconomyManager");
                UVMTransportLineEconomyManager instance = Singleton<UVMTransportLineEconomyManager>.instance;
                int leSize = instance.m_linesExpense.Length;
                int liSize = instance.m_linesIncome.Length;
                int veSize = instance.m_vehiclesExpense.Length;
                int viSize = instance.m_vehiclesIncome.Length;
                int siSize = instance.m_stopIncome.Length;
                var long2 = EncodedArray.Long.BeginRead(s);
                for (int num17 = 0; num17 < leSize; num17++)
                {
                    instance.m_linesExpense[num17] = long2.Read();
                }
                for (int num18 = 0; num18 < liSize; num18++)
                {
                    instance.m_linesIncome[num18] = long2.Read();
                }
                for (int num21 = 0; num21 < siSize; num21++)
                {
                    instance.m_stopIncome[num21] = long2.Read();
                }
                for (int num19 = 0; num19 < veSize; num19++)
                {
                    instance.m_vehiclesExpense[num19] = long2.Read();
                }
                for (int num20 = 0; num20 < viSize; num20++)
                {
                    instance.m_vehiclesIncome[num20] = long2.Read();
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