locals {
  appName = "catcam"
  env     = "dev"
  location = "eastus"
}

locals {
  namePrefix = "${local.appName}-${local.env}"
}