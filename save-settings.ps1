param(
    [string]outFile,
    [string]urls_baseUrl,
    [string]ui_map_googleMaps,
    [string]assetStore_azureBlob_containerName,
    [string]assetStore_azureBlob_connectionString,
    [string]eventStore_mongoDb_configuration,
    [string]eventStore_mongoDb_database,
    [string]store_mongoDb_configuration,
    [string]store_mongoDb_contentDatabase,
    [string]store_mongoDb_database,
    [string]identity_adminEmail,
    [string]identity_adminPassword
)

var $blob = "
{
  "urls": {
    "baseUrl": "$($urls_baseUrl)"
  },

  "ui": {
    "map": {
      "googleMaps": {
        "key": "$($ui_map_googleMaps)"
      }
    }
  },

  "assetStore": {
    "azureBlob": {
      "containerName": "$($assetStore_azureBlob_containerName)",
      "connectionString": "$($assetStore_azureBlob_connectionString)"
    }
  },

  "eventStore": {
    "mongoDb": {
      "configuration": "$($eventStore_mongoDb_configuration)",
      "database": "$($environment)_$($eventStore_mongoDb_database)"
    }
  },

  "store": {
    "mongoDb": {
      "configuration": "$($store_mongoDb_configuration)",
      "contentDatabase": "$($environment)_$($store_mongoDb_contentDatabase)",
      "database": "$($environment)_$($store_mongoDb_database)"
    }
  },

  "identity": {
    "adminEmail": "$($identity_adminEmail)",
    "AdminPassword": "$($identity_adminPassword)",

    "allowPasswordAuth": true,

    "googleClient": "",
    "googleSecret": "",

    "githubClient": "",
    "githubSecret": "",

    "microsoftClient": "",
    "microsoftSecret": "",

    "lockAutomatically": true
  }
}
"

Set-Content -Path "$($outFile)" -Value $blob