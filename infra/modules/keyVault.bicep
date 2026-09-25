@description('Azure region')
param location string

@description('Environment name used in resource naming')
param environmentName string

var uniqueSuffix = take(uniqueString(resourceGroup().id), 6)
var keyVaultName = 'wowitems-kv-${environmentName}-${uniqueSuffix}'

// The vault is created empty - secrets (sql-connection-string,
// blizzard-client-id, blizzard-client-secret) are added manually after
// deployment rather than passed through as deployment parameters. The
// Container App references those secret names via Key Vault URLs regardless
// of how they were created (see modules/containerApp.bicep).
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
  }
}

output keyVaultName string = keyVault.name
output keyVaultUri string = keyVault.properties.vaultUri
