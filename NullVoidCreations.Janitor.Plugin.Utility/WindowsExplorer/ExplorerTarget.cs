﻿using System;
using System.Collections.Generic;
using NullVoidCreations.Janitor.Shared.Models;

namespace NullVoidCreations.Janitor.Plugin.System.WindowsExplorer
{
    public class ExplorerTarget: ScanTargetBase
    {
        public ExplorerTarget()
            : base("Windows Explorer", new Version("1.0.0.0"), DateTime.Now)
        {
            IconSource = "/NullVoidCreations.Janitor.Plugin.System;component/Resources/Explorer.png";

            var areas = new List<ScanAreaBase>()
            {
                new ExplorerAreaRecentDocuments(this),
                new ExplorerAreaThumbnailCache(this)
            };
            Areas = areas;
        }
    }
}
