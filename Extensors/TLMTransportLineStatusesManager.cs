using ColossalFramework;
using Klyte.Commons.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Klyte.TransportLinesManager.Extensors
{
    public partial class TLMTransportLineStatusesManager : SingletonLite<TLMTransportLineStatusesManager>
    {
        public static int BYTES_PER_CYCLE
        {
            get {
                if (m_cachedFrameSize != SimulationManager.DAYTIME_FRAMES)
                {
                    m_cachedFrameSize = SimulationManager.DAYTIME_FRAMES;
                    m_cachedBytesPerCycle = Mathf.RoundToInt(Mathf.Log(SimulationManager.DAYTIME_FRAMES) / Mathf.Log(2)) - 4;
                }
                return m_cachedBytesPerCycle;
            }
        }

        public static uint FRAMES_PER_CYCLE => 1u << (BYTES_PER_CYCLE);
        public static uint FRAMES_PER_CYCLE_MASK => FRAMES_PER_CYCLE - 1;
        public static uint TOTAL_STORAGE_CAPACITY => (1u << (BYTES_PER_CYCLE + 4));
        public static uint OFFSET_FRAMES => DayTimeOffsetFrames & FRAMES_PER_CYCLE_MASK;
        public static uint DayTimeOffsetFrames => SimulationManager.instance.m_enableDayNight ? SimulationManager.instance.m_dayTimeOffsetFrames : 0;
        public static uint INDEX_AND_FRAMES_MASK => TOTAL_STORAGE_CAPACITY - 1;
        public const int CYCLES_HISTORY_SIZE = 16;
        public const int CYCLES_HISTORY_MASK = CYCLES_HISTORY_SIZE - 1;
        public const int CYCLES_HISTORY_ARRAY_SIZE = CYCLES_HISTORY_SIZE + 1;
        public const int CYCLES_CURRENT_DATA_IDX = CYCLES_HISTORY_SIZE;

        private static uint m_cachedFrameSize = 0;
        private static int m_cachedBytesPerCycle = 0;



        public TLMTransportLineStatusesManager()
        {
            InitArray(ref m_linesDataLong, TransportManager.MAX_LINE_COUNT, typeof(LineDataLong));
            InitArray(ref m_vehiclesDataLong, (int)VehicleManager.instance.m_vehicles.m_size, typeof(VehicleDataLong));
            InitArray(ref m_stopDataLong, NetManager.MAX_NODE_COUNT, typeof(StopDataLong));

            InitArray(ref m_linesDataInt, TransportManager.MAX_LINE_COUNT, typeof(LineDataSmallInt));
            InitArray(ref m_vehiclesDataInt, (int)VehicleManager.instance.m_vehicles.m_size, typeof(VehicleDataSmallInt));
            InitArray(ref m_stopDataInt, NetManager.MAX_NODE_COUNT, typeof(StopDataSmallInt));

            InitArray(ref m_linesDataUshort, TransportManager.MAX_LINE_COUNT, typeof(LineDataUshort));
        }

        private void InitArray<T>(ref T[][] array, int size, Type enumType) where T : struct, IConvertible
        {
            array = new T[CYCLES_HISTORY_ARRAY_SIZE * size][];
            for (int k = 0; k < array.Length; k++)
            {
                array[k] = new T[Enum.GetValues(enumType).Length];
            }
        }

        #region Data feeding
        public void AddToLine(ushort lineId, long income, long expense, ref Citizen citizenData, ushort citizenId)
        {
            IncrementInArray(lineId, ref m_linesDataLong, ref m_linesDataInt, (int)LineDataLong.INCOME, (int)LineDataLong.EXPENSE, (int)LineDataSmallInt.TOTAL_PASSENGERS, (int)LineDataSmallInt.TOURIST_PASSENGERS, (int)LineDataSmallInt.STUDENT_PASSENGERS, income, expense, ref citizenData);
            if (!citizenData.Equals(default))
            {
                int idxW = ((((int)citizenData.WealthLevel * 5) + (int)Citizen.GetAgeGroup(citizenData.m_age)) << 1) + (int)Citizen.GetGender(citizenId);
                m_linesDataUshort[(lineId * CYCLES_HISTORY_ARRAY_SIZE) + CYCLES_CURRENT_DATA_IDX][idxW]++;
            }
        }

        public void AddToVehicle(ushort vehicleId, long income, long expense, ref Citizen citizenData) => IncrementInArray(vehicleId, ref m_vehiclesDataLong, ref m_vehiclesDataInt, (int)VehicleDataLong.INCOME, (int)VehicleDataLong.EXPENSE, (int)VehicleDataSmallInt.TOTAL_PASSENGERS, (int)VehicleDataSmallInt.TOURIST_PASSENGERS, (int)VehicleDataSmallInt.STUDENT_PASSENGERS, income, expense, ref citizenData);
        public void AddToStop(ushort stopId, long income, ref Citizen citizenData) => IncrementInArray(stopId, ref m_stopDataLong, ref m_stopDataInt, (int)StopDataLong.INCOME, null, (int)StopDataSmallInt.TOTAL_PASSENGERS, (int)StopDataSmallInt.TOURIST_PASSENGERS, (int)StopDataSmallInt.STUDENT_PASSENGERS, income, 0, ref citizenData);

        private void IncrementInArray(ushort id, ref long[][] arrayRef, ref int[][] arrayRefInt, int incomeIdx, int? expenseIdx, int totalPassIdx, int tourPassIdx, int studPassIdx, long income, long expense, ref Citizen citizenData)
        {
            arrayRef[(id * CYCLES_HISTORY_ARRAY_SIZE) + CYCLES_CURRENT_DATA_IDX][incomeIdx] += income;
            if (expenseIdx is int idx)
            {
                arrayRef[(id * CYCLES_HISTORY_ARRAY_SIZE) + CYCLES_CURRENT_DATA_IDX][idx] += expense;
            }
            if (!citizenData.Equals(default))
            {
                arrayRefInt[(id * CYCLES_HISTORY_ARRAY_SIZE) + CYCLES_CURRENT_DATA_IDX][totalPassIdx]++;
                if ((citizenData.m_flags & Citizen.Flags.Tourist) != 0)
                {
                    arrayRefInt[(id * CYCLES_HISTORY_ARRAY_SIZE) + CYCLES_CURRENT_DATA_IDX][tourPassIdx]++;
                }

                if ((citizenData.m_flags & Citizen.Flags.Student) != 0)
                {
                    arrayRefInt[(id * CYCLES_HISTORY_ARRAY_SIZE) + CYCLES_CURRENT_DATA_IDX][studPassIdx]++;
                }
            }
        }
        #endregion

        #region Generic Getters Income/Expense

        public void GetIncomeAndExpensesForLine(ushort lineId, out long income, out long expenses) => GetGenericIncomeExpense(lineId, out income, out expenses, ref m_linesDataLong, (int)LineDataLong.INCOME, (int)LineDataLong.EXPENSE);

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

        private static T GetAtArray<T>(ushort id, ref T[][] arrayData, int entryIdx, int dataIdx) where T : struct, IComparable => arrayData[(id * 17) + dataIdx][entryIdx];

        private void GetGenericIncome(ushort id, out long income, ref long[][] arrayData, int incomeEntry)
        {
            income = 0L;
            for (int j = 0; j <= 16; j++)
            {
                income += arrayData[(id * 17) + j][incomeEntry];
            }
        }
        #endregion

        #region Specific Income/Expense Getters

        public void GetIncomeAndExpensesForVehicle(ushort vehicleId, out long income, out long expenses) => GetGenericIncomeExpense(vehicleId, out income, out expenses, ref m_vehiclesDataLong, (int)VehicleDataLong.INCOME, (int)VehicleDataLong.EXPENSE);
        public void GetStopIncome(ushort stopId, out long income) => GetGenericIncome(stopId, out income, ref m_stopDataLong, (int)StopDataLong.INCOME);

        public void GetCurrentIncomeAndExpensesForLine(ushort lineId, out long income, out long expenses)
        {
            income = GetAtArray(lineId, ref m_linesDataLong, (int)LineDataLong.INCOME, CYCLES_CURRENT_DATA_IDX);
            expenses = GetAtArray(lineId, ref m_linesDataLong, (int)LineDataLong.EXPENSE, CYCLES_CURRENT_DATA_IDX);
        }
        public void GetCurrentIncomeAndExpensesForVehicles(ushort vehicleId, out long income, out long expenses)
        {
            income = GetAtArray(vehicleId, ref m_vehiclesDataLong, (int)VehicleDataLong.INCOME, CYCLES_CURRENT_DATA_IDX);
            expenses = GetAtArray(vehicleId, ref m_vehiclesDataLong, (int)VehicleDataLong.EXPENSE, CYCLES_CURRENT_DATA_IDX);
        }
        public void GetCurrentStopIncome(ushort stopId, out long income) => income = GetAtArray(stopId, ref m_stopDataLong, (int)StopDataLong.INCOME, CYCLES_CURRENT_DATA_IDX);

        public void GetLastWeekIncomeAndExpensesForLine(ushort lineId, out long income, out long expenses)
        {
            int lastIdx = ((int)CurrentArrayEntryIdx + CYCLES_HISTORY_SIZE - 1) & CYCLES_HISTORY_MASK;
            income = GetAtArray(lineId, ref m_linesDataLong, (int)LineDataLong.INCOME, lastIdx);
            expenses = GetAtArray(lineId, ref m_linesDataLong, (int)LineDataLong.EXPENSE, lastIdx);
        }
        public void GetLastWeekIncomeAndExpensesForVehicles(ushort vehicleId, out long income, out long expenses)
        {
            int lastIdx = ((int)CurrentArrayEntryIdx + CYCLES_HISTORY_SIZE - 1) & CYCLES_HISTORY_MASK;
            income = GetAtArray(vehicleId, ref m_vehiclesDataLong, (int)VehicleDataLong.INCOME, lastIdx);
            expenses = GetAtArray(vehicleId, ref m_vehiclesDataLong, (int)VehicleDataLong.EXPENSE, lastIdx);
        }
        public void GetLastWeekStopIncome(ushort stopId, out long income)
        {
            int lastIdx = ((int)CurrentArrayEntryIdx + CYCLES_HISTORY_SIZE - 1) & CYCLES_HISTORY_MASK;
            income = GetAtArray(stopId, ref m_stopDataLong, (int)StopDataLong.INCOME, lastIdx);
        }
        #endregion

        #region Report extraction
        public List<IncomeExpenseReport> GetLineFinanceReport(ushort lineId)
        {
            var result = new List<IncomeExpenseReport>();
            for (int j = 0; j < 16; j++)
            {
                result.Add(new IncomeExpenseReport
                {
                    Income = GetAtArray(lineId, ref m_linesDataLong, (int)LineDataLong.INCOME, j),
                    Expense = GetAtArray(lineId, ref m_linesDataLong, (int)LineDataLong.EXPENSE, j),
                    RefFrame = GetStartFrameForArrayIdx(j)
                });

            }
            result.Add(new IncomeExpenseReport
            {
                Income = GetAtArray(lineId, ref m_linesDataLong, (int)LineDataLong.INCOME, CYCLES_CURRENT_DATA_IDX),
                Expense = GetAtArray(lineId, ref m_linesDataLong, (int)LineDataLong.EXPENSE, CYCLES_CURRENT_DATA_IDX),
                RefFrame = (Singleton<SimulationManager>.instance.m_currentFrameIndex + OFFSET_FRAMES) & ~FRAMES_PER_CYCLE_MASK
            });
            result.Sort((a, b) => a.RefFrame.CompareTo(b.RefFrame));
            return result;
        }
        public List<StudentsTouristsReport> GetLineStudentTouristsTotalReport(ushort lineId)
        {
            var result = new List<StudentsTouristsReport>();
            for (int j = 0; j < 16; j++)
            {
                result.Add(new StudentsTouristsReport
                {
                    Total = GetAtArray(lineId, ref m_linesDataInt, (int)LineDataSmallInt.TOTAL_PASSENGERS, j),
                    Student = GetAtArray(lineId, ref m_linesDataInt, (int)LineDataSmallInt.STUDENT_PASSENGERS, j),
                    Tourists = GetAtArray(lineId, ref m_linesDataInt, (int)LineDataSmallInt.TOURIST_PASSENGERS, j),
                    RefFrame = GetStartFrameForArrayIdx(j)
                });

            }
            result.Add(new StudentsTouristsReport
            {
                Total = GetAtArray(lineId, ref m_linesDataInt, (int)LineDataSmallInt.TOTAL_PASSENGERS, CYCLES_CURRENT_DATA_IDX),
                Student = GetAtArray(lineId, ref m_linesDataInt, (int)LineDataSmallInt.STUDENT_PASSENGERS, CYCLES_CURRENT_DATA_IDX),
                Tourists = GetAtArray(lineId, ref m_linesDataInt, (int)LineDataSmallInt.TOURIST_PASSENGERS, CYCLES_CURRENT_DATA_IDX),
                RefFrame = (Singleton<SimulationManager>.instance.m_currentFrameIndex + OFFSET_FRAMES) & ~FRAMES_PER_CYCLE_MASK
            });
            result.Sort((a, b) => a.RefFrame.CompareTo(b.RefFrame));
            return result;
        }
        public List<WealthPassengerReport> GetLineWealthReport(ushort lineId)
        {
            var result = new List<WealthPassengerReport>();
            for (int j = 0; j < 16; j++)
            {
                result.Add(new WealthPassengerReport
                {
                    Low = LowWealthData.Select(x => GetAtArray(lineId, ref m_linesDataUshort, (int)x, j)).Sum(x => x),
                    Medium = MedWealthData.Select(x => GetAtArray(lineId, ref m_linesDataUshort, (int)x, j)).Sum(x => x),
                    High = HghWealthData.Select(x => GetAtArray(lineId, ref m_linesDataUshort, (int)x, j)).Sum(x => x),
                    RefFrame = GetStartFrameForArrayIdx(j)
                });

            }
            result.Add(new WealthPassengerReport
            {
                Low = LowWealthData.Select(x => GetAtArray(lineId, ref m_linesDataUshort, (int)x, CYCLES_CURRENT_DATA_IDX)).Sum(x => x),
                Medium = MedWealthData.Select(x => GetAtArray(lineId, ref m_linesDataUshort, (int)x, CYCLES_CURRENT_DATA_IDX)).Sum(x => x),
                High = HghWealthData.Select(x => GetAtArray(lineId, ref m_linesDataUshort, (int)x, CYCLES_CURRENT_DATA_IDX)).Sum(x => x),
                RefFrame = (Singleton<SimulationManager>.instance.m_currentFrameIndex + OFFSET_FRAMES) & ~FRAMES_PER_CYCLE_MASK
            });
            result.Sort((a, b) => a.RefFrame.CompareTo(b.RefFrame));
            return result;
        }
        public List<AgePassengerReport> GetLineAgeReport(ushort lineId)
        {
            var result = new List<AgePassengerReport>();
            for (int j = 0; j < 16; j++)
            {
                result.Add(new AgePassengerReport
                {
                    Child = ChildData.Select(x => GetAtArray(lineId, ref m_linesDataUshort, (int)x, j)).Sum(x => x),
                    Teen = TeenData.Select(x => GetAtArray(lineId, ref m_linesDataUshort, (int)x, j)).Sum(x => x),
                    Young = YoungData.Select(x => GetAtArray(lineId, ref m_linesDataUshort, (int)x, j)).Sum(x => x),
                    Adult = AdultData.Select(x => GetAtArray(lineId, ref m_linesDataUshort, (int)x, j)).Sum(x => x),
                    Elder = ElderData.Select(x => GetAtArray(lineId, ref m_linesDataUshort, (int)x, j)).Sum(x => x),
                    RefFrame = GetStartFrameForArrayIdx(j)
                });

            }
            result.Add(new AgePassengerReport
            {
                Child = ChildData.Select(x => GetAtArray(lineId, ref m_linesDataUshort, (int)x, CYCLES_CURRENT_DATA_IDX)).Sum(x => x),
                Teen = TeenData.Select(x => GetAtArray(lineId, ref m_linesDataUshort, (int)x, CYCLES_CURRENT_DATA_IDX)).Sum(x => x),
                Young = YoungData.Select(x => GetAtArray(lineId, ref m_linesDataUshort, (int)x, CYCLES_CURRENT_DATA_IDX)).Sum(x => x),
                Adult = AdultData.Select(x => GetAtArray(lineId, ref m_linesDataUshort, (int)x, CYCLES_CURRENT_DATA_IDX)).Sum(x => x),
                Elder = ElderData.Select(x => GetAtArray(lineId, ref m_linesDataUshort, (int)x, CYCLES_CURRENT_DATA_IDX)).Sum(x => x),
                RefFrame = (Singleton<SimulationManager>.instance.m_currentFrameIndex + OFFSET_FRAMES) & ~FRAMES_PER_CYCLE_MASK
            });
            result.Sort((a, b) => a.RefFrame.CompareTo(b.RefFrame));
            return result;
        }
        public List<GenderPassengerReport> GetLineGenderReport(ushort lineId)
        {
            var result = new List<GenderPassengerReport>();
            for (int j = 0; j < 16; j++)
            {
                result.Add(new GenderPassengerReport
                {
                    Male = MaleData.Select(x => GetAtArray(lineId, ref m_linesDataUshort, (int)x, j)).Sum(x => x),
                    Female = FemaleData.Select(x => GetAtArray(lineId, ref m_linesDataUshort, (int)x, j)).Sum(x => x),
                    RefFrame = GetStartFrameForArrayIdx(j)
                });

            }
            result.Add(new GenderPassengerReport
            {
                Male = MaleData.Select(x => GetAtArray(lineId, ref m_linesDataUshort, (int)x, CYCLES_CURRENT_DATA_IDX)).Sum(x => x),
                Female = FemaleData.Select(x => GetAtArray(lineId, ref m_linesDataUshort, (int)x, CYCLES_CURRENT_DATA_IDX)).Sum(x => x),
                RefFrame = (Singleton<SimulationManager>.instance.m_currentFrameIndex + OFFSET_FRAMES) & ~FRAMES_PER_CYCLE_MASK
            });
            result.Sort((a, b) => a.RefFrame.CompareTo(b.RefFrame));
            return result;
        }
        public sealed class GenderPassengerReport : BasicReportData
        {
            public long Male { get; set; }
            public long Female { get; set; }
            public long Total => Male + Female;
        }
        public sealed class AgePassengerReport : BasicReportData
        {
            public long Child { get; set; }
            public long Teen { get; set; }
            public long Young { get; set; }
            public long Adult { get; set; }
            public long Elder { get; set; }
            public long Total => Child + Teen + Young + Adult + Elder;
        }
        public sealed class WealthPassengerReport : BasicReportData
        {
            public long Low { get; set; }
            public long Medium { get; set; }
            public long High { get; set; }
            public long Total => Low + Medium + High;
        }
        public sealed class StudentsTouristsReport : BasicReportData
        {
            public long Total { get; set; }
            public long Student { get; set; }
            public long Tourists { get; set; }
        }
        public sealed class IncomeExpenseReport : BasicReportData
        {
            public long Income { get; set; }
            public long Expense { get; set; }
        }
        public abstract class BasicReportData
        {
            public long RefFrame { get; set; }

            public DateTime StartDate => SimulationManager.instance.FrameToTime((uint)RefFrame - OFFSET_FRAMES);
            public DateTime EndDate => SimulationManager.instance.FrameToTime((uint)RefFrame + FRAMES_PER_CYCLE_MASK - OFFSET_FRAMES);
            public float StartDayTime => FrameToDaytime(RefFrame - OFFSET_FRAMES);
            public float EndDayTime => FrameToDaytime(RefFrame + FRAMES_PER_CYCLE_MASK - OFFSET_FRAMES);

            private static float FrameToDaytime(long refFrame)
            {
                float num = (refFrame + DayTimeOffsetFrames) & (SimulationManager.DAYTIME_FRAMES - 1u);
                num *= SimulationManager.DAYTIME_FRAME_TO_HOUR;
                if (num >= 24f)
                {
                    num -= 24f;
                }
                return num;
            }
        }
        #endregion

        #region Cycling
        private uint CurrentArrayEntryIdx => ((Singleton<SimulationManager>.instance.m_currentFrameIndex + OFFSET_FRAMES) >> BYTES_PER_CYCLE) & CYCLES_HISTORY_MASK;

        private long GetStartFrameForArrayIdx(int idx) => ((Singleton<SimulationManager>.instance.m_currentFrameIndex + OFFSET_FRAMES) & ~INDEX_AND_FRAMES_MASK) + (idx << BYTES_PER_CYCLE) - (idx >= CurrentArrayEntryIdx ? TOTAL_STORAGE_CAPACITY : 0);

        public static void SimulationStepImpl(int subStep)
        {
            if (subStep != 0 && subStep != 1000)
            {
                uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex + OFFSET_FRAMES;
                uint frameCounterCycle = currentFrameIndex & FRAMES_PER_CYCLE_MASK;
                if (frameCounterCycle == 0)
                {
                    currentFrameIndex--;
                    uint idxEnum = (currentFrameIndex >> BYTES_PER_CYCLE) & CYCLES_HISTORY_MASK;
                    LogUtils.DoLog($"Stroring data for frame {(currentFrameIndex & ~FRAMES_PER_CYCLE_MASK).ToString("X8")} into idx {idxEnum.ToString("X1")}");

                    FinishCycle(idxEnum, ref instance.m_linesDataLong, TransportManager.MAX_LINE_COUNT);
                    FinishCycle(idxEnum, ref instance.m_vehiclesDataLong, (int)VehicleManager.instance.m_vehicles.m_size);
                    FinishCycle(idxEnum, ref instance.m_stopDataLong, NetManager.MAX_NODE_COUNT);
                    FinishCycle(idxEnum, ref instance.m_linesDataInt, TransportManager.MAX_LINE_COUNT);
                    FinishCycle(idxEnum, ref instance.m_vehiclesDataInt, (int)VehicleManager.instance.m_vehicles.m_size);
                    FinishCycle(idxEnum, ref instance.m_stopDataInt, NetManager.MAX_NODE_COUNT);
                    FinishCycle(idxEnum, ref instance.m_linesDataUshort, TransportManager.MAX_LINE_COUNT);
                }
            }
        }

        private static void FinishCycle<T>(uint idxEnum, ref T[][] arrayRef, int loopSize) where T : struct, IConvertible
        {
            for (int k = 0; k < loopSize; k++)
            {
                int kIdx = (k * CYCLES_HISTORY_ARRAY_SIZE);
                for (int l = 0; l < arrayRef[kIdx].Length; l++)
                {
                    arrayRef[kIdx + idxEnum][l] = arrayRef[kIdx + CYCLES_CURRENT_DATA_IDX][l];
                    arrayRef[kIdx + CYCLES_CURRENT_DATA_IDX][l] = default;
                }
            }
        }

        private static void ClearArray<T>(ref T[][] arrayRef) where T : struct, IComparable
        {
            for (int k = 0; k < arrayRef.Length; k++)
            {
                for (int l = 0; l < arrayRef[k].Length; l++)
                {
                    arrayRef[k][l] = default;
                }
            }
        }


        public static void UpdateData(SimulationManager.UpdateMode mode)
        {
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginLoading("UVMTransportLineEconomyManager.UpdateData");
            if (mode == SimulationManager.UpdateMode.NewMap || mode == SimulationManager.UpdateMode.NewGameFromMap || mode == SimulationManager.UpdateMode.NewScenarioFromMap || mode == SimulationManager.UpdateMode.UpdateScenarioFromMap || mode == SimulationManager.UpdateMode.NewAsset)
            {
                ClearArray(ref SingletonLite<TLMTransportLineStatusesManager>.instance.m_linesDataLong);
                ClearArray(ref SingletonLite<TLMTransportLineStatusesManager>.instance.m_vehiclesDataLong);
                ClearArray(ref SingletonLite<TLMTransportLineStatusesManager>.instance.m_stopDataLong);
                ClearArray(ref SingletonLite<TLMTransportLineStatusesManager>.instance.m_linesDataInt);
                ClearArray(ref SingletonLite<TLMTransportLineStatusesManager>.instance.m_vehiclesDataInt);
                ClearArray(ref SingletonLite<TLMTransportLineStatusesManager>.instance.m_stopDataInt);
                ClearArray(ref SingletonLite<TLMTransportLineStatusesManager>.instance.m_linesDataUshort);
            }
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndLoading();
        }
        #endregion

        private long[][] m_linesDataLong;
        private long[][] m_vehiclesDataLong;
        private long[][] m_stopDataLong;

        private int[][] m_linesDataInt;
        private int[][] m_vehiclesDataInt;
        private int[][] m_stopDataInt;

        private ushort[][] m_linesDataUshort;

        #region Enums
        private enum LineDataLong
        {
            EXPENSE,
            INCOME,
        }
        private enum VehicleDataLong
        {
            EXPENSE,
            INCOME,
        }
        private enum StopDataLong
        {
            INCOME
        }

        private enum LineDataSmallInt
        {
            TOTAL_PASSENGERS,
            TOURIST_PASSENGERS,
            STUDENT_PASSENGERS
        }
        private enum VehicleDataSmallInt
        {
            TOTAL_PASSENGERS,
            TOURIST_PASSENGERS,
            STUDENT_PASSENGERS
        }
        private enum StopDataSmallInt
        {
            TOTAL_PASSENGERS,
            TOURIST_PASSENGERS,
            STUDENT_PASSENGERS
        }
        private enum LineDataUshort
        {
            W1_CHILD_MALE_PASSENGERS,
            W1_TEENS_MALE_PASSENGERS,
            W1_YOUNG_MALE_PASSENGERS,
            W1_ADULT_MALE_PASSENGERS,
            W1_ELDER_MALE_PASSENGERS,
            W2_CHILD_MALE_PASSENGERS,
            W2_TEENS_MALE_PASSENGERS,
            W2_YOUNG_MALE_PASSENGERS,
            W2_ADULT_MALE_PASSENGERS,
            W2_ELDER_MALE_PASSENGERS,
            W3_CHILD_MALE_PASSENGERS,
            W3_TEENS_MALE_PASSENGERS,
            W3_YOUNG_MALE_PASSENGERS,
            W3_ADULT_MALE_PASSENGERS,
            W3_ELDER_MALE_PASSENGERS,
            W1_CHILD_FEML_PASSENGERS,
            W1_TEENS_FEML_PASSENGERS,
            W1_YOUNG_FEML_PASSENGERS,
            W1_ADULT_FEML_PASSENGERS,
            W1_ELDER_FEML_PASSENGERS,
            W2_CHILD_FEML_PASSENGERS,
            W2_TEENS_FEML_PASSENGERS,
            W2_YOUNG_FEML_PASSENGERS,
            W2_ADULT_FEML_PASSENGERS,
            W2_ELDER_FEML_PASSENGERS,
            W3_CHILD_FEML_PASSENGERS,
            W3_TEENS_FEML_PASSENGERS,
            W3_YOUNG_FEML_PASSENGERS,
            W3_ADULT_FEML_PASSENGERS,
            W3_ELDER_FEML_PASSENGERS,
        }

        #endregion

        #region Enum grouping

        private LineDataUshort[] LowWealthData = new LineDataUshort[]
        {
            LineDataUshort.W1_CHILD_MALE_PASSENGERS,
            LineDataUshort.W1_TEENS_MALE_PASSENGERS,
            LineDataUshort.W1_YOUNG_MALE_PASSENGERS,
            LineDataUshort.W1_ADULT_MALE_PASSENGERS,
            LineDataUshort.W1_ELDER_MALE_PASSENGERS,
            LineDataUshort.W1_CHILD_FEML_PASSENGERS,
            LineDataUshort.W1_TEENS_FEML_PASSENGERS,
            LineDataUshort.W1_YOUNG_FEML_PASSENGERS,
            LineDataUshort.W1_ADULT_FEML_PASSENGERS,
            LineDataUshort.W1_ELDER_FEML_PASSENGERS,
};
        private LineDataUshort[] MedWealthData = new LineDataUshort[]
         {
            LineDataUshort.W2_CHILD_MALE_PASSENGERS,
            LineDataUshort.W2_TEENS_MALE_PASSENGERS,
            LineDataUshort.W2_YOUNG_MALE_PASSENGERS,
            LineDataUshort.W2_ADULT_MALE_PASSENGERS,
            LineDataUshort.W2_ELDER_MALE_PASSENGERS,
            LineDataUshort.W2_CHILD_FEML_PASSENGERS,
            LineDataUshort.W2_TEENS_FEML_PASSENGERS,
            LineDataUshort.W2_YOUNG_FEML_PASSENGERS,
            LineDataUshort.W2_ADULT_FEML_PASSENGERS,
            LineDataUshort.W2_ELDER_FEML_PASSENGERS,
};
        private LineDataUshort[] HghWealthData = new LineDataUshort[]
         {
            LineDataUshort.W3_CHILD_MALE_PASSENGERS,
            LineDataUshort.W3_TEENS_MALE_PASSENGERS,
            LineDataUshort.W3_YOUNG_MALE_PASSENGERS,
            LineDataUshort.W3_ADULT_MALE_PASSENGERS,
            LineDataUshort.W3_ELDER_MALE_PASSENGERS,
            LineDataUshort.W3_CHILD_FEML_PASSENGERS,
            LineDataUshort.W3_TEENS_FEML_PASSENGERS,
            LineDataUshort.W3_YOUNG_FEML_PASSENGERS,
            LineDataUshort.W3_ADULT_FEML_PASSENGERS,
            LineDataUshort.W3_ELDER_FEML_PASSENGERS,
        };



        private LineDataUshort[] ChildData = new LineDataUshort[]
        {
            LineDataUshort.W1_CHILD_MALE_PASSENGERS,
            LineDataUshort.W1_CHILD_FEML_PASSENGERS,
            LineDataUshort.W2_CHILD_MALE_PASSENGERS,
            LineDataUshort.W2_CHILD_FEML_PASSENGERS,
            LineDataUshort.W3_CHILD_MALE_PASSENGERS,
            LineDataUshort.W3_CHILD_FEML_PASSENGERS,
};
        private LineDataUshort[] TeenData = new LineDataUshort[]
         {
            LineDataUshort.W1_TEENS_MALE_PASSENGERS,
            LineDataUshort.W1_TEENS_FEML_PASSENGERS,
            LineDataUshort.W2_TEENS_MALE_PASSENGERS,
            LineDataUshort.W2_TEENS_FEML_PASSENGERS,
            LineDataUshort.W3_TEENS_MALE_PASSENGERS,
            LineDataUshort.W3_TEENS_FEML_PASSENGERS,
};
        private LineDataUshort[] YoungData = new LineDataUshort[]
         {
            LineDataUshort.W1_YOUNG_MALE_PASSENGERS,
            LineDataUshort.W1_YOUNG_FEML_PASSENGERS,
            LineDataUshort.W2_YOUNG_MALE_PASSENGERS,
            LineDataUshort.W2_YOUNG_FEML_PASSENGERS,
            LineDataUshort.W3_YOUNG_MALE_PASSENGERS,
            LineDataUshort.W3_YOUNG_FEML_PASSENGERS,
        };
        private LineDataUshort[] AdultData = new LineDataUshort[]
         {
            LineDataUshort.W1_ADULT_MALE_PASSENGERS,
            LineDataUshort.W1_ADULT_FEML_PASSENGERS,
            LineDataUshort.W2_ADULT_MALE_PASSENGERS,
            LineDataUshort.W2_ADULT_FEML_PASSENGERS,
            LineDataUshort.W3_ADULT_MALE_PASSENGERS,
            LineDataUshort.W3_ADULT_FEML_PASSENGERS,
        };
        private LineDataUshort[] ElderData = new LineDataUshort[]
         {
            LineDataUshort.W1_ELDER_MALE_PASSENGERS,
            LineDataUshort.W1_ELDER_FEML_PASSENGERS,
            LineDataUshort.W2_ELDER_MALE_PASSENGERS,
            LineDataUshort.W2_ELDER_FEML_PASSENGERS,
            LineDataUshort.W3_ELDER_MALE_PASSENGERS,
            LineDataUshort.W3_ELDER_FEML_PASSENGERS,
        };
        private LineDataUshort[] MaleData = new LineDataUshort[]
         {
            LineDataUshort.W1_CHILD_MALE_PASSENGERS,
            LineDataUshort.W1_TEENS_MALE_PASSENGERS,
            LineDataUshort.W1_YOUNG_MALE_PASSENGERS,
            LineDataUshort.W1_ADULT_MALE_PASSENGERS,
            LineDataUshort.W1_ELDER_MALE_PASSENGERS,
            LineDataUshort.W2_CHILD_MALE_PASSENGERS,
            LineDataUshort.W2_TEENS_MALE_PASSENGERS,
            LineDataUshort.W2_YOUNG_MALE_PASSENGERS,
            LineDataUshort.W2_ADULT_MALE_PASSENGERS,
            LineDataUshort.W2_ELDER_MALE_PASSENGERS,
            LineDataUshort.W3_CHILD_MALE_PASSENGERS,
            LineDataUshort.W3_TEENS_MALE_PASSENGERS,
            LineDataUshort.W3_YOUNG_MALE_PASSENGERS,
            LineDataUshort.W3_ADULT_MALE_PASSENGERS,
            LineDataUshort.W3_ELDER_MALE_PASSENGERS,
        };
        private LineDataUshort[] FemaleData = new LineDataUshort[]
        {
           LineDataUshort.W1_CHILD_FEML_PASSENGERS,
           LineDataUshort.W1_TEENS_FEML_PASSENGERS,
           LineDataUshort.W1_YOUNG_FEML_PASSENGERS,
           LineDataUshort.W1_ADULT_FEML_PASSENGERS,
           LineDataUshort.W1_ELDER_FEML_PASSENGERS,
           LineDataUshort.W2_CHILD_FEML_PASSENGERS,
           LineDataUshort.W2_TEENS_FEML_PASSENGERS,
           LineDataUshort.W2_YOUNG_FEML_PASSENGERS,
           LineDataUshort.W2_ADULT_FEML_PASSENGERS,
           LineDataUshort.W2_ELDER_FEML_PASSENGERS,
           LineDataUshort.W3_CHILD_FEML_PASSENGERS,
           LineDataUshort.W3_TEENS_FEML_PASSENGERS,
           LineDataUshort.W3_YOUNG_FEML_PASSENGERS,
           LineDataUshort.W3_ADULT_FEML_PASSENGERS,
           LineDataUshort.W3_ELDER_FEML_PASSENGERS,
        };

        #endregion

        #region Serialization Utils
        private void DoWithArray(Enum e, DoWithArrayRef<long> action, DoWithArrayRef<int> actionInt, DoWithArrayRef<ushort> actionUshort)
        {
            switch (e)
            {
                case LineDataLong _:
                    action(ref m_linesDataLong);
                    break;
                case VehicleDataLong _:
                    action(ref m_vehiclesDataLong);
                    break;
                case StopDataLong _:
                    action(ref m_stopDataLong);
                    break;
                case LineDataSmallInt _:
                    actionInt(ref m_linesDataInt);
                    break;
                case VehicleDataSmallInt _:
                    actionInt(ref m_vehiclesDataInt);
                    break;
                case StopDataSmallInt _:
                    actionInt(ref m_stopDataInt);
                    break;
                case LineDataUshort _:
                    actionUshort(ref m_linesDataUshort);
                    break;
            }
        }

        private static int PredictSize(Enum[] enumArray)
        {
            int size = 0;
            foreach (var e in enumArray)
            {
                switch (e)
                {
                    case LineDataLong _:
                        size += 8 * TransportManager.MAX_LINE_COUNT;
                        break;
                    case VehicleDataLong _:
                        size += 8 * (int)VehicleManager.instance.m_vehicles.m_size;
                        break;
                    case StopDataLong _:
                        size += 8 * NetManager.MAX_NODE_COUNT;
                        break;
                    case LineDataSmallInt _:
                        size += 3 * TransportManager.MAX_LINE_COUNT;
                        break;
                    case VehicleDataSmallInt _:
                        size += 3 * (int)VehicleManager.instance.m_vehicles.m_size;
                        break;
                    case StopDataSmallInt _:
                        size += 3 * NetManager.MAX_NODE_COUNT;
                        break;
                    case LineDataUshort _:
                        size += 2 * TransportManager.MAX_LINE_COUNT;
                        break;
                }
            }
            return size;
        }

        private delegate void DoWithArrayRef<T>(ref T[][] arrayRef) where T : struct, IComparable;

        private int GetIdxFor(Enum e)
        {
            switch (e)
            {
                case LineDataLong l:
                    return (int)l;
                case VehicleDataLong l:
                    return (int)l;
                case StopDataLong l:
                    return (int)l;
                case LineDataSmallInt l:
                    return (int)l;
                case VehicleDataSmallInt l:
                    return (int)l;
                case StopDataSmallInt l:
                    return (int)l;
                case LineDataUshort l:
                    return (int)l;
                default:
                    e.GetType();
                    throw new Exception("Invalid data in array deserialize!");
            }
        }

        private static int GetMinVersion(Enum e)
        {
            switch (e)
            {
                case LineDataLong l:
                    switch (l)
                    {
                        case LineDataLong.EXPENSE:
                        case LineDataLong.INCOME:
                            return 0;
                    }
                    break;
                case VehicleDataLong v:
                    switch (v)
                    {
                        case VehicleDataLong.EXPENSE:
                        case VehicleDataLong.INCOME:
                            return 0;
                    }
                    break;
                case StopDataLong s:
                    switch (s)
                    {
                        case StopDataLong.INCOME:
                            return 0;
                    }
                    break;
                case LineDataSmallInt l:
                    switch (l)
                    {
                        case LineDataSmallInt.TOTAL_PASSENGERS:
                        case LineDataSmallInt.TOURIST_PASSENGERS:
                        case LineDataSmallInt.STUDENT_PASSENGERS:
                            return 1;
                    }
                    break;
                case VehicleDataSmallInt v:
                    switch (v)
                    {
                        case VehicleDataSmallInt.TOTAL_PASSENGERS:
                        case VehicleDataSmallInt.TOURIST_PASSENGERS:
                        case VehicleDataSmallInt.STUDENT_PASSENGERS:
                            return 1;
                    }
                    break;
                case StopDataSmallInt s:
                    switch (s)
                    {
                        case StopDataSmallInt.TOTAL_PASSENGERS:
                        case StopDataSmallInt.TOURIST_PASSENGERS:
                        case StopDataSmallInt.STUDENT_PASSENGERS:
                            return 1;
                    }
                    break;
                case LineDataUshort l:
                    switch (l)
                    {
                        case LineDataUshort.W1_CHILD_MALE_PASSENGERS:
                        case LineDataUshort.W1_TEENS_MALE_PASSENGERS:
                        case LineDataUshort.W1_YOUNG_MALE_PASSENGERS:
                        case LineDataUshort.W1_ADULT_MALE_PASSENGERS:
                        case LineDataUshort.W1_ELDER_MALE_PASSENGERS:
                        case LineDataUshort.W2_CHILD_MALE_PASSENGERS:
                        case LineDataUshort.W2_TEENS_MALE_PASSENGERS:
                        case LineDataUshort.W2_YOUNG_MALE_PASSENGERS:
                        case LineDataUshort.W2_ADULT_MALE_PASSENGERS:
                        case LineDataUshort.W2_ELDER_MALE_PASSENGERS:
                        case LineDataUshort.W3_CHILD_MALE_PASSENGERS:
                        case LineDataUshort.W3_TEENS_MALE_PASSENGERS:
                        case LineDataUshort.W3_YOUNG_MALE_PASSENGERS:
                        case LineDataUshort.W3_ADULT_MALE_PASSENGERS:
                        case LineDataUshort.W3_ELDER_MALE_PASSENGERS:
                        case LineDataUshort.W1_CHILD_FEML_PASSENGERS:
                        case LineDataUshort.W1_TEENS_FEML_PASSENGERS:
                        case LineDataUshort.W1_YOUNG_FEML_PASSENGERS:
                        case LineDataUshort.W1_ADULT_FEML_PASSENGERS:
                        case LineDataUshort.W1_ELDER_FEML_PASSENGERS:
                        case LineDataUshort.W2_CHILD_FEML_PASSENGERS:
                        case LineDataUshort.W2_TEENS_FEML_PASSENGERS:
                        case LineDataUshort.W2_YOUNG_FEML_PASSENGERS:
                        case LineDataUshort.W2_ADULT_FEML_PASSENGERS:
                        case LineDataUshort.W2_ELDER_FEML_PASSENGERS:
                        case LineDataUshort.W3_CHILD_FEML_PASSENGERS:
                        case LineDataUshort.W3_TEENS_FEML_PASSENGERS:
                        case LineDataUshort.W3_YOUNG_FEML_PASSENGERS:
                        case LineDataUshort.W3_ADULT_FEML_PASSENGERS:
                        case LineDataUshort.W3_ELDER_FEML_PASSENGERS:
                            return 3;
                    }
                    break;
            }
            return 99999999;
        }

        #endregion

        public const long CURRENT_VERSION = 3;
    }
}