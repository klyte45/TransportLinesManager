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

    internal class TLMLinePassengerAgeReportTab : BasicReportTab<TLMPassengerAgeReportLine, AgePassengerReport>
    {
        protected override string TitleLocaleID { get; } = "K45_TLM_PASSENGERS_AGE_LINE_REPORT";
        public override bool MayBeVisible() =>true;
        protected override List<AgePassengerReport> GetReportData(ushort lineId) => TLMTransportLineStatusesManager.instance.GetLineAgeReport(lineId);
        protected override void AddToTotalizer(ref AgePassengerReport totalizer, AgePassengerReport data)
        {
            totalizer.Child += data.Child;
            totalizer.Teen +=  data.Teen;
            totalizer.Young += data.Young;
            totalizer.Adult += data.Adult;
            totalizer.Elder += data.Elder;
        }


    }
}