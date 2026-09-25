// Entry point for provisioning the WowClassicEraItems infrastructure:
// - A resource group
// - A free-tier Azure SQL Database (serverless, useFreeLimit)
// - An Azure Container Registry (Basic tier - the ACR service itself isn't free,
//   only the SQL database is; see infra/modules/containerRegistry.bicep)
// - An empty Key Vault (add the sql-connection-string, blizzard-client-id,
//   and blizzard-client-secret secrets yourself after deployment)
// - A Container Apps environment + Container App running the API, reading
//   those secrets from Key Vault via its system-assigned managed identity
//
// Deploy with (from repo root):
//   export SQL_ADMIN_PASSWORD='<secret>'
//   az deployment sub create \
//     --location westeurope \
//     --template-file infra/main.bicep \
//     --parameters infra/main.bicepparam
//
// The SQL admin password is read from an environment variable by
// main.bicepparam at deploy time - it's never stored in a file.
targetScope = 'subscription'

@description('Azure region for all resources')
param location string = 'westeurope'

@description('Name of the resource group to create')
param resourceGroupName string = 'rg-wowclassiceraitems'

@description('Short environment name used in resource naming (e.g. dev, prod)')
param environmentName string = 'dev'

@description('SQL Server administrator login')
param sqlAdminLogin string = 'wowitemsadmin'

@secure()
@description('SQL Server administrator password')
param sqlAdminPassword string

@description('Name of the database to create')
param sqlDatabaseName string = 'wow-items'

@description('Container image for the API. Defaults to a public placeholder until the real image is built and pushed to the created ACR.')
param containerImage string = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'

resource rg 'Microsoft.Resources/resourceGroups@2025-04-01' = {
  name: resourceGroupName
  location: location
}

module sql 'modules/sql.bicep' = {
  name: 'sql-deployment'
  scope: rg
  params: {
    location: location
    environmentName: environmentName
    sqlAdminLogin: sqlAdminLogin
    sqlAdminPassword: sqlAdminPassword
    databaseName: sqlDatabaseName
  }
}

module registry 'modules/containerRegistry.bicep' = {
  name: 'acr-deployment'
  scope: rg
  params: {
    location: location
    environmentName: environmentName
  }
}

module keyVault 'modules/keyVault.bicep' = {
  name: 'keyvault-deployment'
  scope: rg
  params: {
    location: location
    environmentName: environmentName
  }
}

module containerApp 'modules/containerApp.bicep' = {
  name: 'containerapp-deployment'
  scope: rg
  params: {
    location: location
    environmentName: environmentName
    containerImage: containerImage
    containerRegistryLoginServer: registry.outputs.loginServer
    containerRegistryUsername: registry.outputs.adminUsername
    containerRegistryPassword: registry.outputs.adminPassword
    keyVaultUri: keyVault.outputs.keyVaultUri
  }
}

// Grant the Container App's managed identity read access to the Key Vault
// secrets, now that its principal id exists.
module keyVaultAccess 'modules/keyVaultAccess.bicep' = {
  name: 'keyvault-access-deployment'
  scope: rg
  params: {
    keyVaultName: keyVault.outputs.keyVaultName
    principalId: containerApp.outputs.principalId
  }
}

output resourceGroupName string = rg.name
output containerAppUrl string = 'https://${containerApp.outputs.fqdn}'
output containerRegistryLoginServer string = registry.outputs.loginServer
output sqlServerFqdn string = sql.outputs.serverFqdn
output sqlDatabaseName string = sqlDatabaseName
output keyVaultName string = keyVault.outputs.keyVaultName
