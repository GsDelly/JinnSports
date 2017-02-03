﻿using PredictorBalancer.Models;
using PredictorDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace PredictorBalancer.Controllers
{
    public class BalancerController : ApiController
    {
        private BalancerMonitor instance;

        public void PostPackage([FromBody]PackageDTO package)
        {
            this.instance = BalancerMonitor.GetInstance();
            // TODO: get app url
            //this.instance.SendIncomingEvents(package, RequestContext.VirtualPathRoot);
            this.instance.SendIncomingEvents(package, $"http://localhost:7611");
        }
    }
}