﻿using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Blur.Processing
{
    public sealed class BlurTask : Task
    {
        [Required]
        public string TargetAssembly { get; set; }

        [Required]
        public string TargetReferences { get; set; }

        [Required]
        public string TargetPath { get; set; }

        public override bool Execute()
        {
            try
            {
                AssemblyResolver.References = TargetReferences.Split(';');

                Processor.Process(Path.GetFullPath(TargetAssembly), TargetPath);
            }
            catch (Exception e)
            {
                if (e is TargetInvocationException && e.InnerException != null)
                    e = e.InnerException;

                StringBuilder msg = new StringBuilder();

                while (e != null)
                {
                    msg.AppendLine(e.Message);
                    msg.AppendLine(e.StackTrace);
                    msg.AppendLine();

                    e = e.InnerException;
                }

                string targetName = e.TargetSite == null
                        ? "Unknown"
                        : $"{e.TargetSite.DeclaringType.FullName}.{e.TargetSite.Name}";

                BuildEngine.LogErrorEvent(new BuildErrorEventArgs("Error", e.HResult.ToString(), e.Source, 0, 0, 0, 0, msg.ToString(), e.HelpLink, targetName));

                return false;
            }

            return true;
        }
    }
}