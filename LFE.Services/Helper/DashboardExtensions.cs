using System;
using LFE.Core.Enums;
using LFE.DataTokens;

namespace LFE.Application.Services.Helper
{
    public static class DashboardExtensions
    {

        public static string EventType2Color(this DashboardEnums.eDbEventTypes type)
        {
            switch (type)
            {
                case DashboardEnums.eDbEventTypes.NewItem: return "#01aaff";
                case DashboardEnums.eDbEventTypes.NewChapter: return "#b2e4e1";
                case DashboardEnums.eDbEventTypes.NewFbStore: return "#d3eab2";
                case DashboardEnums.eDbEventTypes.NewStore: return "#fea700";
                case DashboardEnums.eDbEventTypes.NewMailchimp: return "#f9c1b4";
                case DashboardEnums.eDbEventTypes.Custom: return "#d9c2ec";
                default: return "#d9c2ec";
            }
        }

        public static DateTime CompareToDate(this DateTime firstDate, DateTime secondDate)
        {
            var result = DateTime.Compare(firstDate, secondDate);
           
            return result < 0 ? secondDate : firstDate;
        }        
    }
}
