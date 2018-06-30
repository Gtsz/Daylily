﻿using System;
using System.Collections.Generic;
using System.Text;
using Daylily.Common.Assist;
using Daylily.Common.Models.CQResponse;
using Newtonsoft.Json;

namespace Daylily.Common.Function
{
    public static class JsonHandler
    {
        public static object HandleReportJson(string json)
        {
            dynamic obj = JsonConvert.DeserializeObject(json);
            // 判断post类别
            if (obj.post_type == "message")
            {
                // 私聊
                if (obj.message_type == "private")
                {
                    PrivateMsg parsedObj = JsonConvert.DeserializeObject<PrivateMsg>(json);
                    //PrivateMsg parsedObj = obj as PrivateMsg;
                    try
                    {
                        MessageHandler privateHandler = new MessageHandler(parsedObj);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteException(ex);
                    }
                }

                //群聊
                else if (obj.message_type == "group")
                {
                    GroupMsg parsedObj = JsonConvert.DeserializeObject<GroupMsg>(json);
                    //GroupMsg parsedObj = obj as GroupMsg;
                    try
                    {
                        MessageHandler groupHandler = new MessageHandler(parsedObj);
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteException(ex);
                    }
                }

                //讨论组
                else if (obj.message_type == "discuss")
                {
                    DiscussMsg parsedObj = JsonConvert.DeserializeObject<DiscussMsg>(json);
                    //DiscussMsg parsedObj = (DiscussMsg)obj;
                    try
                    {
                        MessageHandler discussHandler = new MessageHandler(parsedObj);
                        //group_handler.HandleMessage();
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteException(ex);
                        //GroupMsgResponse group_resp = new GroupMsgResponse()
                        //{
                        //    reply = ex.Message,
                        //    auto_escape = false,
                        //    at_sender = true,
                        //    delete = false,
                        //    kick = false,
                        //    ban = false
                        //};
                        //return Json(group_resp);
                    }
                }
            }
            else if (obj.post_type == "event")
            {
                // TODO
            }
            else if (obj.post_type == "request")
            {
                // TODO
            }

            return null;
        }
    }
}
