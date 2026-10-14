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

        //depending on version, name can be "py" or "python", not sure if this variance will lead to issues
        //for python 3.14 "py" should work.
        public PythonBridge(string pythonExecutable = "py")
        {
            _pythonExecutable = pythonExecutable;

            _scriptPath = Path.Combine(AppContext.BaseDirectory, "Python", "capstone_NOAA.py");

            if(!File.Exists(_scriptPath))
            {
                throw new FileNotFoundException("Could not find the Python backend.", _scriptPath);
            }
        }

        //just for testing IPC works
        public async Task<string> TestConnectionAsync(string url)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _pythonExecutable,
                WorkingDirectory = AppContext.BaseDirectory,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            startInfo.ArgumentList.Add(_scriptPath);

            using var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();

            //task<> is asynchronous operation
            Task<string> errorTask = process.StandardError.ReadToEndAsync();

            //Send the URL to Python
            await process.StandardInput.WriteLineAsync(url);
            process.StandardInput.Close();
            //Read what Python prints
            string output = await process.StandardOutput.ReadToEndAsync();

            await process.WaitForExitAsync();

            string errors = await errorTask;

            if (string.IsNullOrWhiteSpace(output))
            {
                throw new Exception("Python returned no output. " + errors);
            }

            return output.Trim();
        }

        //public async Task<PythonGraphResult> GenerateGraphAsync(string url, string xColumn, string yColumn, string zColumn)
        //{
        //    var request = new
        //    {
        //        operation = "generate_graph",
        //        url = url,
        //        settings = new
        //        {
        //            x_column = xColumn,
        //            y_column = yColumn,
        //            z_column = zColumn
        //        }
        //    };

        //    var startInfo = new ProcessStartInfo
        //    {
        //        FileName = _pythonExecutable,
        //        WorkingDirectory = AppContext.BaseDirectory,
        //        RedirectStandardInput = true,
        //        RedirectStandardOutput = true,
        //        RedirectStandardError = true,
        //        UseShellExecute = false,
        //        CreateNoWindow = true
        //    };

        //    startInfo.ArgumentList.Add(_scriptPath);

        //    using var process = new Process
        //    {
        //        StartInfo = startInfo
        //    };

        //    process.Start();

        //    //Read errors concurrently (to avoid blocking python)
        //    Task<string> errorTask = process.StandardError.ReadToEndAsync();

        //    //Send the request to python
        //    string json = JsonSerializer.Serialize(request);
        //    await process.StandardInput.WriteLineAsync(json);
        //    await process.StandardInput.FlushAsync();
        //    process.StandardInput.Close();

        //    //Receive Python's JSON response
        //    string responseText = await process.StandardOutput.ReadToEndAsync();
        //    await process.WaitForExitAsync();
        //    string errors = await errorTask;

        //    if (string.IsNullOrWhiteSpace(responseText))
        //    {
        //        throw new Exception("Python returned no response. " + errors);
        //    }

        //    var result = JsonSerializer.Deserialize<PythonGraphResult>(responseText.Trim());

        //    if (result == null)
        //    {
        //        throw new Exception("Invalid response from Python.");
        //    }

        //    if (result.Status != "success")
        //    {
        //        throw new Exception(result.Message ?? "Python graph generation failed.");
        //    }

        //    if (string.IsNullOrWhiteSpace(result.OutputFile))
        //    {
        //        throw new Exception("Python did not return a graph file path.");
        //    }

        //    return result;

        //}
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
