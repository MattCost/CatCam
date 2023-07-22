# Configure the Azure provider
terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.66.0"
    }

    azuread = {
      source = "hashicorp/azuread"
      version = "2.40.0"
    }
  }

  required_version = ">= 1.1.0"
}

provider "azurerm" {
  features {}
}

data "azurerm_client_config" "current" {}

data "azuread_client_config" "current" {}


# A Single RG that holds everything
resource "azurerm_resource_group" "this" {
  name     = "${local.namePrefix}-rg"
  location = "eastus"

  tags = {
    purpose = "${local.env}"
  }
}

# Storage
resource "azurerm_storage_account" "this" {
  name                     = "${local.appName}${local.env}sta"
  resource_group_name      = azurerm_resource_group.this.name
  location                 = azurerm_resource_group.this.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  tags = {
    purpose = "${local.env}"
  }
}


resource "azurerm_container_registry" "this" {
  name                = "${local.appName}acr"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  sku                 = "Basic"
  admin_enabled       = false
}

# Keyvault
resource "azurerm_key_vault" "example" {
  name                        = "${local.namePrefix}-kv"
  location                    = azurerm_resource_group.this.location
  resource_group_name         = azurerm_resource_group.this.name
  enabled_for_disk_encryption = true
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  soft_delete_retention_days  = 7
  purge_protection_enabled    = false

  sku_name = "standard"

  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = data.azurerm_client_config.current.object_id

    key_permissions = [
      "Get",
    ]

    secret_permissions = [
      "Get",
    ]

    storage_permissions = [
      "Get",
    ]
  }

  tags = {
    purpose = "${local.env}"
  }
}


# IOT Hub
resource "azurerm_iothub" "this" {
  name = "${local.appName}-${local.env}-iothub"

  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location

  sku {
    name     = "S1"
    capacity = "1"
  }

  # endpoint {
  #   type                       = "AzureIotHub.StorageContainer"
  #   connection_string          = azurerm_storage_account.example.primary_blob_connection_string
  #   name                       = "export"
  #   batch_frequency_in_seconds = 60
  #   max_chunk_size_in_bytes    = 10485760
  #   container_name             = azurerm_storage_container.example.name
  #   encoding                   = "Avro"
  #   file_name_format           = "{iothub}/{partition}_{YYYY}_{MM}_{DD}_{HH}_{mm}"
  # }

  # endpoint {
  #   type              = "AzureIotHub.EventHub"
  #   connection_string = azurerm_eventhub_authorization_rule.example.primary_connection_string
  #   name              = "export2"
  # }

  # route {
  #   name           = "export"
  #   source         = "DeviceMessages"
  #   condition      = "true"
  #   endpoint_names = ["export"]
  #   enabled        = true
  # }

  # route {
  #   name           = "export2"
  #   source         = "DeviceMessages"
  #   condition      = "true"
  #   endpoint_names = ["export2"]
  #   enabled        = true
  # }

  # enrichment {
  #   key            = "tenant"
  #   value          = "$twin.tags.Tenant"
  #   endpoint_names = ["export", "export2"]
  # }

  cloud_to_device {
    max_delivery_count = 30
    default_ttl        = "PT1H"
    feedback {
      time_to_live       = "PT1H10M"
      max_delivery_count = 15
      lock_duration      = "PT30S"
    }
  }

  tags = {
    purpose = "${local.env}"
  }
}

# App Hosting
# Service plan for api and webapp

resource "azurerm_service_plan" "this" {
  name                = "${local.namePrefix}-app-service"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_resource_group.this.location
  os_type             = "Linux"
  sku_name            = "F1"

  tags = {
    purpose = "${local.env}"
  }
}


resource "azurerm_linux_web_app" "api" {
  name                = "${local.namePrefix}-api"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_service_plan.this.location
  service_plan_id     = azurerm_service_plan.this.id

  site_config {
    always_on = false
  }
}

resource "azurerm_linux_web_app" "webapp" {
  name                = "${local.namePrefix}-webapp"
  resource_group_name = azurerm_resource_group.this.name
  location            = azurerm_service_plan.this.location
  service_plan_id     = azurerm_service_plan.this.id

  site_config {
    always_on = false
  }
}