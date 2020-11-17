﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lunar.Extensions;

namespace Lunar.FileResolution
{
    internal sealed class FileResolver
    {
        private readonly Process _process;

        private readonly string? _rootDirectoryPath;

        internal FileResolver(Process process, string? rootDirectoryPath)
        {
            _process = process;

            _rootDirectoryPath = rootDirectoryPath;
        }

        internal string? ResolveFilePath(ActivationContext activationContext, string fileName)
        {
            // Search the manifest

            var sxsFilePath = activationContext.ProbeManifest(fileName);

            if (sxsFilePath is not null)
            {
                return sxsFilePath;
            }

            // Search the directory from which the process was loaded

            var processDirectoryFilePath = Path.Combine(Path.GetDirectoryName(_process.MainModule!.FileName)!, fileName);

            if (File.Exists(processDirectoryFilePath))
            {
                return processDirectoryFilePath;
            }

            // Search the System directory

            var systemDirectoryFilePath = Path.Combine(_process.GetSystemDirectoryPath(), fileName);

            if (File.Exists(systemDirectoryFilePath))
            {
                return systemDirectoryFilePath;
            }

            // Search the Windows directory

            var windowsDirectoryFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), fileName);

            if (File.Exists(windowsDirectoryFilePath))
            {
                return windowsDirectoryFilePath;
            }

            // Search the root directory

            if (_rootDirectoryPath is not null)
            {
                var rootDirectoryFilePath = Path.Combine(_rootDirectoryPath, fileName);

                if (File.Exists(rootDirectoryFilePath))
                {
                    return rootDirectoryFilePath;
                }
            }

            // Search the directories listed in the PATH environment variable

            var path = Environment.GetEnvironmentVariable("PATH");

            return path?.Split(";").Where(Directory.Exists).Select(directory => Path.Combine(directory, fileName)).FirstOrDefault(File.Exists);
        }
    }
}