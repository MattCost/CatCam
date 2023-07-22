This will hold infrastructure related files. terraform, etc


az login
az account set --subscription "subid"
az ad sp create-for-rbac --role="Contributor" --scopes="/subscriptions/subid"
Make note of the values returned in the console, you will never see that secret again.

Edit .vscode/settings.json

{
    "terminal.integrated.env.linux": {
        "ARM_CLIENT_ID":"client id",
        "ARM_CLIENT_SECRET" : "hunter2",
        "ARM_SUBSCRIPTION_ID" : "sub id",
        "ARM_TENANT_ID" : "tenant id",
    }
}

Now when you open a terminal, terrafrom provider can use these values.


`terraform init`
`terraform init -upgrade` needed if trying to upgrade the provider version

