#if UNITY_EDITOR
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace FlatBuffers
{
    /// <summary>
    /// process runner
    /// </summary>
    public class ProcessRunner
    {
        /// <summary>
        /// flatc path
        /// </summary>
        private readonly string _flatcPath;

        private bool IsFlatcValid => !string.IsNullOrEmpty(_flatcPath) && File.Exists(_flatcPath);

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="flatcPath"></param>
        public ProcessRunner(
            string flatcPath)
        {
            _flatcPath = flatcPath;
        }

        /// <summary>
        /// run
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public string Run(
            List<string> inputs,
            params EOption[] options)
        {
            if (inputs == null || inputs.Count == 0) return "";
            if (!IsFlatcValid) return $"flatc not found, please check flatc path again!. Current file path is {_flatcPath}";

            var arguments = MakeArguments(inputs, options);
            UnityEngine.Debug.Log($"Run flatc with args: {arguments}");
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.GetFullPath(_flatcPath),
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            var started = process.Start();
            if (!started) return "Process flatc was not started!";

            var errors = new StringBuilder();
            while (!process.StandardOutput.EndOfStream)
            {
                var line = process.StandardOutput.ReadLine();
                UnityEngine.Debug.LogError(line);
                errors.Append(line);
            }

            process.WaitForExit();
            return errors.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputs"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        private string MakeArguments(
            List<string> inputs,
            IEnumerable<EOption> option)
        {
            var outputPath = string.IsNullOrEmpty(FlatDataEditorUtilities.Preferences.outputPath)
                ? Path.GetDirectoryName(inputs.First())
                : FlatDataEditorUtilities.Preferences.outputPath;

            var builder = new StringBuilder();
            foreach (var eOption in option)
            {
                builder.Append($" {eOption.ArgumentData()}");
            }

            builder.Append($" -o {outputPath}");
            inputs.ForEach(input => builder.Append($" {input}"));
            return builder.ToString();
        }
    }
}

#endif