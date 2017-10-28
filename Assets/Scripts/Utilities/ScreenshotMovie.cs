using UnityEngine;

public class ScreenshotMovie : MonoBehaviour
{
    // The folder we place all screenshots inside.
    // If the folder exists we will append numbers to create an empty folder.
    public string folder = "ScreenshotMovieOutput";
    public int frameRate = 25;
    public int sizeMultiplier = 1;

    private string realFolder = "";

    void Start()
    {
        // Set the playback framerate!
        // (real time doesn't influence time anymore)
        Time.captureFramerate = frameRate;

        // Find a folder that doesn't exist yet by appending numbers!
        realFolder = folder;
        int count = 1;
        while (System.IO.Directory.Exists(realFolder))
        {
            realFolder = folder + count;
            count++;
        }
        // Create the folder
        System.IO.Directory.CreateDirectory(realFolder);
    }

    void Update()
    {
        // name is "realFolder/shot 0005.png"
        var name = string.Format("{0}/shot {1:D04}.png", realFolder, Time.frameCount);

        // Capture the screenshot
        Application.CaptureScreenshot(name, sizeMultiplier);
    }
}
