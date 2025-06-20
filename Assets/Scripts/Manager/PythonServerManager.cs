using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using UnityEngine;

public class PythonServerManager : MonoBehaviour
{
    private Process pythonProcess;

    void Start()
    {
        if (IsServerRunning())
        {
            UnityEngine.Debug.Log("Flask server is already running.");
            return;
        }
        else
        {
            LaunchPythonServer();
        }

    }    


    // === Launch the flash python server ===
    private void LaunchPythonServer()
    {
        // Path to Python interpreter.
        string pythonExe = "python";

        // Construct the absolute path to the Flask server script.
        string scriptPath = Path.Combine(Application.streamingAssetsPath, "AprilTagServer", "AprilTagServer.py");

        // Setup process start configuration
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = pythonExe,
            Arguments = $"\"{scriptPath}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        // Initialize the process
        pythonProcess = new Process();
        pythonProcess.StartInfo = startInfo;

        // Hook up event handlers for logging
        pythonProcess.OutputDataReceived += (sender, args) => UnityEngine.Debug.Log($"[Python]: {args.Data}");
        pythonProcess.ErrorDataReceived += (sender, args) => UnityEngine.Debug.Log($"[Python ERROR]: {args.Data}");

        pythonProcess.Start();
        pythonProcess.BeginOutputReadLine();
        pythonProcess.BeginErrorReadLine();

        UnityEngine.Debug.Log("Flask server launched from Unity.");
    }

    // Check if the Flask server is already running ===
    private bool IsServerRunning()
    {
        try
        {
            using (WebClient client = new WebClient())
            {
                string result = client.DownloadString("http://127.0.0.1:5000/ping");
                return result == "pong";
            }
        }
        catch
        {
            return false;
        }
    }

    void OnApplicationQuit()
    {
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            pythonProcess.Kill();
            UnityEngine.Debug.Log("Flask server stopped.");
        }
    }
}
