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