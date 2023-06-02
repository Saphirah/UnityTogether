#Global variables

globalvar Team1Ready 0
globalvar Team2Ready 1
globalvar Team1ReadyCD 2
globalvar Team2ReadyCD 3
globalvar NewRound 4
globalvar HUD1 7
globalvar HUD2 8
globalvar FirstAtkScore 9
globalvar FirstAtkScoreSaved 11
globalvar EvenWincondition 14
globalvar EvenWinconditionCP 15
globalvar OT_Proc 22
globalvar PayloadPercent 25
globalvar CapturePercent 26
globalvar Score 27


/*
rule "Gamemode Settings":
*/

rule "Remove Hero Assembly":
    @Condition isAssemblingHeroes() == true
    
    setMatchTime(5)
    wait(6)
    setMatchTime(10)


rule "Set Variables":
    @Condition isGameInProgress() == false
    @Condition NewRound == 0
    
    Team1Ready = false
    Team1ReadyCD = false
    Team2Ready = false
    Team2ReadyCD = false
    F = true
    G = true
    #HUD1 = 1
    #HUD2 = 1
    #destroyAllHudTexts()
    if getMatchRound() == 2:
        if getCurrentGamemode() != Gamemode.CONTROL:
            Y = 1
    wait(1)
    NewRound = 1


rule "Round Reset":
    @Condition isGameInProgress() == false
    
    NewRound = 0
    #wait(0.25)
    #NewRound = 1
    if getMatchRound() == 2:
        if getCurrentGamemode() != Gamemode.CONTROL:
            Y = 1


rule "Control Game End":
    @Condition teamScore(Team.1) + teamScore(Team.2) == 3 == true
    @Condition getCurrentGamemode() == Gamemode.CONTROL
    
    if teamScore(Team.1) > teamScore(Team.2):
        declareTeamVictory(Team.1)
    elif teamScore(Team.2) > teamScore(Team.1):
        declareTeamVictory(Team.2)


rule "Hybrid + Escort Score Change":
    @Condition isMatchBetweenRounds() == true
    @Condition teamScore(Team.2) <= 3
    @Condition K == 0
    @Condition getCurrentGamemode() != Gamemode.PUSH
    @Condition getCurrentGamemode() != Gamemode.CONTROL
    
    FirstAtkScore = teamScore(Team.2)
    wait(0.25)
    setTeamScore(Team.2, 3)
    K = 1
    wait(30)
    FirstAtkScoreSaved = 1


rule "Game Over (Hybrid + Escort)":
    @Condition FirstAtkScoreSaved == 1
    @Condition isMatchBetweenRounds() == true
    @Condition isTeamOnDefense(Team.2) == true
    @Condition getCurrentGamemode() != Gamemode.ESCORT
    @Condition getCurrentGamemode() != Gamemode.HYBRID
    
    #printLog("Done")
    setTeamScore(Team.2, FirstAtkScore)
    if teamScore(Team.1) > teamScore(Team.2):
        declareTeamVictory(Team.1)
    if teamScore(Team.1) == teamScore(Team.2):
        declareDraw()


/*
rule "Ready + Time System":
*/

rule "Add Time":
    @Event eachPlayer
    @Condition isGameInProgress() == false
    @Condition eventPlayer.isHoldingButton(Button.ULTIMATE) == true
    
    setMatchTime(getMatchTime() + 30)
    smallMessage(getAllPlayers(), "{0} added 30 seconds".format(eventPlayer))
    #Unready
    if eventPlayer.getTeam() == Team.1:
        Team1Ready = false
        #else
    elif eventPlayer.getTeam() == Team.2:
        Team2Ready = false


rule "Ready (T1)":
    @Event eachPlayer
    @Team 1
    @Condition eventPlayer.isHoldingButton(Button.INTERACT) == true
    @Condition eventPlayer.isHoldingButton(Button.CROUCH) == true
    @Condition isGameInProgress() == false
    @Condition Team1ReadyCD == false
    @Condition Team1Ready == false
    
    Team1ReadyCD = true
    Team1Ready = true
    smallMessage(getAllPlayers(), "{0} readied {1}".format(eventPlayer, eventPlayer.getTeam()))
    wait(0.2)
    if eventPlayer.isHoldingButton(Button.INTERACT) and eventPlayer.isHoldingButton(Button.CROUCH):
        goto RULE_START
    Team1ReadyCD = false


rule "Ready (T2)":
    @Event eachPlayer
    @Team 2
    @Condition eventPlayer.isHoldingButton(Button.INTERACT) == true
    @Condition eventPlayer.isHoldingButton(Button.CROUCH) == true
    @Condition isGameInProgress() == false
    @Condition Team2ReadyCD == false
    @Condition Team2Ready == false
    
    Team2ReadyCD = true
    Team2Ready = true
    smallMessage(getAllPlayers(), "{0} readied {1}".format(eventPlayer, eventPlayer.getTeam()))
    wait(0.2)
    if eventPlayer.isHoldingButton(Button.INTERACT) and eventPlayer.isHoldingButton(Button.CROUCH):
        goto RULE_START
    Team2ReadyCD = false


rule "Unready (T1)":
    @Event eachPlayer
    @Team 1
    @Condition eventPlayer.isHoldingButton(Button.INTERACT) == true
    @Condition eventPlayer.isHoldingButton(Button.CROUCH) == true
    @Condition isGameInProgress() == false
    @Condition Team1ReadyCD == false
    @Condition Team1Ready == true
    
    Team1ReadyCD = true
    Team1Ready = false
    smallMessage(getAllPlayers(), "{0} unreadied {1}".format(eventPlayer, eventPlayer.getTeam()))
    wait(0.2)
    if eventPlayer.isHoldingButton(Button.INTERACT) and eventPlayer.isHoldingButton(Button.CROUCH):
        goto RULE_START
    Team1ReadyCD = false


rule "Unready (T2)":
    @Event eachPlayer
    @Team 2
    @Condition eventPlayer.isHoldingButton(Button.INTERACT) == true
    @Condition eventPlayer.isHoldingButton(Button.CROUCH) == true
    @Condition isGameInProgress() == false
    @Condition Team2ReadyCD == false
    @Condition Team2Ready == true
    
    Team2ReadyCD = true
    Team2Ready = false
    smallMessage(getAllPlayers(), "{0} unreadied {1}".format(eventPlayer, eventPlayer.getTeam()))
    wait(0.2)
    if eventPlayer.isHoldingButton(Button.INTERACT) and eventPlayer.isHoldingButton(Button.CROUCH):
        goto RULE_START
    Team2ReadyCD = false


rule "Skip Start Timer":
    @Condition Team1Ready == true
    @Condition Team2Ready == true
    @Condition isGameInProgress() == false
    
    setMatchTime(3)
    wait(2.99)
    if RULE_CONDITION:
        return
    setMatchTime(getMatchTime() + 30)


/*
rule "HUD":
    @Event eachPlayer
    
*/

rule "HUD Spawn":
    @Condition isGameInProgress() == false
    @Condition NewRound == 0
    
    if NewRound == 0:
        wait(1)
        goto RULE_START
    hudHeader(getAllPlayers() if isGameInProgress() == false else getPlayersInSlot(6, Team.ALL), "{2} Ready: {0}".format(Team1Ready, NewRound, Team.1), HudPosition.TOP, 1, Color.GREEN if Team1Ready == true else Color.RED, HudReeval.VISIBILITY_STRING_AND_COLOR, SpecVisibility.DEFAULT)
    HUD1[1] = getLastCreatedText()
    hudHeader(getAllPlayers() if isGameInProgress() == false else getPlayersInSlot(6, Team.ALL), "{2} Ready: {0}".format(Team2Ready, NewRound, Team.2), HudPosition.TOP, 1, Color.GREEN if Team2Ready == true else Color.RED, HudReeval.VISIBILITY_STRING_AND_COLOR, SpecVisibility.DEFAULT)
    HUD1[2] = getLastCreatedText()
    hudSubheader(getAllPlayers(), "Press {0}: Add 30 seconds".format(buttonString(Button.ULTIMATE)), HudPosition.RIGHT, 0, Color.WHITE, HudReeval.VISIBILITY_STRING_AND_COLOR, SpecVisibility.NEVER)
    HUD1[3] = getLastCreatedText()
    hudSubheader(getAllPlayers(), "Press {0} + {1}: Ready Team".format(buttonString(Button.CROUCH), buttonString(Button.INTERACT)), HudPosition.RIGHT, 0, Color.WHITE, HudReeval.VISIBILITY_STRING_AND_COLOR, SpecVisibility.NEVER)
    HUD1[4] = getLastCreatedText()
    hudSubheader(getAllPlayers(), "Hold {0}: Teleport to point (on defense)".format(buttonString(Button.MELEE)), HudPosition.RIGHT, 0, Color.WHITE, HudReeval.VISIBILITY_STRING_AND_COLOR, SpecVisibility.NEVER)
    HUD1[5] = getLastCreatedText()
    hudSubheader(getAllPlayers(), "Press {0}: Change Heroes".format(buttonString(Button.RELOAD)), HudPosition.RIGHT, 0, Color.WHITE, HudReeval.VISIBILITY_STRING_AND_COLOR, SpecVisibility.NEVER)
    HUD1[6] = getLastCreatedText()


rule "Remove Hero Select HUD":
    @Condition isGameInProgress() == true
    
    destroyHudText(HUD1[1])
    destroyHudText(HUD1[2])
    destroyHudText(HUD1[3])
    destroyHudText(HUD1[4])
    destroyHudText(HUD1[5])
    destroyHudText(HUD1[6])


rule "Spectator Scoreboard (T1)":
    @Condition isAssemblingHeroes() == true
    @Condition NewRound == 0
    
    if NewRound == 0:
        wait(10)
        goto RULE_START
    hudSubheader([], "Stats: Hero DMG - Dmg Taken (Tank/DPS) / Healing Done (Support)", HudPosition.TOP, 0, Color.WHITE, HudReeval.VISIBILITY_AND_STRING, SpecVisibility.ALWAYS)
    hudSubheader([], "- Final Blows - Eliminations - Ults Used - Deaths - Accuracy", HudPosition.TOP, 0, Color.WHITE, HudReeval.VISIBILITY_AND_STRING, SpecVisibility.ALWAYS)
    hudSubheader([], l"{0} - {1} - {2}".format(l"{0} - {1} - {2}".format(heroIcon(getPlayersInSlot(0, Team.1).getCurrentHero()), round(getPlayersInSlot(0, Team.1).getStatistic(Stat.HERO_DAMAGE_DEALT)), round(getPlayersInSlot(0, Team.1).getStatistic(Stat.DAMAGE_TAKEN))), l"{0} - {1} - {2}".format(getPlayersInSlot(0, Team.1).getStatistic(Stat.FINAL_BLOWS), getPlayersInSlot(0, Team.1).getStatistic(Stat.ELIMINATIONS), getPlayersInSlot(0, Team.1).getStatistic(Stat.ULTIMATES_USED)), "{0} - {1}%".format(getPlayersInSlot(0, Team.1).getStatistic(Stat.DEATHS), round(100 * getPlayersInSlot(0, Team.1).getStatistic(Stat.WEAPON_ACCURACY)))), HudPosition.LEFT, 2, Color.WHITE, HudReeval.SORT_ORDER_AND_STRING, SpecVisibility.ALWAYS)
    hudSubheader([], l"{0} - {1} - {2}".format(l"{0} - {1} - {2}".format(heroIcon(getPlayersInSlot(1, Team.1).getCurrentHero()), round(getPlayersInSlot(1, Team.1).getStatistic(Stat.HERO_DAMAGE_DEALT)), round(getPlayersInSlot(1, Team.1).getStatistic(Stat.DAMAGE_TAKEN))), l"{0} - {1} - {2}".format(getPlayersInSlot(1, Team.1).getStatistic(Stat.FINAL_BLOWS), getPlayersInSlot(1, Team.1).getStatistic(Stat.ELIMINATIONS), getPlayersInSlot(1, Team.1).getStatistic(Stat.ULTIMATES_USED)), "{0} - {1}%".format(getPlayersInSlot(1, Team.1).getStatistic(Stat.DEATHS), round(100 * getPlayersInSlot(1, Team.1).getStatistic(Stat.WEAPON_ACCURACY)))), HudPosition.LEFT, 3, Color.WHITE, HudReeval.SORT_ORDER_AND_STRING, SpecVisibility.ALWAYS)
    hudSubheader([], l"{0} - {1} - {2}".format(l"{0} - {1} - {2}".format(heroIcon(getPlayersInSlot(2, Team.1).getCurrentHero()), round(getPlayersInSlot(2, Team.1).getStatistic(Stat.HERO_DAMAGE_DEALT)), round(getPlayersInSlot(2, Team.1).getStatistic(Stat.DAMAGE_TAKEN))), l"{0} - {1} - {2}".format(getPlayersInSlot(2, Team.1).getStatistic(Stat.FINAL_BLOWS), getPlayersInSlot(2, Team.1).getStatistic(Stat.ELIMINATIONS), getPlayersInSlot(2, Team.1).getStatistic(Stat.ULTIMATES_USED)), "{0} - {1}%".format(getPlayersInSlot(2, Team.1).getStatistic(Stat.DEATHS), round(100 * getPlayersInSlot(2, Team.1).getStatistic(Stat.WEAPON_ACCURACY)))), HudPosition.LEFT, 4, Color.WHITE, HudReeval.SORT_ORDER_AND_STRING, SpecVisibility.ALWAYS)
    hudSubheader([], l"{0} - {1} - {2}".format(l"{0} - {1} - {2}".format(heroIcon(getPlayersInSlot(3, Team.1).getCurrentHero()), round(getPlayersInSlot(3, Team.1).getStatistic(Stat.HERO_DAMAGE_DEALT)), round(getPlayersInSlot(3, Team.1).getStatistic(Stat.HEALING_DEALT))), l"{0} - {1} - {2}".format(getPlayersInSlot(3, Team.1).getStatistic(Stat.FINAL_BLOWS), getPlayersInSlot(3, Team.1).getStatistic(Stat.ELIMINATIONS), getPlayersInSlot(3, Team.1).getStatistic(Stat.ULTIMATES_USED)), "{0} - {1}%".format(getPlayersInSlot(3, Team.1).getStatistic(Stat.DEATHS), round(100 * getPlayersInSlot(3, Team.1).getStatistic(Stat.WEAPON_ACCURACY)))), HudPosition.LEFT, 5, Color.WHITE, HudReeval.SORT_ORDER_AND_STRING, SpecVisibility.ALWAYS)
    hudSubheader([], l"{0} - {1} - {2}".format(l"{0} - {1} - {2}".format(heroIcon(getPlayersInSlot(4, Team.1).getCurrentHero()), round(getPlayersInSlot(4, Team.1).getStatistic(Stat.HERO_DAMAGE_DEALT)), round(getPlayersInSlot(4, Team.1).getStatistic(Stat.HEALING_DEALT))), l"{0} - {1} - {2}".format(getPlayersInSlot(4, Team.1).getStatistic(Stat.FINAL_BLOWS), getPlayersInSlot(4, Team.1).getStatistic(Stat.ELIMINATIONS), getPlayersInSlot(4, Team.1).getStatistic(Stat.ULTIMATES_USED)), "{0} - {1}%".format(getPlayersInSlot(4, Team.1).getStatistic(Stat.DEATHS), round(100 * getPlayersInSlot(4, Team.1).getStatistic(Stat.WEAPON_ACCURACY)))), HudPosition.LEFT, 6, Color.WHITE, HudReeval.SORT_ORDER_AND_STRING, SpecVisibility.ALWAYS)


rule "Spectator Scoreboard (T2)":
    @Condition isAssemblingHeroes() == true
    @Condition NewRound == 0
    
    if NewRound == 0:
        wait(10)
        goto RULE_START
    hudSubheader([], l"{0} - {1} - {2}".format(l"{0} - {1} - {2}".format(heroIcon(getPlayersInSlot(0, Team.2).getCurrentHero()), round(getPlayersInSlot(0, Team.2).getStatistic(Stat.HERO_DAMAGE_DEALT)), round(getPlayersInSlot(0, Team.2).getStatistic(Stat.DAMAGE_TAKEN))), l"{0} - {1} - {2}".format(getPlayersInSlot(0, Team.2).getStatistic(Stat.FINAL_BLOWS), getPlayersInSlot(0, Team.2).getStatistic(Stat.ELIMINATIONS), getPlayersInSlot(0, Team.2).getStatistic(Stat.ULTIMATES_USED)), "{0} - {1}%".format(getPlayersInSlot(0, Team.2).getStatistic(Stat.DEATHS), round(100 * getPlayersInSlot(0, Team.2).getStatistic(Stat.WEAPON_ACCURACY)))), HudPosition.RIGHT, 2, Color.WHITE, HudReeval.SORT_ORDER_AND_STRING, SpecVisibility.ALWAYS)
    hudSubheader([], l"{0} - {1} - {2}".format(l"{0} - {1} - {2}".format(heroIcon(getPlayersInSlot(1, Team.2).getCurrentHero()), round(getPlayersInSlot(1, Team.2).getStatistic(Stat.HERO_DAMAGE_DEALT)), round(getPlayersInSlot(1, Team.2).getStatistic(Stat.DAMAGE_TAKEN))), l"{0} - {1} - {2}".format(getPlayersInSlot(1, Team.2).getStatistic(Stat.FINAL_BLOWS), getPlayersInSlot(1, Team.2).getStatistic(Stat.ELIMINATIONS), getPlayersInSlot(1, Team.2).getStatistic(Stat.ULTIMATES_USED)), "{0} - {1}%".format(getPlayersInSlot(1, Team.2).getStatistic(Stat.DEATHS), round(100 * getPlayersInSlot(1, Team.2).getStatistic(Stat.WEAPON_ACCURACY)))), HudPosition.RIGHT, 3, Color.WHITE, HudReeval.SORT_ORDER_AND_STRING, SpecVisibility.ALWAYS)
    hudSubheader([], l"{0} - {1} - {2}".format(l"{0} - {1} - {2}".format(heroIcon(getPlayersInSlot(2, Team.2).getCurrentHero()), round(getPlayersInSlot(2, Team.2).getStatistic(Stat.HERO_DAMAGE_DEALT)), round(getPlayersInSlot(2, Team.2).getStatistic(Stat.DAMAGE_TAKEN))), l"{0} - {1} - {2}".format(getPlayersInSlot(2, Team.2).getStatistic(Stat.FINAL_BLOWS), getPlayersInSlot(2, Team.2).getStatistic(Stat.ELIMINATIONS), getPlayersInSlot(2, Team.2).getStatistic(Stat.ULTIMATES_USED)), "{0} - {1}%".format(getPlayersInSlot(2, Team.2).getStatistic(Stat.DEATHS), round(100 * getPlayersInSlot(2, Team.2).getStatistic(Stat.WEAPON_ACCURACY)))), HudPosition.RIGHT, 4, Color.WHITE, HudReeval.SORT_ORDER_AND_STRING, SpecVisibility.ALWAYS)
    hudSubheader([], l"{0} - {1} - {2}".format(l"{0} - {1} - {2}".format(heroIcon(getPlayersInSlot(3, Team.2).getCurrentHero()), round(getPlayersInSlot(3, Team.2).getStatistic(Stat.HERO_DAMAGE_DEALT)), round(getPlayersInSlot(3, Team.2).getStatistic(Stat.HEALING_DEALT))), l"{0} - {1} - {2}".format(getPlayersInSlot(3, Team.2).getStatistic(Stat.FINAL_BLOWS), getPlayersInSlot(3, Team.2).getStatistic(Stat.ELIMINATIONS), getPlayersInSlot(3, Team.2).getStatistic(Stat.ULTIMATES_USED)), "{0} - {1}%".format(getPlayersInSlot(3, Team.2).getStatistic(Stat.DEATHS), round(100 * getPlayersInSlot(3, Team.2).getStatistic(Stat.WEAPON_ACCURACY)))), HudPosition.RIGHT, 5, Color.WHITE, HudReeval.SORT_ORDER_AND_STRING, SpecVisibility.ALWAYS)
    hudSubheader([], l"{0} - {1} - {2}".format(l"{0} - {1} - {2}".format(heroIcon(getPlayersInSlot(4, Team.2).getCurrentHero()), round(getPlayersInSlot(4, Team.2).getStatistic(Stat.HERO_DAMAGE_DEALT)), round(getPlayersInSlot(4, Team.2).getStatistic(Stat.HEALING_DEALT))), l"{0} - {1} - {2}".format(getPlayersInSlot(4, Team.2).getStatistic(Stat.FINAL_BLOWS), getPlayersInSlot(4, Team.2).getStatistic(Stat.ELIMINATIONS), getPlayersInSlot(4, Team.2).getStatistic(Stat.ULTIMATES_USED)), "{0} - {1}%".format(getPlayersInSlot(4, Team.2).getStatistic(Stat.DEATHS), round(100 * getPlayersInSlot(4, Team.2).getStatistic(Stat.WEAPON_ACCURACY)))), HudPosition.RIGHT, 6, Color.WHITE, HudReeval.SORT_ORDER_AND_STRING, SpecVisibility.ALWAYS)


/*
rule "Defense TP":
*/

rule "Back to Spawn":
    @Event eachPlayer
    @Condition isGameInProgress() == false
    @Condition isTeamOnDefense(eventPlayer.getTeam()) == true
    @Condition eventPlayer.isInSpawnRoom() == false
    @Condition eventPlayer.isHoldingButton(Button.RELOAD) == true
    
    wait(0.25)
    if not RULE_CONDITION:
        return
    smallMessage(eventPlayer, "Keep holding to teleport to spawn...")
    wait(2.25)
    if not RULE_CONDITION:
        return
    eventPlayer.setAllowedHeroes(getAllHeroes().exclude(eventPlayer.getCurrentHero()))
    eventPlayer.setAllowedHeroes(getAllHeroes())


rule "BIizzard World":
    @Event eachPlayer
    @Condition Map.BLIZZ_WORLD == getCurrentMap()
    @Condition isGameInProgress() == false
    @Condition isTeamOnDefense(eventPlayer.getTeam()) == true
    @Condition eventPlayer.isInSpawnRoom() == true
    @Condition eventPlayer.isHoldingButton(Button.MELEE) == true
    
    eventPlayer.startForcingPosition(vect(3.583, 8.204, 86.208), false)
    wait(0.05)
    eventPlayer.stopForcingPosition()


rule "Eichenwalde":
    @Event eachPlayer
    @Condition Map.EICHENWALDE == getCurrentMap()
    @Condition isGameInProgress() == false
    @Condition isTeamOnDefense(eventPlayer.getTeam()) == true
    @Condition eventPlayer.isInSpawnRoom() == true
    @Condition eventPlayer.isHoldingButton(Button.MELEE) == true
    
    eventPlayer.startForcingPosition(vect(11.757, 12.262, -84.557), false)
    wait(0.05)
    eventPlayer.stopForcingPosition()


rule "Hollywood":
    @Event eachPlayer
    @Condition Map.HOLLYWOOD == getCurrentMap()
    @Condition isGameInProgress() == false
    @Condition isTeamOnDefense(eventPlayer.getTeam()) == true
    @Condition eventPlayer.isInSpawnRoom() == true
    @Condition eventPlayer.isHoldingButton(Button.MELEE) == true
    
    eventPlayer.startForcingPosition(vect(-18.342, 6.751, -12.035), false)
    wait(0.05)
    eventPlayer.stopForcingPosition()


rule "King's Row":
    @Event eachPlayer
    @Condition Map.KINGS_ROW == getCurrentMap()
    @Condition isGameInProgress() == false
    @Condition isTeamOnDefense(eventPlayer.getTeam()) == true
    @Condition eventPlayer.isInSpawnRoom() == true
    @Condition eventPlayer.isHoldingButton(Button.MELEE) == true
    
    eventPlayer.startForcingPosition(vect(-26.436, 7.351, -49.737), false)
    wait(0.05)
    eventPlayer.stopForcingPosition()


rule "Midtown":
    @Event eachPlayer
    @Condition Map.MIDTOWN == getCurrentMap()
    @Condition isGameInProgress() == false
    @Condition isTeamOnDefense(eventPlayer.getTeam()) == true
    @Condition eventPlayer.isInSpawnRoom() == true
    @Condition eventPlayer.isHoldingButton(Button.MELEE) == true
    
    eventPlayer.startForcingPosition(vect(14.468, 8.807, 58.044), false)
    wait(0.05)
    eventPlayer.stopForcingPosition()


rule "Paraiso":
    @Event eachPlayer
    @Condition Map.PARAISO == getCurrentMap()
    @Condition isGameInProgress() == false
    @Condition isTeamOnDefense(eventPlayer.getTeam()) == true
    @Condition eventPlayer.isInSpawnRoom() == true
    @Condition eventPlayer.isHoldingButton(Button.MELEE) == true
    
    eventPlayer.startForcingPosition(vect(-33.622, 6.91, -11.708), false)
    wait(0.05)
    eventPlayer.stopForcingPosition()


rule "Circuit Royal":
    @Event eachPlayer
    @Condition Map.CIRCUIT_ROYAL == getCurrentMap()
    @Condition isGameInProgress() == false
    @Condition isTeamOnDefense(eventPlayer.getTeam()) == true
    @Condition eventPlayer.isInSpawnRoom() == true
    @Condition eventPlayer.isHoldingButton(Button.MELEE) == true
    
    eventPlayer.startForcingPosition(vect(-12.122, 11.91, -6.436), false)
    wait(0.05)
    eventPlayer.stopForcingPosition()


rule "Dorado":
    @Event eachPlayer
    @Condition Map.DORADO == getCurrentMap()
    @Condition isGameInProgress() == false
    @Condition isTeamOnDefense(eventPlayer.getTeam()) == true
    @Condition eventPlayer.isInSpawnRoom() == true
    @Condition eventPlayer.isHoldingButton(Button.MELEE) == true
    
    eventPlayer.startForcingPosition(vect(36.489, 10.823, -32.637), false)
    wait(0.05)
    eventPlayer.stopForcingPosition()


rule "Havana":
    @Event eachPlayer
    @Condition Map.HAVANA == getCurrentMap()
    @Condition isGameInProgress() == false
    @Condition isTeamOnDefense(eventPlayer.getTeam()) == true
    @Condition eventPlayer.isInSpawnRoom() == true
    @Condition eventPlayer.isHoldingButton(Button.MELEE) == true
    
    eventPlayer.startForcingPosition(vect(-14.062, 6.869, -67.215), false)
    wait(0.05)
    eventPlayer.stopForcingPosition()


rule "Junkertown":
    @Event eachPlayer
    @Condition Map.JUNKERTOWN == getCurrentMap()
    @Condition isGameInProgress() == false
    @Condition isTeamOnDefense(eventPlayer.getTeam()) == true
    @Condition eventPlayer.isInSpawnRoom() == true
    @Condition eventPlayer.isHoldingButton(Button.MELEE) == true
    
    eventPlayer.startForcingPosition(vect(-29.443, 13.303, -68.898), false)
    wait(0.05)
    eventPlayer.stopForcingPosition()


rule "Rialto":
    @Event eachPlayer
    @Condition Map.RIALTO == getCurrentMap()
    @Condition isGameInProgress() == false
    @Condition isTeamOnDefense(eventPlayer.getTeam()) == true
    @Condition eventPlayer.isInSpawnRoom() == true
    @Condition eventPlayer.isHoldingButton(Button.MELEE) == true
    
    eventPlayer.startForcingPosition(vect(55.486, 5.573, -39.518), false)
    wait(0.05)
    eventPlayer.stopForcingPosition()


rule "Route 66":
    @Event eachPlayer
    @Condition Map.ROUTE66 == getCurrentMap()
    @Condition isGameInProgress() == false
    @Condition isTeamOnDefense(eventPlayer.getTeam()) == true
    @Condition eventPlayer.isInSpawnRoom() == true
    @Condition eventPlayer.isHoldingButton(Button.MELEE) == true
    
    eventPlayer.startForcingPosition(vect(32.466, 9.268, -4.244), false)
    wait(0.05)
    eventPlayer.stopForcingPosition()


rule "Shambali Monastery":
    @Event eachPlayer
    @Condition Map.SHAMBALI_MONASTERY == getCurrentMap()
    @Condition isGameInProgress() == false
    @Condition isTeamOnDefense(eventPlayer.getTeam()) == true
    @Condition eventPlayer.isInSpawnRoom() == true
    @Condition eventPlayer.isHoldingButton(Button.MELEE) == true
    
    eventPlayer.startForcingPosition(vect(-5.328, 11.946, 69.438), false)
    wait(0.05)
    eventPlayer.stopForcingPosition()


rule "Watchpoint: Gibraltar":
    @Event eachPlayer
    @Condition Map.WATCHPOINT_GIBRALTAR == getCurrentMap()
    @Condition isGameInProgress() == false
    @Condition isTeamOnDefense(eventPlayer.getTeam()) == true
    @Condition eventPlayer.isInSpawnRoom() == true
    @Condition eventPlayer.isHoldingButton(Button.MELEE) == true
    
    eventPlayer.startForcingPosition(vect(32.151, 4.823, -85.594), false)
    wait(0.05)
    eventPlayer.stopForcingPosition()


/*
rule "Working on...":
*/

/*
rule "(Even) Win Condition":
    @Condition isGameInProgress() == true
    @Condition getMatchTime() < 1
    @Condition FirstAtkScoreSaved != 1
    
    if teamScore(Team.2) == 0:
        if getCapturePercentage() >= EvenWinconditionCP:
            EvenWinconditionCP = getCapturePercentage()
    if teamScore(Team.2) > 0:
        if getPayloadProgressPercentage() >= N:
            N = getPayloadProgressPercentage()
    wait(1)
    if RULE_CONDITION:
        goto RULE_START
    if teamScore(Team.2) > 0:
*/

/*
rule "Win Condition Setting":
    @Condition FirstAtkScoreSaved == 1
    @Condition isGameInProgress() == true
    @Condition teamScore(Team.1) == FirstAtkScore
    
    if FirstAtkScore == 0:
        if getCapturePercentage() > EvenWinconditionCP:
            EvenWincondition = true
    if FirstAtkScore > 0:
        if getPayloadProgressPercentage() > N:
            EvenWincondition = true
*/

/*
rule "Rule 38":
    @Condition FirstAtkScoreSaved == 1
    @Condition isGameInProgress() == true
    @Condition teamScore(Team.1) < 3
    @Condition getMatchTime() < 1
    
    Q = true
    setTeamScore(Team.1, teamScore(Team.1) + (3 - teamScore(Team.1)))
*/

/*
rule "Rule 39":
    @Condition isGameInProgress() == false
    @Condition Q == true
    
    setTeamScore(Team.1, 3)
    setTeamScore(Team.2, FirstAtkScore)
    if EvenWincondition == true:
        declareTeamVictory(Team.1)
*/

/*
rule "Rule 40":
    @Event eachPlayer
    @Condition isGameInProgress() == true
    @Condition eventPlayer.isHoldingButton(Button.RELOAD) == true
    
    setMatchTime(getMatchTime() - 20)
*/

/*
rule "Actual Score Set":
    @Condition teamScore(Team.1) == teamScore(Team.2)
    @Condition (getCapturePercentage() > EvenWinconditionCP - 1 or getPayloadProgressPercentage() > N - 1) == true
    @Condition FirstAtkScoreSaved == 1
    
    setTeamScore(Team.2, 3)
*/

/*
rule "Rule 43":
*/

/*
rule "Rule 44":
    @Condition getPlayers(Team.2).isOnObjective() == true
    
    OT_Proc = true
    printLog("step 1")
    waitUntil(getPlayers(Team.2).isOnObjective() == false, 99999)
    printLog("step 2")
    wait(3, Wait.RESTART_WHEN_TRUE)
    printLog("step 3")
    OT_Proc = false
    printLog("step 4")
*/

rule "Save Payload":
    hudText(getAllPlayers(), PayloadPercent[1], Score[1], CapturePercent[1], HudPosition.LEFT, 0, Color.AQUA, Color.AQUA, Color.AQUA, HudReeval.VISIBILITY_SORT_ORDER_STRING_AND_COLOR, SpecVisibility.ALWAYS)
    hudText(getAllPlayers(), PayloadPercent[2], Score[2], CapturePercent[2], HudPosition.LEFT, 0, Color.AQUA, Color.AQUA, Color.AQUA, HudReeval.VISIBILITY_SORT_ORDER_STRING_AND_COLOR, SpecVisibility.ALWAYS)

rule "Save Payload":
    @Condition getCurrentGamemode() == Gamemode.ESCORT or getCurrentGamemode() == Gamemode.HYBRID
    @Condition isGameInProgress()
    while(true):
        PayloadPercent[getMatchRound()] = max(getPayloadProgressPercentage(), PayloadPercent[getMatchRound()])
        Score[getMatchRound()] = max(teamScore(Team.2 if(getMatchRound() == 1) else Team.1), Score[getMatchRound()])
        CapturePercent[getMatchRound()] = max(getCapturePercentage(), CapturePercent[getMatchRound()])
        wait(0.1, Wait.ABORT_WHEN_FALSE)
    

rule "Game Over (Hybrid + Escort)":
    @Condition isMatchBetweenRounds() == true
    @Condition isTeamOnDefense(Team.2) == true
    @Condition getCurrentGamemode() == Gamemode.ESCORT or getCurrentGamemode() == Gamemode.HYBRID

    printLog(CapturePercent[1])
    printLog(PayloadPercent[1])
    printLog(Score[1])
    printLog(CapturePercent[2])
    printLog(PayloadPercent[2])
    printLog(Score[2])

    setTeamScore(Team.1, Score[2])
    setTeamScore(Team.2, Score[1])
    
    if(CapturePercent[1] + PayloadPercent[1] == CapturePercent[2] + PayloadPercent[2]):
        declareDraw()
    else:
        if(CapturePercent[1] + PayloadPercent[1] > CapturePercent[2] + PayloadPercent[2]):
            declareTeamVictory(Team.2)
        else:
            declareTeamVictory(Team.1)

rule "Rule 45":
    @Condition getPlayersInSlot(0, Team.2).isHoldingButton(Button.CROUCH) == true
    
    setMatchTime(10)