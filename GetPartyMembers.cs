using System;
using Dalamud.Plugin;
using ImGuiNET;
using ImGuiScene;
using Dalamud.Configuration;
using Num = System.Numerics;
using Dalamud.Game.Command;
using Dalamud.Interface;
using Dalamud.Game.ClientState.Actors;
using Dalamud.Game.ClientState.Actors.Types;

namespace PixelPerfect
{
    class GetPartyMembers
    {
        //This will hold all the actors. 
        // private PlayerCharacter[] _partyMembers = new PlayerCharacter[8];
        //Names of all the players we get from the party list, we check the actor table against this. 
        private string[] playerNames;

        private DalamudPluginInterface _pi;
        //The party list object in the game. 


        public GetPartyMembers(PlayerCharacter player, DalamudPluginInterface pi)
        {
            //Consideration:
            //  IF I keep the array the same size every time, then I don't need
            //  to keep creating new arrays, just adding more info to the same one.
            //  But I need a way to terminate it, and for people to get the size of it.
            //  Better encapsulation if it DOESN"T have null elements.
            //  But more 'expensive' if I create new arrays...
            //  Also, depending on if Pets or Companions are party members, may have unintended consequences...
            _pi = pi;
            //If this is 0, we can't populate the list. 
            //This is probably the case if there's no party at all?
            //Furthermore, if a pet counts as being in a party this might have unintended consequences. 

            if(_pi.ClientState.PartyList.Length == 0)
            {
                Dalamud.Plugin.PluginLog.Log(" Party length of Zero. ");
            }

            return;
        }
    }
}
