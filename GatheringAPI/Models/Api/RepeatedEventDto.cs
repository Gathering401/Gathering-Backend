using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GatheringAPI.Models.Api
{
    public class RepeatedEventDto
    {
        public long EventId { get; set; }

        public Repeat ERepeat { get; set; }

        public string RepeatString { get; set; }

        public string EventName { get; set; }

        public int DayOfMonth { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public MonthOfYear MonthOfYear { get; set; }

        public DateTime FirstEventDate { get; set; }

        public DateTime EndEventDate { get; set; }
    }
}
