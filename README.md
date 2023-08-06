# CatCam

Private Cloud Based Webcam platform.

Want webcams to capture motion, and notify you? but want to use your own private cloud resources?

This project helps you build your own private cloud based security cam setup.
Edge devices with webcams run OpenCV to capture motion clips.
When motion is detected the clip is saved locally on the edge device. 
When a clip is saved, an IOT Message is generated and broadcast.
Clips can be uploaded on demand
Optionally the clip can always be uploaded to azure storage.

Webapp to view all saved clips. Stream from azure storage. Stream from edge device (live view)


# Repo Layout

```
/deploy - deploy code to app service, deploy edge modules to devices, etc
/infrastructure - contains everything needed to build the azure resources.
/pipelines - azure devops pipeline files
/src - all application source code
/scripts - local dev scripts
```


# Dev Notes
Ongoing dev notes
## HTTPS
https is broken using default dotnet dev certs on linux.
issues with openssl not trusting the self-signed cert, unless a bit is set.
https://stackoverflow.com/questions/55485511/how-to-run-dotnet-dev-certs-https-trust

git hub issues, wasted hours on it.

https://github.com/BorisWilhelms/create-dotnet-devcert/tree/main
https://blog.wille-zone.de/post/aspnetcore-devcert-for-ubuntu
this didn't fully work


TODO - try this
https://fedoramagazine.org/set-up-a-net-development-environment/

ref
https://docs.fedoraproject.org/en-US/quick-docs/using-shared-system-certificates/

Resources needed.
Iothub
Storage act for vids
cosmos db? table storage?
azure ad / app reg


## TODO
- app roles and auth handlers needed? not for prototype
- iothub client wrapper
    - list devices
    - generate deployment
    - send deployment
- outline apis needed
- can we automate app service secret/env var configuration from terraform?
- create tf "bootstamp" script to create SP + storage act.
- setup tf to use remote state in said storage act.
- define site/location/camera structure

## Secrets
dotnet user-secrets init
dotnet user-secrets set "Movies:ServiceApiKey" "12345"

## Camera interface notes
opencv can open rtsp on pi, seems slow.


## Live Stream Mode
Camera connected to pi
webapp submits request to api "start livestream"
api opens websocket
api sends response to webapp with websocket port number.
api invokes direct method to pi "direct stream pls"
pi streams to websocket
browser pulls from websocket

