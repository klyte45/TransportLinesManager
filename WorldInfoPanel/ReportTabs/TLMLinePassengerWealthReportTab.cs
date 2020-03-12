using ColossalFramework.UI;
using Klyte.Commons.Extensors;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensors;
using System.Collections.Generic;
using UnityEngine;
using static Klyte.TransportLinesManager.Extensors.TLMTransportLineStatusesManager;
using static Klyte.TransportLinesManager.UI.TLMReportsTab;

namespace Klyte.TransportLinesManager.UI
{

    internal class TLMLinePassengerWealthReportTab : BasicReportTab<TLMPassengerWealthReportLine, WealthPassengerReport>
    {
        protected override string TitleLocaleID { get; } = "K45_TLM_PASSENGERS_WEALTH_LINE_REPORT";
        public override bool MayBeVisible() => true;
        protected override List<WealthPassengerReport> GetReportData(ushort lineId) => TLMTransportLineStatusesManager.instance.GetLineWealthReport(lineId);
        protected override void AddToTotalizer(ref WealthPassengerReport totalizer, WealthPassengerReport data)
        {
            totalizer.Low +=    data.Low;
            totalizer.Medium += data.Medium;
            totalizer.High +=   data.High;
        }
    }
}