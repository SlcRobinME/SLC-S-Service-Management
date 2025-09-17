namespace SLCSMDSGetServiceByServiceType
{
	using Skyline.DataMiner.Analytics.GenericInterface;

	internal class Arguments : IGQIInputArguments
	{
		private readonly GQIStringArgument serviceTypeArg = new GQIStringArgument("Service Type") { IsRequired = false };

		public string ServiceType { get; set; }

		public GQIArgument[] GetInputArguments()
		{
			return new GQIArgument[]
			{
				serviceTypeArg,
			};
		}

		public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
		{
			ServiceType = args.GetArgumentValue(serviceTypeArg);

			return new OnArgumentsProcessedOutputArgs();
		}
	}
}
