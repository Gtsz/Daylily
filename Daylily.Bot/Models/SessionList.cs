﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Daylily.Bot.Enum;
using Daylily.CoolQ.Interface.CqHttp;
using Daylily.CoolQ.Models.CqResponse;
using Daylily.CoolQ.Models.CqResponse.Api;

namespace Daylily.Bot.Models
{
    public class SessionList
    {
        public ConcurrentDictionary<Identity, SessionSettings> Sessions { get; } =
            new ConcurrentDictionary<Identity, SessionSettings>();

        public SessionSettings this[Identity identity] =>
            Sessions.ContainsKey(identity) ? Sessions[identity] : null;

        public void AddOrUpdateGroup(GroupInfoV2 info)
        {
            var item = new Identity(info.GroupId, MessageType.Group);
            if (Sessions.Keys.Contains(item))
                Sessions[item].Update(info);
            Sessions.TryAdd(item, new SessionSettings(info));
        }

        public void RemoveGroup(string groupId)
        {
            var item = new Identity(groupId, MessageType.Group);
            if (Sessions.Keys.Contains(item))
                Sessions.TryRemove(item, out _);
        }

        public void TryAdd(Msg message)
        {
            switch (message)
            {
                case PrivateMsg privateMsg:
                    {
                        var item = new Identity(privateMsg.UserId, MessageType.Private);
                        if (Sessions.Keys.Contains(item))
                            return;
                        Sessions.TryAdd(item, new SessionSettings(privateMsg));
                        break;
                    }
                case DiscussMsg discussMsg:
                    {
                        var item = new Identity(discussMsg.DiscussId, MessageType.Discuss);
                        if (Sessions.Keys.Contains(item))
                            return;
                        Sessions.TryAdd(item, new SessionSettings(discussMsg));
                        break;
                    }
                case GroupMsg groupMsg:
                    {
                        var item = new Identity(groupMsg.GroupId, MessageType.Group);
                        if (Sessions.Keys.Contains(item))
                            return;
                        Sessions.TryAdd(item, new SessionSettings(groupMsg));
                        break;
                    }
            }
        }

        public class SessionSettings
        {
            public string Id { get; private set; }
            public string Name { get; private set; }
            public MessageType MessageType { get; private set; }
            public int MsgLimit { get; private set; }
            public bool LockMsg { get; set; } = false; // 用于判断是否超出消息阀值
            public GroupInfoV2 GroupInfo { get; private set; }
            public StrangerInfo PrivateInfo { get; private set; }
            public ConcurrentQueue<object> MsgQueue { get; } = new ConcurrentQueue<object>();

            public SessionSettings(Msg message)
            {
                switch (message)
                {
                    case PrivateMsg privateMsg:
                        Id = privateMsg.UserId.ToString();
                        MessageType = MessageType.Private;
                        MsgLimit = 4;
                        PrivateInfo = UpdatePrivateInfo(privateMsg.UserId);
                        Name = PrivateInfo.Nickname;
                        break;
                    case DiscussMsg discussMsg:
                        Id = discussMsg.DiscussId.ToString();
                        MessageType = MessageType.Discuss;
                        MsgLimit = 10;
                        Name = "组" + Id; // todo
                        break;
                    case GroupMsg groupMsg:
                        Id = groupMsg.GroupId.ToString();
                        MessageType = MessageType.Group;
                        MsgLimit = 10;
                        GroupInfo = UpdateGroupInfo(groupMsg.GroupId);
                        Name = GroupInfo.GroupName;
                        break;
                }
            }

            public SessionSettings(GroupInfoV2 info)
            {
                Update(info);
            }

            public void Update(GroupInfoV2 info)
            {
                Id = info.GroupId.ToString();
                Name = info.GroupName;
                MessageType = MessageType.Group;
                MsgLimit = 10;
                GroupInfo = info;
            }

            private readonly object _taskLock = new object();
            private Task _task;

            public bool TryRun(Action action)
            {
                bool isTaskFree = _task == null || _task.IsCanceled || _task.IsCompleted;
                if (isTaskFree)
                {
                    lock (_taskLock)
                    {
                        isTaskFree = _task == null || _task.IsCanceled || _task.IsCompleted;
                        if (isTaskFree)
                        {
                            _task = Task.Run(action);
                        }
                    }
                }

                return isTaskFree;
            }

            private static StrangerInfo UpdatePrivateInfo(long id)
            {
                StrangerInfo obj;
                try
                {
                    obj = CqApi.GetStrangerInfo(id.ToString()).Data;
                }
                catch
                {
                    obj = InitInfo();
                }

                return obj ?? InitInfo();

                StrangerInfo InitInfo()
                {
                    var groupInfoV2 = new StrangerInfo
                    {
                        Nickname = id.ToString(),
                        UserId = id,
                        Age = "-1",
                        Sex = "unknown",
                    };
                    return groupInfoV2;
                }
            }

            private static GroupInfoV2 UpdateGroupInfo(long id)
            {
                GroupInfoV2 obj;
                try
                {
                    obj = CqApi.GetGroupInfoV2(id.ToString()).Data;
                }
                catch
                {
                    obj = InitInfo();
                }

                return obj ?? InitInfo();

                GroupInfoV2 InitInfo()
                {
                    var groupInfoV2 = new GroupInfoV2
                    {
                        GroupName = "群" + id,
                        GroupId = long.Parse(id.ToString()),
                        Admins = new List<GroupInfoV2Admins>(),
                    };
                    return groupInfoV2;
                }
            }
        }
    }
}
