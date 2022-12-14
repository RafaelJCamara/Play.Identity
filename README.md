# Play.Identity.Contracts
Identity contracts used by Play Economy services.

## Create and publish package
```powershell
$version="1.0.3"
$owner="RafaelJCamara"
$gh_pat="[PERSONAL ACCESS TOKEN HERE]"

dotnet pack src\Play.Identity.Contracts\ --configuration Release -p:PackageVersion=$version -p:RepositoryUrl=https://github.com/$owner/Play.Identity -o ..\packages

dotnet nuget push ..\packages\Play.Identity.Contracts.$version.nupkg --api-key $gh_pat --source "github"
```

## Build the docker image
```powershell
$version="1.0.3"
$env:GH_OWNER="RafaelJCamara"
$env:GH_PAT="[PERSONAL ACCESS TOKEN HERE]"

docker build --secret id=GH_OWNER --secret id=GH_PAT -t play.identity:$version .
```


## Run the docker image
```powershell
$version="1.0.3"
$adminPass="[PASSWORD HERE]"

docker run -it --rm -p 5002:5002 --name identity -e MongoDbSettings__Host=mongo -e RabbitMQSettings__Host=rabbitmq -e IdentitySettings__AdminUserPassWord=$adminPass --network playinfra_default play.identity:$version
```