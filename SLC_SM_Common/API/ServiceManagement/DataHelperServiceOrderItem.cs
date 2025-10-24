namespace Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcServicemanagement;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	/// <inheritdoc />
	public class DataHelperServiceOrderItem : DataHelper<Models.ServiceOrderItem>
	{
		/// <inheritdoc />
		public DataHelperServiceOrderItem(IConnection connection) : base(connection, SlcServicemanagementIds.Definitions.ServiceOrderItems)
		{
		}

		/// <inheritdoc />
		public override Guid CreateOrUpdate(Models.ServiceOrderItem item)
		{
			DomInstance domInstance = New(item.ID);
			var existingStatusId = _domHelper.DomInstances.Read(DomInstanceExposers.Id.Equal(domInstance.ID)).FirstOrDefault()?.StatusId;
			if (existingStatusId != null)
			{
				domInstance.StatusId = existingStatusId;
			}

			var instance = new ServiceOrderItemsInstance(domInstance);
			instance.ServiceOrderItemInfo.Name = item.Name;
			instance.ServiceOrderItemInfo.Action = item.Action;
			instance.ServiceOrderItemInfo.ServiceStartTime = item.StartTime;
			instance.ServiceOrderItemInfo.ServiceEndTime = item.EndTime;
			instance.ServiceOrderItemInfo.ServiceIndefiniteRuntime = item.IndefiniteRuntime;

			instance.ServiceOrderItemServiceInfo.ServiceSpecification = item.SpecificationId;
			instance.ServiceOrderItemServiceInfo.ServiceCategory = item.ServiceCategoryId;
			instance.ServiceOrderItemServiceInfo.Service = item.ServiceId;

			var dataHelperConfigurations = new DataHelperServiceOrderItemConfigurationValue(_connection);
			instance.ServiceOrderItemServiceInfo.ServiceOrderItemConfigurations.Clear();
			foreach (var configurationValue in item.Configurations.Where(c => c?.ConfigurationParameter != null))
			{
				instance.ServiceOrderItemServiceInfo.ServiceOrderItemConfigurations.Add(dataHelperConfigurations.CreateOrUpdate(configurationValue));
			}

			return CreateOrUpdateInstance(instance);
		}

		/// <inheritdoc />
		public override bool TryDelete(Models.ServiceOrderItem item)
		{
			bool ok = true;

			foreach (var serviceOrderItemConfigurationValue in item.Configurations)
			{
				ok &= TryDelete(serviceOrderItemConfigurationValue.ID);
			}

			return ok && TryDelete(item.ID);
		}

		/// <inheritdoc />
		protected override List<Models.ServiceOrderItem> Read(IEnumerable<DomInstance> domInstances)
		{
			var instances = domInstances.Select(x => new ServiceOrderItemsInstance(x)).ToList();
			if (instances.Count < 1)
			{
				return new List<Models.ServiceOrderItem>();
			}

			var helperConfigurations = GetRequiredServiceOrderItemConfigurationValues(instances);

			return instances.Select(
					x => new Models.ServiceOrderItem
					{
						ID = x.ID.Id,
						StatusId = x.StatusId,
						Name = x.ServiceOrderItemInfo.Name,
						Action = x.ServiceOrderItemInfo.Action,
						StartTime = x.ServiceOrderItemInfo.ServiceStartTime,
						EndTime = x.ServiceOrderItemInfo.ServiceEndTime,
						IndefiniteRuntime = x.ServiceOrderItemInfo.ServiceIndefiniteRuntime,
						SpecificationId = x.ServiceOrderItemServiceInfo.ServiceSpecification,
						ServiceCategoryId = x.ServiceOrderItemServiceInfo.ServiceCategory,
						Configurations = helperConfigurations.Where(c => x.ServiceOrderItemServiceInfo.ServiceOrderItemConfigurations.Contains(c.ID)).ToList(),
						ServiceId = x.ServiceOrderItemServiceInfo.Service,
					})
				.ToList();
		}

		private List<Models.ServiceOrderItemConfigurationValue> GetRequiredServiceOrderItemConfigurationValues(List<ServiceOrderItemsInstance> instances)
		{
			FilterElement<Models.ServiceOrderItemConfigurationValue> filter = new ORFilterElement<Models.ServiceOrderItemConfigurationValue>();
			var guids = instances.Where(i => i?.ServiceOrderItemServiceInfo?.ServiceOrderItemConfigurations != null).SelectMany(i => i.ServiceOrderItemServiceInfo.ServiceOrderItemConfigurations).Distinct().ToList();
			foreach (Guid guid in guids)
			{
				filter = filter.OR(ServiceOrderItemConfigurationValueExposers.Guid.Equal(guid));
			}

			return guids.Count > 0 ? new DataHelperServiceOrderItemConfigurationValue(_connection).Read(filter) : new List<Models.ServiceOrderItemConfigurationValue>();
		}
	}
}