cd ..
patch=$(cat src/EdgeModules/FileWatcher/patch.txt)
((patch++))
echo "patch $patch"
echo $patch > src/EdgeModules/FileWatcher/patch.txt
TOKEN=$(az acr login --name catcamacr --expose-token --output tsv --query accessToken)
docker build --tag catcamacr.azurecr.io/file-watcher:0.0.$patch-local-amd64 -f EdgeModules/FileWatcher/Dockerfile.amd64 src
docker build --tag catcamacr.azurecr.io/file-watcher:0.0.$patch-local-arm32v7 -f EdgeModules/FileWatcher/Dockerfile.arm32v7 src
docker login catcamacr.azurecr.io --username 00000000-0000-0000-0000-000000000000 --password-stdin <<< $TOKEN
docker push catcamacr.azurecr.io/file-watcher:0.0.$patch-local-amd64
docker push catcamacr.azurecr.io/file-watcher:0.0.$patch-local-arm32v7