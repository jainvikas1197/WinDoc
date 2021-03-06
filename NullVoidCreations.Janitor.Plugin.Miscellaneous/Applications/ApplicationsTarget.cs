﻿using System;
using System.Collections.Generic;
using NullVoidCreations.Janitor.Shared.Base;

namespace NullVoidCreations.Janitor.Plugin.Miscellaneous.Applications
{
    public class ApplicationsTarget: ScanTargetBase
    {
        public ApplicationsTarget()
            : base("Applications", new Version("17.3.22.0"), new DateTime(2017, 3, 22))
        {
            IconSource = "pack://application:,,,/plugin_miscellaneous;component/Resources/Application22.png";

            var areas = new List<ScanAreaBase>()
            {
                new ApplicationsAreaFoxitReader(this),
                new ApplicationsAreaOffice(this),
            };
            Areas = areas;
        }
    }
}
