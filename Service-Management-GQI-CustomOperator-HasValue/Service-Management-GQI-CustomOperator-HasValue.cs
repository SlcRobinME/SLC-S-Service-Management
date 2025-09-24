/*
****************************************************************************
*  Copyright (c) 2023,  Skyline Communications NV  All Rights Reserved.    *
****************************************************************************

By using this script, you expressly agree with the usage terms and
conditions set out below.
This script and all related materials are protected by copyrights and
other intellectual property rights that exclusively belong
to Skyline Communications.

A user license granted for this script is strictly for personal use only.
This script may not be used in any way by anyone without the prior
written consent of Skyline Communications. Any sublicensing of this
script is forbidden.

Any modifications to this script by the user are only allowed for
personal use and within the intended purpose of the script,
and will remain the sole responsibility of the user.
Skyline Communications will not be responsible for any damages or
malfunctions whatsoever of the script resulting from a modification
or adaptation by the user.

The content of this script is confidential information.
The user hereby agrees to keep this confidential information strictly
secret and confidential and not to disclose or reveal it, in whole
or in part, directly or indirectly to any person, entity, organization
or administration without the prior written consent of
Skyline Communications.

Any inquiries can be addressed to:

    Skyline Communications NV
    Ambachtenstraat 33
    B-8870 Izegem
    Belgium
    Tel.    : +32 51 31 35 69
    Fax.    : +32 51 31 01 29
    E-mail    : info@skyline.be
    Web        : www.skyline.be
    Contact    : Ben Vandenberghe

****************************************************************************
Revision History:

DATE        VERSION        AUTHOR            COMMENTS

dd/mm/2023    1.0.0.1        RCA, Skyline    Initial version
****************************************************************************
*/
using System;
using Skyline.DataMiner.Analytics.GenericInterface;

[GQIMetaData(Name = "Service-Management-GQI-CustomOperator-HasValue")]
public class MyCustomOperator : IGQIColumnOperator, IGQIRowOperator, IGQIInputArguments
{
	private readonly GQIStringArgument _argument = new GQIStringArgument("Column Name") { IsRequired = true };
	private readonly GQIBooleanColumn _isEmptyColumn = new GQIBooleanColumn("Has Value");
	private string _columnName = String.Empty;

	public GQIArgument[] GetInputArguments()
	{
		return new GQIArgument[] { _argument };
	}

	public void HandleColumns(GQIEditableHeader header)
	{
		header.AddColumns(_isEmptyColumn);
	}

	public void HandleRow(GQIEditableRow row)
	{
		row.SetValue(_isEmptyColumn, !String.IsNullOrEmpty(row.GetValue(_columnName)?.ToString()));
	}

	public OnArgumentsProcessedOutputArgs OnArgumentsProcessed(OnArgumentsProcessedInputArgs args)
	{
		_columnName = args.GetArgumentValue(_argument);
		return new OnArgumentsProcessedOutputArgs();
	}
}