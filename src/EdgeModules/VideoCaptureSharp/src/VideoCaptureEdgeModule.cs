using System.Configuration;
using System.Drawing;
using System.Reflection.Metadata;
using CatCam.EdgeCommon.Modules;
using CatCam.EdgeCommon.Services.Clients;
using Emgu.CV;
using Emgu.CV.Util;
using Microsoft.Extensions.Logging;

namespace CatCam.EdgeModules.VideoCaptureSharp;

public class VideoCaptureEdgeModule : EdgeModuleBase<VideoCaptureConfiguration>
{
    private Task _mainCaptureTask = Task.CompletedTask;

    private bool _motionDetected = false;
    private bool _captureActive = false;

    private string _clipFilename = string.Empty;

    private DateTime _motionDetectedAt;
    private DateTime _motionEndedAt;

    private TimeSpan _minMotionDuration = TimeSpan.FromSeconds(2);
    private TimeSpan _preMotionDuration = TimeSpan.FromSeconds(2);
    private TimeSpan _postMotionDuration = TimeSpan.FromSeconds(5);

    private VideoWriter? _videoWriter;

    private class BufferEntry
    {
        public Mat? Mat { get; set; }
        public DateTime Timestamp { get; set; }
    }
    private List<BufferEntry> _buffer = new();

    public VideoCaptureEdgeModule(ILogger<EdgeModuleBase<VideoCaptureConfiguration>> logger, IModuleClientWrapper moduleClient) : base(logger, moduleClient)
    {
    }

    public override async Task StartProcessing(CancellationToken token)
    {
        await base.StartProcessing(token);

        _mainCaptureTask = CaptureLoop(CancellationTokenSource.Token);
    }

    public override async Task StopProcessing(CancellationToken token)
    {
        await base.StopProcessing(token);

        try
        {
            await _mainCaptureTask;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Capture Loop produced exception");
        }

    }

    protected override async Task ProcessUpdatedConfiguration()
    {
        await Task.CompletedTask;
        try
        {
            _preMotionDuration = TimeSpan.FromSeconds(Configuration.PreMotionCaptureDurationSeconds);
            _postMotionDuration = TimeSpan.FromSeconds(Configuration.PostMotionCaptureDurationSeconds);
            _minMotionDuration = TimeSpan.FromSeconds(Configuration.MinMotionDurationSeconds);

            if (!Directory.Exists($"{Configuration.TempPath}/{Configuration.CameraName}"))
            {
                Directory.CreateDirectory($"{Configuration.TempPath}/{Configuration.CameraName}");
            }
            if (!Directory.Exists($"{Configuration.SavePath}/{Configuration.CameraName}"))
            {
                Directory.CreateDirectory($"{Configuration.SavePath}/{Configuration.CameraName}");
            }            
        }
        catch(Exception ex)
        {
            Logger.LogError(ex, "Unable to process updated configuration");
            throw;
            //If we emit a Fatal error event, the module will just restart adn get the same desired properties.... not sure what to do.
        }
    }

    private async Task CaptureLoop(CancellationToken token)
    {
        await Task.CompletedTask;
        try
        {
            using (Mat frame = new Mat())
            using (VideoCapture capture = new VideoCapture(Configuration.CameraUrl))
            {
                try
                {
                    var mog2 = new BackgroundSubtractorMOG2();
                    while (!token.IsCancellationRequested)
                    {
                        if (!capture.Read(frame))
                        {
                            Logger.LogError("Unable to capture frame");
                            OnFatalErrorOccurred(new Exception("Unable to capture frame"), "CaptureLoop failed");
                        }

                        // If not actively saving the video, place in buffer (then trim) incase motion is detected
                        var now = DateTime.UtcNow;
                        if (!_captureActive)
                        {
                            _buffer.Add(new BufferEntry { Mat = frame, Timestamp = now });
                            
                            var oldFrameCutoff = now - _preMotionDuration;
                            int i = 0;
                            while( _buffer[i].Timestamp < oldFrameCutoff) i++;
                            _buffer.RemoveRange(0,i);

                            // while (_buffer[0].Timestamp < saveNewerThan) //- now > _preMotionDuration)
                            // {
                            //     _buffer.RemoveAt(0);
                            // }
                        }

                        var anchor = new Point(-1, -1);
                        Mat gray = new();
                        CvInvoke.CvtColor(frame, gray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
                        Mat fgMask = new();
                        mog2.Apply(gray, fgMask);
                        Mat eroded = new();
                        Mat dilated = new();
                        var kernel = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Ellipse, new Size(5, 5), anchor);
                        CvInvoke.Erode(fgMask, eroded, kernel, anchor, 1, Emgu.CV.CvEnum.BorderType.Default, new Emgu.CV.Structure.MCvScalar(0)); 
                        // CvInvoke.Erode(fgMask, eroded, kernel, anchor, 1, Emgu.CV.CvEnum.BorderType.Constant, new Emgu.CV.Structure.MCvScalar(255, 255, 255)); //I think constant didn't work
                        CvInvoke.Dilate(fgMask, dilated, kernel, anchor, 1, Emgu.CV.CvEnum.BorderType.Default, new Emgu.CV.Structure.MCvScalar(0)); //not sure what to use for color (scalar arg)

                        var contours = new VectorOfVectorOfPoint();
                        var hierarchy = new Mat();

                        CvInvoke.FindContours(dilated, contours, hierarchy, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

                        // Logger.LogDebug("We have {Count} contours to process", contours.Size);
                        bool largeContour = false;
                        for (int i = 0; i < contours.Size; i++)
                        {
                            var area = CvInvoke.ContourArea(contours[i]);
                            // Logger.LogDebug("Contour {Iteration} - Area {Area}", i, area);
                            if (area > 1000)
                            {
                                largeContour = true;
                                Logger.LogDebug("Significant contour found. Contour {Iteration} Area {Area}", i, area);
                            }
                        }

                        if (largeContour && !_motionDetected)
                        {
                            Logger.LogDebug("Setting MotionDetected to true");
                            _motionDetectedAt = now;
                            _motionDetected = true;
                        }

                        if (_motionDetected && !largeContour)
                        {
                            Logger.LogDebug("Setting MotionDected to  false");
                            _motionEndedAt = now;
                            _motionDetected = false;
                        }

                        if (_captureActive)
                        {
                            Logger.LogDebug("Capture active - adding frame to temp file");

                            if (Configuration.IncludeTimestamp)
                            {
                                //include timestamp on image?
                                // ts = timestamp.strftime(activeConfig.displayTimestampFormat)
                                // cv2.putText(frame, ts, (10, frame.shape[0] - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.35, (0, 0, 255), 1)

                            }

                            if (Configuration.DrawBoundingBoxes)
                            {
                                // # Draw bounding box around contour
                                // x, y, w, h = cv2.boundingRect(contour)
                                // cv2.rectangle(frame, (x, y), (x+w, y+h), (0, 255, 0), 2)
                            }

                            if (_videoWriter?.IsOpened ?? false)
                            {
                                _videoWriter.Write(frame);
                            }
                            else
                            {
                                Logger.LogWarning("Capture Active is true, but Video Write is not opened!");
                            }


                            if (!_motionDetected)
                            {
                                if (now - _motionEndedAt > _postMotionDuration)
                                {
                                    Logger.LogDebug("Stopping capture. Completing video file, then moving to final location.");
                                    _captureActive = false;

                                    _videoWriter?.Dispose();

                                    File.Move(
                                        $"{Configuration.TempPath}/{Configuration.CameraName}/{_clipFilename}",
                                        $"{Configuration.SavePath}/{Configuration.CameraName}/{_clipFilename}");
                                }
                            }
                        }

                        if (_motionDetected)
                        {

                            if ((!_captureActive) && (now - _motionDetectedAt > _minMotionDuration))
                            {
                                Logger.LogDebug("Motion has been going on long enough. Creating temp file, dumping buffer to temp file. Clearing buffer");
                                _clipFilename = $"{now.ToString(Configuration.FilenameTimestampFormat)}{Configuration.FilenameExtension}";
                                var tempFilename = $"{this.Configuration.TempPath}/{Configuration.CameraName}/{_clipFilename}";

                                _videoWriter = new VideoWriter(tempFilename, VideoWriter.Fourcc('M', 'J', 'P', 'G'), 10, new Size(frame.Width, frame.Height), true);
                                if (_videoWriter.IsOpened)
                                {
                                    foreach (var _bufFrame in _buffer)
                                    {
                                        _videoWriter.Write(_bufFrame.Mat);
                                    }
                                    _buffer.Clear();
                                }
                                else
                                {
                                    Logger.LogError("Unable to open output file");
                                }
                                _captureActive = true;

                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Capture Loop Error");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                    OnFatalErrorOccurred(ex, "Can't see shit");
                }
                Logger.LogDebug("Capture Loop stopping");

            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Capture Loop Outer catch");
            OnFatalErrorOccurred(ex, "Fucked");
        }
    }

}