function AzDevOpsLogin() {   
    Write-Host "=========================================="
    Write-Host "AzDevOpsLogin"  
    Write-Host "==========================================" 
    az login --allow-no-subscriptions 
    Write-Host  
}

function AzDevOpsInitCli() {
    Write-Host "=========================================="
    Write-Host "AzDevOpsInitCli :  $global:url > $global:projet"  
    Write-Host "==========================================" 
    az extension add --name azure-devops 
    az devops configure -d organization=$global:url project=$global:projet
    az devops configure -l  
    Write-Host 
}

function AzDevOpsPipelinesList() { 
    Write-Host "=========================================="
    Write-Host "AzDevOpsPipelinesList"
    Write-Host "=========================================="     
    az pipelines build definition list -o table  
    Write-Host  
}

function AzDevOpsPipelinesInitQueueId() {
    Write-Host "=========================================="
    Write-Host "AzDevOpsPipelinesInitQueueId : '$global:queue'"  
    Write-Host "==========================================" 
    $response = az pipelines queue list -o json --query "[?name=='$global:queue']" | ConvertFrom-Json
    $global:queueId = $response.id
    Write-Host "AzDevOpsPipelinesInitQueueId : $global:queueId"   
}

function AzDevOpsPipelinesInitServiceConnectionId() {
    Write-Host "=========================================="
    Write-Host "AzDevOpsPipelinesInitServiceConnectionId : '$global:serviceConnection'"  
    Write-Host "==========================================" 
    $response = az devops service-endpoint list -o json --query "[?name=='$global:serviceConnection']" | ConvertFrom-Json
    $global:serviceConnectionId = $response.id
    Write-Host "AzDevOpsPipelinesInitServiceConnectionId : $global:serviceConnectionId"   
}


function AzDevOpsPipelinesCreate(
    [Parameter(Mandatory = $True)][ValidateNotNullOrEmpty()][string ] $name ) {
    Write-Host "=========================================="
    Write-Host "AzDevOpsPipelinesCreate : $name"  
    Write-Host "==========================================" 
    az pipelines create `
        --name "nuget - $name"   `
        --org $global:url   `
        --project $global:projet    `
        --queue-id $global:queueId    `
        --service-connection $global:serviceConnectionId    `
        --repository $global:repository  `
        --repository-type "GitHub" `
        --yaml-path "tools/devops/nuget-$name-pipeline.yml" `
        --yaml-path "tools/devops/nuget-$name-pipeline.yml" `
        -o table  

    Write-Host 
}


function AzDevOpsPipelinesUpdate(
    [Parameter(Mandatory = $True)][ValidateNotNullOrEmpty()][string ] $name ,
    [Parameter(Mandatory = $True)][ValidateNotNullOrEmpty()][string ] $id 
    
) {
    Write-Host "=========================================="
    Write-Host "AzDevOpsPipelinesUpdate : $name $id"  
    Write-Host "==========================================" 
    az pipelines update  `
        --id $id  `
        --yaml-path "tools/devops/nuget-$name-pipeline.yml" `
        -o table  

    Write-Host 
} 
function AzDevOpsPipelinesCreateOrUpdate(
    [Parameter(Mandatory = $True)][ValidateNotNullOrEmpty()][string ] $name ) {
    Write-Host "=========================================="
    Write-Host "AzDevOpsPipelinesCreateOrUpdate : $name"  
    Write-Host "==========================================" 
   
    $response = az pipelines show --name "nuget - $name" -o json | ConvertFrom-Json
    if ($response.id) {
        AzDevOpsPipelinesUpdate $name $response.id
    }
    else {
        AzDevOpsPipelinesCreate $name
    }


    Write-Host 
} 




$global:url = "https://krosoft.visualstudio.com" 
$global:projet = "Krosoft.Extensions"
$global:serviceConnection = "krosoft-dev"
$global:repository = "https://github.com/krosoft-dev/Krosoft.Extensions"
$global:queue = "Azure Pipelines"

AzDevOpsLogin
AzDevOpsInitCli $global:url $global:projet
AzDevOpsPipelinesList
AzDevOpsPipelinesInitQueueId
AzDevOpsPipelinesInitServiceConnectionId 
 
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Blocking"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Blocking.Abstractions"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Blocking.Memory"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Blocking.Redis"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Core"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Cqrs"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Cqrs.Behaviors"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Cqrs.Behaviors.Identity"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Cqrs.Behaviors.Validations"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Data.Abstractions"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Data.EntityFramework"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Data.EntityFramework.InMemory"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Data.EntityFramework.PostgreSql"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Data.EntityFramework.Sqlite"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Data.EntityFramework.SqlServer"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Data.Json"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Events"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Events.Identity" 
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Mapping"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Pdf"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Polly"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Reporting.Csv"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Testing"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Testing.Cqrs"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Testing.Data.EntityFramework"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Testing.WebApi"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Validations"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.WebApi"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.WebApi.Blocking"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.WebApi.HealthChecks"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.WebApi.Identity"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.WebApi.Swagger"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.WebApi.Swagger.HealthChecks"
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Zip" 
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Hosting" 
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Application.Cache.Distributed.Redis" 
AzDevOpsPipelinesCreateOrUpdate "Krosoft.Extensions.Yarp" 