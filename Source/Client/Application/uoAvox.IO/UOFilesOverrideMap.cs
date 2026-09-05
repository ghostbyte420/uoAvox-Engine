using uoAvox.Utility.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace uoAvox.IO
{
    public class UOFilesOverrideMap : Dictionary<string, string>
    {
        public static string OverrideFile { get; set; }

        public static UOFilesOverrideMap Instance { get; private set; } = new UOFilesOverrideMap();

        private UOFilesOverrideMap() : base(StringComparer.InvariantCultureIgnoreCase)
        {
        }

        public void Load()
        {
            if (!Path.Exists(OverrideFile))
            {
                Log.Trace($"No Override File or Directory found, ignoring.");
                return; // if the file doesn't exist then we ignore
            }

            if (Path.HasExtension(OverrideFile))
            {
                Log.Trace($"Loading Override File:\t\t{OverrideFile}");

                using (FileStream stream = new FileStream(OverrideFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (StreamReader reader = new StreamReader(stream))
                {
                    // we will gracefully ignore any failures when trying to read
                    while (!reader.EndOfStream)
                    {
                        try
                        {
                            string line = reader.ReadLine();
                            string testCommentLine = line.TrimStart(' ');
                            if (testCommentLine.IndexOf(';') == 0 || testCommentLine.IndexOf('#') == 0)
                                continue; // skip comment lines aka ; or #
                            string[] segments = line.Split('=');
                            if (segments.Length == 2)
                            {
                                string fileName = segments[0];
                                string filePath = segments[1];

                                Log.Trace($"Override entry: {fileName} => {filePath}.");

                                this[fileName] = filePath;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Warn("Something went wrong when trying to parse UOFileOverride file.");
                            Log.Warn(ex.ToString());
                        }
                    }
                }
            }
            else
            {
                Log.Trace($"Loading Override Directory:\t{OverrideFile}");

                try
                {
                    IEnumerable<string> files = Directory.EnumerateFiles(OverrideFile, "*.*", SearchOption.AllDirectories);

                    foreach (string filePath in files)
                    {
                        string fileName = Path.GetFileName(filePath);

                        Log.Trace($"Override entry: {fileName} => {filePath}.");

                        this[fileName] = filePath;
                    }
                }
                catch (Exception ex)
                {
                    Log.Warn("Something went wrong when trying to parse UOFileOverride folder.");
                    Log.Warn(ex.ToString());
                }
            }
        }
    }
}