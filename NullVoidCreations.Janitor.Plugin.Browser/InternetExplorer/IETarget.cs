﻿using System;
using NullVoidCreations.Janitor.Shared.Models;
using System.Collections.Generic;

namespace NullVoidCreations.Janitor.Plugin.Browser.InternetExplorer
{
    public class IETarget: ScanTargetBase
    {
        public IETarget()
            : base("Internet Explorer", new Version("1.0.0.0"), DateTime.Now)
        {
            IconSource = "/NullVoidCreations.Janitor.Plugin.Browser;component/Resources/InternetExplorer.png";

            var areas = new List<ScanAreaBase>()
            {
                new IEAreaTemporaryInternetFiles(this),
                new IEAreaHistory(this),
                new IEAreaCookies(this)
            };
            Areas = areas;
        }

    }
}