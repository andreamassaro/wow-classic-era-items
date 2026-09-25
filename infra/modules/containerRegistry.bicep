@description('Azure region')
param location string

@description('Environment name used in resource naming')
param environmentName string

// Note: unlike Azure SQL Database, Azure Container Registry has no free tier -
// Basic is the cheapest paid tier (~$0.167/day). Only the SQL database in this
// setup is actually free.
var uniqueSuffix = uniqueString(resourceGroup().id)
var acrName = 'wowitemsacr${environmentName}${uniqueSuffix}'

resource registry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: acrName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

output loginServer string = registry.properties.loginServer
#disable-next-line outputs-should-not-contain-secrets
output adminUsername string = registry.listCredentials().username
#disable-next-line outputs-should-not-contain-secrets
output adminPassword string = registry.listCredentials().passwords[0].value
