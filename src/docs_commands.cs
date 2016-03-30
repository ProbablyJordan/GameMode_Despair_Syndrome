//This is a script file for /help and /rules commands
function serverCmdHelp(%this, %cat)
{
	%cat = strLwr(%cat);
	switch$ (%cat)
	{
		case "1" or "gamemode":
			%text[%count++] = "\c3[GAMEMODE]";
			%text[%count++] = " \c6For Trial gametype, the first day is the day without the killer. However, as soon as night strikes, the culprit is picked.";
			%text[%count++] = " \c6Killer's objective is to murder someone and get away with it. They can only kill up to three people (two if investigation period has started).";
			%text[%count++] = " \c6Once the killer does their job, they have to blend in and try to seem least suspicious.";
			%text[%count++] = " \c6A body discovery announcement is initated when at least two people examine/click the body. The announcement also happens at morning/night when bodies are undiscovered.";
			%text[%count++] = " \c6After a body has been discovered, investigation period starts. After investigation period ends, the trial period starts.";
			%text[%count++] = " \c6In trial, everyone is teleported to the courtroom where you are given time to discuss everyone's alibis and possible suspects.";
			%text[%count++] = " \c6After trial period is over, it's time to vote the most suspicious person.";
			%text[%count++] = " \c6If majority vote is correct and everyone votes the killer, the killer dies and everyone else lives.";
			%text[%count++] = " \c6However, if the vote is a tie, people didn't vote or majority vote is wrong, the killer wins and everyone else is executed.";
			%text[%count++] = " \c5Page Up to read the above.";
		case "2" or "chat":
			%text[%count++] = "\c3[CHAT]";
			%text[%count++] = " \c6To use OOC (Out-Of-Character) chat, use \c3TEAM CHAT\c6 (Default key: \c3Y\c6)";
			%text[%count++] = " \c6Normal chat is local/IC (In-Character) chat. Put ! before your message to shout, @ to whisper (like so: \c3\"!shout\"\c6 or \c3\"@whisper\"\c6)";
			%text[%count++] = " \c6You can also say /me *action* to do an IC action, like \c3\"John Doe grabs a weapon.\"";
			%text[%count++] = " \c5Page Up to read the above.";
		case "3" or "killer":
			%text[%count++] = "\c3[KILLER]";
			%text[%count++] = " \c6You can become the killer at first night or by killing the killer.";
			%text[%count++] = " \c6Once you become the killer, you have to kill someone and get away with it. Killing people in public or having a killing spree is completely discouraged.";
			%text[%count++] = " \c6You can only kill a maximum of 3 people (2 if one or more bodies have been discovered).";
			%text[%count++] = " \c6You can clean up the blood with a mop and bucket, and to wash blood off of yourself you have to use the sink.";
			%text[%count++] = " \c6But be careful! At night time, no sinks work, meaning that you will have to wait until day to clean the blood off of yourself!";
			%text[%count++] = " \c5Page Up to read the above.";
		case "4" or "mechanics":
			%text[%count++] = "\c3[MECHANICS]";
			%text[%count++] = " \c6You can sprint with \c3JET key\c6 (default\c3: RightClick\c6)";
			%text[%count++] = " \c6To unlock/lock the door with the key, press LEFTCLICK and \c3JET key\c6 (default\c3: RightClick\c6) respectively.";
			%text[%count++] = " \c3Conserve your stamina!\c6 Swinging weapons, blocking with weapons, sprinting and jumping deplete your stamina.";
			%text[%count++] = " \c6Weapons like Umbrella and Pan are /NON-LETHAL/, meaning that they will only drain stamina and knock people out when their stamina is low.";
			%text[%count++] = " \c6Press \c3LIGHT KEY\c6 (default\c3: R\c6) when aiming at a door to knock, and when aiming at the corpse to loot it.";
			%text[%count++] = " \c6To navigate the inventory menu, use building controls (move brick forward/backward, brick plant key and cancel brick key)";
			%text[%count++] = " \c3Double-click and hold\c6 to carry around a corpse. Hold click to carry an item. Fast click to pick up an item.";
			%text[%count++] = " \c6Dormitory rooms are \c3SOUNDPROOF\c6, meaning all sound made inside the room is isolated. Nobody will hear you scream on the outside unless you leave the door open!";
			%text[%count++] = " \c6To sleep, say \c3/sleep\c6. You will be unconscious for some time.";
			%text[%count++] = " \c5Page Up to read the above.";
		case "5" or "combat":
			%text[%count++] = "\c3[COMBAT]";
			%text[%count++] = " \c6You can find melee weapons all over the school.";
			%text[%count++] = " \c6Pans and Umbrellas are \c3NON-LETHAL WEAPONS\c6, meaning that they will drain stamina and knock people out when their stamina is low.";
			%text[%count++] = " \c6Pipe Wrenches and Canes can \c3BREAK DOWN DOORS\c6, however, it takes a while to do so and it's incredibly loud.";
			%text[%count++] = " \c6You can block incoming attacks with \c3JET\c6 key (default\c3: RightClick\c6). Earlier you block, less damage you receive. Blocking drains attacker's and your stamina.";
			%text[%count++] = " \c6Attacks from behind have multiplied damage. Use this to your advantage!";
			%text[%count++] = " \c6Knives cannot block, but they are incredibly fast and drain little stamina from you. They're a great offensive weapon.";
			%text[%count++] = " \c5Page Up to read the above.";
		case "6" or "rules":
			%text[%count++] = "\c3[RULES]";
			%text[%count++] = " \c31\c6. \c0Don't be a dick!\c6 We're all here to have fun. If you block doorways, lock people in your rooms or break dormitory doors as a non-killer w/o a reason you will be banned.";
			%text[%count++] = " \c32\c6. \c0Do not gamethrow!\c6 This means don't screw up evidence by cleaning up the crime scene and don't toss the body into incinerator before it's even discovered by everyone else.";
			%text[%count++] = " \c33\c6. \c0Don't freekill!\c6 It's really obvious to admins when you freekill. We will figure if it's self-defence or not, but expect to be banned if you kill someone without a reason.";
			%text[%count++] = " \c34\c6. \c0Don't knock people out randomly!\c6 It slows down the game, distracts from the killer, and just pisses people off.";
			%text[%count++] = " \c35\c6. \c0Don't metagame!\c6 Do not use OOC chat for in-game info! If you are seen talking about ongoing round in team chat chances are you will be banned.";
			%text[%count++] = " \c36\c6. \c0Don't ERP (Erotic RolePlay)!\c6 It is obnoxious as fuck and serves no purpose other than to get some preteens' dick wet and annoy everyone else.";
			%text[%count++] = " \c0    EXAMPLES PROVIDED IN THIS LIST ARE ONLY EXAMPLES. THEY DO NOT ENCAPSULATE THE FULL EXTENT OF THE RULE'S EFFECT. PLEASE USE /REPORT IF YOU WANT TO CLARIFY A RULE WITH AN ADMIN.";
			%text[%count++] = " \c3If someone is breaking the rules, use /report *message* to get an admin's attention!";
			%text[%count++] = " \c5Page Up to read the above.";
		case "7" or "admin":
			if (!(%this.isAdmin || %this.isSuperAdmin))
				return;
			%text[%count++] = "\c3[ADMIN]";
			%text[%count++] = " \c6/mute \c3name time \c7- \c6Restrict \c3name \c6from using OOC chat for \c3time";
			%text[%count++] = " \c6/unmute \c3name \c7- \c6Unmute \c3name \c6, allowing them to use OOC chat again";
			%text[%count++] = " \c6/icmute \c3name time \c7- \c6Restrict \c3name \c6from using in-character chat for \c3time";
			%text[%count++] = " \c6/icunmute \c3name \c7- \c6Unmute \c3name \c6, allowing them to use in-character chat again";
			%text[%count++] = " \c6/viewMute \c7- \c6See everybody that is muted";
			%text[%count++] = " \c6/getKiller \c7- \c6Find out who the killer is (for administration purposes, if you do it to cheat you'll probably be de-admined (and banned!))";
			%text[%count++] = " \c6/whoIs \c3name \c7- \c6Find out who \c3name\c6's in-game alias is";
			%text[%count++] = " \c6/viewInv \c3name \c7- \c6View a client's inventory";
			%text[%count++] = " \c6/viewQueue \c7- \c6See who is in line to be the murderer";
			%text[%count++] = " \c6/addToQueue \c3name \c7- \c6Remove \c3name \c6from the murderer queue";
			%text[%count++] = " \c6/removeFromQueue \c3name \c7- \c6Add \c3name \c6to the murderer queue";
			%text[%count++] = " \c6/damageLogs \c3name \c7- \c6See who \c3name \c6has damaged over the course of the round";
			%text[%count++] = " \c6/toggleOOC \c7- \c6Globally turn on/off OOC chat";
			%text[%count++] = " \c6/reset \c7- \c6Reset the round, if the murderer hasn't done anything in like 5 days you should probably use this command";
			%text[%count++] = " \c6/ac \c3message \c7- \c6Talk through admin-only chat";
			%text[%count++] = " \c5Page Up to read the above.";
		default:
			%text[%count++] = "<font:impact:30>\c6Welcome to Despair Syndrome!";
			%text[%count++] = "\c6======";
			%text[%count++] = " \c6Available topics:";
			%text[%count++] = "   \c31\c6 - \c3gamemode\c6: How the gamemode progresses (for Trial gametype)";
			%text[%count++] = "   \c32\c6 - \c3chat\c6: Chat functionality explained";
			%text[%count++] = "   \c33\c6 - \c3killer\c6: What to do if you're killer";
			%text[%count++] = "   \c34\c6 - \c3mechanics\c6: Game mechanics - sprinting, locking doors, etc.";
			%text[%count++] = "   \c35\c6 - \c3combat\c6: How to melee like a pro.";
			%text[%count++] = "   \c36\c6 - \c3rules\c6: Read this to avoid getting banned";
			if (%this.isAdmin || %this.isSuperAdmin)
				%text[%count++] = "   \c37\c6 - \c3admin\c6: Various admin commands";
			%text[%count++] = "\c6======";
			%text[%count++] = "\c5Say \c3/help *category*\c5 for more info on certain topics.";
	}

	for (%i=1; %i<=%count; %i++)
		messageClient(%this, '', %text[%i]);
}

function serverCmdRules(%this)
{
	serverCmdHelp(%this, "rules");
}