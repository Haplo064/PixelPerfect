using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Num = System.Numerics;
using Dalamud.Plugin;

namespace PixelPerfect
{
    class JobRingWrapper
    {
        private int[,] _radii;
        private Num.Vector4[,] _colors;
        private int[] meleerange = { }; //will use -1 default instead.
        private int indexoffset = 19;
        private static float _modulo = 255;

        //General Colors
        private Num.Vector4 aoeDPS = new Num.Vector4(161 / _modulo, 34 / _modulo, 55 / _modulo, 255 / _modulo);
        private Num.Vector4 aoeHeal = new Num.Vector4(19 / _modulo, 117/_modulo, 35 / _modulo, 255 / _modulo);
        private Num.Vector4 aoeMit = new Num.Vector4(66 / _modulo, 207 / _modulo, 200 / _modulo, 255 / _modulo);
        private Num.Vector4 meleeRange = new Num.Vector4(255f / _modulo, 255f / _modulo, 255f / _modulo, 255f / _modulo);
        private Num.Vector4 nullVec = new Num.Vector4(-1, -1, -1, -1);

        //Special Colors
        private Num.Vector4 exHeal = new Num.Vector4(195 / _modulo, 54 / _modulo, 164 / _modulo, 255 / _modulo);
        public JobRingWrapper()
        {
            _colors = new Num.Vector4[,]
            {
                //Starts at pld, so index-19. 3 fields for three colors.
                {aoeMit, meleeRange, nullVec }, //pld
                {aoeDPS, meleeRange, nullVec}, //mnk
                {aoeMit, meleeRange, nullVec}, //war
                {aoeDPS, aoeDPS, nullVec }, //drg
                {aoeDPS, aoeMit, nullVec }, //brd
                {exHeal, aoeDPS, meleeRange }, // whm
                {aoeMit, nullVec, nullVec}, //blm
                {nullVec, nullVec, nullVec },  //acn
                {nullVec, nullVec, nullVec },  //smn
                {aoeHeal, nullVec, nullVec }, //sch
                {nullVec, nullVec, nullVec }, //rog
                {meleeRange, nullVec, nullVec }, //nin
                {aoeMit, nullVec, nullVec }, //mch
                {aoeMit, meleeRange, nullVec }, //drk
                {aoeHeal, nullVec, nullVec }, //ast
                {meleeRange, nullVec, nullVec }, //sam
                {aoeDPS, nullVec, nullVec }, //rdm
                {nullVec, nullVec, nullVec }, //blu
                {aoeMit, meleeRange, nullVec }, //gnb
                {exHeal, aoeDPS, meleeRange }, //dnc

            };

            _radii = new int[,]
            {
                {-1,0,0}, //classes and gatherers etc
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                {-1,0,0},
                { 15, 5 , 0 },   //pld
                { 15, 5, 0 },   //mnk
                { 15, 5, 0 },   //war
                { 15, 10, 0 },  //drg
                { 25, 20, 0 },  //brd
                { 20, 15, 6 },   //whm
                { 25, 0, 0},    //blm
                { -1, 0, 0 },   //Arcanist    
                { -1, 0, 0},    //Summoner, not relavent?
                { 15, 5, 0 },   //Scholar, can we get Faerie location?
                {-1, 0, 0 },    //Rogue
                { 5, 0, 0 },    //Ninja
                { 20, 0, 0 },   //mch
                { 15, 5 , 0 },  //DRK
                { 15, 0, 0 },   //ast
                { 5, 0, 0 },    //sam
                { 15, 0, 0 }, //rdm
                { -1, 0, 0 }, //blu
                { 15, 5 , 0 }, //gnb
                { 20, 15, 5} //dnc
            };


        }

        public Num.Vector4[]  GetColors(int index)
        {
            index -= indexoffset;
            if (index  < 0)
                return null;

            Num.Vector4[] colorlist = new Num.Vector4[3];
            for (int i = 0; i < 3; i++)
            {
                colorlist[i] = _colors[index, i];
            }
            return colorlist;
        }

        public int[] GetRadii(int index)
        {
            if (index < 19) return null;

            int[] templist = new int[3];

            for(int i = 0; i < templist.Length; i++)
            {
                templist[i] = _radii[index, i];
            }
            return templist;
        }
    }
}




/* issues: Maintainability.
 */