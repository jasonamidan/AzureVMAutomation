// See https://aka.ms/new-console-template for more information
using System;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace azure_config
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create the management client. This will be used for all the operations
            //that we will perform in Azure

            var credentials = SdkContext.AzureCredentialsFactory.FromFile("./azureauth.properties");

            var azure = Azure.Configure()
            .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
            .Authenticate(credentials)
            .WithDefaultSubscription();

            //First of all, we need to create a resource group.

            var groupName = "az204-ResourceGroup";
            var vmName = "az204VMTesting";
            var location = Region.USCentral;
            var vNetName = "az204VNET";
            var vNetAddress = "172.16.0.0/16";
            var subnetName = "az204Subnet";
            var subnetAddress = "172.16.0.0/24";
            var nicName = "az204NIC";
            var adminUser = "azureadminuser";
            var adminPassword = "Pa$$w0rd!2022";

            Console.WriteLine($"Creating Resource Group {groupName} ...");
            var resourceGroup = azure.ResourceGroups.Define(groupName)
            .WithRegion(location)
            .Create();

            //Every virtual machine needs to be connected to a virtual network.

            Console.WriteLine($"Creating Virtual Network {vNetName} ...");

            var network = azure.Networks.Define(vNetName)
            .WithRegion(location)
            .WithExistingResourceGroup(groupName)
            .WithAddressSpace(vNetAddress)
            .WithSubnet(subnetName, subnetAddress)
            .Create();

            //Any Virtual Machine needs a Network Interface for connecting to the vNet

            Console.WriteLine($"Creating Network Interface {nicName} ...");

            var nic = azure.NetworkInterfaces.Define(nicName)
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .WithExistingPrimaryNetwork(network)
                .WithSubnet(subnetName)
                .WithPrimaryPrivateIPAddressDynamic()
                .Create();

            //Create the virtual machine
            Console.WriteLine($"Creating Virtual Machine {vmName} ...");

            azure.VirtualMachines.Define(vmName)
            .WithRegion(location)
            .WithExistingResourceGroup(groupName)
            .WithExistingPrimaryNetworkInterface(nic)
            .WithLatestWindowsImage("MicrosoftWindowsServer", "WindowsServer", "2022-datacenter-azure-edition")
            .WithAdminUsername(adminUser)
            .WithAdminPassword(adminPassword)
            .WithComputerName(vmName)
            .WithSize(VirtualMachineSizeTypes.StandardD2sV3)
            .Create();


        }
    }
}
