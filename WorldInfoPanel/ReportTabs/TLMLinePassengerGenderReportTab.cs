using ColossalFramework.UI;
using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using Klyte.TransportLinesManager.Extensions;
using System.Collections.Generic;
using UnityEngine;
using static Klyte.TransportLinesManager.Extensions.TLMTransportLineStatusesManager;
using static Klyte.TransportLinesManager.UI.TLMReportsTab;

namespace Klyte.TransportLinesManager.UI
{

    internal class TLMLinePassengerGenderReportTab : BasicReportTab<TLMPassengerGenderReportLine, GenderPassengerReport>
    {
        protected override string TitleLocaleID { get; } = "K45_TLM_PASSENGERS_GENDER_LINE_REPORT";
        public override bool MayBeVisible() => true;
        protected override List<GenderPassengerReport> GetReportData(ushort lineId) => TLMTransportLineStatusesManager.instance.GetLineGenderReport(lineId);
        protected override void AddToTotalizer(ref GenderPassengerReport totalizer, GenderPassengerReport data)
        {
            totalizer.Male +=   data.Male;
            totalizer.Female += data.Female;
        }

    }
}