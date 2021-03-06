﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GatheringAPI.Models
{
    public class EventRepeat
    {
        public long EventRepeatId { get; set; }

        public string EventName { get; set; }

        public string Location { get; set; }

        public string Description { get; set; }

        public List<RepeatedEvent> IndividualEvents { get; set; }

        [Required]
        public Repeat ERepeat { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        public int DayOfMonth { get; set; }

        public MonthOfYear MonthOfYear { get; set; }

        public DateTime FirstEventDate { get; set; }

        public DateTime EndEventDate { get; set; }
    }

    public enum Repeat
    {
        Weekly,
        Monthly,
        Yearly,
        Once
    }

    public enum MonthOfYear
    {
        January,
        February,
        March,
        April,
        May,
        June,
        July,
        August,
        September,
        October,
        November,
        December
    }
}
