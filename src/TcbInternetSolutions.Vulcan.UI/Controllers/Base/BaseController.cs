﻿using System.Web.Mvc;
using TcbInternetSolutions.Vulcan.Core;

namespace TcbInternetSolutions.Vulcan.UI.Controllers.Base
{
    public abstract class BaseController : Controller
    {
        protected BaseController(IVulcanHandler vulcanHandler)
        {
            VulcanHandler = vulcanHandler;
        }

        public IVulcanHandler VulcanHandler { get; }
    }
}
