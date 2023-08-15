import asyncio
import sys
import signal
import threading
from azure.iot.device.aio import IoTHubModuleClient
import cv2
import os
import datetime
import shutil
# from .configs import ModuleConfig

class ModuleConfig:
    cameraUrl = 'http://192.168.50.22:8080/video'
    cameraName = 'camera'
    enableSaving = True

    minCaptureDuration = 2
    minContourArea = 1000
    postMotionCaptureDuration = 5
   
    debugLog = True
    displayTimestampFormat = "%A %d %B %Y %I:%M:%S%p"
    savePathBase = '/data/'
    filenameTimestampFormat = "%Y%m%d_%H%M%S"
    filenameExtension = ".avi"

# Globals
twinCallbacks = 0
stopProcessing = False

activeConfig = ModuleConfig()

# Event indicating client stop
stop_event = threading.Event()

def create_client():
    client = IoTHubModuleClient.create_from_edge_environment()
    client.on_twin_desired_properties_patch_received = receive_twin_patch_handler
    return client


async def receive_twin_patch_handler(twin_patch):
    global stopProcessing
    global twinCallbacks

    print("Twin Patch received")
    print("     {}".format(twin_patch))

    stopProcessing = True

    if "cameraUrl" in twin_patch:
        activeConfig.cameraUrl = twin_patch["cameraUrl"]
    if "cameraName" in twin_patch:
        activeConfig.cameraName = twin_patch["cameraName"]

    twinCallbacks += 1
    stopProcessing = False
    print("Total calls confirmed: {}".format(twinCallbacks))


async def run_sample(client):

    while True:
        cap = cv2.VideoCapture(activeConfig.cameraUrl) #, cv2.CAP_FFMPEG)
        dest = cv2.VideoWriter()

        if not cap.isOpened():
            print('Cannot open RTSP stream')
            exit(-1)

        # Create the MOG2 background subtractor object
        # Read about what this does, seems to do the average for us
        mog = cv2.createBackgroundSubtractorMOG2()

        # Initialize Motion Tracking info
        motionActive = False
        saveCapture = False
        motionStoppedAt = datetime.datetime.now()
        motionStartedAt = motionStoppedAt
        saveCaptureFilename = "/tmp/" + activeConfig.cameraName + motionStartedAt.strftime(activeConfig.filenameTimestampFormat) + activeConfig.filenameExtension

        while not stopProcessing:
            ret, frame = cap.read()
            if not ret:
                print('Error reading video stream')
                break
            
            timestamp = datetime.datetime.now()
            
            # Convert the frame to grayscale
            gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
                
            # then subtract the background
            fgmask = mog.apply(gray)
            
            # Apply morphological operations to reduce noise and fill gaps
            kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (5, 5))
            fgmask = cv2.erode(fgmask, kernel, iterations=1)
            fgmask = cv2.dilate(fgmask, kernel, iterations=1)
            
            # Find changes (edges?)
            contours, hierarchy = cv2.findContours(fgmask, cv2.RETR_EXTERNAL, cv2.CHAIN_APPROX_SIMPLE)
            
            for contour in contours:
                # Ignore small contours
                if cv2.contourArea(contour) < activeConfig.minContourArea:
                    continue
                
                # If there are some non-trivial contours, motion has been detected.
                
                # When transitioning to motionActive, save timestamp, generate filenames.
                if not motionActive:
                    motionStartedAt = timestamp
                    if activeConfig.enableSaving:
                        saveCaptureFilenameTmp = "/tmp/" + activeConfig.cameraName + motionStartedAt.strftime(activeConfig.filenameTimestampFormat) + activeConfig.filenameExtension
                        saveCaptureFilename = activeConfig.savePathBase + activeConfig.cameraName + motionStartedAt.strftime(activeConfig.filenameTimestampFormat) + activeConfig.filenameExtension
                        saveCapture = True
                        size = ( int(cap.get(3)), int(cap.get(4)))
                        dest = cv2.VideoWriter(saveCaptureFilenameTmp, cv2.VideoWriter_fourcc(*'MJPG'), 10, size)
                        if not dest.isOpened():
                            print("Error. Unable to open " + saveCaptureFilenameTmp)
                                
                # Finally enable motion tracking bit
                motionActive = True
                
                # Draw bounding box around contour
                x, y, w, h = cv2.boundingRect(contour)
                cv2.rectangle(frame, (x, y), (x+w, y+h), (0, 255, 0), 2)

            # If there were no contours, no more motion. so reset bit, and grab ending time.
            if motionActive and (len(contours) == 0):
                motionActive = False
                motionStoppedAt = timestamp
                    
            # Add formatted timestamp to frame
            ts = timestamp.strftime(activeConfig.displayTimestampFormat)
            cv2.putText(frame, ts, (10, frame.shape[0] - 10), cv2.FONT_HERSHEY_SIMPLEX, 0.35, (0, 0, 255), 1)

            # Display the frame to the GUI
            # cv2.imshow('Motion Detection', frame)
            

            if motionActive:
                if activeConfig.debugLog:
                    print("Motion Active. Started At " +
                        motionStartedAt.strftime(activeConfig.filenameTimestampFormat) + " Now " +
                        datetime.datetime.now().strftime(activeConfig.filenameTimestampFormat) )
                    
            
            # If Capture is active, save the frame to the file
            if saveCapture:
                if dest.isOpened():
                    if activeConfig.debugLog:
                        print("adding frame to " + saveCaptureFilenameTmp)
                    dest.write(frame)
                else:
                    print("Save capture is true, but dest file is not open :(")

            # Handle capturing a few seconds after motion is no longer detected
            if saveCapture and not motionActive:
                duration = datetime.datetime.now() - motionStartedAt;
                if duration < datetime.timedelta(seconds=activeConfig.minCaptureDuration):
                    saveCapture = False

                if activeConfig.debugLog:
                    print("saveCapture is true, but motionActive is false.")
                extraDuration = datetime.datetime.now() - motionStoppedAt;
                if(extraDuration > datetime.timedelta(seconds=activeConfig.postMotionCaptureDuration)):
                    if activeConfig.debugLog:
                        print("Post Capture duration elapsed. Stopping capture")
                    saveCapture = False;
                    dest.release()
                    # os.rename(saveCaptureFilenameTmp, saveCaptureFilename)
                    shutil.copyfile(saveCaptureFilenameTmp, saveCaptureFilename)
                    # flush file
            
            # Check for exit key
            # if cv2.waitKey(5) == ord('q'):
            #    break

        #end of while not stop processing           
        print("processing will restart.")
        cap.release()
        cv2.destroyAllWindows()
    
    # end of while true
    print("this will never happen")


def main():
    if not sys.version >= "3.5.3":
        raise Exception( "The sample requires python 3.5.3+. Current version of Python: %s" % sys.version )
    print ( "IoT Hub Client for Python" )

    # Define a handler to cleanup when module is is terminated by Edge
    def module_termination_handler(signal, frame):
        print ("IoTHubClient sample stopped by Edge")
        stop_event.set()

    # Set the Edge termination handler
    signal.signal(signal.SIGTERM, module_termination_handler)

    # Run the sample
    while True:
        loop = asyncio.get_event_loop()
        
        # NOTE: Client is implicitly connected due to the handler being set on it
        client = create_client()
        
        try:
            STOP_PROCESSING = False
            loop.run_until_complete(run_sample(client))
        except Exception as e:
            print("Unexpected error %s " % e)
            raise
        finally:
            print("Shutting down IoT Hub Client...")
            loop.run_until_complete(client.shutdown())
            loop.close()

if __name__ == "__main__":
    main()