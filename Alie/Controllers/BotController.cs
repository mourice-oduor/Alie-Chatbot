// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.11.1

using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using System.Threading.Tasks;
using AdaptiveCards;
using Alie.Dialogs.Details;
using Alie.Dialogs.Operations;
using Alie.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System;

namespace Alie.Controllers
{
    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter Adapter;
        private readonly IBot Bot;

        public BotController(IBotFrameworkHttpAdapter adapter, IBot bot)
        {
            Adapter = adapter;
            Bot = bot;
        }

        [HttpPost, HttpGet]
        public async Task PostAsync()
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await Adapter.ProcessAsync(Request, Response, Bot);
        }

        //[HttpPost]
        //public IActionResult Create(Validations validations)
        //{
        //    string message = "";

        //    if (ModelState.IsValid)
        //    {
        //        //message = "product " + validations.Name + " created successfully";
        //    }
        //    else
        //    {
        //        return View(validations);
        //    }
        //    return Content(message);
        //}

        //private IActionResult View(Validations validations)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
