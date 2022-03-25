using System;
using System.IO;

namespace OpenTKVoxelEngine_Utils
{
    public static class Utility
    {

        /// <summary>
        /// Get and returns the application main root directory.
        /// </summary>
        /// <returns></returns>
        public static string GetRootDirectory()
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\..\"));
        }

        /// <summary>
        /// Get and return the application directory using the provided path.
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public static string GetRootDirectory(string directoryPath)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\..\", directoryPath));
        }

    }
}
