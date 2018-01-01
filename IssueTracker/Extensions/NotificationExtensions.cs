#region copyright 
//Copyright (c) 2014 The Board of Trustees of The University of Alabama
//All rights reserved.
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions
//are met:
//
//1. Redistributions of source code must retain the above copyright
//notice, this list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright
//notice, this list of conditions and the following disclaimer in the
//documentation and/or other materials provided with the distribution.
//3. Neither the name of the University nor the names of the contributors
//may be used to endorse or promote products derived from this software
//without specific prior written permission.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
//LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
//FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
//THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
//INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
//HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
//STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
//ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
//OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

using System;
using System.Collections.Generic;
using IssueTracker.Services.Services.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace IssueTracker.Web.Extensions
{
    /*                                                                      *
     *      This extension was derived from Brad Christie's answer          *
     *      on StackOverflow.                                               *
     *                                                                      *
     *      The original code can be found at:                              *
     *      http://stackoverflow.com/a/18338264/998328                      *
     *                                                                      */

    public static class NotificationExtensions
    {
        private static readonly IDictionary<NotificationType, string> NotificationKeys =
            new Dictionary<NotificationType, string>
            {
                { NotificationType.Error, "App.Notifications.Error" },
                { NotificationType.Warning, "App.Notifications.Warning" },
                { NotificationType.Success, "App.Notifications.Success" },
                { NotificationType.Info, "App.Notifications.Info" }
            };

        public static void AddNotification(this Controller controller, string message, NotificationType notificationType)
        {
            var tempData = controller.TempData;
            var notificationKey = GetNotificationKeyByType(notificationType);
            var messages = new HashSet<string>();

            if (tempData.ContainsKey(notificationKey))
            {
                messages = JsonConvert.DeserializeObject<HashSet<string>>(
                    tempData[notificationKey].ToString());
            }

            messages.Add(message);

            tempData[notificationKey] = JsonConvert.SerializeObject(messages);
        }

        public static IEnumerable<string> GetNotifications(this IHtmlHelper htmlHelper, NotificationType notificationType)
        {
            var tempData = htmlHelper.ViewContext.TempData;
            var notificationKey = GetNotificationKeyByType(notificationType);
            if (tempData.ContainsKey(notificationKey))
            {
                return JsonConvert.DeserializeObject<HashSet<string>>(
                    tempData[notificationKey].ToString());
            }

            return null;
        }

        private static string GetNotificationKeyByType(NotificationType notificationType)
        {
            try
            {
                return NotificationKeys[notificationType];
            }
            catch (IndexOutOfRangeException e)
            {
                var exception = new ArgumentException("Key is invalid", nameof(notificationType), e);
                throw exception;
            }
        }
    }


}