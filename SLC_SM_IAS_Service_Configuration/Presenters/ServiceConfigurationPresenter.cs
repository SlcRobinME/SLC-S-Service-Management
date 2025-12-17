namespace SLC_SM_IAS_Service_Configuration.Presenters
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text.RegularExpressions;

	using DomHelpers.SlcConfigurations;

	using Skyline.DataMiner.Automation;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API;
	using Skyline.DataMiner.ProjectApi.ServiceManagement.API.ServiceManagement;
	using Skyline.DataMiner.Utils.InteractiveAutomationScript;
	using Skyline.DataMiner.Utils.ServiceManagement.Common.Extensions;

	using SLC_SM_IAS_Service_Configuration.Views;

	public partial class ServiceConfigurationPresenter
	{
		private const string StandaloneCollapseButtonTitle = "Standalone Parameters";
		private readonly IEngine engine;
		private readonly InteractiveController controller;
		private readonly Models.Service instance;
		private readonly ServiceConfigurationView view;
		private ConfigurationDataRecord configuration;
		private DataHelpersConfigurations repoConfig;
		private DataHelpersServiceManagement repoService;
		private bool showDetails;
		private Models.ServiceSpecification serviceSpecifivation;

		private int collapeButtonWidth = 85;
		private int addButtonWidth = 70;
		private int deleteProfileButtonWidth = 55;
		private int buttonWidth = 200;

		private int detailsColumnIndex = 5;
		private int parameterValueColumnIndex = 3;

		public ServiceConfigurationPresenter(IEngine engine, InteractiveController controller, ServiceConfigurationView view, Models.Service instance)
		{
			this.engine = engine;
			this.controller = controller;
			this.view = view;
			this.instance = instance;
			this.showDetails = false;

			view.BtnCancel.MaxWidth = buttonWidth;
			view.BtnCancel.Pressed += (sender, args) => throw new ScriptAbortException("OK");
			view.BtnShowValueDetails.MaxWidth = buttonWidth;
			view.BtnShowValueDetails.Pressed += (sender, args) =>
			{
				showDetails = !showDetails;
				view.BtnShowValueDetails.Text = !showDetails ? view.BtnShowValueDetails.Text.Replace("Hide", "Show") : view.BtnShowValueDetails.Text.Replace("Show", "Hide");

				foreach (var details in view.Details)
				{
					if (details.Key == StandaloneCollapseButtonTitle)
					{
						ShowHideStandaloneParametersDetails(showDetails, details.Value);
						continue;
					}

					ShowHideProfileParametersDetails(showDetails, details.Key, details.Value);
				}
			};
			view.BtnUpdate.MaxWidth = buttonWidth;
			view.BtnUpdate.Pressed += (sender, args) =>
			{
				StoreModels();
				throw new ScriptAbortException("OK");
			};

			view.StandaloneParameters.Pressed += (sender, args) =>
			{
				if (sender is CollapseButton collapseButton)
				{
					ShowHideStandaloneParametersDetails(showDetails, view.Details[collapseButton.Tooltip]);
				}
			};
		}

		public void LoadFromModel()
		{
			repoService = new DataHelpersServiceManagement(engine.GetUserConnection());
			repoConfig = new DataHelpersConfigurations(engine.GetUserConnection());

			var configParams = repoConfig.ConfigurationParameters.Read();
			serviceSpecifivation = instance.ServiceSpecificationId.HasValue
					? repoService.ServiceSpecifications.Read(Skyline.DataMiner.ProjectApi.ServiceManagement.SDM.ServiceSpecificationExposers.Guid.Equal(instance.ServiceSpecificationId.Value))[0]
					: null;

			if (instance.Configuration != null)
			{
				configuration = ConfigurationDataRecord.BuildConfigurationDataRecordRecord(instance.Configuration, configParams, serviceSpecifivation, engine);
			}

			BuildUI(false);
		}

		public void StoreModels()
		{
			if (configuration.State == State.Delete)
			{
				repoService.ServiceConfigurations.TryDelete(configuration.ServiceConfig);
			}

			foreach (var standaloneParam in configuration.ServiceParameterConfigs)
			{
				if (standaloneParam.State == State.Delete)
				{
					repoService.ServiceConfigurationValues.TryDelete(standaloneParam.ServiceParameterConfig);
				}
			}

			foreach (var profile in configuration.ServiceProfileConfigs)
			{
				if (profile.State == State.Delete)
				{
					repoService.ServiceProfiles.TryDelete(profile.ServiceProfileConfig);
					continue;
				}

				foreach (var profileParameter in profile.ProfileParameterConfigs)
				{
					if (profileParameter.State == State.Delete)
					{
						repoConfig.ConfigurationParameterValues.TryDelete(profileParameter.ConfigurationParamValue);
					}
				}
			}

			repoService.Services.CreateOrUpdate(instance);
		}

		private void AddStandaloneConfigModel(Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter selectedParameter)
		{
			var configurationParameterInstance = selectedParameter ?? new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter();
			var config = new Models.ServiceConfigurationValue
			{
				ID = Guid.NewGuid(),
				Mandatory = false,
				ConfigurationParameter = new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameterValue
				{
					Label = String.Empty,
					Type = configurationParameterInstance.Type,
					ConfigurationParameterId = configurationParameterInstance.ID,
					NumberOptions = configurationParameterInstance.NumberOptions,
					DiscreteOptions = configurationParameterInstance.DiscreteOptions,
					TextOptions = configurationParameterInstance.TextOptions,
				},
			};
			if (config.ConfigurationParameter.NumberOptions != null)
			{
				config.ConfigurationParameter.NumberOptions.ID = Guid.NewGuid();
			}

			if (config.ConfigurationParameter.DiscreteOptions != null)
			{
				config.ConfigurationParameter.DiscreteOptions.ID = Guid.NewGuid();
			}

			if (config.ConfigurationParameter.TextOptions != null)
			{
				config.ConfigurationParameter.TextOptions.ID = Guid.NewGuid();
			}

			instance.Configuration.Parameters.Add(config);

			configuration.ServiceParameterConfigs.Add(StandaloneParameterDataRecord.BuildParameterDataRecord(config, configurationParameterInstance));
		}

		private void AddProfileConfigModel(Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.Profile selectedProfile, Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ProfileDefinition profileDefinition)
		{
			var profileInstance = selectedProfile ?? new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.Profile();
			var profileDefinitionInstance = profileDefinition ?? new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ProfileDefinition();
			var profileConfig = new Models.ServiceProfile
			{
				ID = Guid.NewGuid(),
				Mandatory = false,
				ProfileDefinition = profileDefinitionInstance,
				Profile = new Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.Profile
				{
					Name = profileInstance.Name.ReplaceTrailingParentesisContent(instance.ServiceID),
					ProfileDefinitionReference = profileInstance.ProfileDefinitionReference,
					ConfigurationParameterValues = profileInstance.ConfigurationParameterValues,
					Profiles = profileInstance.Profiles,
					TestedProtocols = profileInstance.TestedProtocols,
				},
			};

			instance.Configuration.Profiles.Add(profileConfig);
			configuration.ServiceProfileConfigs.Add(ProfileDataRecord.BuildProfileRecord(profileConfig, repoConfig.ConfigurationParameters.Read(), serviceSpecifivation, engine));
		}

		private void BuildHeaderRow(int row, CollapseButton collapseButtom)
		{
			var lblLabel = new Label("Label") { Style = TextStyle.Heading, IsVisible = !collapseButtom.IsCollapsed, MaxWidth = 100 };
			var lblParameter = new Label("Parameter") { Style = TextStyle.Heading, IsVisible = !collapseButtom.IsCollapsed, MaxWidth = 100 };
			var lblLink = new Label("Link") { Style = TextStyle.Heading , IsVisible = !collapseButtom.IsCollapsed , MaxWidth = 100 };
			var lblValue = new Label("Value") { Style = TextStyle.Heading , IsVisible = !collapseButtom.IsCollapsed , MaxWidth = 100 };
			var lblUnit = new Label("Unit") { Style = TextStyle.Heading , IsVisible = !collapseButtom.IsCollapsed , MaxWidth = 100 };
			var lblStart = new Label("Start") { Style = TextStyle.Heading , IsVisible = !collapseButtom.IsCollapsed , MaxWidth = 100 };
			var lblEnd = new Label("End") { Style = TextStyle.Heading , IsVisible = !collapseButtom.IsCollapsed , MaxWidth = 100 };
			var lblStop = new Label("Step Size") { Style = TextStyle.Heading , IsVisible = !collapseButtom.IsCollapsed , MaxWidth = 100 };
			var lblDecimals = new Label("Decimals") { Style = TextStyle.Heading , IsVisible = !collapseButtom.IsCollapsed , MaxWidth = 100 };
			var lblValues = new Label("Values") { Style = TextStyle.Heading , IsVisible = !collapseButtom.IsCollapsed , MaxWidth = 100 };

			view.AddWidget(lblLabel, row, 0);
			collapseButtom.LinkedWidgets.Add(lblLabel);
			view.AddWidget(lblParameter, row, 1);
			collapseButtom.LinkedWidgets.Add(lblParameter);
			view.AddWidget(lblLink, row, 2);
			collapseButtom.LinkedWidgets.Add(lblLink);
			view.AddWidget(lblValue, row, 3);
			collapseButtom.LinkedWidgets.Add(lblValue);
			view.AddWidget(lblUnit, row, 4);
			collapseButtom.LinkedWidgets.Add(lblUnit);

			view.Details[collapseButtom.Tooltip].AddWidget(lblStart, 0, 0);
			view.Details[collapseButtom.Tooltip].AddWidget(lblEnd, 0, 1);
			view.Details[collapseButtom.Tooltip].AddWidget(lblStop, 0, 2);
			view.Details[collapseButtom.Tooltip].AddWidget(lblDecimals, 0, 3);
			view.Details[collapseButtom.Tooltip].AddWidget(lblValues, 0, 4);
		}

		private void BuildUI(bool showDetails)
		{
			this.showDetails = showDetails;
			view.Clear();
			view.Details.Clear();

			int row = 0;
			view.AddWidget(view.TitleDetails, row, 0, 1, 2);
			view.AddWidget(new WhiteSpace() , ++row, 0);
			view.AddWidget(view.BtnShowValueDetails, ++row, 0);
			view.AddWidget(new WhiteSpace(), ++row, 0);

			row = BuildProfileAdditionUI(row);

			row = BuildStandaloneParametersUI(showDetails, row);

			row = BuildProfilesUI(showDetails, row);

			view.AddWidget(new WhiteSpace(), ++row, 0);
			view.AddWidget(view.BtnUpdate, ++row, 0, HorizontalAlignment.Center);
			view.AddWidget(view.BtnCancel, row, 1);
		}

		private int BuildProfileAdditionUI(int row)
		{
			view.AddWidget(new Label("Add Profile:") { Style = TextStyle.Heading, MaxWidth = 100 }, ++row, 0, HorizontalAlignment.Right);

			var profileDefinitionOptions = repoConfig.ProfileDefinitions.Read().Select(x => new Option<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ProfileDefinition>(x.Name, x)).OrderBy(x => x.DisplayValue).ToList();
			profileDefinitionOptions.Insert(0, new Option<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ProfileDefinition>("- Profile Definition -", null));
			view.ProfileDefintionFilter = new DropDown<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ProfileDefinition>(profileDefinitionOptions)
			{
				IsDisplayFilterShown = true,
				// MaxWidth = dropdownWidth,
			};
			view.AddWidget(view.ProfileDefintionFilter, row, 1,1, 2);
			view.ProfileDefintionFilter.Changed += (sender, args) =>
			{
				if (args == null || args.Selected == null)
				{
					view.ProfileToAdd.Options = new List<Option<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.Profile>>
					{
						new Option<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.Profile>("- Profile -", null),
					};
				}

				var newProfileOptions = repoConfig.Profiles.Read(Skyline.DataMiner.ProjectApi.ServiceManagement.SDM.ProfileExposers.ProfileDefinitionID.Equal(args.Selected.ID))
						.Select(x => new Option<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.Profile>(x.Name, x)).OrderBy(x => x.DisplayValue).ToList();
				newProfileOptions.Insert(0, new Option<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.Profile>("- Profile -", null));
				view.ProfileToAdd.Options = newProfileOptions;
			};

			view.ProfileToAdd = new DropDown<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.Profile>(
				new List<Option<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.Profile>>
				{
					new Option<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.Profile>("- Profile -", null),
				})
			{
				IsDisplayFilterShown = true,
				// MaxWidth = dropdownWidth,
			};
			view.AddWidget(view.ProfileToAdd, row, 3);

			var addProfileButton = new Button("Add") { Width = addButtonWidth };
			view.AddWidget(addProfileButton, row, 4);
			view.StandaloneParameters.LinkedWidgets.Add(addProfileButton);
			addProfileButton.Pressed += (sender, args) =>
			{
				if (view.ProfileToAdd == null || view.ProfileToAdd.Selected == null)
				{
					return;
				}

				AddProfileConfigModel(view.ProfileToAdd.Selected, view.ProfileDefintionFilter?.Selected);
				BuildUI(showDetails);
			};

			view.AddWidget(new WhiteSpace(), ++row, 0);
			return row;
		}

		private int BuildProfilesUI(bool showDetails, int row)
		{
			foreach (var profile in configuration.ServiceProfileConfigs.Where(x => x.State != State.Delete))
			{
				view.Engine.GenerateInformation("Building UI for profile: " + profile.Profile.Name);

				if (!view.ProfileCollapseButtons.TryGetValue(profile.Profile.Name, out var collapseButton))
				{
					collapseButton = new CollapseButton(true)
					{
						ExpandText = "+",
						CollapseText = "-",
						Tooltip = profile.Profile.Name,
						MaxWidth = collapeButtonWidth,
					};
				}

				collapseButton.LinkedWidgets.Clear();

				view.Details[profile.Profile.Name] = new Section();
				view.AddWidget(new Label(profile.Profile.Name) { Style = TextStyle.Bold, MaxWidth = 150 }, ++row, 1);
				view.AddWidget(collapseButton, row, 0, HorizontalAlignment.Center);
				var delete = new Button("🚫") { IsEnabled = profile.CanBeDeleted, MaxWidth = deleteProfileButtonWidth };
				view.AddWidget(delete, row, 2);
				delete.Pressed += DeleteProfile(profile);

				BuildHeaderRow(++row, collapseButton);

				int originalSectionRow = row;
				int sectionRow = 0;

				foreach (var profileParameter in profile.ProfileParameterConfigs.Where(x => x.State != State.Delete))
				{
					BuildParameterUIRow(collapseButton, profileParameter, ++row, ++sectionRow, null, true);
				}

				view.AddSection(view.Details[profile.Profile.Name], originalSectionRow, 5);
				collapseButton.LinkedWidgets.AddRange(view.Details[profile.Profile.Name].Widgets);
				view.Details[profile.Profile.Name].IsVisible = showDetails;

				view.ProfileCollapseButtons[profile.Profile.Name] = collapseButton;
				collapseButton.Pressed += (sender, args) =>
				{
					if (sender is CollapseButton cb)
					{
						ShowHideProfileParametersDetails(showDetails, cb.Tooltip, view.Details[cb.Tooltip]);
					}
				};

				ShowHideProfileParametersDetails(showDetails, collapseButton.Tooltip, view.Details[collapseButton.Tooltip]);
			}

			return row;
		}

		private int BuildStandaloneParametersUI(bool showDetails, int row)
		{
			view.StandaloneParameters.MaxWidth = collapeButtonWidth;
			view.StandaloneParameters.LinkedWidgets.Clear();
			view.Details[StandaloneCollapseButtonTitle] = new Section();
			view.AddWidget(new Label(ServiceConfigurationView.StandaloneCollapseButtonTitle) { Style = TextStyle.Bold, MaxWidth = 250 }, ++row, 1, 1, 5);
			view.AddWidget(view.StandaloneParameters, row, 0, HorizontalAlignment.Center);
			BuildHeaderRow(++row, view.StandaloneParameters);

			int originalSectionRow = row;
			int sectionRow = 0;
			foreach (var standaloneParameter in configuration.ServiceParameterConfigs.Where(x => x.State != State.Delete))
			{
				BuildParameterUIRow(view.StandaloneParameters, standaloneParameter, ++row, ++sectionRow, DeleteStandaloneParameter(standaloneParameter), standaloneParameter.ServiceParameterConfig.Mandatory);
			}

			view.AddSection(view.Details[StandaloneCollapseButtonTitle], originalSectionRow, detailsColumnIndex);
			view.StandaloneParameters.LinkedWidgets.AddRange(view.Details[StandaloneCollapseButtonTitle].Widgets);
			ShowHideStandaloneParametersDetails(showDetails, view.Details[StandaloneCollapseButtonTitle]);

			var whiteSpaceAfterParameters = new WhiteSpace { IsVisible = !view.StandaloneParameters.IsCollapsed, MaxWidth = 20};
			view.AddWidget(whiteSpaceAfterParameters, ++row, 0);
			view.StandaloneParameters.LinkedWidgets.Add(whiteSpaceAfterParameters);

			var parameterToAddLabel = new Label("Add Parameter:") { Style = TextStyle.Heading, IsVisible = !view.StandaloneParameters.IsCollapsed, MaxWidth = 100 };
			view.AddWidget(parameterToAddLabel, ++row, 0, HorizontalAlignment.Right);
			view.StandaloneParameters.LinkedWidgets.Add(parameterToAddLabel);

			var parameterOptions = repoConfig.ConfigurationParameters.Read().Select(x => new Option<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter>(x.Name, x)).OrderBy(x => x.DisplayValue).ToList();
			parameterOptions.Insert(0, new Option<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter>("- Add -", null));
			view.StandaloneParametersToAdd = new DropDown<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter>(parameterOptions)
			{
				IsVisible = !view.StandaloneParameters.IsCollapsed,
				// MaxWidth = dropdownWidth,
			};
			view.AddWidget(view.StandaloneParametersToAdd, row, 1);
			view.StandaloneParameters.LinkedWidgets.Add(view.StandaloneParametersToAdd);

			var addParameterButton = new Button("Add") { IsVisible = !view.StandaloneParameters.IsCollapsed, MaxWidth = addButtonWidth };
			view.AddWidget(addParameterButton, row, 2);
			view.StandaloneParameters.LinkedWidgets.Add(addParameterButton);
			addParameterButton.Pressed += (sender, args) =>
			{
				if (view.StandaloneParametersToAdd == null || view.StandaloneParametersToAdd.Selected == null)
				{
					return;
				}

				AddStandaloneConfigModel(view.StandaloneParametersToAdd.Selected);
				BuildUI(view.Details[ServiceConfigurationView.StandaloneCollapseButtonTitle].IsVisible);
			};

			return row;
		}

		private void BuildParameterUIRow(CollapseButton collapseButtom, IParameterDataRecord record, int row, int sectionRow, EventHandler<EventArgs> deleteEventHandler, bool mandatory = true)
		{
			// Init
			var label = new TextBox(record.ConfigurationParamValue.Label) { IsVisible = !collapseButtom.IsCollapsed};
			var parameter = new DropDown<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter>(
				new[] { new Option<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter>(record.ConfigurationParam.Name, record.ConfigurationParam) })
			{
				IsEnabled = false,
				IsVisible = !collapseButtom.IsCollapsed,
			};
			var link = new CheckBox { IsChecked = record.ConfigurationParamValue.LinkedConfigurationReference != null, IsVisible = !collapseButtom.IsCollapsed };
			var unit = new DropDown<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationUnit>(
				new[] { new Option<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationUnit>("-", null) })
			{ IsEnabled = false, MaxWidth = 80, IsVisible = !collapseButtom.IsCollapsed };
			var start = new Numeric { IsEnabled = false, MaxWidth = 100, IsVisible = !collapseButtom.IsCollapsed };
			var end = new Numeric { IsEnabled = false, MaxWidth = 100, IsVisible = !collapseButtom.IsCollapsed };
			var step = new Numeric { IsEnabled = false, Minimum = 0, Maximum = 1, MaxWidth = 100, IsVisible = !collapseButtom.IsCollapsed };
			var decimals = new Numeric { StepSize = 1, Minimum = 0, Maximum = 6, IsEnabled = false, MaxWidth = 80, IsVisible = !collapseButtom.IsCollapsed };
			var values = new Button("...") { IsEnabled = false, IsVisible = !collapseButtom.IsCollapsed };
			var delete = new Button("🚫") { IsEnabled = !mandatory, IsVisible = !collapseButtom.IsCollapsed };

			label.Changed += (sender, args) => record.ConfigurationParamValue.Label = args.Value;

			if (deleteEventHandler != null)
			{
				delete.Pressed += deleteEventHandler;
			}

			link.Changed += (sender, args) =>
			{
				record.ConfigurationParamValue.LinkedConfigurationReference = args.IsChecked ? "Dummy Link" : null;
				BuildUI(view.Details[collapseButtom.Tooltip].IsVisible);
			};

			if (record.ConfigurationParamValue.LinkedConfigurationReference != null)
			{
				var referenceDropdown = new DropDown { IsVisible = !collapseButtom.IsCollapsed };
				view.AddWidget(referenceDropdown, row, parameterValueColumnIndex);
				collapseButtom.LinkedWidgets.Add(referenceDropdown);
			}
			else
			{
				switch (parameter.Selected.Type)
				{
					case SlcConfigurationsIds.Enums.Type.Number:
						collapseButtom.LinkedWidgets.Add(AddNumericWidgets(record, row, parameter, unit, start, end, step, decimals, !collapseButtom.IsCollapsed));

						break;

					case SlcConfigurationsIds.Enums.Type.Discrete:
						collapseButtom.LinkedWidgets.Add(AddDiscreteWidgets(record, row, !collapseButtom.IsCollapsed));

						break;

					default:
						collapseButtom.LinkedWidgets.Add(AddTextWidgets(record, row, !collapseButtom.IsCollapsed));

						break;
				}
			}

			// Populate row
			view.AddWidget(label, row, 0);
			collapseButtom.LinkedWidgets.Add(label);
			view.AddWidget(parameter, row, 1 );
			collapseButtom.LinkedWidgets.Add(parameter);
			view.AddWidget(link, row, 2);
			collapseButtom.LinkedWidgets.Add(link);
			view.AddWidget(unit, row, 4);
			collapseButtom.LinkedWidgets.Add(unit);

			view.Details[collapseButtom.Tooltip].AddWidget(start, sectionRow, 0);
			view.Details[collapseButtom.Tooltip].AddWidget(end, sectionRow, 1);
			view.Details[collapseButtom.Tooltip].AddWidget(step, sectionRow, 2);
			view.Details[collapseButtom.Tooltip].AddWidget(decimals, sectionRow, 3);
			view.Details[collapseButtom.Tooltip].AddWidget(values, sectionRow, 4);

			view.AddWidget(delete, row, 10);
			collapseButtom.LinkedWidgets.Add(delete);
		}

		private EventHandler<EventArgs> DeleteStandaloneParameter(StandaloneParameterDataRecord record)
		{
			return (sender, args) =>
			{
				record.State = State.Delete;
				instance.Configuration.Parameters.Remove(record.ServiceParameterConfig);
				BuildUI(showDetails);
			};
		}

		private EventHandler<EventArgs> DeleteProfile(ProfileDataRecord record)
		{
			return (sender, args) =>
			{
				record.State = State.Delete;
				var result = instance.Configuration.Profiles.Remove(record.ServiceProfileConfig);
				view.Engine.GenerateInformation($"Profile {record.Profile.Name} removed from instance: " + result);
				BuildUI(showDetails);
			};
		}

		private TextBox AddTextWidgets(IParameterDataRecord record, int row, bool isVisible = true)
		{
			var value = new TextBox(record.ConfigurationParamValue.StringValue ?? record.ConfigurationParamValue.TextOptions?.Default ?? String.Empty)
			{
				Tooltip = record.ConfigurationParamValue.TextOptions?.UserMessage ?? String.Empty,
				IsVisible = isVisible,
			};
			value.Changed += (sender, args) =>
			{
				if (record.ConfigurationParamValue.TextOptions?.Regex != null && !Regex.IsMatch(args.Value, record.ConfigurationParamValue.TextOptions.Regex))
				{
					value.ValidationState = UIValidationState.Invalid;
					value.ValidationText = $"Input did not match Regex '{record.ConfigurationParamValue.TextOptions.Regex}' - reverted to previous value";
					value.Text = args.Previous;
					return;
				}

				value.ValidationState = UIValidationState.Valid;
				value.ValidationText = record.ConfigurationParamValue.TextOptions?.UserMessage;
				record.ConfigurationParamValue.StringValue = args.Value;
			};
			view.AddWidget(value, row, parameterValueColumnIndex);
			return value;
		}

		private DropDown<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue> AddDiscreteWidgets(IParameterDataRecord record, int row, bool isVisible = true)
		{
			var discretes = record.ConfigurationParamValue.DiscreteOptions.DiscreteValues
											.Select(x => new Option<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue>(x.Value, x))
											.OrderBy(x => x.DisplayValue)
											.ToList();

			var value = new DropDown<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.DiscreteValue>(discretes) { IsVisible = isVisible };
			if (record.ConfigurationParamValue.StringValue != null
				&& value.Options.Any(x => x.DisplayValue == record.ConfigurationParamValue.StringValue))
			{
				value.Selected = value.Options.First(x => x.DisplayValue == record.ConfigurationParamValue.StringValue).Value;
			}

			if (record.ConfigurationParamValue.StringValue == null)
			{
				record.ConfigurationParamValue.StringValue = value.Selected?.Value;
			}

			value.Changed += (sender, args) => { record.ConfigurationParamValue.StringValue = args.SelectedOption.DisplayValue; };
			view.AddWidget(value, row, parameterValueColumnIndex);
			return value;
		}

		private Numeric AddNumericWidgets(IParameterDataRecord record, int row, DropDown<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter> parameter, DropDown<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationUnit> unit, Numeric start, Numeric end, Numeric step, Numeric decimals, bool isVisible = true)
		{
			double minimum = record.ConfigurationParamValue.NumberOptions.MinRange ?? -10_000;
			double maximum = record.ConfigurationParamValue.NumberOptions.MaxRange ?? 10_000;
			int decimalVal = Convert.ToInt32(record.ConfigurationParamValue.NumberOptions.Decimals);
			double stepSize = record.ConfigurationParamValue.NumberOptions.StepSize ?? 1;
			Numeric value = new Numeric(record.ConfigurationParamValue.DoubleValue ?? record.ConfigurationParamValue.NumberOptions.DefaultValue ?? 0)
			{
				Minimum = minimum,
				Maximum = maximum,
				StepSize = stepSize,
				Decimals = decimalVal,
				IsVisible = isVisible,
			};
			unit.SetOptions(GetUnits(record.ConfigurationParamValue.NumberOptions, parameter.Selected));
			unit.Selected = GetDefaultUnit(record.ConfigurationParamValue.NumberOptions, parameter.Selected);
			unit.IsEnabled = true;
			start.Value = minimum;
			start.IsEnabled = true;
			end.Value = maximum;
			end.IsEnabled = true;
			decimals.Value = decimalVal;
			decimals.IsEnabled = true;
			step.Value = stepSize;
			step.StepSize = 1 / Math.Pow(10, decimalVal);
			step.Decimals = decimalVal;
			step.IsEnabled = true;

			start.Changed += (sender, args) =>
			{
				value.Minimum = args.Value;
				step.Minimum = args.Value;
				record.ConfigurationParamValue.NumberOptions.MinRange = args.Value;
			};
			end.Changed += (sender, args) =>
			{
				value.Maximum = args.Value;
				step.Maximum = args.Value;
				record.ConfigurationParamValue.NumberOptions.MaxRange = args.Value;
			};
			decimals.Changed += (sender, args) =>
			{
				value.Decimals = Convert.ToInt32(args.Value);
				step.Decimals = Convert.ToInt32(args.Value);
				double newStepsize = 1 / Math.Pow(10, args.Value);
				value.StepSize = newStepsize;
				step.StepSize = newStepsize;
				record.ConfigurationParamValue.NumberOptions.Decimals = Convert.ToInt32(args.Value);
			};
			step.Changed += (sender, args) =>
			{
				value.StepSize = args.Value;
				record.ConfigurationParamValue.NumberOptions.StepSize = args.Value;
			};
			unit.Changed += (sender, args) => record.ConfigurationParamValue.NumberOptions.DefaultUnit = args.Selected;
			value.Changed += (sender, args) => { record.ConfigurationParamValue.DoubleValue = args.Value; };
			view.AddWidget(value, row, parameterValueColumnIndex);
			return value;
		}

		private Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationUnit GetDefaultUnit(
			Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.NumberParameterOptions numberValueOptions,
			Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter parameter)
		{
			if (numberValueOptions != null)
			{
				return numberValueOptions.DefaultUnit;
			}

			if (parameter.NumberOptions != null)
			{
				return parameter.NumberOptions.DefaultUnit;
			}

			return null;
		}

		private List<Option<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationUnit>> GetUnits(
			Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.NumberParameterOptions numberValueOptions,
			Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationParameter parameter)
		{
			var units = new List<Option<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationUnit>>();
			if (numberValueOptions?.DefaultUnit != null)
			{
				units.AddRange(numberValueOptions.Units.Select(x => new Option<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationUnit>(x.Name, x)));
			}
			else if (parameter.NumberOptions?.DefaultUnit != null)
			{
				units.AddRange(parameter.NumberOptions.Units.Select(x => new Option<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationUnit>(x.Name, x)));
			}

			units = units.OrderBy(x => x.DisplayValue).ToList();

			units.Insert(0, new Option<Skyline.DataMiner.ProjectApi.ServiceManagement.API.Configurations.Models.ConfigurationUnit>("-", null));
			return units;
		}

		private void ShowHideProfileParametersDetails(bool showDetails, string profileName, Section details)
		{
			details.IsVisible = showDetails
								? showDetails && !view.ProfileCollapseButtons[profileName].IsCollapsed
								: showDetails;
		}

		private void ShowHideStandaloneParametersDetails(bool showDetails, Section section)
		{
			section.IsVisible = showDetails
							? showDetails && !view.StandaloneParameters.IsCollapsed
							: showDetails;
		}
	}
}