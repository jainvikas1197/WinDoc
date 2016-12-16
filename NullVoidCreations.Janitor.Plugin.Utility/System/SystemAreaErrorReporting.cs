﻿using System.Collections.Generic;
using System.IO;
using NullVoidCreations.Janitor.Shared.Helpers;
using NullVoidCreations.Janitor.Shared.Models;

namespace NullVoidCreations.Janitor.Plugin.System.System
{
    public class SystemAreaErrorReporting: ScanAreaBase
    {
        public SystemAreaErrorReporting(ScanTargetBase target)
            : base("Windows Error Reporting", target)
        {

        }

        public override List<Issue> Analyse()
        {
            var paths = new string[]
            {
                Path.Combine(KnownPaths.Instance.ProgramDataDirectory , @"Microsoft\Windows\WER")
            };

            Issues.Clear();
            foreach (var path in paths)
                foreach (var file in new DirectoryWalker(path, IncludeFile))
                    Issues.Add(new Issue(Target, this, file));

            return Issues;
        }

        public override List<Issue> Fix()
        {
            return null;
        }

        bool IncludeFile(string path)
        {
            return new FileInfo(path).Extension.Equals(".wer");
        }
    }
}