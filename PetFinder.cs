using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud;
using Dalamud.Plugin;
using Dalamud.Game.ClientState.Actors;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState;
using Dalamud.Interface;



/*This Class Retrieves the pet actor for a given playercharacter actor.
 *  and returns NULL if it either doesn't exist, or the player is NOT
 *  a Summoner, Arcanist, or Scholar.
 */

namespace PixelPerfect
{
    class PetFinder
    {
        private DalamudPluginInterface _plugininterface;

        public Actor GetPlayerPet(PlayerCharacter character, DalamudPluginInterface pi)
        {
            _plugininterface = pi;
            //if player job isn't one of these three, return a null actor.
            if ((int)character.ClassJob.Id < 26 || (int)character.ClassJob.Id > 28)
            {
                return null;
            }

            //Time to search the actor table >:(
            // return null;
            //Iterate through this table until we either reach the end or find a pet.
            for (int i = 0; i < _plugininterface.ClientState.Actors.Length; i++)
            {
                //make sure the entry isn't null before we seg fault
                if(_plugininterface.ClientState.Actors[i] != null)
                {
                    //next, check that the actor is a battlenpc (pet)
                    if (_plugininterface.ClientState.Actors[i].ObjectKind == ObjectKind.BattleNpc)
                    {
                        //Lets check that the owner is right.
                        Dalamud.Game.ClientState.Actors.Types.NonPlayer.BattleNpc tempact = (Dalamud.Game.ClientState.Actors.Types.NonPlayer.BattleNpc)_plugininterface.ClientState.Actors[i];
                        if (tempact.OwnerId == character.ActorId)
                        {
                            return tempact;
                        }
                    }
                }    
            }
            //if we search the entire list, and it's not there, return null. 
           return null;

        }
    }
}
