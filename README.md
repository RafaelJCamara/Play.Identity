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
$version="1.0.6"
$env:GH_OWNER="RafaelJCamara"
$env:GH_PAT="[PERSONAL ACCESS TOKEN HERE]"
$appname="playeconomy"

docker build --secret id=GH_OWNER --secret id=GH_PAT -t "$appname.azurecr.io/play.identity:$version" .
```

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
# Each microservice should have its own namespace (set of resources for the microservice)
```powershell
$namespace="identity"
kubectl create namespace $namespace
```

## Create the Kubernetes secrets
```powershell
kubectl create secret generic identity-secrets 
--from-literal=cosmosdb-connectionstring=$cosmosDbConnString
--from-literal=servicebus-connectionstring=$serviceBusConnString
--from-literal=admin-password=$adminPass -n $namespace
```

## Create the Kubernetes pod

```powershell
kubectl apply -f .\kubernetes\identity.yaml -n $namespace
```

## Creating the Azure Managed Identity and granting it access to Key Vault secrets
```powershell
az identity create --resource-group $appname --name $namespace
$IDENTITY_CLIENT_ID=az identity show -g $appname -n $namespace --query clientId -otsv
# creates in key vault this policy with the name identity (or whatever is on the appname) and everyone this a part of this only has the permissions of "get" and "set"
az keyvault set-policy -n $appname --secret-permissions get list --spn $IDENTITY_CLIENT_ID
```

## Establish the federated identity credential
```powershell
$AKS_OIDC_ISSUER=az aks show -n $appname -g $appname --query "oidcIssuerProfile.issuerUrl" -otsv

az identity federated-credential create --name $namespace --identity-name $namespace --resource-group $appname --issuer $AKS_OIDC_ISSUER --subject "system:serviceaccount:${namespace}:${namespace}-serviceaccount"
```