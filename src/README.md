API
    The backend api service

webapp
    A webui for viewing clips
    
Common
    Models and classes needed between multiple apps

EdgeModules
    VideoCapture
        python, uses opencv to save motion clips to mounted storage volume

    FileWatcher
        c#, watches mounted storage volume to upload files to azure blob storage

    local build - using script file, tags with latest.
    pipeline build - tags with pipeline maintained version