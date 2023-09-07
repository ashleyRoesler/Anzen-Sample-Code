# Anzen Sample Code
Various C# scripts from _Anzen: Echoes of War_ that I either made from scratch or heavily contributed to.

`AudioOptionsController`: Gives the audio options menu functionality. The player can toggle on/off and adjust the overall audio volume, or they can configure each type of audio (music, dialogue, SFX) separately.

`CampaignGameModeManager`: Runs the campaign game mode. Spawns/respawns players, transitions from the lobby to the actual game, and spawns a "Pilot" for each player (the little floating robot that follows the player).

`InventoryComponent`: Manages the player's inventory. 

`MainMenuUIController`: Gives the main menu functionality. Players can navigate to the game mode menu, where they choose arena or campaign. They can also start the game, either offline or online.

`PlayerAcceptanceManager`: A temporary fix to stop players from joining game lobbies that were already full, or games that had already started. It uses Unity MLAPI's custom messaging and callback system to check if the player's client can connect to the game server, and will send a rejection message back to the client if they cannot join.

`GameModeCard`: An example of one of our types of "Cards", which is how we update many different UI elements to display information about something. In this case, the `GameModeCard` is used to display game mode information on the game mode buttons in the game mode selection menu.

`VFXModule`: This is a component that can be added on to our "skill behavior" prefabs, which are how we customize player and enemy skills/attacks. This specific module lets you add VFX to the skill. For example, it adds a weapon trail to the player's mace when they use the "leap" skill, and another vfx when they hit something with that skill. Or, it adds a trail to the boss Okkar's arm when he tries to slap the player.

![leapVFXaddon](https://github.com/ashleyRoesler/Anzen-Sample-Code/assets/37816332/c27569ed-f84f-46c1-9cb0-cc557d4ab206)
![OkkarSlapVFXaddon](https://github.com/ashleyRoesler/Anzen-Sample-Code/assets/37816332/9b1f9fa9-c8bd-47fc-a34e-7e9f08485146)

### Custom Timeline Signals
To create our cutscenes, we additively load our cutscene scene on top of our game scene. An issue with this is that the cutscene timeline cannot communicate with our custom event system, which allows us to trigger a myriad of different events with an "event trigger" component, like opening doors or changing object models. In one of our cutscenes, a large machine is destroyed, and we needed to be able to swap out the machine's model so that it looks broken in-game after the cutscene ends. To do this, I made a system that takes Unity's built-in timeline signals and re-invokes the signal. Then we can have receivers that listen for that new signal, and trigger our custom events.

`ExtendedTimelineSignal`: Does the "re-invoking" part of the signal, sending it out to our event receivers.

`TimelineReceiver`: Receives the signal and triggers the events.

![LoomCutsceneSignal](https://github.com/ashleyRoesler/Anzen-Sample-Code/assets/37816332/b1998b1e-3c14-4d59-bb42-2149241190bb)
![LoomEvent](https://github.com/ashleyRoesler/Anzen-Sample-Code/assets/37816332/8519597a-615f-471c-862c-7ee913f1ca2c)

### Item Tooltips
`ItemSlotCard`: Another one of our "Cards". This one is used to update item tooltips for items in the player's inventory. It contains three other kinds of cards, which allows us to use the same script for all item types, like armor, weapons, and consumables.

`ItemCard`: Shows the generic item info that is shared across all item types, like item name, rarity, and flavor text.

`EquippableCard`: Shows the info common to all equippable items (weapons and armor). These are the attributes and stats that the items add to the player.

`WeaponCard`: Shows the info specific to weapons, like damage and speed.

![ItemTooltip](https://github.com/ashleyRoesler/Anzen-Sample-Code/assets/37816332/6027ebbf-50ad-4d93-ad41-7c5d12ea81a9)
![Screenshot (288)](https://github.com/ashleyRoesler/Anzen-Sample-Code/assets/37816332/c4b6d85d-c62f-4fc5-8958-a8e23a4e95f4)
