using ColossalFramework;
using ColossalFramework.IO;
using System;
using System.Reflection;

namespace Klyte.TransportLinesManager.Extensors
{
    public class UVMTransportLineEconomyManager : SimulationManagerBase<UVMTransportLineEconomyManager, UVMTransportLineEconomyProperties>, ISimulationManager
    // Token: 0x02000349 RID: 841
    {


        // Token: 0x0600292E RID: 10542 RVA: 0x001B61B4 File Offset: 0x001B45B4
        protected override void Awake()
        {
            base.Awake();
            m_linesExpense = new long[17 * TransportManager.MAX_LINE_COUNT];
            m_linesIncome = new long[17 * TransportManager.MAX_LINE_COUNT];

            m_vehiclesExpense = new long[17 * VehicleManager.MAX_VEHICLE_COUNT];
            m_vehiclesIncome = new long[17 * VehicleManager.MAX_VEHICLE_COUNT];

            m_stopIncome = new long[17 * NetManager.MAX_NODE_COUNT];
        }

        // Token: 0x06002932 RID: 10546 RVA: 0x001B62BA File Offset: 0x001B46BA
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


        // Token: 0x06002954 RID: 10580 RVA: 0x001B763C File Offset: 0x001B5A3C
        protected override void SimulationStepImpl(int subStep)
        {
            if (subStep != 0 && subStep != 1000)
            {
                uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
                uint idxEnum = (currentFrameIndex >> 8) & 15u;
                uint frameCounterCycle = currentFrameIndex & 255u;
                if (frameCounterCycle == 255u)
                {
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

        // Token: 0x06002955 RID: 10581 RVA: 0x001B7DA7 File Offset: 0x001B61A7
        public override void GetData(FastList<IDataContainer> data)
        {
            base.GetData(data);
            data.Add(new Data());
        }

        // Token: 0x06002956 RID: 10582 RVA: 0x001B7DBC File Offset: 0x001B61BC
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

        // Token: 0x04002185 RID: 8581
        private long[] m_linesExpense;

        // Token: 0x04002187 RID: 8583
        private long[] m_linesIncome;
        // Token: 0x04002185 RID: 8581
        private long[] m_vehiclesExpense;

        // Token: 0x04002187 RID: 8583
        private long[] m_vehiclesIncome;

        // Token: 0x04002187 RID: 8583
        private long[] m_stopIncome;



        public class Data : IDataContainer
        {
            // Token: 0x06002965 RID: 10597 RVA: 0x001B81EC File Offset: 0x001B65EC
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

            // Token: 0x06002966 RID: 10598 RVA: 0x001B8568 File Offset: 0x001B6968
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

            // Token: 0x06002967 RID: 10599 RVA: 0x001B8F3B File Offset: 0x001B733B
            public void AfterDeserialize(DataSerializer s)
            {
                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginAfterDeserialize(s, "UVMTransportLineEconomyManager");
                Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndAfterDeserialize(s, "UVMTransportLineEconomyManager");
            }
        }
    }
}