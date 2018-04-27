﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaylilyWeb.Models.CQResponse.Api
{
    public class GroupMemberInfo
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
        [JsonProperty(PropertyName = "retcode")]
        public int Retcode { get; set; }
        [JsonProperty(PropertyName = "data")]
        public _GroupMemberInfo Data { get; set; }
    }
    public class _GroupMemberInfo
    {
        [JsonProperty(PropertyName = "group_id")]
        public long GroupId { get; set; }
        [JsonProperty(PropertyName = "user_id")]
        public long UserId { get; set; }
        [JsonProperty(PropertyName = "nickname")]
        public string Nickname { get; set; }
        [JsonProperty(PropertyName = "card")]
        public string Card { get; set; }
        [JsonProperty(PropertyName = "sex")]
        public string Sex { get; set; }
        [JsonProperty(PropertyName = "age")]
        public int Age { get; set; }
        [JsonProperty(PropertyName = "area")]
        public string Area { get; set; }
        [JsonProperty(PropertyName = "join_time")]
        public DateTime JoinTime { get; set; }
        [JsonProperty(PropertyName = "last_sent_time")]
        public DateTime LastSentTime { get; set; }
        [JsonProperty(PropertyName = "level")]
        public string Level { get; set; }
        [JsonProperty(PropertyName = "role")]
        public string Role { get; set; }
        [JsonProperty(PropertyName = "unfriendly")]
        public bool Unfriendly { get; set; }
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "title_expire_time")]
        public DateTime TitleExpireTime { get; set; }
        [JsonProperty(PropertyName = "card_changeable")]
        public bool CardChangeable { get; set; }
    }
}