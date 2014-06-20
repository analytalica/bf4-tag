#region imports
using System;
using System.IO;
using System.Timers;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;

using PRoCon.Core;
using PRoCon.Core.Plugin;
#endregion

namespace PRoConEvents
{
    public class Tag : PRoConPluginAPI, IPRoConPluginInterface
    {

        #region classVariables

        private bool pluginEnabled = false;
        private enumBoolYesNo started = enumBoolYesNo.No;
        private enumBoolYesNo end = enumBoolYesNo.No;
        private bool playing = false;
        private List<CPlayerInfo> players;
        private List<CPlayerInfo> startingTeam;
        private List<CPlayerInfo> team;
        private CPlayerInfo speaking;
        private bool playersRequested = false;
        private bool teamRequested = false;
        private bool playerRequested = false;
        private CPlayerInfo it = new CPlayerInfo();
        private int itTeamId = -1;
        private int itSquadId = -1;
        //private List<DateTime> tags = new List<DateTime>();
        private DateTime startTime = DateTime.UtcNow;
        private DateTime recentTag = DateTime.UtcNow;
        private DateTime lessRecentTag = DateTime.UtcNow;
        private double firstTeamTime = 0.0;
        private double secondTeamTime = 0.0;
        private int firstTeamId = 0;
        private int secondTeamId = 0;
        private int choseIt = -1;
        private Timer timer = new Timer();
        private double time = -1;
        private List<char> specialCharacters = new List<char>(){'!', '@', '/'};
        //private String debugLevelString = "1";
        private int debugLevel = 1;

        #endregion

        #region pluginDescribers  //Tag(), GetPluginName(), GetPluginVersion(), GetPluginAuthor(), GetPluginWebsite(), GetPluginDescription()

        public Tag()
        {

        }

        public string GetPluginName()
        {
            return "Tag!";
        }

        public string GetPluginVersion()
        {
            return "1.6.5";
        }

        public string GetPluginAuthor()
        {
            return "F0rceTen2112";
        }

        public string GetPluginWebsite()
        {
            return "purebattlefield.org";
        }

        public string GetPluginDescription()
        {
            return "Sets a random player to \"it\". Whoever kills those that are \"it\" become \"it\"." + 
                   "<br>Set \"Starting Team\" to the team you want the starting person to be on (1 or 2 for team, -1 for random)." +
                   "<br>Set start to \"Yes\" to start the round." +
                   "<br>Set end to \"Yes\", the team with the most time having the \"it\" person on their team will be winner." +
                   "<br>Set round time to any number (can be a decimal) to make the round end then." +
                   "<br>__-1 for no setting. Using this does not disable the End function." +
                   "<br>" +
                   "<br>Use these commands for in chat:" +
                   "<br>!it     for who's it" +
                   "<br>!score  for current score" +
                   "<br>!rules  for game rules" +
                   "<br>!time   for time left" +
                   "<br>!help   for this list" +
                   "<br>" +
                   "<br>Team indices for Vanilla maps as follows:" +
                   "<br>Zavod 311__________Team 1 == RU____Team 2 == US<br>-------------------------------------" +
                   "<br>Lancang Dam________Team 1 == RU____Team 2 == CN<br>-------------------------------------" +
                   "<br>Flood Zone_________Team 1 == US____Team 2 == CN<br>-------------------------------------" +
                   "<br>Golmud Railway_____Team 1 == RU____Team 2 == CN<br>-------------------------------------" +
                   "<br>Paracel Storm______Team 1 == US____Team 2 == CN<br>-------------------------------------" +
                   "<br>Operation Locker___Team 1 == RU____Team 2 == US<br>-------------------------------------" +
                   "<br>Hainan Resort______Team 1 == US____Team 2 == CN<br>-------------------------------------" +
                   "<br>Siege of Shanghai__Team 1 == US____Team 2 == CN<br>-------------------------------------" +
                   "<br>Rogue Transmission_Team 1 == RU____Team 2 == US<br>-------------------------------------" +
                   "<br>Dawnbreaker________Team 1 == US____Team 2 == CN<br>-------------------------------------" +
                   "<br><br>China Rising" +
                   "<br>Silk Road__________Team 1 == US____Team 2 == CN<br>-------------------------------------" +
                   "<br>Altai Range________Team 1 == US____Team 2 == CN<br>-------------------------------------" +
                   "<br>Guilin Peaks_______Team 1 == US____Team 2 == CN<br>-------------------------------------" +
                   "<br>Dragon Pass________Team 1 == US____Team 2 == CN<br>-------------------------------------" +
                   "<br><br>Second Assault" +
                   "<br>Caspian Border 2014___Team 1 == US____Team 2 == RU<br>-------------------------------------" +
                   "<br>Firestorm 2014________Team 1 == US____Team 2 == RU<br>-------------------------------------" +
                   "<br>Operation Metro 2014__Team 1 == US____Team 2 == RU<br>-------------------------------------" +
                   "<br>Gulf of Oman 2014_____Team 1 == US____Team 2 == RU<br>-------------------------------------" +
                   "<br><br>Naval Strike" +
                   "<br>Lost Islands_______Team 1 == US____Team 2 == CH<br>-------------------------------------" +
                   "<br>Nansha Strike______Team 1 == US____Team 2 == CH<br>-------------------------------------" +
                   "<br>Wave Breaker_______Team 1 == US____Team 2 == CH<br>-------------------------------------" +
                   "<br>Operation Mort_____Team 1 == US____Team 2 == CH<br>-------------------------------------";
        }

        #endregion

        #region message  //toChat( ... ), yell( ... ), toConsole(msgLevel, message)

        public void toChat(String message)
        {
            if (!message.Contains("\n"))
            {
                this.ExecuteCommand("procon.protected.send", "admin.say", message, "all");
            }
            else
            {
                string[] multiMsg = message.Split(new string[] { "\n" }, StringSplitOptions.None);
                foreach (string send in multiMsg)
                {
                    toChat(send);
                }
            }
        }

        public void toChat(String message, String playerName)
        {
            if (!message.Contains("\n"))
            {
                this.ExecuteCommand("procon.protected.send", "admin.say", message, "player", playerName);
            }
            else
            {
                string[] multiMsg = message.Split(new string[] { "\n" }, StringSplitOptions.None);
                foreach (string send in multiMsg)
                {
                    toChat(send, playerName);
                }
            }
        }

        public void toChat(String message, int teamId)
        {
            if (!message.Contains("\n"))
            {
                this.ExecuteCommand("procon.protected.send", "admin.say", message, "team", teamId + "");
            }
            else
            {
                string[] multiMsg = message.Split(new string[] { "\n" }, StringSplitOptions.None);
                foreach (string send in multiMsg)
                {
                    toChat(send, teamId);
                }
            }
        }

        public void toChat(String message, int teamId, int squadId)
        {
            if (!message.Contains("\n"))
            {
                this.ExecuteCommand("procon.protected.send", "admin.say", message, "squad", teamId + "", squadId + "");
            }
            else
            {
                string[] multiMsg = message.Split(new string[] { "\n" }, StringSplitOptions.None);
                foreach (string send in multiMsg)
                {
                    toChat(send, teamId, squadId);
                }
            }
        }

        public void yell(String message)
        {
            if (!message.Contains("\n"))
            {
                this.ExecuteCommand("procon.protected.send", "admin.yell", message, "10", "all");
            }
            else
            {
                string[] multiMsg = message.Split(new string[] { "\n" }, StringSplitOptions.None);
                foreach (string send in multiMsg)
                {
                    yell(send);
                }
            }
        }

        public void yell(String message, int teamId)
        {
            if (!message.Contains("\n"))
            {
                this.ExecuteCommand("procon.protected.send", "admin.yell", message, "10", "team", teamId + "");
            }
            else
            {
                string[] multiMsg = message.Split(new string[] { "\n" }, StringSplitOptions.None);
                foreach (string send in multiMsg)
                {
                    yell(send, teamId);
                }
            }
        }

        public void yell(String message, int teamId, int squadId)
        {
            if (!message.Contains("\n"))
            {
                this.ExecuteCommand("procon.protected.send", "admin.yell", message, "10", "squad", teamId + "", squadId + "");
            }
            else
            {
                string[] multiMsg = message.Split(new string[] { "\n" }, StringSplitOptions.None);
                foreach (string send in multiMsg)
                {
                    yell(send, teamId, squadId);
                }
            }
        }

        public void yell(String message, String playerName)
        {
            if (!message.Contains("\n"))
            {
                this.ExecuteCommand("procon.protected.send", "admin.yell", message, "10", "player", playerName);
            }
            else
            {
                string[] multiMsg = message.Split(new string[] { "\n" }, StringSplitOptions.None);
                foreach (string send in multiMsg)
                {
                    yell(send, playerName);
                }
            }
        }

        public void toConsole(int msgLevel, String message)
        {
            if (debugLevel >= msgLevel)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", message);
            }
        }

        #endregion

        #region helpers

        private void timerEvent(object sender, ElapsedEventArgs e)
        {
            this.endGame();
        }

        private void endGame()
        {
            this.timer.Enabled = false;
            //this.tags.Add(DateTime.UtcNow);
            this.lessRecentTag = this.recentTag;
            this.recentTag = DateTime.UtcNow;
            if (this.it.TeamID == this.firstTeamId)
                this.firstTeamTime += (this.recentTag - this.lessRecentTag).TotalMilliseconds;
            else
                this.secondTeamTime += (this.recentTag - this.lessRecentTag).TotalMilliseconds;
            this.playing = false;
            this.end = enumBoolYesNo.No;
            if (this.firstTeamTime > this.secondTeamTime)
            {
                this.yell("Your team won with " + timeChange(firstTeamTime / 1000) + " of having someone be \"it\" against their " + timeChange(secondTeamTime / 1000) + "!", firstTeamId);
                this.yell("Your team lost with " + timeChange(secondTeamTime / 1000) + " of having someone be \"it\" against their " + timeChange(firstTeamTime / 1000) + "!", secondTeamId);

                this.toChat("Your team won with " + timeChange(firstTeamTime / 1000) + " of having someone be \"it\" against their " + timeChange(secondTeamTime / 1000) + "!", firstTeamId);
                this.toChat("Your team lost with " + timeChange(secondTeamTime / 1000) + " of having someone be \"it\" against their " + timeChange(firstTeamTime / 1000) + "!", secondTeamId);
            }
            else if (this.firstTeamTime < this.secondTeamTime)
            {
                this.yell("Your team lost with " + timeChange(firstTeamTime / 1000) + " of having someone be \"it\" against their " + timeChange(secondTeamTime / 1000) + "!", firstTeamId);
                this.yell("Your team won with " + timeChange(secondTeamTime / 1000) + " of having someone be \"it\" against their " + timeChange(firstTeamTime / 1000) + "!", secondTeamId);

                this.toChat("Your team lost with " + timeChange(firstTeamTime / 1000) + " of having someone be \"it\" against their " + timeChange(secondTeamTime / 1000) + "!", firstTeamId);
                this.toChat("Your team won with " + timeChange(secondTeamTime / 1000) + " of having someone be \"it\" against their " + timeChange(firstTeamTime / 1000) + "!", secondTeamId);
            }
            else
            {
                this.yell("Holy god. You all managed to have equal times of " + timeChange(firstTeamTime / 1000) + ". Didn't even think that was remotely possible.");

                this.toChat("Holy god. You all managed to have equal times of " + timeChange(firstTeamTime / 1000) + ". Didn't even think that was remotely possible.");
            }
            this.toConsole(1, "Tag ended.");
            this.recentTag = DateTime.UtcNow;
            this.lessRecentTag = DateTime.UtcNow;
            this.firstTeamTime = 0;
            this.secondTeamTime = 0;
            this.firstTeamId = 0;
            this.secondTeamId = 0;
            this.it = new CPlayerInfo();
        }

        private void startGame(CPlayerInfo player)
        {
            this.it = player;
            this.itTeamId = this.it.TeamID;
            this.itSquadId = this.it.SquadID;
            this.firstTeamId = this.it.TeamID;
            this.secondTeamId = (this.firstTeamId % 2) + 1;
            //this.toChat("Tag is strarting! Type !rules for rules or !help for a list of commands.");
            this.yell(it.SoldierName + " is it! Go get 'em!!!");
            this.toChat(it.SoldierName + " is it! Go get 'em!!!");
            //this.tags.Add(DateTime.UtcNow);
            this.lessRecentTag = this.recentTag;
            this.recentTag = DateTime.UtcNow;
            this.startTime = DateTime.UtcNow;
            if (time > 0)
            {
                this.timer = new Timer(time * 60000);
                this.timer.Elapsed += new ElapsedEventHandler(timerEvent);
                this.timer.Enabled = true;
            }
            this.playing = true;
            this.toConsole(1, "Tag started.");
        }

        private void someoneSpoke(string speaker, string message)
        {
            if (specialCharacters.Contains(message[0]))
            {
                //int TeamID = whoSpoke(speaker).TeamID;
                //string whichTeam = "";
                //if (this.playing && TeamID == this.it.TeamID)
                //    whichTeam = "Your Team!";
                //else
                //    whichTeam = "Enemy Team!";
                if (message.Substring(1) == "it")
                {
                    if (playing)
                        this.toChat(it.SoldierName + " has been it for " + timeChange((DateTime.UtcNow - this.recentTag).TotalMilliseconds / 1000) + ".", speaker);
                    else
                        this.toChat("There is no game going on right now.", speaker);
                }
                else if (message.Substring(1) == "score")
                {
                    if (playing)
                    {
                        if (this.it.TeamID == this.firstTeamId)
                            this.toChat("The it team has a total of " + timeChange((firstTeamTime + (DateTime.UtcNow - this.recentTag).TotalMilliseconds) / 1000) + "." +
                                "\nThe non-it team has a total of " + timeChange(secondTeamTime / 1000) + ".", speaker);
                        else
                            this.toChat("The it team has a total of " + timeChange((secondTeamTime + (DateTime.UtcNow - this.recentTag).TotalMilliseconds) / 1000) + "." +
                                "\nThe non-it team has a total of " + timeChange(firstTeamTime / 1000) + ".", speaker);
                    }
                    else
                        this.toChat("There is no game going on.", speaker);
                }
                else if (message.Substring(1) == "help" || message.Substring(1) == "")
                    this.toChat("Type these commands for:\n" +
                                "  !it     who's it.\n" +
                                "  !score  the current score.\n" +
                                "  !rules  game rules.\n" +
                                "  !time   time left.\n" +
                                "  !help   this list.", speaker);
                else if (message.Substring(1) == "rules")
                {
                    if (this.playing)
                        this.toChat("You are playing tag!\n" +
                                    "The goal is to kill/defend whoever is it!\n" +
                                    it.SoldierName + " is it right now.\n" +
                                    "Type \"!help\" for more commands.", speaker);
                    else
                        this.toChat("This is tag!\n" +
                                    "The goal is to kill/defend whoever is it!\n" +
                                    "Type \"!help\" for more commands.", speaker);
                }
                else if (message.Substring(1) == "time" || message.Substring(1) == "timeLeft")
                {
                    if (this.playing)
                    {
                        if (time > 0)
                            this.toChat("There are " + timeChange((time * 60.0) - (((DateTime.UtcNow - startTime).TotalMilliseconds) / 1000.0)) + " left.", speaker);
                        else
                            this.toChat("There is no time limit set.", speaker);
                    }
                    else
                        this.toChat("There is no game going on.", speaker);
                }
                else if (message == "!test")
                    ;
            }
        }

        //private CPlayerInfo whoSpoke(string speaker)
        //{
        //    this.speaking = new CPlayerInfo();
        //    this.ExecuteCommand("procon.protected.send", "admin.ListPlayers", "player", speaker);
        //    while (this.speaking.SoldierName == "") { }
        //    return this.speaking;
        //}

        private string timeChange(double secondsD)
        {
            int seconds = (int)(secondsD + 0.5);
            if (seconds < 60)
                return seconds + "s";
            else
                return (seconds / 60) + "m" + (seconds % 60) + "s";
        }

        #endregion

        #region pluginLoadedEnableDisable  //pluginLoaded( ... ), OnPluginEnable(), OnPluginDisable()

        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion)
        {
            this.RegisterEvents(this.GetType().Name, "OnPluginLoaded", "OnPlayerKilled", "OnListPlayers", "OnGlobalChat", "OnTeamChat", "OnSquadChat", "OnPlayerTeamChange", "OnPlayerLeft", "OnRoundOver");
        }

        public void OnPluginEnable()
        {
            this.pluginEnabled = true;
            this.toConsole(1, "Tag Enabled.");
        }

        public void OnPluginDisable()
        {
            this.pluginEnabled = false;
            this.started = enumBoolYesNo.No;
            this.playing = false;
            this.recentTag = DateTime.UtcNow;
            this.lessRecentTag = DateTime.UtcNow;
            this.firstTeamTime = 0;
            this.secondTeamTime = 0;
            this.firstTeamId = 0;
            this.secondTeamId = 0;
            this.it = new CPlayerInfo();
            this.timer.Enabled = false;
            this.toConsole(1, "Tag Disabled");
        }

        #endregion

        #region rconCommands  //OnListPlayers(players, subset), OnPlayerKilled(kKillerVictimDetails)

        public void OnRoundOver(int winningTeamId)
        {
            if (this.pluginEnabled && this.playing)
            {
                this.endGame();
            }
        }

        public void OnPlayerLeft(CPlayerInfo playerInfo)
        {
            if (this.pluginEnabled && this.it.SoldierName == playerInfo.SoldierName)
            {
                this.toChat(this.it.SoldierName + " left. Whoops.");
                this.yell(this.it.SoldierName + " left. Whoops.");
                this.teamRequested = true;
                this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "team", ((this.it.TeamID % 2) + 1) + "");
            }
        }

        public void OnPlayerTeamChange(string soldierName, int teamId, int squadId)
        {
            if (this.pluginEnabled && this.it.SoldierName == soldierName && this.itTeamId != teamId)
            {
                this.toConsole(1, "The \"it\" person is trying to switch teams. Switching back.");
                this.toChat("Please don't switch teams while you are it.", soldierName);
                this.ExecuteCommand("procon.protected.send", "admin.movePlayer", soldierName, this.itTeamId + "", this.itSquadId + "", "true");
            }
        }

        public void OnGlobalChat(string speaker, string message)
        {
            if (this.pluginEnabled)
                someoneSpoke(speaker, message);
        }

        public void OnTeamChat(string speaker, string message, int teamId)
        {
            if (this.pluginEnabled)
                someoneSpoke(speaker, message);
        }

        public void OnSquadChat(string speaker, string message, int teamId, int squadId)
        {
            if (this.pluginEnabled)
                someoneSpoke(speaker, message);
        }

        public void OnListPlayers(List<CPlayerInfo> players, CPlayerSubset subset)
        {
            //this.toConsole(1, "(Tag, 2) Players received.");
            //this.toChat("Tag ListPlayer sent:\n" +
            //               "SoldierName  " + subset.SoldierName +
            //             "\nTeamID       " + subset.TeamID + 
            //             "\nSquadID      " + subset.SquadID);
            /*if (subset.TeamID == 0 && subset.SquadID == 0 && !this.playersRequested && !this.teamRequested)
            {
                this.toConsole(3, "Tag! subset.TeamID == 0 && subset.SquadID == 0 && !this.playersRequested && !this.teamRequested");
                this.players = players;
            }
            else if (subset.TeamID == 0 && subset.SquadID == 0 && !this.teamRequested)
            {
                this.toConsole(3, "Tag! subset.TeamID == 0 && subset.SquadID == 0 && !this.teamRequested");
                this.playersRequested = false;
                this.players = players;
                Random rand = new Random();
                this.startGame(this.players[rand.Next(0, this.players.Count)]);
            }
            //else if (subset.SoldierName != "")
            //    this.speaking = players[0];
            else if (!this.teamRequested)
            {
                this.toConsole(3, "Tag! ListPlayers --> !this.teamRequested");
                //this.startingTeam = players;
                Random rand = new Random();
                startGame(players[rand.Next(0, players.Count)]);
            }
            else
            {
                this.toConsole(3, "Tag! ListPlayers --> else  and  subset.TeamID = " + subset.TeamID + "  and  this.it.TeamID = " + this.it.TeamID);
                this.teamRequested = false;
                this.lessRecentTag = this.recentTag;
                this.recentTag = DateTime.UtcNow;
                if (this.it.TeamID == this.firstTeamId)
                    this.firstTeamTime += (this.recentTag - this.lessRecentTag).TotalMilliseconds;
                else
                    this.secondTeamTime += (this.recentTag - this.lessRecentTag).TotalMilliseconds;
                Random rand = new Random();
                this.it = players[rand.Next(0, players.Count)];
                this.itTeamId = this.it.TeamID;
                this.itSquadId = this.it.SquadID;
                this.toChat("Now " + this.it.SoldierName + " is it!");
                this.yell("Now " + this.it.SoldierName + " is it!");
            }*/
            if (this.pluginEnabled)
            {
                if (subset.TeamID != 0 && this.teamRequested && !this.playersRequested && this.playing)
                {
                    this.toConsole(2, "Tag!: ListPlayers ---> subset.TeamID != 0 && this.teamRequested && !this.playersRequested");
                    this.teamRequested = false;
                    this.lessRecentTag = this.recentTag;
                    this.recentTag = DateTime.UtcNow;
                    if (this.it.TeamID == this.firstTeamId)
                        this.firstTeamTime += (this.recentTag - this.lessRecentTag).TotalMilliseconds;
                    else
                        this.secondTeamTime += (this.recentTag - this.lessRecentTag).TotalMilliseconds;
                    Random rand = new Random();
                    this.it = players[rand.Next(0, players.Count)];
                    this.itTeamId = this.it.TeamID;
                    this.itSquadId = this.it.SquadID;
                    this.toChat("Now " + this.it.SoldierName + " is it!");
                    this.yell("Now " + this.it.SoldierName + " is it!");
                }
                else if (subset.TeamID == 0 && this.playersRequested && !this.teamRequested)
                {
                    this.toConsole(2, "Tag!: ListPlayers ---> subset.TeamID == 0 && this.playersRequested && !this.teamRequested");
                    this.players = players;
                    Random rand = new Random();
                    this.startGame(this.players[rand.Next(0, this.players.Count)]);
                }
                else if (subset.TeamID != 0 && !this.teamRequested)
                {
                    this.toConsole(2, "Tag!: ListPlayers ---> subset.TeamID != 0 && !this.teamRequested");
                    Random rand = new Random();
                    startGame(players[rand.Next(0, players.Count)]);
                }
                else
                {
                    this.toConsole(3, "Tag!: ListPlayers ---> else");
                    this.players = players;
                }
            }
        }

        public void OnPlayerKilled(Kill kKillerVictimDetails)
        {
            if (this.pluginEnabled && this.playing)
            {
                if (kKillerVictimDetails.Victim.SoldierName == this.it.SoldierName)
                {
                    if (kKillerVictimDetails.Killer.SoldierName.Length != 0 && !kKillerVictimDetails.IsSuicide)
                    {
                        //this.tags.Add(DateTime.UtcNow);
                        this.lessRecentTag = this.recentTag;
                        this.recentTag = DateTime.UtcNow;
                        if (this.it.TeamID == this.firstTeamId)
                            this.firstTeamTime += (this.recentTag - this.lessRecentTag).TotalMilliseconds;
                        else
                            this.secondTeamTime += (this.recentTag - this.lessRecentTag).TotalMilliseconds;
                        this.it = kKillerVictimDetails.Killer;
                        this.itTeamId = this.it.TeamID;
                        this.itSquadId = this.it.SquadID;
                        this.toChat(this.it.SoldierName + " got " + kKillerVictimDetails.Victim.SoldierName + "! Now " + this.it.SoldierName + " is it!");
                        this.yell(this.it.SoldierName + " got " + kKillerVictimDetails.Victim.SoldierName + "! Now " + this.it.SoldierName + " is it!");
                    }
                    else
                    {
                        this.toChat(this.it.SoldierName + " suicided! Nice job!");
                        this.yell(this.it.SoldierName + " suicided! Nice job!");
                        this.teamRequested = true;
                        this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "team", ((this.it.TeamID % 2) + 1) + "");
                        this.toConsole(2, "this.ExecuteCommand(\"procon.protected.send\", \"admin.listPlayers\", \"team\", ((this.it.TeamID % 2) + 1) + \"\");");
                    }
                }
            }
        }

        #endregion

        #region pluginVariables  //GetDisplayPluginVariables(), GetPluginVariables(), SetPluginVariable( ... )

        public List<CPluginVariable> GetDisplayPluginVariables()
        {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();
            //lstReturn.Add(new CPluginVariable("Section Header|Printed", typeof(type), variable));
            lstReturn.Add(new CPluginVariable("Settings|Starting Team (1, 2, or anything else)", typeof(int), this.choseIt));
            lstReturn.Add(new CPluginVariable("Settings|Debug Level", typeof(int), this.debugLevel));
            lstReturn.Add(new CPluginVariable("Settings|Round time(m)", typeof(double), this.time));
            lstReturn.Add(new CPluginVariable("Game|Start?", typeof(enumBoolYesNo), this.started));
            lstReturn.Add(new CPluginVariable("Game|End?", typeof(enumBoolYesNo), this.end));
            return lstReturn;
        }

        public List<CPluginVariable> GetPluginVariables()
        {
            return GetDisplayPluginVariables();
        }

        //Set variables.
        public void SetPluginVariable(String strVariable, String strValue)
        {
            if (strVariable.Contains("Starting Team (1, 2, or anything else)"))
            {
                int original = this.choseIt;
                try {
                    this.choseIt = Convert.ToInt32(strValue);
                    if ((this.choseIt == 1 || this.choseIt == 2))
                        this.toConsole(1, "Tag!: There will now be a starting team:  " + this.choseIt + "");
                    else
                        this.toConsole(1, "Tag!: There will no longer be a starting team.");
                }
                catch (Exception e)
                {
                    this.toConsole(1, "Tag!: Needs to be an integer.");
                    this.choseIt = original;
                }
            }
            else if (strVariable.Contains("Round time(m)"))
            {
                double original = this.time;
                try
                {
                    this.time = Convert.ToDouble(strValue);
                    if (this.time > 0)
                        this.toConsole(1, "Tag!: There will now be a time limit:  " + this.time + " minutes.");
                    else
                        this.toConsole(1, "Tag!: There will no longer be a time limit.");
                }
                catch (Exception e)
                {
                    this.toConsole(1, "Tag!: Needs to be an number.");
                    this.time = original;
                }
            }
            else if (strVariable.Contains("Debug Level"))
            {
                try
                {
                    this.debugLevel = Convert.ToInt32(strValue);
                    this.toConsole(1, "Tag!: Debug Level set to " + this.debugLevel + "");
                }
                catch (Exception e)
                {
                    this.toConsole(1, "Tag!: Needs to be an integer.");
                    this.debugLevel = 1;
                }
            }
            else if (Regex.Match(strVariable, @"Start?").Success && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                if ((enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue) == enumBoolYesNo.Yes)
                {
                    if (this.pluginEnabled)
                    {
                        this.recentTag = DateTime.UtcNow;
                        this.lessRecentTag = DateTime.UtcNow;
                        this.firstTeamTime = 0;
                        this.secondTeamTime = 0;
                        this.firstTeamId = 0;
                        this.secondTeamId = 0;
                        this.it = new CPlayerInfo();
                        this.started = enumBoolYesNo.Yes;
                        this.toConsole(1, "Tag starting.");
                        this.toChat("Tag starting!\nType \"!it\" for who's it, \"!score\" for the current score, \"!rules\" for rules, \"!time\" for time left, and \"!help\" for these commands.");
                        Random rand = new Random();
                        if (choseIt == 1 || choseIt == 2)
                        {
                            this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "team", choseIt + "");
                            this.toConsole(2, "Tag! this.ExecuteCommand(\"procon.protected.send\", \"admin.listPlayers\", \"team\", choseIt + \"\");");
                        }
                        else
                        {
                            this.playersRequested = true;
                            this.ExecuteCommand("procon.protected.send", "admin.listPlayers", "all");
                            this.toConsole(2, "Tag! this.ExecuteCommand(\"procon.protected.send\", \"admin.listPlayers\", \"all\");");
                        }
                    }
                    else
                        this.toConsole(1, "Tag!: Please enable plugin first.");
                    this.started = enumBoolYesNo.No;
                }
                else if ((enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue) == enumBoolYesNo.No)
                {
                    this.started = enumBoolYesNo.No;
                }
            }
            else if (Regex.Match(strVariable, @"End?").Success && Enum.IsDefined(typeof(enumBoolYesNo), strValue))
            {
                if ((enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue) == enumBoolYesNo.Yes)
                {
                    if (this.pluginEnabled && this.playing)
                    {
                        this.endGame();
                    }
                    else
                        this.toConsole(1, "Tag!: Please enable plugin first and be sure a game is going on.");
                    this.started = enumBoolYesNo.No;
                }
                else if ((enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue) == enumBoolYesNo.No)
                {
                    this.started = enumBoolYesNo.No;
                }
            }
        }

        #endregion

    }
}