cd ..
patch=$(cat src/EdgeModules/VideoCaptureSharp/patch.txt)
((patch++))
echo "patch $patch"
echo $patch > src/EdgeModules/VideoCaptureSharp/patch.txt
TOKEN=$(az acr login --name catcamacr --expose-token --output tsv --query accessToken)
# docker build --tag catcamacr.azurecr.io/video-capture-sharp:0.0.$patch-local-amd64 -f EdgeModules/VideoCaptureSharp/Dockerfile.amd64 src
docker build --tag catcamacr.azurecr.io/video-capture-sharp:0.0.$patch-local-arm32v7 -f EdgeModules/VideoCaptureSharp/Dockerfile.arm32v7 src
docker login catcamacr.azurecr.io --username 00000000-0000-0000-0000-000000000000 --password-stdin <<< $TOKEN
docker push catcamacr.azurecr.io/video-capture-sharp:0.0.$patch-local-arm32v7