## Introduction
A driver which exposes interfaces to make it easy to query Azure DevOps. This driver was written to avoid the pains of using the Azure DevOps web interface.


# Installation

1. For installing the driver, download the latest `AzureDevOpsDataContextDriver.lpx` file from relases section.
2. Open LINQPad and click on `Add Connection` and then click on `View more drivers` button.
3. Once you're in the `Choose a driver` screen, since this driver has not been published as an official driver, you'd have to click on `Browse` button and select the `AzureDevOpsDataContextDriver.lpx` file.

# Creating a new connection

Once you've installed the driver and try to create a new connection it will ask for a Azure DevOps url which is typically like in the format

    https://{REPLACE-WITH-YOUR-INSTANCE}.visualstudio.com
    
The create new connection also asks for a `Personal Access Token` which can be generated following the instructions in this [link](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=vsts).

## Usage

The driver injects a `IQueryable` interface for querying called `WorkItems` class. Example usage would shown below.

    WorkItems.Where(x => x.AssignedTo == "<name>" && x.IterationPath == @"<iteration-path>")
