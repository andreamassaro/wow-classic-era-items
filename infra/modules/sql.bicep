@description('Azure region')
param location string

@description('Environment name used in resource naming')
param environmentName string

@description('SQL Server administrator login')
param sqlAdminLogin string

@secure()
param sqlAdminPassword string

@description('Name of the database to create')
param databaseName string = 'wow-items'

var uniqueSuffix = uniqueString(resourceGroup().id)
var sqlServerName = 'wowitems-sql-${environmentName}-${uniqueSuffix}'

resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
  }
}

// Lets Azure-hosted resources (like the Container App) reach the server without
// needing to enumerate individual outbound IPs. Container Apps' outbound IPs
// aren't static, so this is the practical option for a free/dev setup; for a
// production, higher-trust setup consider VNet integration + private endpoint instead.
resource allowAzureServices 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
  parent: sqlServer
  name: 'AllowAllAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Free-tier serverless General Purpose database (Azure SQL Database free offer):
// 100,000 vCore seconds + 32GB storage/month, free forever, auto-paused once
// the monthly limit is exhausted. One free-tier database is allowed per
// subscription with useFreeLimit: true (up to 10 total free databases allowed,
// but only the first can use the "AutoPause" free-limit behavior per docs).
resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
  parent: sqlServer
  name: databaseName
  location: location
  sku: {
    name: 'GP_S_Gen5'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 2
  }
  properties: {
    autoPauseDelay: 60
    minCapacity: json('0.5')
    useFreeLimit: true
    freeLimitExhaustionBehavior: 'AutoPause'
    maxSizeBytes: 34359738368 // 32 GB - the free offer's storage limit
  }
}

output serverFqdn string = sqlServer.properties.fullyQualifiedDomainName
#disable-next-line outputs-should-not-contain-secrets
output connectionString string = 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Database=${sqlDatabase.name};User ID=${sqlAdminLogin};Password=${sqlAdminPassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
