﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GatheringAPI.Models.Api
{
    public class GroupDto
    {
        public long GroupId { get; set; }
        [Required]
        public string GroupName { get; set; }

        public string Description { get; set; }

        public string Location { get; set; }

        public List<GroupEventDto> GroupEvents { get; set; }

        public List<RepeatedEventDto> GroupRepeatedEvents { get; set; }

        public List<GroupUserDto> GroupUsers { get; set; }

        public List<JoinRequestDto> RequestsToJoin { get; set; }

        public long MaxUsers { get; set; }

        public long MaxEvents { get; set; }

        public UserDto Owner { get; set; }
    }

    public class GroupEventDto
    {
        public long EventId { get; set; }
        public string EventName { get; set; }

        public Repeat ERepeat { get; set; }

        public DateTime Start { get; set; }

        public DateTime? End { get; set; }

        public int DayOfMonth { get; set; }

        public decimal Cost { get; set; }

        public string Location { get; set; }
    }
}
