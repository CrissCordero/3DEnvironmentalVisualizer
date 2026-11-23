using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;

namespace UIPrototype
{
    internal class PythonBridge
    {
        private readonly string _pythonExecutable;
        private readonly string _scriptPath;

        //helper function to find UIPrototype\UIPrototype\Python no matter where repo is called.
        internal static string ProjectPythonFolder()
        {
            string? dir = AppContext.BaseDirectory;
            while (!string.IsNullOrEmpty(dir))
            {
                if (Directory.GetFiles(dir, "*.csproj").Length > 0)
                {
                    return Path.Combine(dir, "Python");
                }

                dir = Path.GetDirectoryName(dir.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            }

            //for when not running from inside a project (like a final copy), use copied folder next to .exe
            return Path.Combine(AppContext.BaseDirectory, "Python");
        }

        //depending on version, name can be "py" or "python", not sure if this variance will lead to issues
        //for python 3.14 "py" should work.
        public PythonBridge(string pythonExecutable = "py")
        {
            _pythonExecutable = pythonExecutable;

            _scriptPath = Path.Combine(ProjectPythonFolder(), "capstone_3Dtest_pyVista.py");

            if(!File.Exists(_scriptPath))
            {
                throw new FileNotFoundException("Could not find the Python backend at: " + _scriptPath, _scriptPath);
            }
        }

        public async Task<PythonGraphResult> GenerateGraphAsync(string csvPath, double[] depths, double lomin, double lomax, double lamin, double lamax, int stride, int nColors)
        {
            string outputFile = Path.Combine(Path.GetTempPath(), $"woa_{Guid.NewGuid():N}.html");

            var request = new
            {
                operation = "generate_graph",
                settings = new
                {
                    csv_path = csvPath,
                    depths,
                    lomin,
                    lomax,
                    lamin,
                    lamax,
                    stride,
                    n_colors = nColors,
                    output_file = outputFile
                }
            };

            var startInfo = new ProcessStartInfo
            {
                FileName = _pythonExecutable, WorkingDirectory = AppContext.BaseDirectory,
                RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true,
                UseShellExecute = false, CreateNoWindow = true
            };
            startInfo.ArgumentList.Add(_scriptPath);

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            Task<string> errorTask = process.StandardError.ReadToEndAsync();

            await process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(request));
            process.StandardInput.Close();

            string responseText = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();
            string errors = await errorTask;

            if (string.IsNullOrWhiteSpace(responseText))
                throw new Exception("Python returned no response. " + errors);

            var result = JsonSerializer.Deserialize<PythonGraphResult>(responseText.Trim())
                ?? throw new Exception("Invalid response from Python.");

            if (result.Status != "success")
                throw new Exception(result.Message ?? "Python graph generation failed.");
            if (string.IsNullOrWhiteSpace(result.OutputFile))
                throw new Exception("Python did not return a graph file path");

            return result;
        }
        
    }

    internal class PythonGraphResult
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("output_file")]
        public string? OutputFile { get; set; } = "";

        [JsonPropertyName("message")]
        public string? Message { get; set; } = "";


    }
}
