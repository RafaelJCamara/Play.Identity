# Play.Identity
Identity microservice.

## Create and publish Identity.Contracts NuGet package
```powershell
$version="1.0.4"
$owner="RafaelJCamara"
$gh_pat="[PERSONAL ACCESS TOKEN HERE]"

dotnet pack src\Play.Identity.Contracts\ --configuration Release -p:PackageVersion=$version -p:RepositoryUrl=https://github.com/$owner/Play.Identity -o ..\packages

dotnet nuget push ..\packages\Play.Identity.Contracts.$version.nupkg --api-key $gh_pat --source "github"
```

## Build the docker image
```powershell
$version="1.0.5"
$env:GH_OWNER="RafaelJCamara"
$env:GH_PAT="[PERSONAL ACCESS TOKEN HERE]"
$appname="playeconomy"

docker build --secret id=GH_OWNER --secret id=GH_PAT -t "$appname.azurecr.io/play.identity:$version" .
```

;
## Run the docker image
```powershell
$version="1.0.5"
$adminPass="[PASSWORD HERE]"
$cosmosDbConnString="[CONN STRING HERE]"
$serviceBusConnString="[CONN STRING HERE]"

docker run -it --rm -p 5002:5002 --name identity -e MongoDbSettings__ConnectionString=$cosmosDbConnString -e ServiceBusSettings__ConnectionString=$serviceBusConnString -e ServiceSettings__MessageBroker="SERVICEBUS" -e IdentitySettings__AdminUserPassWord=$adminPass play.identity:$version
```


## Publish docker image
```powershell
az acr login --name $appname
docker push "$appname.azurecr.io/play.identity:$version"
```

## Create the Kubernetes namespace
# Creates namespace for resources tht would be used in the identity pod 
```powershell
$namespace="identity"
kubectl create namespace $namespace
```
