# Service Management

## About

This package contains a framework (data model) together with a couple of sample applications that you can use to **manage the end-to-end lifecycle of services** within your organization. The end-to-end service lifecycle within the framework aims to handle design, testing, ordering, inventorisation, orchestration, assurance, change and delete of services, up to costing and billing. In order to offer a modular architecture in line with industry standards, the framework is based on [TM Forum Open APIs](https://www.tmforum.org/oda/open-apis/directory). The current state of the framework is focussed on:

- Design (Service Specifications)
- Ordering (Service Order Portal)
- Inventorisation (Service Inventory)
- Other aspects of the service lifecycle will be handled by gradually added features

Services within this framework can servce multiple **use-cases** in different industries:

- **Satellite**
	- Uplink and Downlink Services
	- VSAT terminals
- **Media & Broadcast**
	- Event Management
	- Channel Management
- **Telecommunciations**
	- Fixed Network Services
	- Mobile Services
 - **IT**
	- Software Services
	- Hardware Services
	- Security Services

> [!NOTE]
> The package makes use of MediaOps as reservation and orchestration layer for the Service Item components within the Services in this framework.


## Key Features

### Future TMF compliance 

The goal is to make Service Managmenent framework within DataMiner compatible with [TM Forum APIs](https://www.tmforum.org/oda/open-apis/directory). TM Forum is a global industry association for service providers and their suppliers in the telecommunications and digital services sectors. Its main purpose is to provide standardized frameworks (including APIs) and collaborative tools that help companies reduce complexity, improve interoperability, and accelerate service delivery.

### Service Catalog

The Service Catalog allows to define Service Specifications. A **Service Specification** is an end-to-end description of what a Service of a specific type should consist of. The specification contains one or more Service Items which can be either xOps Workflows (resulting in Jobs) or references to SRM Booking managers (resulting in SRM Bookings). Next to the Service Items, the Specification allows to define the properties and configurations required. These properties and configuration parameters are used in Operation to orchestrate and inventorise the Services delivered.

![Service Catalog](./Images/service_catalog_list.png)

![Service Catalog Specification](./Images/service_catalog_specifications.png)

### Service Ordering Portal

The Service Ordering Portal app allow to create and view Service Orders. Service Orders can either be created manual or using [TM Forum](https://www.tmforum.org/oda/open-apis/directory/service-ordering-management-api-TMF641/) compatbile APIs. Through a Service Order Item, an actual Service is ordered, based on what is defined in the Service Specification in the Service Catalog.

The Order will pass through a statefull lifecycle and will be updated when the actual Services that are created in the Service Inventory evolve.

![Service Ordering Portal](./Images/service_order_portal_list.png)

![Service Ordering Portal Order](./Images/service_order_portal_instance.png)

### Service Inventory

The Service Inventory application provides an overview of all the Services in the system. Services can either be created directly from the Inventory app, through API or originating from an Order in the Service Ordering Portal. A Service contains one or more Service Items (Workflows or SRM Bookings) as defined in the Service Specfication that was used to initiate the Service. The Service will also inherit all the properties and configurations from a Service Specification. 

![Service Inventory](./Images/service_inventory_list.png)

![Service Inventory Instance](./Images/service_inventory_service.png)

## Prerequisites

- MediaOps version 1.2.3 (exact version): can be deployed directly from the [Catalog](https://catalog.dataminer.services/details/1b67a623-4ca6-4d25-8b3d-ed4e39496a75).

## Pricing

The applications part of this package will consume DataMiner credits, based on the level of usage of the apps. The DataMiner credits will be deducted monthly based on the metered usage. More information about the pricing of DataMiner usage-based services can be found in the [DataMiner Pricing Overview](https://docs.dataminer.services/dataminer-overview/Pricing/Pricing_Usage_based_service.html). 

## Support

For additional help or to discuss additional use-cases, reach out to [Skyline Product Marketing](mailto:team.product.marketing@skyline.be).
