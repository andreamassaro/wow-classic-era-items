@description('Azure region')
param location string

@description('Environment name used in resource naming')
param environmentName string

@description('Container image to deploy, e.g. <acr-login-server>/wowclassiceraitems-api:latest')
param containerImage string

@description('ACR login server')
param containerRegistryLoginServer string

@description('ACR admin username')
param containerRegistryUsername string

@secure()
@description('ACR admin password')
param containerRegistryPassword string

@description('Base URI of the Key Vault holding the app secrets, e.g. https://myvault.vault.azure.net/')
param keyVaultUri string

var logAnalyticsName = 'wowitems-logs-${environmentName}'
var containerAppEnvName = 'wowitems-env-${environmentName}'
var containerAppName = 'wowitems-api-${environmentName}'

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource containerAppEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: containerAppEnvName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

resource containerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: containerAppName
  location: location
  // Needed to read secrets from Key Vault via the "identity: system" secret
  // references below. Requires a "Key Vault Secrets User" role assignment on
  // the vault, granted after this resource is created (see main.bicep) since
  // the principal id isn't known until the identity exists.
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
      }
      registries: [
        {
          server: containerRegistryLoginServer
          username: containerRegistryUsername
          passwordSecretRef: 'registry-password'
        }
      ]
      secrets: [
        {
          name: 'registry-password'
          value: containerRegistryPassword
        }
        {
          name: 'sql-connection-string'
          keyVaultUrl: '${keyVaultUri}secrets/sql-connection-string'
          identity: 'system'
        }
        {
          name: 'blizzard-client-id'
          keyVaultUrl: '${keyVaultUri}secrets/blizzard-client-id'
          identity: 'system'
        }
        {
          name: 'blizzard-client-secret'
          keyVaultUrl: '${keyVaultUri}secrets/blizzard-client-secret'
          identity: 'system'
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'api'
          image: containerImage
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ASPNETCORE_URLS'
              value: 'http://+:8080'
            }
            {
              name: 'ConnectionStrings__DefaultConnection'
              secretRef: 'sql-connection-string'
            }
            {
              name: 'Blizzard__ClientId'
              secretRef: 'blizzard-client-id'
            }
            {
              name: 'Blizzard__ClientSecret'
              secretRef: 'blizzard-client-secret'
            }
          ]
        }
      ]
      // Scales to zero when idle, which keeps this within the Container Apps
      // monthly free grant (180,000 vCPU-s / 360,000 GiB-s / 2M requests) for
      // light/dev usage.
      scale: {
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

output fqdn string = containerApp.properties.configuration.ingress.fqdn
output principalId string = containerApp.identity.principalId
