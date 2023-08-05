cd ..
patch=$(cat src/EdgeModules/VideoCapture/patch.txt)
((patch++))
echo "patch $patch"
echo $patch > src/EdgeModules/VideoCapture/patch.txt
TOKEN=$(az acr login --name catcamacr --expose-token --output tsv --query accessToken)
docker build --tag catcamacr.azurecr.io/video-capture:0.0.$patch-local-amd64 -f Dockerfile.amd64 src/EdgeModules/VideoCapture/
docker build --tag catcamacr.azurecr.io/video-capture:0.0.$patch-local-arm32v7 -f Dockerfile.arm32v7 src/EdgeModules/VideoCapture/
docker login catcamacr.azurecr.io --username 00000000-0000-0000-0000-000000000000 --password-stdin <<< $TOKEN
docker push catcamacr.azurecr.io/video-capture:0.0.$patch-local-amd64
docker push catcamacr.azurecr.io/video-capture:0.0.$patch-local-arm32v7