using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Codaxy.Inventory.App.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SnakeCaseNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activation_Asset_AssetId",
                table: "Activation"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Activation_Person_PersonId",
                table: "Activation"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Activation_Volume_VolumeId",
                table: "Activation"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Asset_AssetSubstatus_AssetSubstatusId",
                table: "Asset"
            );

            migrationBuilder.DropForeignKey(name: "FK_Asset_AssetType_AssetTypeId", table: "Asset");

            migrationBuilder.DropForeignKey(
                name: "FK_Asset_Availability_AvailabilityId",
                table: "Asset"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Asset_BusinessEntity_BusinessEntityId",
                table: "Asset"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Asset_Confidentiality_ConfidentialityId",
                table: "Asset"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Asset_Importance_ImportanceId",
                table: "Asset"
            );

            migrationBuilder.DropForeignKey(name: "FK_Asset_Integrity_IntegrityId", table: "Asset");

            migrationBuilder.DropForeignKey(name: "FK_Asset_Location_LocationId", table: "Asset");

            migrationBuilder.DropForeignKey(name: "FK_Asset_Person_PersonId", table: "Asset");

            migrationBuilder.DropForeignKey(name: "FK_Asset_Vendor_VendorId", table: "Asset");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetSubstatus_AssetStatus_AssetStatusId",
                table: "AssetSubstatus"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_AssetType_AssetCategory_AssetCategoryId",
                table: "AssetType"
            );

            migrationBuilder.DropForeignKey(name: "FK_City_Country_CountryCode", table: "City");

            migrationBuilder.DropForeignKey(name: "FK_Cloud_Volume_VolumeId", table: "Cloud");

            migrationBuilder.DropForeignKey(
                name: "FK_ElectronicDevice_Asset_AssetId",
                table: "ElectronicDevice"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_ElectronicDevice_ElectronicDeviceType_ElectronicDeviceTypeId",
                table: "ElectronicDevice"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_ElectronicDevice_Manufacturer_ManufacturerId",
                table: "ElectronicDevice"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_ElectronicDeviceTypeElectronicDeviceTag_ElectronicDeviceTag~",
                table: "ElectronicDeviceTypeElectronicDeviceTag"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_ElectronicDeviceTypeElectronicDeviceTag_ElectronicDeviceTyp~",
                table: "ElectronicDeviceTypeElectronicDeviceTag"
            );

            migrationBuilder.DropForeignKey(name: "FK_Furniture_Asset_AssetId", table: "Furniture");

            migrationBuilder.DropForeignKey(
                name: "FK_Furniture_FurnitureType_FurnitureTypeId",
                table: "Furniture"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Information_Availability_AvailabilityId",
                table: "Information"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Information_Confidentiality_ConfidentialityId",
                table: "Information"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Information_Importance_ImportanceId",
                table: "Information"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Information_InformationType_InformationTypeId",
                table: "Information"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Information_Integrity_IntegrityId",
                table: "Information"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Information_Person_PersonId",
                table: "Information"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Information_Project_ProjectId",
                table: "Information"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_InformationLocation_Cloud_CloudId",
                table: "InformationLocation"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_InformationLocation_ElectronicDevice_ElectronicDeviceId",
                table: "InformationLocation"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_InformationLocation_Information_InformationId",
                table: "InformationLocation"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_InformationLocation_Location_PhysicalLocationId",
                table: "InformationLocation"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_InformationLocation_Software_SoftwareId",
                table: "InformationLocation"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_InformationLocation_VirtualMachine_VirtualMachineId",
                table: "InformationLocation"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_InformationTagInformation_InformationTag_InformationTagId",
                table: "InformationTagInformation"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_InformationTagInformation_Information_InformationId",
                table: "InformationTagInformation"
            );

            migrationBuilder.DropForeignKey(name: "FK_License_Asset_AssetId", table: "License");

            migrationBuilder.DropForeignKey(
                name: "FK_License_Currency_CurrencyId",
                table: "License"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_License_LicenseExpirationModel_LicenseExpirationModelId",
                table: "License"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_License_LicenseModel_LicenseModelId",
                table: "License"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_License_LicenseType_LicenseTypeId",
                table: "License"
            );

            migrationBuilder.DropForeignKey(name: "FK_License_Period_PeriodId", table: "License");

            migrationBuilder.DropForeignKey(name: "FK_Location_City_CityId", table: "Location");

            migrationBuilder.DropForeignKey(
                name: "FK_Location_Country_CountryCode",
                table: "Location"
            );

            migrationBuilder.DropForeignKey(name: "FK_Location_State_StateId", table: "Location");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceContract_Asset_AssetId",
                table: "MaintenanceContract"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceContract_MaintenanceType_MaintenanceTypeId",
                table: "MaintenanceContract"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceContract_Vendor_VendorId",
                table: "MaintenanceContract"
            );

            migrationBuilder.DropForeignKey(name: "FK_Project_Client_ClientId", table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Person_ProjectOwnerId",
                table: "Project"
            );

            migrationBuilder.DropForeignKey(name: "FK_Software_Volume_VolumeId", table: "Software");

            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareOrService_Manufacturer_ManufacturerId",
                table: "SoftwareOrService"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_SoftwareOrService_SoftwareOrServiceCategory_SoftwareOrServi~",
                table: "SoftwareOrService"
            );

            migrationBuilder.DropForeignKey(name: "FK_State_Country_CountryCode", table: "State");

            migrationBuilder.DropForeignKey(name: "FK_Volume_License_LicenseId", table: "Volume");

            migrationBuilder.DropForeignKey(
                name: "FK_Volume_SoftwareOrService_SoftwareOrServiceId",
                table: "Volume"
            );

            migrationBuilder.DropForeignKey(
                name: "FK_Volume_VolumeType_VolumeTypeId",
                table: "Volume"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_Volume", table: "Volume");

            migrationBuilder.DropPrimaryKey(name: "PK_Vendor", table: "Vendor");

            migrationBuilder.DropPrimaryKey(name: "PK_State", table: "State");

            migrationBuilder.DropPrimaryKey(name: "PK_Software", table: "Software");

            migrationBuilder.DropPrimaryKey(name: "PK_Sequence", table: "Sequence");

            migrationBuilder.DropPrimaryKey(name: "PK_Project", table: "Project");

            migrationBuilder.DropPrimaryKey(name: "PK_Person", table: "Person");

            migrationBuilder.DropPrimaryKey(name: "PK_Period", table: "Period");

            migrationBuilder.DropPrimaryKey(name: "PK_Manufacturer", table: "Manufacturer");

            migrationBuilder.DropPrimaryKey(name: "PK_Location", table: "Location");

            migrationBuilder.DropPrimaryKey(name: "PK_License", table: "License");

            migrationBuilder.DropPrimaryKey(name: "PK_Integrity", table: "Integrity");

            migrationBuilder.DropPrimaryKey(name: "PK_Information", table: "Information");

            migrationBuilder.DropPrimaryKey(name: "PK_Importance", table: "Importance");

            migrationBuilder.DropPrimaryKey(name: "PK_Furniture", table: "Furniture");

            migrationBuilder.DropPrimaryKey(name: "PK_Currency", table: "Currency");

            migrationBuilder.DropPrimaryKey(name: "PK_Country", table: "Country");

            migrationBuilder.DropPrimaryKey(name: "PK_Confidentiality", table: "Confidentiality");

            migrationBuilder.DropPrimaryKey(name: "PK_Cloud", table: "Cloud");

            migrationBuilder.DropPrimaryKey(name: "PK_Client", table: "Client");

            migrationBuilder.DropPrimaryKey(name: "PK_City", table: "City");

            migrationBuilder.DropPrimaryKey(name: "PK_Availability", table: "Availability");

            migrationBuilder.DropPrimaryKey(name: "PK_Asset", table: "Asset");

            migrationBuilder.DropPrimaryKey(name: "PK_Activation", table: "Activation");

            migrationBuilder.DropPrimaryKey(name: "PK_VolumeType", table: "VolumeType");

            migrationBuilder.DropPrimaryKey(name: "PK_VirtualMachine", table: "VirtualMachine");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SoftwareOrServiceCategory",
                table: "SoftwareOrServiceCategory"
            );

            migrationBuilder.DropPrimaryKey(
                name: "PK_SoftwareOrService",
                table: "SoftwareOrService"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_MaintenanceType", table: "MaintenanceType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MaintenanceContract",
                table: "MaintenanceContract"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_LicenseType", table: "LicenseType");

            migrationBuilder.DropPrimaryKey(name: "PK_LicenseModel", table: "LicenseModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LicenseExpirationModel",
                table: "LicenseExpirationModel"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_LicenseClass", table: "LicenseClass");

            migrationBuilder.DropPrimaryKey(name: "PK_LicenseCategory", table: "LicenseCategory");

            migrationBuilder.DropPrimaryKey(name: "PK_InformationType", table: "InformationType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InformationTagInformation",
                table: "InformationTagInformation"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_InformationTag", table: "InformationTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InformationLocation",
                table: "InformationLocation"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_FurnitureType", table: "FurnitureType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ElectronicDeviceTypeElectronicDeviceTag",
                table: "ElectronicDeviceTypeElectronicDeviceTag"
            );

            migrationBuilder.DropPrimaryKey(
                name: "PK_ElectronicDeviceType",
                table: "ElectronicDeviceType"
            );

            migrationBuilder.DropPrimaryKey(
                name: "PK_ElectronicDeviceTag",
                table: "ElectronicDeviceTag"
            );

            migrationBuilder.DropPrimaryKey(name: "PK_ElectronicDevice", table: "ElectronicDevice");

            migrationBuilder.DropPrimaryKey(name: "PK_BusinessEntity", table: "BusinessEntity");

            migrationBuilder.DropPrimaryKey(name: "PK_AuditLog", table: "AuditLog");

            migrationBuilder.DropPrimaryKey(name: "PK_AssetType", table: "AssetType");

            migrationBuilder.DropPrimaryKey(name: "PK_AssetSubstatus", table: "AssetSubstatus");

            migrationBuilder.DropPrimaryKey(name: "PK_AssetStatus", table: "AssetStatus");

            migrationBuilder.DropPrimaryKey(name: "PK_AssetCategory", table: "AssetCategory");

            migrationBuilder.RenameTable(name: "Volume", newName: "volume");

            migrationBuilder.RenameTable(name: "Vendor", newName: "vendor");

            migrationBuilder.RenameTable(name: "State", newName: "state");

            migrationBuilder.RenameTable(name: "Software", newName: "software");

            migrationBuilder.RenameTable(name: "Sequence", newName: "sequence");

            migrationBuilder.RenameTable(name: "Project", newName: "project");

            migrationBuilder.RenameTable(name: "Person", newName: "person");

            migrationBuilder.RenameTable(name: "Period", newName: "period");

            migrationBuilder.RenameTable(name: "Manufacturer", newName: "manufacturer");

            migrationBuilder.RenameTable(name: "Location", newName: "location");

            migrationBuilder.RenameTable(name: "License", newName: "license");

            migrationBuilder.RenameTable(name: "Integrity", newName: "integrity");

            migrationBuilder.RenameTable(name: "Information", newName: "information");

            migrationBuilder.RenameTable(name: "Importance", newName: "importance");

            migrationBuilder.RenameTable(name: "Furniture", newName: "furniture");

            migrationBuilder.RenameTable(name: "Currency", newName: "currency");

            migrationBuilder.RenameTable(name: "Country", newName: "country");

            migrationBuilder.RenameTable(name: "Confidentiality", newName: "confidentiality");

            migrationBuilder.RenameTable(name: "Cloud", newName: "cloud");

            migrationBuilder.RenameTable(name: "Client", newName: "client");

            migrationBuilder.RenameTable(name: "City", newName: "city");

            migrationBuilder.RenameTable(name: "Availability", newName: "availability");

            migrationBuilder.RenameTable(name: "Asset", newName: "asset");

            migrationBuilder.RenameTable(name: "Activation", newName: "activation");

            migrationBuilder.RenameTable(name: "VolumeType", newName: "volume_type");

            migrationBuilder.RenameTable(name: "VirtualMachine", newName: "virtual_machine");

            migrationBuilder.RenameTable(
                name: "SoftwareOrServiceCategory",
                newName: "software_or_service_category"
            );

            migrationBuilder.RenameTable(name: "SoftwareOrService", newName: "software_or_service");

            migrationBuilder.RenameTable(name: "MaintenanceType", newName: "maintenance_type");

            migrationBuilder.RenameTable(
                name: "MaintenanceContract",
                newName: "maintenance_contract"
            );

            migrationBuilder.RenameTable(name: "LicenseType", newName: "license_type");

            migrationBuilder.RenameTable(name: "LicenseModel", newName: "license_model");

            migrationBuilder.RenameTable(
                name: "LicenseExpirationModel",
                newName: "license_expiration_model"
            );

            migrationBuilder.RenameTable(name: "LicenseClass", newName: "license_class");

            migrationBuilder.RenameTable(name: "LicenseCategory", newName: "license_category");

            migrationBuilder.RenameTable(name: "InformationType", newName: "information_type");

            migrationBuilder.RenameTable(
                name: "InformationTagInformation",
                newName: "information_tag_information"
            );

            migrationBuilder.RenameTable(name: "InformationTag", newName: "information_tag");

            migrationBuilder.RenameTable(
                name: "InformationLocation",
                newName: "information_location"
            );

            migrationBuilder.RenameTable(name: "FurnitureType", newName: "furniture_type");

            migrationBuilder.RenameTable(
                name: "ElectronicDeviceTypeElectronicDeviceTag",
                newName: "electronic_device_type_electronic_device_tag"
            );

            migrationBuilder.RenameTable(
                name: "ElectronicDeviceType",
                newName: "electronic_device_type"
            );

            migrationBuilder.RenameTable(
                name: "ElectronicDeviceTag",
                newName: "electronic_device_tag"
            );

            migrationBuilder.RenameTable(name: "ElectronicDevice", newName: "electronic_device");

            migrationBuilder.RenameTable(name: "BusinessEntity", newName: "business_entity");

            migrationBuilder.RenameTable(name: "AuditLog", newName: "audit_log");

            migrationBuilder.RenameTable(name: "AssetType", newName: "asset_type");

            migrationBuilder.RenameTable(name: "AssetSubstatus", newName: "asset_substatus");

            migrationBuilder.RenameTable(name: "AssetStatus", newName: "asset_status");

            migrationBuilder.RenameTable(name: "AssetCategory", newName: "asset_category");

            migrationBuilder.RenameColumn(name: "Quantity", table: "volume", newName: "quantity");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "volume",
                newName: "description"
            );

            migrationBuilder.RenameColumn(name: "Id", table: "volume", newName: "id");

            migrationBuilder.RenameColumn(
                name: "VolumeTypeId",
                table: "volume",
                newName: "volume_type_id"
            );

            migrationBuilder.RenameColumn(
                name: "SoftwareOrServiceId",
                table: "volume",
                newName: "software_or_service_id"
            );

            migrationBuilder.RenameColumn(
                name: "LicenseId",
                table: "volume",
                newName: "license_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Volume_VolumeTypeId",
                table: "volume",
                newName: "ix_volume_volume_type_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Volume_SoftwareOrServiceId",
                table: "volume",
                newName: "ix_volume_software_or_service_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Volume_LicenseId",
                table: "volume",
                newName: "ix_volume_license_id"
            );

            migrationBuilder.RenameColumn(name: "Web", table: "vendor", newName: "web");

            migrationBuilder.RenameColumn(name: "Phone", table: "vendor", newName: "phone");

            migrationBuilder.RenameColumn(name: "Name", table: "vendor", newName: "name");

            migrationBuilder.RenameColumn(name: "Location", table: "vendor", newName: "location");

            migrationBuilder.RenameColumn(name: "Email", table: "vendor", newName: "email");

            migrationBuilder.RenameColumn(name: "Id", table: "vendor", newName: "id");

            migrationBuilder.RenameColumn(
                name: "VATNumber",
                table: "vendor",
                newName: "vat_number"
            );

            migrationBuilder.RenameColumn(
                name: "RegistrationNumber",
                table: "vendor",
                newName: "registration_number"
            );

            migrationBuilder.RenameColumn(
                name: "MobilePhone",
                table: "vendor",
                newName: "mobile_phone"
            );

            migrationBuilder.RenameColumn(
                name: "ContactPerson",
                table: "vendor",
                newName: "contact_person"
            );

            migrationBuilder.RenameColumn(name: "Name", table: "state", newName: "name");

            migrationBuilder.RenameColumn(name: "Id", table: "state", newName: "id");

            migrationBuilder.RenameColumn(
                name: "CountryCode",
                table: "state",
                newName: "country_code"
            );

            migrationBuilder.RenameIndex(
                name: "IX_State_CountryCode",
                table: "state",
                newName: "ix_state_country_code"
            );

            migrationBuilder.RenameColumn(name: "Name", table: "software", newName: "name");

            migrationBuilder.RenameColumn(name: "Id", table: "software", newName: "id");

            migrationBuilder.RenameColumn(
                name: "VolumeId",
                table: "software",
                newName: "volume_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Software_VolumeId",
                table: "software",
                newName: "ix_software_volume_id"
            );

            migrationBuilder.RenameColumn(name: "Id", table: "sequence", newName: "id");

            migrationBuilder.RenameColumn(
                name: "AssetInventoryNumber",
                table: "sequence",
                newName: "asset_inventory_number"
            );

            migrationBuilder.RenameColumn(name: "Name", table: "project", newName: "name");

            migrationBuilder.RenameColumn(name: "Id", table: "project", newName: "id");

            migrationBuilder.RenameColumn(
                name: "ProjectOwnerId",
                table: "project",
                newName: "project_owner_id"
            );

            migrationBuilder.RenameColumn(name: "ClientId", table: "project", newName: "client_id");

            migrationBuilder.RenameIndex(
                name: "IX_Project_ProjectOwnerId",
                table: "project",
                newName: "ix_project_project_owner_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Project_ClientId",
                table: "project",
                newName: "ix_project_client_id"
            );

            migrationBuilder.RenameColumn(name: "Name", table: "person", newName: "name");

            migrationBuilder.RenameColumn(name: "Email", table: "person", newName: "email");

            migrationBuilder.RenameColumn(name: "Id", table: "person", newName: "id");

            migrationBuilder.RenameColumn(name: "Text", table: "period", newName: "text");

            migrationBuilder.RenameColumn(name: "Id", table: "period", newName: "id");

            migrationBuilder.RenameColumn(name: "URL", table: "manufacturer", newName: "url");

            migrationBuilder.RenameColumn(name: "Name", table: "manufacturer", newName: "name");

            migrationBuilder.RenameColumn(name: "Id", table: "manufacturer", newName: "id");

            migrationBuilder.RenameColumn(name: "Street", table: "location", newName: "street");

            migrationBuilder.RenameColumn(name: "Room", table: "location", newName: "room");

            migrationBuilder.RenameColumn(name: "Name", table: "location", newName: "name");

            migrationBuilder.RenameColumn(name: "Floor", table: "location", newName: "floor");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "location",
                newName: "description"
            );

            migrationBuilder.RenameColumn(name: "Id", table: "location", newName: "id");

            migrationBuilder.RenameColumn(name: "StateId", table: "location", newName: "state_id");

            migrationBuilder.RenameColumn(
                name: "PostalCode",
                table: "location",
                newName: "postal_code"
            );

            migrationBuilder.RenameColumn(
                name: "HouseNumber",
                table: "location",
                newName: "house_number"
            );

            migrationBuilder.RenameColumn(
                name: "CountryCode",
                table: "location",
                newName: "country_code"
            );

            migrationBuilder.RenameColumn(name: "CityId", table: "location", newName: "city_id");

            migrationBuilder.RenameIndex(
                name: "IX_Location_StateId",
                table: "location",
                newName: "ix_location_state_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Location_CountryCode",
                table: "location",
                newName: "ix_location_country_code"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Location_CityId",
                table: "location",
                newName: "ix_location_city_id"
            );

            migrationBuilder.RenameColumn(
                name: "SubscriptionFee",
                table: "license",
                newName: "subscription_fee"
            );

            migrationBuilder.RenameColumn(
                name: "SubscriptionExpirationDate",
                table: "license",
                newName: "subscription_expiration_date"
            );

            migrationBuilder.RenameColumn(
                name: "RegistrationNumber",
                table: "license",
                newName: "registration_number"
            );

            migrationBuilder.RenameColumn(name: "PeriodId", table: "license", newName: "period_id");

            migrationBuilder.RenameColumn(
                name: "ManagementConsoleUrl",
                table: "license",
                newName: "management_console_url"
            );

            migrationBuilder.RenameColumn(
                name: "LicenseTypeId",
                table: "license",
                newName: "license_type_id"
            );

            migrationBuilder.RenameColumn(
                name: "LicenseModelId",
                table: "license",
                newName: "license_model_id"
            );

            migrationBuilder.RenameColumn(
                name: "LicenseExpirationModelId",
                table: "license",
                newName: "license_expiration_model_id"
            );

            migrationBuilder.RenameColumn(
                name: "KeyIdentifier",
                table: "license",
                newName: "key_identifier"
            );

            migrationBuilder.RenameColumn(
                name: "CurrencyId",
                table: "license",
                newName: "currency_id"
            );

            migrationBuilder.RenameColumn(
                name: "AutoRenew",
                table: "license",
                newName: "auto_renew"
            );

            migrationBuilder.RenameColumn(name: "AssetId", table: "license", newName: "asset_id");

            migrationBuilder.RenameIndex(
                name: "IX_License_PeriodId",
                table: "license",
                newName: "ix_license_period_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_License_LicenseTypeId",
                table: "license",
                newName: "ix_license_license_type_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_License_LicenseModelId",
                table: "license",
                newName: "ix_license_license_model_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_License_LicenseExpirationModelId",
                table: "license",
                newName: "ix_license_license_expiration_model_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_License_CurrencyId",
                table: "license",
                newName: "ix_license_currency_id"
            );

            migrationBuilder.RenameColumn(name: "Weight", table: "integrity", newName: "weight");

            migrationBuilder.RenameColumn(name: "Level", table: "integrity", newName: "level");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "integrity",
                newName: "description"
            );

            migrationBuilder.RenameColumn(name: "Id", table: "integrity", newName: "id");

            migrationBuilder.RenameColumn(name: "Note", table: "information", newName: "note");

            migrationBuilder.RenameColumn(name: "Name", table: "information", newName: "name");

            migrationBuilder.RenameColumn(
                name: "Incomplete",
                table: "information",
                newName: "incomplete"
            );

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "information",
                newName: "description"
            );

            migrationBuilder.RenameColumn(name: "Author", table: "information", newName: "author");

            migrationBuilder.RenameColumn(name: "Id", table: "information", newName: "id");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "information",
                newName: "project_id"
            );

            migrationBuilder.RenameColumn(
                name: "PersonalInformation",
                table: "information",
                newName: "personal_information"
            );

            migrationBuilder.RenameColumn(
                name: "PersonId",
                table: "information",
                newName: "person_id"
            );

            migrationBuilder.RenameColumn(
                name: "IntegrityId",
                table: "information",
                newName: "integrity_id"
            );

            migrationBuilder.RenameColumn(
                name: "InformationTypeId",
                table: "information",
                newName: "information_type_id"
            );

            migrationBuilder.RenameColumn(
                name: "ImportanceId",
                table: "information",
                newName: "importance_id"
            );

            migrationBuilder.RenameColumn(
                name: "ConfidentialityId",
                table: "information",
                newName: "confidentiality_id"
            );

            migrationBuilder.RenameColumn(
                name: "ClientsPersonalInformation",
                table: "information",
                newName: "clients_personal_information"
            );

            migrationBuilder.RenameColumn(
                name: "AvailabilityId",
                table: "information",
                newName: "availability_id"
            );

            migrationBuilder.RenameColumn(
                name: "AccessRights",
                table: "information",
                newName: "access_rights"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Information_ProjectId",
                table: "information",
                newName: "ix_information_project_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Information_PersonId",
                table: "information",
                newName: "ix_information_person_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Information_IntegrityId",
                table: "information",
                newName: "ix_information_integrity_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Information_InformationTypeId",
                table: "information",
                newName: "ix_information_information_type_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Information_ImportanceId",
                table: "information",
                newName: "ix_information_importance_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Information_ConfidentialityId",
                table: "information",
                newName: "ix_information_confidentiality_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Information_AvailabilityId",
                table: "information",
                newName: "ix_information_availability_id"
            );

            migrationBuilder.RenameColumn(name: "Level", table: "importance", newName: "level");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "importance",
                newName: "description"
            );

            migrationBuilder.RenameColumn(name: "Id", table: "importance", newName: "id");

            migrationBuilder.RenameColumn(name: "Model", table: "furniture", newName: "model");

            migrationBuilder.RenameColumn(
                name: "FurnitureTypeId",
                table: "furniture",
                newName: "furniture_type_id"
            );

            migrationBuilder.RenameColumn(name: "AssetId", table: "furniture", newName: "asset_id");

            migrationBuilder.RenameIndex(
                name: "IX_Furniture_FurnitureTypeId",
                table: "furniture",
                newName: "ix_furniture_furniture_type_id"
            );

            migrationBuilder.RenameColumn(name: "Text", table: "currency", newName: "text");

            migrationBuilder.RenameColumn(name: "Id", table: "currency", newName: "id");

            migrationBuilder.RenameColumn(name: "Name", table: "country", newName: "name");

            migrationBuilder.RenameColumn(name: "Code", table: "country", newName: "code");

            migrationBuilder.RenameColumn(
                name: "Weight",
                table: "confidentiality",
                newName: "weight"
            );

            migrationBuilder.RenameColumn(
                name: "Level",
                table: "confidentiality",
                newName: "level"
            );

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "confidentiality",
                newName: "description"
            );

            migrationBuilder.RenameColumn(name: "Id", table: "confidentiality", newName: "id");

            migrationBuilder.RenameColumn(name: "Name", table: "cloud", newName: "name");

            migrationBuilder.RenameColumn(name: "Id", table: "cloud", newName: "id");

            migrationBuilder.RenameColumn(name: "VolumeId", table: "cloud", newName: "volume_id");

            migrationBuilder.RenameColumn(
                name: "ManagementURL",
                table: "cloud",
                newName: "management_url"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Cloud_VolumeId",
                table: "cloud",
                newName: "ix_cloud_volume_id"
            );

            migrationBuilder.RenameColumn(name: "Name", table: "client", newName: "name");

            migrationBuilder.RenameColumn(name: "Id", table: "client", newName: "id");

            migrationBuilder.RenameColumn(name: "Name", table: "city", newName: "name");

            migrationBuilder.RenameColumn(name: "Id", table: "city", newName: "id");

            migrationBuilder.RenameColumn(
                name: "CountryCode",
                table: "city",
                newName: "country_code"
            );

            migrationBuilder.RenameIndex(
                name: "IX_City_CountryCode",
                table: "city",
                newName: "ix_city_country_code"
            );

            migrationBuilder.RenameColumn(name: "Weight", table: "availability", newName: "weight");

            migrationBuilder.RenameColumn(name: "Level", table: "availability", newName: "level");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "availability",
                newName: "description"
            );

            migrationBuilder.RenameColumn(name: "Id", table: "availability", newName: "id");

            migrationBuilder.RenameColumn(name: "URL", table: "asset", newName: "url");

            migrationBuilder.RenameColumn(name: "Name", table: "asset", newName: "name");

            migrationBuilder.RenameColumn(
                name: "Incomplete",
                table: "asset",
                newName: "incomplete"
            );

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "asset",
                newName: "description"
            );

            migrationBuilder.RenameColumn(name: "Id", table: "asset", newName: "id");

            migrationBuilder.RenameColumn(name: "VendorId", table: "asset", newName: "vendor_id");

            migrationBuilder.RenameColumn(
                name: "PurchaseValue",
                table: "asset",
                newName: "purchase_value"
            );

            migrationBuilder.RenameColumn(
                name: "PurchaseDate",
                table: "asset",
                newName: "purchase_date"
            );

            migrationBuilder.RenameColumn(name: "PersonId", table: "asset", newName: "person_id");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "asset",
                newName: "location_id"
            );

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "asset",
                newName: "last_modified"
            );

            migrationBuilder.RenameColumn(
                name: "InvoiceNumber",
                table: "asset",
                newName: "invoice_number"
            );

            migrationBuilder.RenameColumn(
                name: "InventoryNumber",
                table: "asset",
                newName: "inventory_number"
            );

            migrationBuilder.RenameColumn(
                name: "IntegrityId",
                table: "asset",
                newName: "integrity_id"
            );

            migrationBuilder.RenameColumn(
                name: "ImportanceId",
                table: "asset",
                newName: "importance_id"
            );

            migrationBuilder.RenameColumn(
                name: "ConfidentialityId",
                table: "asset",
                newName: "confidentiality_id"
            );

            migrationBuilder.RenameColumn(
                name: "BusinessEntityId",
                table: "asset",
                newName: "business_entity_id"
            );

            migrationBuilder.RenameColumn(
                name: "AvailabilityId",
                table: "asset",
                newName: "availability_id"
            );

            migrationBuilder.RenameColumn(
                name: "AssetTypeId",
                table: "asset",
                newName: "asset_type_id"
            );

            migrationBuilder.RenameColumn(
                name: "AssetSubstatusId",
                table: "asset",
                newName: "asset_substatus_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Asset_VendorId",
                table: "asset",
                newName: "ix_asset_vendor_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Asset_PersonId",
                table: "asset",
                newName: "ix_asset_person_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Asset_LocationId",
                table: "asset",
                newName: "ix_asset_location_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Asset_InventoryNumber",
                table: "asset",
                newName: "ix_asset_inventory_number"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Asset_IntegrityId",
                table: "asset",
                newName: "ix_asset_integrity_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Asset_ImportanceId",
                table: "asset",
                newName: "ix_asset_importance_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Asset_ConfidentialityId",
                table: "asset",
                newName: "ix_asset_confidentiality_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Asset_BusinessEntityId",
                table: "asset",
                newName: "ix_asset_business_entity_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Asset_AvailabilityId",
                table: "asset",
                newName: "ix_asset_availability_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Asset_AssetTypeId",
                table: "asset",
                newName: "ix_asset_asset_type_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Asset_AssetSubstatusId",
                table: "asset",
                newName: "ix_asset_asset_substatus_id"
            );

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "activation",
                newName: "quantity"
            );

            migrationBuilder.RenameColumn(name: "Id", table: "activation", newName: "id");

            migrationBuilder.RenameColumn(
                name: "VolumeId",
                table: "activation",
                newName: "volume_id"
            );

            migrationBuilder.RenameColumn(
                name: "PersonId",
                table: "activation",
                newName: "person_id"
            );

            migrationBuilder.RenameColumn(
                name: "DeactivationDate",
                table: "activation",
                newName: "deactivation_date"
            );

            migrationBuilder.RenameColumn(
                name: "AssetId",
                table: "activation",
                newName: "asset_id"
            );

            migrationBuilder.RenameColumn(
                name: "ActivationDate",
                table: "activation",
                newName: "activation_date"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Activation_VolumeId",
                table: "activation",
                newName: "ix_activation_volume_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Activation_PersonId",
                table: "activation",
                newName: "ix_activation_person_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_Activation_AssetId",
                table: "activation",
                newName: "ix_activation_asset_id"
            );

            migrationBuilder.RenameColumn(name: "Text", table: "volume_type", newName: "text");

            migrationBuilder.RenameColumn(name: "Id", table: "volume_type", newName: "id");

            migrationBuilder.RenameColumn(name: "Name", table: "virtual_machine", newName: "name");

            migrationBuilder.RenameColumn(name: "Id", table: "virtual_machine", newName: "id");

            migrationBuilder.RenameColumn(
                name: "IPAddress",
                table: "virtual_machine",
                newName: "ip_address"
            );

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "software_or_service_category",
                newName: "name"
            );

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "software_or_service_category",
                newName: "id"
            );

            migrationBuilder.RenameColumn(
                name: "Url",
                table: "software_or_service",
                newName: "url"
            );

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "software_or_service",
                newName: "name"
            );

            migrationBuilder.RenameColumn(name: "Id", table: "software_or_service", newName: "id");

            migrationBuilder.RenameColumn(
                name: "SoftwareOrServiceCategoryId",
                table: "software_or_service",
                newName: "software_or_service_category_id"
            );

            migrationBuilder.RenameColumn(
                name: "ManufacturerId",
                table: "software_or_service",
                newName: "manufacturer_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_SoftwareOrService_SoftwareOrServiceCategoryId",
                table: "software_or_service",
                newName: "ix_software_or_service_software_or_service_category_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_SoftwareOrService_ManufacturerId",
                table: "software_or_service",
                newName: "ix_software_or_service_manufacturer_id"
            );

            migrationBuilder.RenameColumn(name: "Text", table: "maintenance_type", newName: "text");

            migrationBuilder.RenameColumn(name: "Id", table: "maintenance_type", newName: "id");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "maintenance_contract",
                newName: "description"
            );

            migrationBuilder.RenameColumn(name: "Id", table: "maintenance_contract", newName: "id");

            migrationBuilder.RenameColumn(
                name: "VendorId",
                table: "maintenance_contract",
                newName: "vendor_id"
            );

            migrationBuilder.RenameColumn(
                name: "ServiceDueDate",
                table: "maintenance_contract",
                newName: "service_due_date"
            );

            migrationBuilder.RenameColumn(
                name: "MaintenanceTypeId",
                table: "maintenance_contract",
                newName: "maintenance_type_id"
            );

            migrationBuilder.RenameColumn(
                name: "ExpirationDate",
                table: "maintenance_contract",
                newName: "expiration_date"
            );

            migrationBuilder.RenameColumn(
                name: "ContractNumber",
                table: "maintenance_contract",
                newName: "contract_number"
            );

            migrationBuilder.RenameColumn(
                name: "ContactNumber",
                table: "maintenance_contract",
                newName: "contact_number"
            );

            migrationBuilder.RenameColumn(
                name: "ContactName",
                table: "maintenance_contract",
                newName: "contact_name"
            );

            migrationBuilder.RenameColumn(
                name: "ContactEmail",
                table: "maintenance_contract",
                newName: "contact_email"
            );

            migrationBuilder.RenameColumn(
                name: "AssetId",
                table: "maintenance_contract",
                newName: "asset_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceContract_VendorId",
                table: "maintenance_contract",
                newName: "ix_maintenance_contract_vendor_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceContract_MaintenanceTypeId",
                table: "maintenance_contract",
                newName: "ix_maintenance_contract_maintenance_type_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceContract_AssetId",
                table: "maintenance_contract",
                newName: "ix_maintenance_contract_asset_id"
            );

            migrationBuilder.RenameColumn(name: "Text", table: "license_type", newName: "text");

            migrationBuilder.RenameColumn(name: "Id", table: "license_type", newName: "id");

            migrationBuilder.RenameColumn(name: "Text", table: "license_model", newName: "text");

            migrationBuilder.RenameColumn(name: "Id", table: "license_model", newName: "id");

            migrationBuilder.RenameColumn(
                name: "Text",
                table: "license_expiration_model",
                newName: "text"
            );

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "license_expiration_model",
                newName: "id"
            );

            migrationBuilder.RenameColumn(name: "Text", table: "license_class", newName: "text");

            migrationBuilder.RenameColumn(name: "Id", table: "license_class", newName: "id");

            migrationBuilder.RenameColumn(name: "Text", table: "license_category", newName: "text");

            migrationBuilder.RenameColumn(name: "Id", table: "license_category", newName: "id");

            migrationBuilder.RenameColumn(name: "Name", table: "information_type", newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "information_type",
                newName: "description"
            );

            migrationBuilder.RenameColumn(name: "Id", table: "information_type", newName: "id");

            migrationBuilder.RenameColumn(
                name: "InformationTagId",
                table: "information_tag_information",
                newName: "information_tag_id"
            );

            migrationBuilder.RenameColumn(
                name: "InformationId",
                table: "information_tag_information",
                newName: "information_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_InformationTagInformation_InformationTagId",
                table: "information_tag_information",
                newName: "ix_information_tag_information_information_tag_id"
            );

            migrationBuilder.RenameColumn(name: "Name", table: "information_tag", newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "information_tag",
                newName: "description"
            );

            migrationBuilder.RenameColumn(name: "Id", table: "information_tag", newName: "id");

            migrationBuilder.RenameColumn(
                name: "URL",
                table: "information_location",
                newName: "url"
            );

            migrationBuilder.RenameColumn(name: "Id", table: "information_location", newName: "id");

            migrationBuilder.RenameColumn(
                name: "VirtualMachineId",
                table: "information_location",
                newName: "virtual_machine_id"
            );

            migrationBuilder.RenameColumn(
                name: "SoftwareId",
                table: "information_location",
                newName: "software_id"
            );

            migrationBuilder.RenameColumn(
                name: "PhysicalLocationId",
                table: "information_location",
                newName: "physical_location_id"
            );

            migrationBuilder.RenameColumn(
                name: "InformationId",
                table: "information_location",
                newName: "information_id"
            );

            migrationBuilder.RenameColumn(
                name: "ElectronicDeviceId",
                table: "information_location",
                newName: "electronic_device_id"
            );

            migrationBuilder.RenameColumn(
                name: "CloudId",
                table: "information_location",
                newName: "cloud_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_InformationLocation_VirtualMachineId",
                table: "information_location",
                newName: "ix_information_location_virtual_machine_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_InformationLocation_SoftwareId",
                table: "information_location",
                newName: "ix_information_location_software_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_InformationLocation_PhysicalLocationId",
                table: "information_location",
                newName: "ix_information_location_physical_location_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_InformationLocation_InformationId",
                table: "information_location",
                newName: "ix_information_location_information_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_InformationLocation_ElectronicDeviceId",
                table: "information_location",
                newName: "ix_information_location_electronic_device_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_InformationLocation_CloudId",
                table: "information_location",
                newName: "ix_information_location_cloud_id"
            );

            migrationBuilder.RenameColumn(name: "Name", table: "furniture_type", newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "furniture_type",
                newName: "description"
            );

            migrationBuilder.RenameColumn(name: "Id", table: "furniture_type", newName: "id");

            migrationBuilder.RenameColumn(
                name: "ElectronicDeviceTagId",
                table: "electronic_device_type_electronic_device_tag",
                newName: "electronic_device_tag_id"
            );

            migrationBuilder.RenameColumn(
                name: "ElectronicDeviceTypeId",
                table: "electronic_device_type_electronic_device_tag",
                newName: "electronic_device_type_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_ElectronicDeviceTypeElectronicDeviceTag_ElectronicDeviceTag~",
                table: "electronic_device_type_electronic_device_tag",
                newName: "ix_electronic_device_type_electronic_device_tag_electronic_dev"
            );

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "electronic_device_type",
                newName: "name"
            );

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "electronic_device_type",
                newName: "description"
            );

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "electronic_device_type",
                newName: "id"
            );

            migrationBuilder.RenameColumn(
                name: "HoldLicences",
                table: "electronic_device_type",
                newName: "hold_licences"
            );

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "electronic_device_tag",
                newName: "name"
            );

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "electronic_device_tag",
                newName: "description"
            );

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "electronic_device_tag",
                newName: "id"
            );

            migrationBuilder.RenameColumn(
                name: "SerialNumber",
                table: "electronic_device",
                newName: "serial_number"
            );

            migrationBuilder.RenameColumn(
                name: "ModelName",
                table: "electronic_device",
                newName: "model_name"
            );

            migrationBuilder.RenameColumn(
                name: "ModelCode",
                table: "electronic_device",
                newName: "model_code"
            );

            migrationBuilder.RenameColumn(
                name: "ManufacturingDate",
                table: "electronic_device",
                newName: "manufacturing_date"
            );

            migrationBuilder.RenameColumn(
                name: "ManufacturerId",
                table: "electronic_device",
                newName: "manufacturer_id"
            );

            migrationBuilder.RenameColumn(
                name: "GuaranteeNumber",
                table: "electronic_device",
                newName: "guarantee_number"
            );

            migrationBuilder.RenameColumn(
                name: "GuaranteeExpirationDate",
                table: "electronic_device",
                newName: "guarantee_expiration_date"
            );

            migrationBuilder.RenameColumn(
                name: "ElectronicDeviceTypeId",
                table: "electronic_device",
                newName: "electronic_device_type_id"
            );

            migrationBuilder.RenameColumn(
                name: "AssetId",
                table: "electronic_device",
                newName: "asset_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_ElectronicDevice_ManufacturerId",
                table: "electronic_device",
                newName: "ix_electronic_device_manufacturer_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_ElectronicDevice_ElectronicDeviceTypeId",
                table: "electronic_device",
                newName: "ix_electronic_device_electronic_device_type_id"
            );

            migrationBuilder.RenameColumn(name: "Text", table: "business_entity", newName: "text");

            migrationBuilder.RenameColumn(name: "Id", table: "business_entity", newName: "id");

            migrationBuilder.RenameColumn(name: "Table", table: "audit_log", newName: "table");

            migrationBuilder.RenameColumn(name: "Email", table: "audit_log", newName: "email");

            migrationBuilder.RenameColumn(name: "Id", table: "audit_log", newName: "id");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "audit_log",
                newName: "transaction_id"
            );

            migrationBuilder.RenameColumn(
                name: "TimeCreated",
                table: "audit_log",
                newName: "time_created"
            );

            migrationBuilder.RenameColumn(
                name: "OldValuesJson",
                table: "audit_log",
                newName: "old_values_json"
            );

            migrationBuilder.RenameColumn(
                name: "NewValuesJson",
                table: "audit_log",
                newName: "new_values_json"
            );

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "audit_log",
                newName: "entity_id"
            );

            migrationBuilder.RenameColumn(
                name: "ActionType",
                table: "audit_log",
                newName: "action_type"
            );

            migrationBuilder.RenameIndex(
                name: "IX_AuditLog_Table_EntityId",
                table: "audit_log",
                newName: "ix_audit_log_table_entity_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_AuditLog_EntityId",
                table: "audit_log",
                newName: "ix_audit_log_entity_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_AuditLog_Email",
                table: "audit_log",
                newName: "ix_audit_log_email"
            );

            migrationBuilder.RenameColumn(name: "Name", table: "asset_type", newName: "name");

            migrationBuilder.RenameColumn(name: "Id", table: "asset_type", newName: "id");

            migrationBuilder.RenameColumn(
                name: "AssetCategoryId",
                table: "asset_type",
                newName: "asset_category_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_AssetType_AssetCategoryId",
                table: "asset_type",
                newName: "ix_asset_type_asset_category_id"
            );

            migrationBuilder.RenameColumn(
                name: "Substatus",
                table: "asset_substatus",
                newName: "substatus"
            );

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "asset_substatus",
                newName: "description"
            );

            migrationBuilder.RenameColumn(name: "Id", table: "asset_substatus", newName: "id");

            migrationBuilder.RenameColumn(
                name: "AssetStatusId",
                table: "asset_substatus",
                newName: "asset_status_id"
            );

            migrationBuilder.RenameIndex(
                name: "IX_AssetSubstatus_AssetStatusId",
                table: "asset_substatus",
                newName: "ix_asset_substatus_asset_status_id"
            );

            migrationBuilder.RenameColumn(name: "Status", table: "asset_status", newName: "status");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "asset_status",
                newName: "description"
            );

            migrationBuilder.RenameColumn(name: "Id", table: "asset_status", newName: "id");

            migrationBuilder.RenameColumn(name: "Name", table: "asset_category", newName: "name");

            migrationBuilder.RenameColumn(name: "Id", table: "asset_category", newName: "id");

            migrationBuilder.AddPrimaryKey(name: "pk_volume", table: "volume", column: "id");

            migrationBuilder.AddPrimaryKey(name: "pk_vendor", table: "vendor", column: "id");

            migrationBuilder.AddPrimaryKey(name: "pk_state", table: "state", column: "id");

            migrationBuilder.AddPrimaryKey(name: "pk_software", table: "software", column: "id");

            migrationBuilder.AddPrimaryKey(name: "pk_sequence", table: "sequence", column: "id");

            migrationBuilder.AddPrimaryKey(name: "pk_project", table: "project", column: "id");

            migrationBuilder.AddPrimaryKey(name: "pk_person", table: "person", column: "id");

            migrationBuilder.AddPrimaryKey(name: "pk_period", table: "period", column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_manufacturer",
                table: "manufacturer",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(name: "pk_location", table: "location", column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_license",
                table: "license",
                column: "asset_id"
            );

            migrationBuilder.AddPrimaryKey(name: "pk_integrity", table: "integrity", column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_information",
                table: "information",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_importance",
                table: "importance",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_furniture",
                table: "furniture",
                column: "asset_id"
            );

            migrationBuilder.AddPrimaryKey(name: "pk_currency", table: "currency", column: "id");

            migrationBuilder.AddPrimaryKey(name: "pk_country", table: "country", column: "code");

            migrationBuilder.AddPrimaryKey(
                name: "pk_confidentiality",
                table: "confidentiality",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(name: "pk_cloud", table: "cloud", column: "id");

            migrationBuilder.AddPrimaryKey(name: "pk_client", table: "client", column: "id");

            migrationBuilder.AddPrimaryKey(name: "pk_city", table: "city", column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_availability",
                table: "availability",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(name: "pk_asset", table: "asset", column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_activation",
                table: "activation",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_volume_type",
                table: "volume_type",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_virtual_machine",
                table: "virtual_machine",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_software_or_service_category",
                table: "software_or_service_category",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_software_or_service",
                table: "software_or_service",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_maintenance_type",
                table: "maintenance_type",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_maintenance_contract",
                table: "maintenance_contract",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_license_type",
                table: "license_type",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_license_model",
                table: "license_model",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_license_expiration_model",
                table: "license_expiration_model",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_license_class",
                table: "license_class",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_license_category",
                table: "license_category",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_information_type",
                table: "information_type",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_information_tag_information",
                table: "information_tag_information",
                columns: new[] { "information_id", "information_tag_id" }
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_information_tag",
                table: "information_tag",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_information_location",
                table: "information_location",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_furniture_type",
                table: "furniture_type",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_electronic_device_type_electronic_device_tag",
                table: "electronic_device_type_electronic_device_tag",
                columns: new[] { "electronic_device_type_id", "electronic_device_tag_id" }
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_electronic_device_type",
                table: "electronic_device_type",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_electronic_device_tag",
                table: "electronic_device_tag",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_electronic_device",
                table: "electronic_device",
                column: "asset_id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_business_entity",
                table: "business_entity",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(name: "pk_audit_log", table: "audit_log", column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_asset_type",
                table: "asset_type",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_asset_substatus",
                table: "asset_substatus",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_asset_status",
                table: "asset_status",
                column: "id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "pk_asset_category",
                table: "asset_category",
                column: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_activation_assets_asset_id",
                table: "activation",
                column: "asset_id",
                principalTable: "asset",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_activation_persons_person_id",
                table: "activation",
                column: "person_id",
                principalTable: "person",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_activation_volumes_volume_id",
                table: "activation",
                column: "volume_id",
                principalTable: "volume",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_asset_asset_substatuses_asset_substatus_id",
                table: "asset",
                column: "asset_substatus_id",
                principalTable: "asset_substatus",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_asset_asset_types_asset_type_id",
                table: "asset",
                column: "asset_type_id",
                principalTable: "asset_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_asset_availabilities_availability_id",
                table: "asset",
                column: "availability_id",
                principalTable: "availability",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_asset_business_entities_business_entity_id",
                table: "asset",
                column: "business_entity_id",
                principalTable: "business_entity",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_asset_confidentialities_confidentiality_id",
                table: "asset",
                column: "confidentiality_id",
                principalTable: "confidentiality",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_asset_importances_importance_id",
                table: "asset",
                column: "importance_id",
                principalTable: "importance",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_asset_integrities_integrity_id",
                table: "asset",
                column: "integrity_id",
                principalTable: "integrity",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_asset_locations_location_id",
                table: "asset",
                column: "location_id",
                principalTable: "location",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_asset_persons_person_id",
                table: "asset",
                column: "person_id",
                principalTable: "person",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_asset_vendors_vendor_id",
                table: "asset",
                column: "vendor_id",
                principalTable: "vendor",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_asset_substatus_asset_status_asset_status_id",
                table: "asset_substatus",
                column: "asset_status_id",
                principalTable: "asset_status",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_asset_type_asset_category_asset_category_id",
                table: "asset_type",
                column: "asset_category_id",
                principalTable: "asset_category",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_city_countries_country_code",
                table: "city",
                column: "country_code",
                principalTable: "country",
                principalColumn: "code"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_cloud_volumes_volume_id",
                table: "cloud",
                column: "volume_id",
                principalTable: "volume",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_electronic_device_asset_asset_id",
                table: "electronic_device",
                column: "asset_id",
                principalTable: "asset",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_electronic_device_electronic_device_types_electronic_device",
                table: "electronic_device",
                column: "electronic_device_type_id",
                principalTable: "electronic_device_type",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_electronic_device_manufacturers_manufacturer_id",
                table: "electronic_device",
                column: "manufacturer_id",
                principalTable: "manufacturer",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_electronic_device_type_electronic_device_tag_electronic_dev",
                table: "electronic_device_type_electronic_device_tag",
                column: "electronic_device_tag_id",
                principalTable: "electronic_device_tag",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_electronic_device_type_electronic_device_tag_electronic_dev1",
                table: "electronic_device_type_electronic_device_tag",
                column: "electronic_device_type_id",
                principalTable: "electronic_device_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_furniture_asset_asset_id",
                table: "furniture",
                column: "asset_id",
                principalTable: "asset",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_furniture_furniture_types_furniture_type_id",
                table: "furniture",
                column: "furniture_type_id",
                principalTable: "furniture_type",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_information_availability_availability_id",
                table: "information",
                column: "availability_id",
                principalTable: "availability",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_information_confidentiality_confidentiality_id",
                table: "information",
                column: "confidentiality_id",
                principalTable: "confidentiality",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_information_importance_importance_id",
                table: "information",
                column: "importance_id",
                principalTable: "importance",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_information_information_types_information_type_id",
                table: "information",
                column: "information_type_id",
                principalTable: "information_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_information_integrities_integrity_id",
                table: "information",
                column: "integrity_id",
                principalTable: "integrity",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_information_persons_person_id",
                table: "information",
                column: "person_id",
                principalTable: "person",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_information_projects_project_id",
                table: "information",
                column: "project_id",
                principalTable: "project",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_information_location_cloud_cloud_id",
                table: "information_location",
                column: "cloud_id",
                principalTable: "cloud",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_information_location_electronic_device_electronic_device_id",
                table: "information_location",
                column: "electronic_device_id",
                principalTable: "electronic_device",
                principalColumn: "asset_id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_information_location_information_information_id",
                table: "information_location",
                column: "information_id",
                principalTable: "information",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_information_location_locations_physical_location_id",
                table: "information_location",
                column: "physical_location_id",
                principalTable: "location",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_information_location_softwares_software_id",
                table: "information_location",
                column: "software_id",
                principalTable: "software",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_information_location_virtual_machines_virtual_machine_id",
                table: "information_location",
                column: "virtual_machine_id",
                principalTable: "virtual_machine",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_information_tag_information_information_information_id",
                table: "information_tag_information",
                column: "information_id",
                principalTable: "information",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_information_tag_information_information_tag_information_tag",
                table: "information_tag_information",
                column: "information_tag_id",
                principalTable: "information_tag",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_license_asset_asset_id",
                table: "license",
                column: "asset_id",
                principalTable: "asset",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_license_currency_currency_id",
                table: "license",
                column: "currency_id",
                principalTable: "currency",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_license_license_expiration_models_license_expiration_model_",
                table: "license",
                column: "license_expiration_model_id",
                principalTable: "license_expiration_model",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_license_license_models_license_model_id",
                table: "license",
                column: "license_model_id",
                principalTable: "license_model",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_license_license_types_license_type_id",
                table: "license",
                column: "license_type_id",
                principalTable: "license_type",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_license_periods_period_id",
                table: "license",
                column: "period_id",
                principalTable: "period",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_location_city_city_id",
                table: "location",
                column: "city_id",
                principalTable: "city",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_location_country_country_code",
                table: "location",
                column: "country_code",
                principalTable: "country",
                principalColumn: "code"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_location_states_state_id",
                table: "location",
                column: "state_id",
                principalTable: "state",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_maintenance_contract_asset_asset_id",
                table: "maintenance_contract",
                column: "asset_id",
                principalTable: "asset",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_maintenance_contract_maintenance_types_maintenance_type_id",
                table: "maintenance_contract",
                column: "maintenance_type_id",
                principalTable: "maintenance_type",
                principalColumn: "id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_maintenance_contract_vendors_vendor_id",
                table: "maintenance_contract",
                column: "vendor_id",
                principalTable: "vendor",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_project_client_client_id",
                table: "project",
                column: "client_id",
                principalTable: "client",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_project_person_project_owner_id",
                table: "project",
                column: "project_owner_id",
                principalTable: "person",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_software_volumes_volume_id",
                table: "software",
                column: "volume_id",
                principalTable: "volume",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_software_or_service_manufacturer_manufacturer_id",
                table: "software_or_service",
                column: "manufacturer_id",
                principalTable: "manufacturer",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_software_or_service_software_or_service_categories_software",
                table: "software_or_service",
                column: "software_or_service_category_id",
                principalTable: "software_or_service_category",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_state_country_country_code",
                table: "state",
                column: "country_code",
                principalTable: "country",
                principalColumn: "code"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_volume_license_license_id",
                table: "volume",
                column: "license_id",
                principalTable: "license",
                principalColumn: "asset_id",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "fk_volume_software_or_service_software_or_service_id",
                table: "volume",
                column: "software_or_service_id",
                principalTable: "software_or_service",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "fk_volume_volume_types_volume_type_id",
                table: "volume",
                column: "volume_type_id",
                principalTable: "volume_type",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_activation_assets_asset_id",
                table: "activation"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_activation_persons_person_id",
                table: "activation"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_activation_volumes_volume_id",
                table: "activation"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_asset_asset_substatuses_asset_substatus_id",
                table: "asset"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_asset_asset_types_asset_type_id",
                table: "asset"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_asset_availabilities_availability_id",
                table: "asset"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_asset_business_entities_business_entity_id",
                table: "asset"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_asset_confidentialities_confidentiality_id",
                table: "asset"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_asset_importances_importance_id",
                table: "asset"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_asset_integrities_integrity_id",
                table: "asset"
            );

            migrationBuilder.DropForeignKey(name: "fk_asset_locations_location_id", table: "asset");

            migrationBuilder.DropForeignKey(name: "fk_asset_persons_person_id", table: "asset");

            migrationBuilder.DropForeignKey(name: "fk_asset_vendors_vendor_id", table: "asset");

            migrationBuilder.DropForeignKey(
                name: "fk_asset_substatus_asset_status_asset_status_id",
                table: "asset_substatus"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_asset_type_asset_category_asset_category_id",
                table: "asset_type"
            );

            migrationBuilder.DropForeignKey(name: "fk_city_countries_country_code", table: "city");

            migrationBuilder.DropForeignKey(name: "fk_cloud_volumes_volume_id", table: "cloud");

            migrationBuilder.DropForeignKey(
                name: "fk_electronic_device_asset_asset_id",
                table: "electronic_device"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_electronic_device_electronic_device_types_electronic_device",
                table: "electronic_device"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_electronic_device_manufacturers_manufacturer_id",
                table: "electronic_device"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_electronic_device_type_electronic_device_tag_electronic_dev",
                table: "electronic_device_type_electronic_device_tag"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_electronic_device_type_electronic_device_tag_electronic_dev1",
                table: "electronic_device_type_electronic_device_tag"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_furniture_asset_asset_id",
                table: "furniture"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_furniture_furniture_types_furniture_type_id",
                table: "furniture"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_information_availability_availability_id",
                table: "information"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_information_confidentiality_confidentiality_id",
                table: "information"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_information_importance_importance_id",
                table: "information"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_information_information_types_information_type_id",
                table: "information"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_information_integrities_integrity_id",
                table: "information"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_information_persons_person_id",
                table: "information"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_information_projects_project_id",
                table: "information"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_information_location_cloud_cloud_id",
                table: "information_location"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_information_location_electronic_device_electronic_device_id",
                table: "information_location"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_information_location_information_information_id",
                table: "information_location"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_information_location_locations_physical_location_id",
                table: "information_location"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_information_location_softwares_software_id",
                table: "information_location"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_information_location_virtual_machines_virtual_machine_id",
                table: "information_location"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_information_tag_information_information_information_id",
                table: "information_tag_information"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_information_tag_information_information_tag_information_tag",
                table: "information_tag_information"
            );

            migrationBuilder.DropForeignKey(name: "fk_license_asset_asset_id", table: "license");

            migrationBuilder.DropForeignKey(
                name: "fk_license_currency_currency_id",
                table: "license"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_license_license_expiration_models_license_expiration_model_",
                table: "license"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_license_license_models_license_model_id",
                table: "license"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_license_license_types_license_type_id",
                table: "license"
            );

            migrationBuilder.DropForeignKey(name: "fk_license_periods_period_id", table: "license");

            migrationBuilder.DropForeignKey(name: "fk_location_city_city_id", table: "location");

            migrationBuilder.DropForeignKey(
                name: "fk_location_country_country_code",
                table: "location"
            );

            migrationBuilder.DropForeignKey(name: "fk_location_states_state_id", table: "location");

            migrationBuilder.DropForeignKey(
                name: "fk_maintenance_contract_asset_asset_id",
                table: "maintenance_contract"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_maintenance_contract_maintenance_types_maintenance_type_id",
                table: "maintenance_contract"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_maintenance_contract_vendors_vendor_id",
                table: "maintenance_contract"
            );

            migrationBuilder.DropForeignKey(name: "fk_project_client_client_id", table: "project");

            migrationBuilder.DropForeignKey(
                name: "fk_project_person_project_owner_id",
                table: "project"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_software_volumes_volume_id",
                table: "software"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_software_or_service_manufacturer_manufacturer_id",
                table: "software_or_service"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_software_or_service_software_or_service_categories_software",
                table: "software_or_service"
            );

            migrationBuilder.DropForeignKey(name: "fk_state_country_country_code", table: "state");

            migrationBuilder.DropForeignKey(name: "fk_volume_license_license_id", table: "volume");

            migrationBuilder.DropForeignKey(
                name: "fk_volume_software_or_service_software_or_service_id",
                table: "volume"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_volume_volume_types_volume_type_id",
                table: "volume"
            );

            migrationBuilder.DropPrimaryKey(name: "pk_volume", table: "volume");

            migrationBuilder.DropPrimaryKey(name: "pk_vendor", table: "vendor");

            migrationBuilder.DropPrimaryKey(name: "pk_state", table: "state");

            migrationBuilder.DropPrimaryKey(name: "pk_software", table: "software");

            migrationBuilder.DropPrimaryKey(name: "pk_sequence", table: "sequence");

            migrationBuilder.DropPrimaryKey(name: "pk_project", table: "project");

            migrationBuilder.DropPrimaryKey(name: "pk_person", table: "person");

            migrationBuilder.DropPrimaryKey(name: "pk_period", table: "period");

            migrationBuilder.DropPrimaryKey(name: "pk_manufacturer", table: "manufacturer");

            migrationBuilder.DropPrimaryKey(name: "pk_location", table: "location");

            migrationBuilder.DropPrimaryKey(name: "pk_license", table: "license");

            migrationBuilder.DropPrimaryKey(name: "pk_integrity", table: "integrity");

            migrationBuilder.DropPrimaryKey(name: "pk_information", table: "information");

            migrationBuilder.DropPrimaryKey(name: "pk_importance", table: "importance");

            migrationBuilder.DropPrimaryKey(name: "pk_furniture", table: "furniture");

            migrationBuilder.DropPrimaryKey(name: "pk_currency", table: "currency");

            migrationBuilder.DropPrimaryKey(name: "pk_country", table: "country");

            migrationBuilder.DropPrimaryKey(name: "pk_confidentiality", table: "confidentiality");

            migrationBuilder.DropPrimaryKey(name: "pk_cloud", table: "cloud");

            migrationBuilder.DropPrimaryKey(name: "pk_client", table: "client");

            migrationBuilder.DropPrimaryKey(name: "pk_city", table: "city");

            migrationBuilder.DropPrimaryKey(name: "pk_availability", table: "availability");

            migrationBuilder.DropPrimaryKey(name: "pk_asset", table: "asset");

            migrationBuilder.DropPrimaryKey(name: "pk_activation", table: "activation");

            migrationBuilder.DropPrimaryKey(name: "pk_volume_type", table: "volume_type");

            migrationBuilder.DropPrimaryKey(name: "pk_virtual_machine", table: "virtual_machine");

            migrationBuilder.DropPrimaryKey(
                name: "pk_software_or_service_category",
                table: "software_or_service_category"
            );

            migrationBuilder.DropPrimaryKey(
                name: "pk_software_or_service",
                table: "software_or_service"
            );

            migrationBuilder.DropPrimaryKey(name: "pk_maintenance_type", table: "maintenance_type");

            migrationBuilder.DropPrimaryKey(
                name: "pk_maintenance_contract",
                table: "maintenance_contract"
            );

            migrationBuilder.DropPrimaryKey(name: "pk_license_type", table: "license_type");

            migrationBuilder.DropPrimaryKey(name: "pk_license_model", table: "license_model");

            migrationBuilder.DropPrimaryKey(
                name: "pk_license_expiration_model",
                table: "license_expiration_model"
            );

            migrationBuilder.DropPrimaryKey(name: "pk_license_class", table: "license_class");

            migrationBuilder.DropPrimaryKey(name: "pk_license_category", table: "license_category");

            migrationBuilder.DropPrimaryKey(name: "pk_information_type", table: "information_type");

            migrationBuilder.DropPrimaryKey(
                name: "pk_information_tag_information",
                table: "information_tag_information"
            );

            migrationBuilder.DropPrimaryKey(name: "pk_information_tag", table: "information_tag");

            migrationBuilder.DropPrimaryKey(
                name: "pk_information_location",
                table: "information_location"
            );

            migrationBuilder.DropPrimaryKey(name: "pk_furniture_type", table: "furniture_type");

            migrationBuilder.DropPrimaryKey(
                name: "pk_electronic_device_type_electronic_device_tag",
                table: "electronic_device_type_electronic_device_tag"
            );

            migrationBuilder.DropPrimaryKey(
                name: "pk_electronic_device_type",
                table: "electronic_device_type"
            );

            migrationBuilder.DropPrimaryKey(
                name: "pk_electronic_device_tag",
                table: "electronic_device_tag"
            );

            migrationBuilder.DropPrimaryKey(
                name: "pk_electronic_device",
                table: "electronic_device"
            );

            migrationBuilder.DropPrimaryKey(name: "pk_business_entity", table: "business_entity");

            migrationBuilder.DropPrimaryKey(name: "pk_audit_log", table: "audit_log");

            migrationBuilder.DropPrimaryKey(name: "pk_asset_type", table: "asset_type");

            migrationBuilder.DropPrimaryKey(name: "pk_asset_substatus", table: "asset_substatus");

            migrationBuilder.DropPrimaryKey(name: "pk_asset_status", table: "asset_status");

            migrationBuilder.DropPrimaryKey(name: "pk_asset_category", table: "asset_category");

            migrationBuilder.RenameTable(name: "volume", newName: "Volume");

            migrationBuilder.RenameTable(name: "vendor", newName: "Vendor");

            migrationBuilder.RenameTable(name: "state", newName: "State");

            migrationBuilder.RenameTable(name: "software", newName: "Software");

            migrationBuilder.RenameTable(name: "sequence", newName: "Sequence");

            migrationBuilder.RenameTable(name: "project", newName: "Project");

            migrationBuilder.RenameTable(name: "person", newName: "Person");

            migrationBuilder.RenameTable(name: "period", newName: "Period");

            migrationBuilder.RenameTable(name: "manufacturer", newName: "Manufacturer");

            migrationBuilder.RenameTable(name: "location", newName: "Location");

            migrationBuilder.RenameTable(name: "license", newName: "License");

            migrationBuilder.RenameTable(name: "integrity", newName: "Integrity");

            migrationBuilder.RenameTable(name: "information", newName: "Information");

            migrationBuilder.RenameTable(name: "importance", newName: "Importance");

            migrationBuilder.RenameTable(name: "furniture", newName: "Furniture");

            migrationBuilder.RenameTable(name: "currency", newName: "Currency");

            migrationBuilder.RenameTable(name: "country", newName: "Country");

            migrationBuilder.RenameTable(name: "confidentiality", newName: "Confidentiality");

            migrationBuilder.RenameTable(name: "cloud", newName: "Cloud");

            migrationBuilder.RenameTable(name: "client", newName: "Client");

            migrationBuilder.RenameTable(name: "city", newName: "City");

            migrationBuilder.RenameTable(name: "availability", newName: "Availability");

            migrationBuilder.RenameTable(name: "asset", newName: "Asset");

            migrationBuilder.RenameTable(name: "activation", newName: "Activation");

            migrationBuilder.RenameTable(name: "volume_type", newName: "VolumeType");

            migrationBuilder.RenameTable(name: "virtual_machine", newName: "VirtualMachine");

            migrationBuilder.RenameTable(
                name: "software_or_service_category",
                newName: "SoftwareOrServiceCategory"
            );

            migrationBuilder.RenameTable(name: "software_or_service", newName: "SoftwareOrService");

            migrationBuilder.RenameTable(name: "maintenance_type", newName: "MaintenanceType");

            migrationBuilder.RenameTable(
                name: "maintenance_contract",
                newName: "MaintenanceContract"
            );

            migrationBuilder.RenameTable(name: "license_type", newName: "LicenseType");

            migrationBuilder.RenameTable(name: "license_model", newName: "LicenseModel");

            migrationBuilder.RenameTable(
                name: "license_expiration_model",
                newName: "LicenseExpirationModel"
            );

            migrationBuilder.RenameTable(name: "license_class", newName: "LicenseClass");

            migrationBuilder.RenameTable(name: "license_category", newName: "LicenseCategory");

            migrationBuilder.RenameTable(name: "information_type", newName: "InformationType");

            migrationBuilder.RenameTable(
                name: "information_tag_information",
                newName: "InformationTagInformation"
            );

            migrationBuilder.RenameTable(name: "information_tag", newName: "InformationTag");

            migrationBuilder.RenameTable(
                name: "information_location",
                newName: "InformationLocation"
            );

            migrationBuilder.RenameTable(name: "furniture_type", newName: "FurnitureType");

            migrationBuilder.RenameTable(
                name: "electronic_device_type_electronic_device_tag",
                newName: "ElectronicDeviceTypeElectronicDeviceTag"
            );

            migrationBuilder.RenameTable(
                name: "electronic_device_type",
                newName: "ElectronicDeviceType"
            );

            migrationBuilder.RenameTable(
                name: "electronic_device_tag",
                newName: "ElectronicDeviceTag"
            );

            migrationBuilder.RenameTable(name: "electronic_device", newName: "ElectronicDevice");

            migrationBuilder.RenameTable(name: "business_entity", newName: "BusinessEntity");

            migrationBuilder.RenameTable(name: "audit_log", newName: "AuditLog");

            migrationBuilder.RenameTable(name: "asset_type", newName: "AssetType");

            migrationBuilder.RenameTable(name: "asset_substatus", newName: "AssetSubstatus");

            migrationBuilder.RenameTable(name: "asset_status", newName: "AssetStatus");

            migrationBuilder.RenameTable(name: "asset_category", newName: "AssetCategory");

            migrationBuilder.RenameColumn(name: "quantity", table: "Volume", newName: "Quantity");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Volume",
                newName: "Description"
            );

            migrationBuilder.RenameColumn(name: "id", table: "Volume", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "volume_type_id",
                table: "Volume",
                newName: "VolumeTypeId"
            );

            migrationBuilder.RenameColumn(
                name: "software_or_service_id",
                table: "Volume",
                newName: "SoftwareOrServiceId"
            );

            migrationBuilder.RenameColumn(
                name: "license_id",
                table: "Volume",
                newName: "LicenseId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_volume_volume_type_id",
                table: "Volume",
                newName: "IX_Volume_VolumeTypeId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_volume_software_or_service_id",
                table: "Volume",
                newName: "IX_Volume_SoftwareOrServiceId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_volume_license_id",
                table: "Volume",
                newName: "IX_Volume_LicenseId"
            );

            migrationBuilder.RenameColumn(name: "web", table: "Vendor", newName: "Web");

            migrationBuilder.RenameColumn(name: "phone", table: "Vendor", newName: "Phone");

            migrationBuilder.RenameColumn(name: "name", table: "Vendor", newName: "Name");

            migrationBuilder.RenameColumn(name: "location", table: "Vendor", newName: "Location");

            migrationBuilder.RenameColumn(name: "email", table: "Vendor", newName: "Email");

            migrationBuilder.RenameColumn(name: "id", table: "Vendor", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "vat_number",
                table: "Vendor",
                newName: "VATNumber"
            );

            migrationBuilder.RenameColumn(
                name: "registration_number",
                table: "Vendor",
                newName: "RegistrationNumber"
            );

            migrationBuilder.RenameColumn(
                name: "mobile_phone",
                table: "Vendor",
                newName: "MobilePhone"
            );

            migrationBuilder.RenameColumn(
                name: "contact_person",
                table: "Vendor",
                newName: "ContactPerson"
            );

            migrationBuilder.RenameColumn(name: "name", table: "State", newName: "Name");

            migrationBuilder.RenameColumn(name: "id", table: "State", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "country_code",
                table: "State",
                newName: "CountryCode"
            );

            migrationBuilder.RenameIndex(
                name: "ix_state_country_code",
                table: "State",
                newName: "IX_State_CountryCode"
            );

            migrationBuilder.RenameColumn(name: "name", table: "Software", newName: "Name");

            migrationBuilder.RenameColumn(name: "id", table: "Software", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "volume_id",
                table: "Software",
                newName: "VolumeId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_software_volume_id",
                table: "Software",
                newName: "IX_Software_VolumeId"
            );

            migrationBuilder.RenameColumn(name: "id", table: "Sequence", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "asset_inventory_number",
                table: "Sequence",
                newName: "AssetInventoryNumber"
            );

            migrationBuilder.RenameColumn(name: "name", table: "Project", newName: "Name");

            migrationBuilder.RenameColumn(name: "id", table: "Project", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "project_owner_id",
                table: "Project",
                newName: "ProjectOwnerId"
            );

            migrationBuilder.RenameColumn(name: "client_id", table: "Project", newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "ix_project_project_owner_id",
                table: "Project",
                newName: "IX_Project_ProjectOwnerId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_project_client_id",
                table: "Project",
                newName: "IX_Project_ClientId"
            );

            migrationBuilder.RenameColumn(name: "name", table: "Person", newName: "Name");

            migrationBuilder.RenameColumn(name: "email", table: "Person", newName: "Email");

            migrationBuilder.RenameColumn(name: "id", table: "Person", newName: "Id");

            migrationBuilder.RenameColumn(name: "text", table: "Period", newName: "Text");

            migrationBuilder.RenameColumn(name: "id", table: "Period", newName: "Id");

            migrationBuilder.RenameColumn(name: "url", table: "Manufacturer", newName: "URL");

            migrationBuilder.RenameColumn(name: "name", table: "Manufacturer", newName: "Name");

            migrationBuilder.RenameColumn(name: "id", table: "Manufacturer", newName: "Id");

            migrationBuilder.RenameColumn(name: "street", table: "Location", newName: "Street");

            migrationBuilder.RenameColumn(name: "room", table: "Location", newName: "Room");

            migrationBuilder.RenameColumn(name: "name", table: "Location", newName: "Name");

            migrationBuilder.RenameColumn(name: "floor", table: "Location", newName: "Floor");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Location",
                newName: "Description"
            );

            migrationBuilder.RenameColumn(name: "id", table: "Location", newName: "Id");

            migrationBuilder.RenameColumn(name: "state_id", table: "Location", newName: "StateId");

            migrationBuilder.RenameColumn(
                name: "postal_code",
                table: "Location",
                newName: "PostalCode"
            );

            migrationBuilder.RenameColumn(
                name: "house_number",
                table: "Location",
                newName: "HouseNumber"
            );

            migrationBuilder.RenameColumn(
                name: "country_code",
                table: "Location",
                newName: "CountryCode"
            );

            migrationBuilder.RenameColumn(name: "city_id", table: "Location", newName: "CityId");

            migrationBuilder.RenameIndex(
                name: "ix_location_state_id",
                table: "Location",
                newName: "IX_Location_StateId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_location_country_code",
                table: "Location",
                newName: "IX_Location_CountryCode"
            );

            migrationBuilder.RenameIndex(
                name: "ix_location_city_id",
                table: "Location",
                newName: "IX_Location_CityId"
            );

            migrationBuilder.RenameColumn(
                name: "subscription_fee",
                table: "License",
                newName: "SubscriptionFee"
            );

            migrationBuilder.RenameColumn(
                name: "subscription_expiration_date",
                table: "License",
                newName: "SubscriptionExpirationDate"
            );

            migrationBuilder.RenameColumn(
                name: "registration_number",
                table: "License",
                newName: "RegistrationNumber"
            );

            migrationBuilder.RenameColumn(name: "period_id", table: "License", newName: "PeriodId");

            migrationBuilder.RenameColumn(
                name: "management_console_url",
                table: "License",
                newName: "ManagementConsoleUrl"
            );

            migrationBuilder.RenameColumn(
                name: "license_type_id",
                table: "License",
                newName: "LicenseTypeId"
            );

            migrationBuilder.RenameColumn(
                name: "license_model_id",
                table: "License",
                newName: "LicenseModelId"
            );

            migrationBuilder.RenameColumn(
                name: "license_expiration_model_id",
                table: "License",
                newName: "LicenseExpirationModelId"
            );

            migrationBuilder.RenameColumn(
                name: "key_identifier",
                table: "License",
                newName: "KeyIdentifier"
            );

            migrationBuilder.RenameColumn(
                name: "currency_id",
                table: "License",
                newName: "CurrencyId"
            );

            migrationBuilder.RenameColumn(
                name: "auto_renew",
                table: "License",
                newName: "AutoRenew"
            );

            migrationBuilder.RenameColumn(name: "asset_id", table: "License", newName: "AssetId");

            migrationBuilder.RenameIndex(
                name: "ix_license_period_id",
                table: "License",
                newName: "IX_License_PeriodId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_license_license_type_id",
                table: "License",
                newName: "IX_License_LicenseTypeId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_license_license_model_id",
                table: "License",
                newName: "IX_License_LicenseModelId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_license_license_expiration_model_id",
                table: "License",
                newName: "IX_License_LicenseExpirationModelId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_license_currency_id",
                table: "License",
                newName: "IX_License_CurrencyId"
            );

            migrationBuilder.RenameColumn(name: "weight", table: "Integrity", newName: "Weight");

            migrationBuilder.RenameColumn(name: "level", table: "Integrity", newName: "Level");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Integrity",
                newName: "Description"
            );

            migrationBuilder.RenameColumn(name: "id", table: "Integrity", newName: "Id");

            migrationBuilder.RenameColumn(name: "note", table: "Information", newName: "Note");

            migrationBuilder.RenameColumn(name: "name", table: "Information", newName: "Name");

            migrationBuilder.RenameColumn(
                name: "incomplete",
                table: "Information",
                newName: "Incomplete"
            );

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Information",
                newName: "Description"
            );

            migrationBuilder.RenameColumn(name: "author", table: "Information", newName: "Author");

            migrationBuilder.RenameColumn(name: "id", table: "Information", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "project_id",
                table: "Information",
                newName: "ProjectId"
            );

            migrationBuilder.RenameColumn(
                name: "personal_information",
                table: "Information",
                newName: "PersonalInformation"
            );

            migrationBuilder.RenameColumn(
                name: "person_id",
                table: "Information",
                newName: "PersonId"
            );

            migrationBuilder.RenameColumn(
                name: "integrity_id",
                table: "Information",
                newName: "IntegrityId"
            );

            migrationBuilder.RenameColumn(
                name: "information_type_id",
                table: "Information",
                newName: "InformationTypeId"
            );

            migrationBuilder.RenameColumn(
                name: "importance_id",
                table: "Information",
                newName: "ImportanceId"
            );

            migrationBuilder.RenameColumn(
                name: "confidentiality_id",
                table: "Information",
                newName: "ConfidentialityId"
            );

            migrationBuilder.RenameColumn(
                name: "clients_personal_information",
                table: "Information",
                newName: "ClientsPersonalInformation"
            );

            migrationBuilder.RenameColumn(
                name: "availability_id",
                table: "Information",
                newName: "AvailabilityId"
            );

            migrationBuilder.RenameColumn(
                name: "access_rights",
                table: "Information",
                newName: "AccessRights"
            );

            migrationBuilder.RenameIndex(
                name: "ix_information_project_id",
                table: "Information",
                newName: "IX_Information_ProjectId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_information_person_id",
                table: "Information",
                newName: "IX_Information_PersonId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_information_integrity_id",
                table: "Information",
                newName: "IX_Information_IntegrityId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_information_information_type_id",
                table: "Information",
                newName: "IX_Information_InformationTypeId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_information_importance_id",
                table: "Information",
                newName: "IX_Information_ImportanceId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_information_confidentiality_id",
                table: "Information",
                newName: "IX_Information_ConfidentialityId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_information_availability_id",
                table: "Information",
                newName: "IX_Information_AvailabilityId"
            );

            migrationBuilder.RenameColumn(name: "level", table: "Importance", newName: "Level");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Importance",
                newName: "Description"
            );

            migrationBuilder.RenameColumn(name: "id", table: "Importance", newName: "Id");

            migrationBuilder.RenameColumn(name: "model", table: "Furniture", newName: "Model");

            migrationBuilder.RenameColumn(
                name: "furniture_type_id",
                table: "Furniture",
                newName: "FurnitureTypeId"
            );

            migrationBuilder.RenameColumn(name: "asset_id", table: "Furniture", newName: "AssetId");

            migrationBuilder.RenameIndex(
                name: "ix_furniture_furniture_type_id",
                table: "Furniture",
                newName: "IX_Furniture_FurnitureTypeId"
            );

            migrationBuilder.RenameColumn(name: "text", table: "Currency", newName: "Text");

            migrationBuilder.RenameColumn(name: "id", table: "Currency", newName: "Id");

            migrationBuilder.RenameColumn(name: "name", table: "Country", newName: "Name");

            migrationBuilder.RenameColumn(name: "code", table: "Country", newName: "Code");

            migrationBuilder.RenameColumn(
                name: "weight",
                table: "Confidentiality",
                newName: "Weight"
            );

            migrationBuilder.RenameColumn(
                name: "level",
                table: "Confidentiality",
                newName: "Level"
            );

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Confidentiality",
                newName: "Description"
            );

            migrationBuilder.RenameColumn(name: "id", table: "Confidentiality", newName: "Id");

            migrationBuilder.RenameColumn(name: "name", table: "Cloud", newName: "Name");

            migrationBuilder.RenameColumn(name: "id", table: "Cloud", newName: "Id");

            migrationBuilder.RenameColumn(name: "volume_id", table: "Cloud", newName: "VolumeId");

            migrationBuilder.RenameColumn(
                name: "management_url",
                table: "Cloud",
                newName: "ManagementURL"
            );

            migrationBuilder.RenameIndex(
                name: "ix_cloud_volume_id",
                table: "Cloud",
                newName: "IX_Cloud_VolumeId"
            );

            migrationBuilder.RenameColumn(name: "name", table: "Client", newName: "Name");

            migrationBuilder.RenameColumn(name: "id", table: "Client", newName: "Id");

            migrationBuilder.RenameColumn(name: "name", table: "City", newName: "Name");

            migrationBuilder.RenameColumn(name: "id", table: "City", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "country_code",
                table: "City",
                newName: "CountryCode"
            );

            migrationBuilder.RenameIndex(
                name: "ix_city_country_code",
                table: "City",
                newName: "IX_City_CountryCode"
            );

            migrationBuilder.RenameColumn(name: "weight", table: "Availability", newName: "Weight");

            migrationBuilder.RenameColumn(name: "level", table: "Availability", newName: "Level");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Availability",
                newName: "Description"
            );

            migrationBuilder.RenameColumn(name: "id", table: "Availability", newName: "Id");

            migrationBuilder.RenameColumn(name: "url", table: "Asset", newName: "URL");

            migrationBuilder.RenameColumn(name: "name", table: "Asset", newName: "Name");

            migrationBuilder.RenameColumn(
                name: "incomplete",
                table: "Asset",
                newName: "Incomplete"
            );

            migrationBuilder.RenameColumn(
                name: "description",
                table: "Asset",
                newName: "Description"
            );

            migrationBuilder.RenameColumn(name: "id", table: "Asset", newName: "Id");

            migrationBuilder.RenameColumn(name: "vendor_id", table: "Asset", newName: "VendorId");

            migrationBuilder.RenameColumn(
                name: "purchase_value",
                table: "Asset",
                newName: "PurchaseValue"
            );

            migrationBuilder.RenameColumn(
                name: "purchase_date",
                table: "Asset",
                newName: "PurchaseDate"
            );

            migrationBuilder.RenameColumn(name: "person_id", table: "Asset", newName: "PersonId");

            migrationBuilder.RenameColumn(
                name: "location_id",
                table: "Asset",
                newName: "LocationId"
            );

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Asset",
                newName: "LastModified"
            );

            migrationBuilder.RenameColumn(
                name: "invoice_number",
                table: "Asset",
                newName: "InvoiceNumber"
            );

            migrationBuilder.RenameColumn(
                name: "inventory_number",
                table: "Asset",
                newName: "InventoryNumber"
            );

            migrationBuilder.RenameColumn(
                name: "integrity_id",
                table: "Asset",
                newName: "IntegrityId"
            );

            migrationBuilder.RenameColumn(
                name: "importance_id",
                table: "Asset",
                newName: "ImportanceId"
            );

            migrationBuilder.RenameColumn(
                name: "confidentiality_id",
                table: "Asset",
                newName: "ConfidentialityId"
            );

            migrationBuilder.RenameColumn(
                name: "business_entity_id",
                table: "Asset",
                newName: "BusinessEntityId"
            );

            migrationBuilder.RenameColumn(
                name: "availability_id",
                table: "Asset",
                newName: "AvailabilityId"
            );

            migrationBuilder.RenameColumn(
                name: "asset_type_id",
                table: "Asset",
                newName: "AssetTypeId"
            );

            migrationBuilder.RenameColumn(
                name: "asset_substatus_id",
                table: "Asset",
                newName: "AssetSubstatusId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_asset_vendor_id",
                table: "Asset",
                newName: "IX_Asset_VendorId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_asset_person_id",
                table: "Asset",
                newName: "IX_Asset_PersonId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_asset_location_id",
                table: "Asset",
                newName: "IX_Asset_LocationId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_asset_inventory_number",
                table: "Asset",
                newName: "IX_Asset_InventoryNumber"
            );

            migrationBuilder.RenameIndex(
                name: "ix_asset_integrity_id",
                table: "Asset",
                newName: "IX_Asset_IntegrityId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_asset_importance_id",
                table: "Asset",
                newName: "IX_Asset_ImportanceId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_asset_confidentiality_id",
                table: "Asset",
                newName: "IX_Asset_ConfidentialityId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_asset_business_entity_id",
                table: "Asset",
                newName: "IX_Asset_BusinessEntityId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_asset_availability_id",
                table: "Asset",
                newName: "IX_Asset_AvailabilityId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_asset_asset_type_id",
                table: "Asset",
                newName: "IX_Asset_AssetTypeId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_asset_asset_substatus_id",
                table: "Asset",
                newName: "IX_Asset_AssetSubstatusId"
            );

            migrationBuilder.RenameColumn(
                name: "quantity",
                table: "Activation",
                newName: "Quantity"
            );

            migrationBuilder.RenameColumn(name: "id", table: "Activation", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "volume_id",
                table: "Activation",
                newName: "VolumeId"
            );

            migrationBuilder.RenameColumn(
                name: "person_id",
                table: "Activation",
                newName: "PersonId"
            );

            migrationBuilder.RenameColumn(
                name: "deactivation_date",
                table: "Activation",
                newName: "DeactivationDate"
            );

            migrationBuilder.RenameColumn(
                name: "asset_id",
                table: "Activation",
                newName: "AssetId"
            );

            migrationBuilder.RenameColumn(
                name: "activation_date",
                table: "Activation",
                newName: "ActivationDate"
            );

            migrationBuilder.RenameIndex(
                name: "ix_activation_volume_id",
                table: "Activation",
                newName: "IX_Activation_VolumeId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_activation_person_id",
                table: "Activation",
                newName: "IX_Activation_PersonId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_activation_asset_id",
                table: "Activation",
                newName: "IX_Activation_AssetId"
            );

            migrationBuilder.RenameColumn(name: "text", table: "VolumeType", newName: "Text");

            migrationBuilder.RenameColumn(name: "id", table: "VolumeType", newName: "Id");

            migrationBuilder.RenameColumn(name: "name", table: "VirtualMachine", newName: "Name");

            migrationBuilder.RenameColumn(name: "id", table: "VirtualMachine", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "ip_address",
                table: "VirtualMachine",
                newName: "IPAddress"
            );

            migrationBuilder.RenameColumn(
                name: "name",
                table: "SoftwareOrServiceCategory",
                newName: "Name"
            );

            migrationBuilder.RenameColumn(
                name: "id",
                table: "SoftwareOrServiceCategory",
                newName: "Id"
            );

            migrationBuilder.RenameColumn(name: "url", table: "SoftwareOrService", newName: "Url");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "SoftwareOrService",
                newName: "Name"
            );

            migrationBuilder.RenameColumn(name: "id", table: "SoftwareOrService", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "software_or_service_category_id",
                table: "SoftwareOrService",
                newName: "SoftwareOrServiceCategoryId"
            );

            migrationBuilder.RenameColumn(
                name: "manufacturer_id",
                table: "SoftwareOrService",
                newName: "ManufacturerId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_software_or_service_software_or_service_category_id",
                table: "SoftwareOrService",
                newName: "IX_SoftwareOrService_SoftwareOrServiceCategoryId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_software_or_service_manufacturer_id",
                table: "SoftwareOrService",
                newName: "IX_SoftwareOrService_ManufacturerId"
            );

            migrationBuilder.RenameColumn(name: "text", table: "MaintenanceType", newName: "Text");

            migrationBuilder.RenameColumn(name: "id", table: "MaintenanceType", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "MaintenanceContract",
                newName: "Description"
            );

            migrationBuilder.RenameColumn(name: "id", table: "MaintenanceContract", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "vendor_id",
                table: "MaintenanceContract",
                newName: "VendorId"
            );

            migrationBuilder.RenameColumn(
                name: "service_due_date",
                table: "MaintenanceContract",
                newName: "ServiceDueDate"
            );

            migrationBuilder.RenameColumn(
                name: "maintenance_type_id",
                table: "MaintenanceContract",
                newName: "MaintenanceTypeId"
            );

            migrationBuilder.RenameColumn(
                name: "expiration_date",
                table: "MaintenanceContract",
                newName: "ExpirationDate"
            );

            migrationBuilder.RenameColumn(
                name: "contract_number",
                table: "MaintenanceContract",
                newName: "ContractNumber"
            );

            migrationBuilder.RenameColumn(
                name: "contact_number",
                table: "MaintenanceContract",
                newName: "ContactNumber"
            );

            migrationBuilder.RenameColumn(
                name: "contact_name",
                table: "MaintenanceContract",
                newName: "ContactName"
            );

            migrationBuilder.RenameColumn(
                name: "contact_email",
                table: "MaintenanceContract",
                newName: "ContactEmail"
            );

            migrationBuilder.RenameColumn(
                name: "asset_id",
                table: "MaintenanceContract",
                newName: "AssetId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_maintenance_contract_vendor_id",
                table: "MaintenanceContract",
                newName: "IX_MaintenanceContract_VendorId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_maintenance_contract_maintenance_type_id",
                table: "MaintenanceContract",
                newName: "IX_MaintenanceContract_MaintenanceTypeId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_maintenance_contract_asset_id",
                table: "MaintenanceContract",
                newName: "IX_MaintenanceContract_AssetId"
            );

            migrationBuilder.RenameColumn(name: "text", table: "LicenseType", newName: "Text");

            migrationBuilder.RenameColumn(name: "id", table: "LicenseType", newName: "Id");

            migrationBuilder.RenameColumn(name: "text", table: "LicenseModel", newName: "Text");

            migrationBuilder.RenameColumn(name: "id", table: "LicenseModel", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "text",
                table: "LicenseExpirationModel",
                newName: "Text"
            );

            migrationBuilder.RenameColumn(
                name: "id",
                table: "LicenseExpirationModel",
                newName: "Id"
            );

            migrationBuilder.RenameColumn(name: "text", table: "LicenseClass", newName: "Text");

            migrationBuilder.RenameColumn(name: "id", table: "LicenseClass", newName: "Id");

            migrationBuilder.RenameColumn(name: "text", table: "LicenseCategory", newName: "Text");

            migrationBuilder.RenameColumn(name: "id", table: "LicenseCategory", newName: "Id");

            migrationBuilder.RenameColumn(name: "name", table: "InformationType", newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "InformationType",
                newName: "Description"
            );

            migrationBuilder.RenameColumn(name: "id", table: "InformationType", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "information_tag_id",
                table: "InformationTagInformation",
                newName: "InformationTagId"
            );

            migrationBuilder.RenameColumn(
                name: "information_id",
                table: "InformationTagInformation",
                newName: "InformationId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_information_tag_information_information_tag_id",
                table: "InformationTagInformation",
                newName: "IX_InformationTagInformation_InformationTagId"
            );

            migrationBuilder.RenameColumn(name: "name", table: "InformationTag", newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "InformationTag",
                newName: "Description"
            );

            migrationBuilder.RenameColumn(name: "id", table: "InformationTag", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "url",
                table: "InformationLocation",
                newName: "URL"
            );

            migrationBuilder.RenameColumn(name: "id", table: "InformationLocation", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "virtual_machine_id",
                table: "InformationLocation",
                newName: "VirtualMachineId"
            );

            migrationBuilder.RenameColumn(
                name: "software_id",
                table: "InformationLocation",
                newName: "SoftwareId"
            );

            migrationBuilder.RenameColumn(
                name: "physical_location_id",
                table: "InformationLocation",
                newName: "PhysicalLocationId"
            );

            migrationBuilder.RenameColumn(
                name: "information_id",
                table: "InformationLocation",
                newName: "InformationId"
            );

            migrationBuilder.RenameColumn(
                name: "electronic_device_id",
                table: "InformationLocation",
                newName: "ElectronicDeviceId"
            );

            migrationBuilder.RenameColumn(
                name: "cloud_id",
                table: "InformationLocation",
                newName: "CloudId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_information_location_virtual_machine_id",
                table: "InformationLocation",
                newName: "IX_InformationLocation_VirtualMachineId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_information_location_software_id",
                table: "InformationLocation",
                newName: "IX_InformationLocation_SoftwareId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_information_location_physical_location_id",
                table: "InformationLocation",
                newName: "IX_InformationLocation_PhysicalLocationId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_information_location_information_id",
                table: "InformationLocation",
                newName: "IX_InformationLocation_InformationId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_information_location_electronic_device_id",
                table: "InformationLocation",
                newName: "IX_InformationLocation_ElectronicDeviceId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_information_location_cloud_id",
                table: "InformationLocation",
                newName: "IX_InformationLocation_CloudId"
            );

            migrationBuilder.RenameColumn(name: "name", table: "FurnitureType", newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "FurnitureType",
                newName: "Description"
            );

            migrationBuilder.RenameColumn(name: "id", table: "FurnitureType", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "electronic_device_tag_id",
                table: "ElectronicDeviceTypeElectronicDeviceTag",
                newName: "ElectronicDeviceTagId"
            );

            migrationBuilder.RenameColumn(
                name: "electronic_device_type_id",
                table: "ElectronicDeviceTypeElectronicDeviceTag",
                newName: "ElectronicDeviceTypeId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_electronic_device_type_electronic_device_tag_electronic_dev",
                table: "ElectronicDeviceTypeElectronicDeviceTag",
                newName: "IX_ElectronicDeviceTypeElectronicDeviceTag_ElectronicDeviceTag~"
            );

            migrationBuilder.RenameColumn(
                name: "name",
                table: "ElectronicDeviceType",
                newName: "Name"
            );

            migrationBuilder.RenameColumn(
                name: "description",
                table: "ElectronicDeviceType",
                newName: "Description"
            );

            migrationBuilder.RenameColumn(name: "id", table: "ElectronicDeviceType", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "hold_licences",
                table: "ElectronicDeviceType",
                newName: "HoldLicences"
            );

            migrationBuilder.RenameColumn(
                name: "name",
                table: "ElectronicDeviceTag",
                newName: "Name"
            );

            migrationBuilder.RenameColumn(
                name: "description",
                table: "ElectronicDeviceTag",
                newName: "Description"
            );

            migrationBuilder.RenameColumn(name: "id", table: "ElectronicDeviceTag", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "serial_number",
                table: "ElectronicDevice",
                newName: "SerialNumber"
            );

            migrationBuilder.RenameColumn(
                name: "model_name",
                table: "ElectronicDevice",
                newName: "ModelName"
            );

            migrationBuilder.RenameColumn(
                name: "model_code",
                table: "ElectronicDevice",
                newName: "ModelCode"
            );

            migrationBuilder.RenameColumn(
                name: "manufacturing_date",
                table: "ElectronicDevice",
                newName: "ManufacturingDate"
            );

            migrationBuilder.RenameColumn(
                name: "manufacturer_id",
                table: "ElectronicDevice",
                newName: "ManufacturerId"
            );

            migrationBuilder.RenameColumn(
                name: "guarantee_number",
                table: "ElectronicDevice",
                newName: "GuaranteeNumber"
            );

            migrationBuilder.RenameColumn(
                name: "guarantee_expiration_date",
                table: "ElectronicDevice",
                newName: "GuaranteeExpirationDate"
            );

            migrationBuilder.RenameColumn(
                name: "electronic_device_type_id",
                table: "ElectronicDevice",
                newName: "ElectronicDeviceTypeId"
            );

            migrationBuilder.RenameColumn(
                name: "asset_id",
                table: "ElectronicDevice",
                newName: "AssetId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_electronic_device_manufacturer_id",
                table: "ElectronicDevice",
                newName: "IX_ElectronicDevice_ManufacturerId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_electronic_device_electronic_device_type_id",
                table: "ElectronicDevice",
                newName: "IX_ElectronicDevice_ElectronicDeviceTypeId"
            );

            migrationBuilder.RenameColumn(name: "text", table: "BusinessEntity", newName: "Text");

            migrationBuilder.RenameColumn(name: "id", table: "BusinessEntity", newName: "Id");

            migrationBuilder.RenameColumn(name: "table", table: "AuditLog", newName: "Table");

            migrationBuilder.RenameColumn(name: "email", table: "AuditLog", newName: "Email");

            migrationBuilder.RenameColumn(name: "id", table: "AuditLog", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "transaction_id",
                table: "AuditLog",
                newName: "TransactionId"
            );

            migrationBuilder.RenameColumn(
                name: "time_created",
                table: "AuditLog",
                newName: "TimeCreated"
            );

            migrationBuilder.RenameColumn(
                name: "old_values_json",
                table: "AuditLog",
                newName: "OldValuesJson"
            );

            migrationBuilder.RenameColumn(
                name: "new_values_json",
                table: "AuditLog",
                newName: "NewValuesJson"
            );

            migrationBuilder.RenameColumn(
                name: "entity_id",
                table: "AuditLog",
                newName: "EntityId"
            );

            migrationBuilder.RenameColumn(
                name: "action_type",
                table: "AuditLog",
                newName: "ActionType"
            );

            migrationBuilder.RenameIndex(
                name: "ix_audit_log_table_entity_id",
                table: "AuditLog",
                newName: "IX_AuditLog_Table_EntityId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_audit_log_entity_id",
                table: "AuditLog",
                newName: "IX_AuditLog_EntityId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_audit_log_email",
                table: "AuditLog",
                newName: "IX_AuditLog_Email"
            );

            migrationBuilder.RenameColumn(name: "name", table: "AssetType", newName: "Name");

            migrationBuilder.RenameColumn(name: "id", table: "AssetType", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "asset_category_id",
                table: "AssetType",
                newName: "AssetCategoryId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_asset_type_asset_category_id",
                table: "AssetType",
                newName: "IX_AssetType_AssetCategoryId"
            );

            migrationBuilder.RenameColumn(
                name: "substatus",
                table: "AssetSubstatus",
                newName: "Substatus"
            );

            migrationBuilder.RenameColumn(
                name: "description",
                table: "AssetSubstatus",
                newName: "Description"
            );

            migrationBuilder.RenameColumn(name: "id", table: "AssetSubstatus", newName: "Id");

            migrationBuilder.RenameColumn(
                name: "asset_status_id",
                table: "AssetSubstatus",
                newName: "AssetStatusId"
            );

            migrationBuilder.RenameIndex(
                name: "ix_asset_substatus_asset_status_id",
                table: "AssetSubstatus",
                newName: "IX_AssetSubstatus_AssetStatusId"
            );

            migrationBuilder.RenameColumn(name: "status", table: "AssetStatus", newName: "Status");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "AssetStatus",
                newName: "Description"
            );

            migrationBuilder.RenameColumn(name: "id", table: "AssetStatus", newName: "Id");

            migrationBuilder.RenameColumn(name: "name", table: "AssetCategory", newName: "Name");

            migrationBuilder.RenameColumn(name: "id", table: "AssetCategory", newName: "Id");

            migrationBuilder.AddPrimaryKey(name: "PK_Volume", table: "Volume", column: "Id");

            migrationBuilder.AddPrimaryKey(name: "PK_Vendor", table: "Vendor", column: "Id");

            migrationBuilder.AddPrimaryKey(name: "PK_State", table: "State", column: "Id");

            migrationBuilder.AddPrimaryKey(name: "PK_Software", table: "Software", column: "Id");

            migrationBuilder.AddPrimaryKey(name: "PK_Sequence", table: "Sequence", column: "Id");

            migrationBuilder.AddPrimaryKey(name: "PK_Project", table: "Project", column: "Id");

            migrationBuilder.AddPrimaryKey(name: "PK_Person", table: "Person", column: "Id");

            migrationBuilder.AddPrimaryKey(name: "PK_Period", table: "Period", column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Manufacturer",
                table: "Manufacturer",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(name: "PK_Location", table: "Location", column: "Id");

            migrationBuilder.AddPrimaryKey(name: "PK_License", table: "License", column: "AssetId");

            migrationBuilder.AddPrimaryKey(name: "PK_Integrity", table: "Integrity", column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Information",
                table: "Information",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_Importance",
                table: "Importance",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_Furniture",
                table: "Furniture",
                column: "AssetId"
            );

            migrationBuilder.AddPrimaryKey(name: "PK_Currency", table: "Currency", column: "Id");

            migrationBuilder.AddPrimaryKey(name: "PK_Country", table: "Country", column: "Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Confidentiality",
                table: "Confidentiality",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(name: "PK_Cloud", table: "Cloud", column: "Id");

            migrationBuilder.AddPrimaryKey(name: "PK_Client", table: "Client", column: "Id");

            migrationBuilder.AddPrimaryKey(name: "PK_City", table: "City", column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Availability",
                table: "Availability",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(name: "PK_Asset", table: "Asset", column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Activation",
                table: "Activation",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_VolumeType",
                table: "VolumeType",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_VirtualMachine",
                table: "VirtualMachine",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_SoftwareOrServiceCategory",
                table: "SoftwareOrServiceCategory",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_SoftwareOrService",
                table: "SoftwareOrService",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_MaintenanceType",
                table: "MaintenanceType",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_MaintenanceContract",
                table: "MaintenanceContract",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_LicenseType",
                table: "LicenseType",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_LicenseModel",
                table: "LicenseModel",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_LicenseExpirationModel",
                table: "LicenseExpirationModel",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_LicenseClass",
                table: "LicenseClass",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_LicenseCategory",
                table: "LicenseCategory",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_InformationType",
                table: "InformationType",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_InformationTagInformation",
                table: "InformationTagInformation",
                columns: new[] { "InformationId", "InformationTagId" }
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_InformationTag",
                table: "InformationTag",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_InformationLocation",
                table: "InformationLocation",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_FurnitureType",
                table: "FurnitureType",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_ElectronicDeviceTypeElectronicDeviceTag",
                table: "ElectronicDeviceTypeElectronicDeviceTag",
                columns: new[] { "ElectronicDeviceTypeId", "ElectronicDeviceTagId" }
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_ElectronicDeviceType",
                table: "ElectronicDeviceType",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_ElectronicDeviceTag",
                table: "ElectronicDeviceTag",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_ElectronicDevice",
                table: "ElectronicDevice",
                column: "AssetId"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_BusinessEntity",
                table: "BusinessEntity",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(name: "PK_AuditLog", table: "AuditLog", column: "Id");

            migrationBuilder.AddPrimaryKey(name: "PK_AssetType", table: "AssetType", column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AssetSubstatus",
                table: "AssetSubstatus",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_AssetStatus",
                table: "AssetStatus",
                column: "Id"
            );

            migrationBuilder.AddPrimaryKey(
                name: "PK_AssetCategory",
                table: "AssetCategory",
                column: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Activation_Asset_AssetId",
                table: "Activation",
                column: "AssetId",
                principalTable: "Asset",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Activation_Person_PersonId",
                table: "Activation",
                column: "PersonId",
                principalTable: "Person",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Activation_Volume_VolumeId",
                table: "Activation",
                column: "VolumeId",
                principalTable: "Volume",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_AssetSubstatus_AssetSubstatusId",
                table: "Asset",
                column: "AssetSubstatusId",
                principalTable: "AssetSubstatus",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_AssetType_AssetTypeId",
                table: "Asset",
                column: "AssetTypeId",
                principalTable: "AssetType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Availability_AvailabilityId",
                table: "Asset",
                column: "AvailabilityId",
                principalTable: "Availability",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_BusinessEntity_BusinessEntityId",
                table: "Asset",
                column: "BusinessEntityId",
                principalTable: "BusinessEntity",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Confidentiality_ConfidentialityId",
                table: "Asset",
                column: "ConfidentialityId",
                principalTable: "Confidentiality",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Importance_ImportanceId",
                table: "Asset",
                column: "ImportanceId",
                principalTable: "Importance",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Integrity_IntegrityId",
                table: "Asset",
                column: "IntegrityId",
                principalTable: "Integrity",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Location_LocationId",
                table: "Asset",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Person_PersonId",
                table: "Asset",
                column: "PersonId",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Asset_Vendor_VendorId",
                table: "Asset",
                column: "VendorId",
                principalTable: "Vendor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_AssetSubstatus_AssetStatus_AssetStatusId",
                table: "AssetSubstatus",
                column: "AssetStatusId",
                principalTable: "AssetStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_AssetType_AssetCategory_AssetCategoryId",
                table: "AssetType",
                column: "AssetCategoryId",
                principalTable: "AssetCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_City_Country_CountryCode",
                table: "City",
                column: "CountryCode",
                principalTable: "Country",
                principalColumn: "Code"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Cloud_Volume_VolumeId",
                table: "Cloud",
                column: "VolumeId",
                principalTable: "Volume",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ElectronicDevice_Asset_AssetId",
                table: "ElectronicDevice",
                column: "AssetId",
                principalTable: "Asset",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ElectronicDevice_ElectronicDeviceType_ElectronicDeviceTypeId",
                table: "ElectronicDevice",
                column: "ElectronicDeviceTypeId",
                principalTable: "ElectronicDeviceType",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ElectronicDevice_Manufacturer_ManufacturerId",
                table: "ElectronicDevice",
                column: "ManufacturerId",
                principalTable: "Manufacturer",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ElectronicDeviceTypeElectronicDeviceTag_ElectronicDeviceTag~",
                table: "ElectronicDeviceTypeElectronicDeviceTag",
                column: "ElectronicDeviceTagId",
                principalTable: "ElectronicDeviceTag",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_ElectronicDeviceTypeElectronicDeviceTag_ElectronicDeviceTyp~",
                table: "ElectronicDeviceTypeElectronicDeviceTag",
                column: "ElectronicDeviceTypeId",
                principalTable: "ElectronicDeviceType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Furniture_Asset_AssetId",
                table: "Furniture",
                column: "AssetId",
                principalTable: "Asset",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Furniture_FurnitureType_FurnitureTypeId",
                table: "Furniture",
                column: "FurnitureTypeId",
                principalTable: "FurnitureType",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Information_Availability_AvailabilityId",
                table: "Information",
                column: "AvailabilityId",
                principalTable: "Availability",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Information_Confidentiality_ConfidentialityId",
                table: "Information",
                column: "ConfidentialityId",
                principalTable: "Confidentiality",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Information_Importance_ImportanceId",
                table: "Information",
                column: "ImportanceId",
                principalTable: "Importance",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Information_InformationType_InformationTypeId",
                table: "Information",
                column: "InformationTypeId",
                principalTable: "InformationType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Information_Integrity_IntegrityId",
                table: "Information",
                column: "IntegrityId",
                principalTable: "Integrity",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Information_Person_PersonId",
                table: "Information",
                column: "PersonId",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Information_Project_ProjectId",
                table: "Information",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_InformationLocation_Cloud_CloudId",
                table: "InformationLocation",
                column: "CloudId",
                principalTable: "Cloud",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_InformationLocation_ElectronicDevice_ElectronicDeviceId",
                table: "InformationLocation",
                column: "ElectronicDeviceId",
                principalTable: "ElectronicDevice",
                principalColumn: "AssetId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_InformationLocation_Information_InformationId",
                table: "InformationLocation",
                column: "InformationId",
                principalTable: "Information",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_InformationLocation_Location_PhysicalLocationId",
                table: "InformationLocation",
                column: "PhysicalLocationId",
                principalTable: "Location",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_InformationLocation_Software_SoftwareId",
                table: "InformationLocation",
                column: "SoftwareId",
                principalTable: "Software",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_InformationLocation_VirtualMachine_VirtualMachineId",
                table: "InformationLocation",
                column: "VirtualMachineId",
                principalTable: "VirtualMachine",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_InformationTagInformation_InformationTag_InformationTagId",
                table: "InformationTagInformation",
                column: "InformationTagId",
                principalTable: "InformationTag",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_InformationTagInformation_Information_InformationId",
                table: "InformationTagInformation",
                column: "InformationId",
                principalTable: "Information",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_Asset_AssetId",
                table: "License",
                column: "AssetId",
                principalTable: "Asset",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_Currency_CurrencyId",
                table: "License",
                column: "CurrencyId",
                principalTable: "Currency",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_LicenseExpirationModel_LicenseExpirationModelId",
                table: "License",
                column: "LicenseExpirationModelId",
                principalTable: "LicenseExpirationModel",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_LicenseModel_LicenseModelId",
                table: "License",
                column: "LicenseModelId",
                principalTable: "LicenseModel",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_LicenseType_LicenseTypeId",
                table: "License",
                column: "LicenseTypeId",
                principalTable: "LicenseType",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_License_Period_PeriodId",
                table: "License",
                column: "PeriodId",
                principalTable: "Period",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Location_City_CityId",
                table: "Location",
                column: "CityId",
                principalTable: "City",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Location_Country_CountryCode",
                table: "Location",
                column: "CountryCode",
                principalTable: "Country",
                principalColumn: "Code"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Location_State_StateId",
                table: "Location",
                column: "StateId",
                principalTable: "State",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceContract_Asset_AssetId",
                table: "MaintenanceContract",
                column: "AssetId",
                principalTable: "Asset",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceContract_MaintenanceType_MaintenanceTypeId",
                table: "MaintenanceContract",
                column: "MaintenanceTypeId",
                principalTable: "MaintenanceType",
                principalColumn: "Id"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceContract_Vendor_VendorId",
                table: "MaintenanceContract",
                column: "VendorId",
                principalTable: "Vendor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Client_ClientId",
                table: "Project",
                column: "ClientId",
                principalTable: "Client",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Person_ProjectOwnerId",
                table: "Project",
                column: "ProjectOwnerId",
                principalTable: "Person",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Software_Volume_VolumeId",
                table: "Software",
                column: "VolumeId",
                principalTable: "Volume",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareOrService_Manufacturer_ManufacturerId",
                table: "SoftwareOrService",
                column: "ManufacturerId",
                principalTable: "Manufacturer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_SoftwareOrService_SoftwareOrServiceCategory_SoftwareOrServi~",
                table: "SoftwareOrService",
                column: "SoftwareOrServiceCategoryId",
                principalTable: "SoftwareOrServiceCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_State_Country_CountryCode",
                table: "State",
                column: "CountryCode",
                principalTable: "Country",
                principalColumn: "Code"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Volume_License_LicenseId",
                table: "Volume",
                column: "LicenseId",
                principalTable: "License",
                principalColumn: "AssetId",
                onDelete: ReferentialAction.Restrict
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Volume_SoftwareOrService_SoftwareOrServiceId",
                table: "Volume",
                column: "SoftwareOrServiceId",
                principalTable: "SoftwareOrService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );

            migrationBuilder.AddForeignKey(
                name: "FK_Volume_VolumeType_VolumeTypeId",
                table: "Volume",
                column: "VolumeTypeId",
                principalTable: "VolumeType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }
    }
}
