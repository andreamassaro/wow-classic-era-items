using 'main.bicep'

param location = 'westeurope'
param resourceGroupName = 'rg-wowclassiceraitems'
param environmentName = 'dev'
param sqlAdminLogin = 'wowitemsadmin'
param sqlDatabaseName = 'wow-items'
param containerImage = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'

// The SQL admin password is read from an environment variable at deploy time -
// never stored here. Set it before deploying:
//   export SQL_ADMIN_PASSWORD='...'
param sqlAdminPassword = readEnvironmentVariable('SQL_ADMIN_PASSWORD')
