# Service Management

## About

This package contains multiple apps and a framework that you can use to manage the end-to-end life cycle of Service within your organization. 

Services within this model can servce multiple use-cases in different industries:
- Satellite
	- Uplink and Downlink Services
	- VSAT
- Media & Broadcast
	- Event Management
	- Channel Management
- Telecommunciations
	- Fixed Network Services
	- Mobile
 - IT
	- Software Services
	- Security Services

> [!NOTE]
> The package makes use of MediaOps as reservation and orchestration layer of the component of the services (called Service Items) within this framework.


## Key Features

### Future TMF compliance 

The goal is to make Service Managmenent completely compatible with TM Forum APIs. TM Forum is a global industry association for service providers and their suppliers in the telecommunications and digital services sectors. Its main purpose is to provide standardized frameworks (including APIs) and collaborative tools that help companies reduce complexity, improve interoperability, and accelerate service delivery.

### Service Catalog

The Service Catalog allows to define Service Specification. A Service Specification is an end-to-end description of what a Service of a specific type should consist of. The specifications contain one or more Service Items which can be either Workflows (resulting in Jobs) or references to SRM Booking managers (resulting in SRM Bookings). Next to the Service Items, the Specification also allows to define the properties and configurations linked to the specific type of service.

![Service Catalog](~/CatalogInformation/Images/service_order_portal_list.png)

![Service Catalog Specification](~/CatalogInformation/Images/service_catalog_specifications.png)

### Service Ordering Portal

The Service Ordering Portal app allow to view Service Orders. Service Orders can either be created manual or (in the future) Orders can be created over the TMF compatbile APIs. Through a Service Order Item within a Service Order, an actual Service is ordered, based on what is defined in the Service Catalog.

The Order will pass through a statefull lifecycle and will be updated when the actual Services that are created in the Service Inventory evolve.

![Service Ordering Portal](~/Images/service_order_portal_list.png)

![Service Ordering Portal Order](~/Images/service_order_portal_list.png)

### Service Inventory

The Service Inventory application provides an overview of all the Services. Services can either be created directly from the Inventory app our can originate from an Order in the Service Ordering Portal. A Service contains one or more Service Items (Workflows or SRM Bookings) as defined in the Service Specfications that was specified when creating the Service. Next to that, Service Items can be added or removed on the fly. The Service will also inherit all the properties and configurations from a Service Specification, but the operator is free to add/remove properties and configurations on the fly. 

![Service Inventory](~/Images/service_order_portal_list.png)

![Service Inventory Instance](~/Images/service_order_portal_list.png)

## Prerequisites

- MediaOps version 1.2.3 (exact version): can be deployed directly from the [Catalog](https://catalog.dataminer.services/details/1b67a623-4ca6-4d25-8b3d-ed4e39496a75).

## Pricing

The applications part of this package will consume DataMiner credits, based on the level of usage of the apps. The DataMiner credits will be deducted monthly based on the metered usage. More information about the pricing of DataMiner usage-based services can be found in the [DataMiner Pricing Overview](https://docs.dataminer.services/dataminer-overview/Pricing/Pricing_Usage_based_service.html). 

## Support

For additional help or to discuss additional use-cases, reach out to [Skyline Product Marketing](mailto:team.product.marketing@skyline.be).
