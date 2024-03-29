//basically robbed this from BLG default prefs
//https://forum.blockland.us/index.php?topic=320521.0 heres a good reference
function BOF_registerPref(%cat, %title, %type, %variable, %addon, %default, %params, %className) {
	if(%className $= "")
		%className = "BOF_preference";

    %pref = new ScriptObject(Preference)
    {
        className     = %className;

        addon         = %addon;
        category      = %cat;
        title         = %title;

        variable      = %variable;

        type          = %type;
        params        = %params;
        defaultValue  = %default;

        hostOnly      = false;
        secret        = false;

        loadNow        = false; // load value on creation instead of with pool (optional)
        noSave         = false; // do not save (optional)
        requireRestart = false; // denotes a restart is required (optional)
    };

	return %pref;
}

function Pref_BufferOverflow_Enabled::onUpdate(%this, %value)
{
	%silent = $Pref::Server::BufferOverflowFix::Silent;
	%funcStr = $Pref::Server::BufferOverflowFix::Enabled ? "Enable" : "Disable";
	commandToAll('BufferOverflowSet', %funcStr, %silent);
}
function Pref_BufferOverflow_Distance::onUpdate(%this, %value)
{
	%distance = getMax($Pref::Server::BufferOverflowFix::Distance, 100);
	%distance = mFloatLength(%distance, 1); //match the pref decimal length of 1
	$Pref::Server::BufferOverflowFix::Distance = %distance;

	commandToAll('BufferOverflowSet', "Distance", %distance);

	Pref_BufferOverflow_InstantDistance::onUpdate();
}
function Pref_BufferOverflow_InstantDistance::onUpdate(%this, %value)
{
	%distance = getMax($Pref::Server::BufferOverflowFix::InstantDistance, $Pref::Server::BufferOverflowFix::Distance * 1.1);
	%distance = mFloatLength(%distance, 1); //match the pref decimal length of 1
	$Pref::Server::BufferOverflowFix::InstantDistance = %distance;
	commandToAll('BufferOverflowSet', "InstantDistance", %distance);
}

if(!$BufferOverflow::SetUpPrefs)
{
	registerPreferenceAddon("Script_BufferOverflowFix", "Buffer Overflow Settings", "control_power_blue");

	BOF_registerPref("Options", "Enabled"		 		, "bool", "$Pref::Server::BufferOverflowFix::Enabled" 		  , "Script_BufferOverflowFix", 0	, ""		  , "Pref_BufferOverflow_Enabled" );
	BOF_registerPref("Options", "Silent Enable/Disable" , "bool", "$Pref::Server::BufferOverflowFix::Silent" 		  , "Script_BufferOverflowFix", 0	, ""		  , "Pref_BufferOverflow_Silent" );
	BOF_registerPref("Options", "Distance"		 		, "num" , "$Pref::Server::BufferOverflowFix::Distance"		  , "Script_BufferOverflowFix", 600	, "1 100000 1", "Pref_BufferOverflow_Distance");
	BOF_registerPref("Options", "InstantDistance"		, "num" , "$Pref::Server::BufferOverflowFix::InstantDistance" , "Script_BufferOverflowFix", 800	, "1 100000 1", "Pref_BufferOverflow_InstantDistance");

	$BufferOverflow::SetUpPrefs = true;
}


package Script_Server_BufferOverflowFix
{
	//when a client spawns
	function GameConnection::onClientEnterGame(%this)
	{
		if($Pref::Server::BufferOverflowFix::Enabled)
		{
			commandToClient(%this, 'BufferOverflowHandshake');

			%silent = $Pref::Server::BufferOverflowFix::Silent;
			commandToClient(%this, 'BufferOverflowSet', "Enable", %silent);
			commandToClient(%this, 'BufferOverflowSet', "Distance", $Pref::Server::BufferOverflowFix::Distance);
			commandToClient(%this, 'BufferOverflowSet', "InstantDistance", $Pref::Server::BufferOverflowFix::InstantDistance);
		}

		return parent::onClientEnterGame(%this);
	}
};
activatePackage(Script_Server_BufferOverflowFix);