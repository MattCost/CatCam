This will hold deployment related files


To deploy edge modules, we can make a token
then use that token instead of the admin user

az acr token create --name MyToken --registry myregistry \
  --repository samples/hello-world \
  content/write content/read \
  --output json


az acr scope-map list -r catcamacr
az acr token create --name edge-deployments --registry catcamacr --scope-map _repositories_pull
edge-deployments user now has read-only access to entire acr.
could scope it down to edge modules.