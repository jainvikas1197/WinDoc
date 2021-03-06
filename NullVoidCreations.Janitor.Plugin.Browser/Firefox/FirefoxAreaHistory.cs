﻿using System.Collections.Generic;
using System.IO;
using NullVoidCreations.Janitor.Shared.Base;
using NullVoidCreations.Janitor.Shared.Helpers;
using NullVoidCreations.Janitor.Shared.Models;

namespace NullVoidCreations.Janitor.Plugin.Browser.Firefox
{
    public class FirefoxAreaHistory : ScanAreaBase
    {
        public FirefoxAreaHistory(ScanTargetBase target)
            : base("Internet History", target)
        {

        }

        public override IEnumerable<IssueBase> Analyse()
        {
            var profiles = (Target as FirefoxTarget).Profiles;

            var paths = new List<string>();
            foreach (var profile in profiles)
            {
                paths.Add(Path.Combine(KnownPaths.Instance.AppDataLocal, string.Format(@"Mozilla\Firefox\Profiles\{0}\thumbnails", profile)));
            }

            Issues.Clear();
            foreach (var directory in paths)
            {
                foreach (var file in new DirectoryWalker(directory))
                {
                    var issue = new FileIssueModel(Target, this, file);
                    Issues.Add(issue);
                    yield return issue;
                }
            }
        }
    }
}
