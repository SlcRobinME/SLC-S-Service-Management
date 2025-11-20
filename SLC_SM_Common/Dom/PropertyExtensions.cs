namespace Library.Dom
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using DomHelpers.SlcProperties;
	using DomHelpers.SlcWorkflow;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public static class PropertyExtensions
	{
		public static ICollection<PropertyValuesInstance> GetIcons(Func<DMSMessage[], DMSMessage[]> messages)
		{
			return new DomHelper(messages, SlcPropertiesIds.ModuleId)
				.DomInstances
				.Read(
					DomInstanceExposers.DomDefinitionId.Equal(SlcPropertiesIds.Definitions.PropertyValues.Id)
						.AND(DomInstanceExposers.FieldValues.DomInstanceField(SlcPropertiesIds.Sections.PropertyValue.PropertyName).Equal("Icon")))
				.Select(p => new PropertyValuesInstance(p))
				.ToArray();
		}

		public static ICollection<PropertyValuesInstance> GetCategories(Func<DMSMessage[], DMSMessage[]> messages)
		{
			return new DomHelper(messages, SlcPropertiesIds.ModuleId)
				.DomInstances
				.Read(
					DomInstanceExposers.DomDefinitionId.Equal(SlcPropertiesIds.Definitions.PropertyValues.Id)
						.AND(DomInstanceExposers.FieldValues.DomInstanceField(SlcPropertiesIds.Sections.PropertyValue.PropertyName).Equal("Category")))
				.Select(p => new PropertyValuesInstance(p))
				.ToArray();
		}
	}

	public static class WorkflowExtensions
	{
		public static ICollection<WorkflowsInstance> GetWorkflows(Func<DMSMessage[], DMSMessage[]> messages)
		{
			return new DomHelper(messages, SlcWorkflowIds.ModuleId)
				.DomInstances
				.Read(
					DomInstanceExposers.DomDefinitionId.Equal(SlcWorkflowIds.Definitions.Workflows.Id)
						.AND(DomInstanceExposers.StatusId.Equal(SlcWorkflowIds.Behaviors.Workflow_Behavior.Statuses.Complete)))
				.Select(x => new WorkflowsInstance(x))
				.ToArray();

		}
	}
}