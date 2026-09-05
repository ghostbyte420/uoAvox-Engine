using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps
{
    class CreditsGump : Gump
    {
        private const ushort BACKGROUND_IMG = 0x0500;
        private Point _offset;
        private uint _lastUpdate;

        #region uoAvocation Credits Screen [01-01]

        //TODO
        private const string CREDITS =
@"
    Copyright (C) 2022-2023 uoAvocation (https://www.uoavocation.net)


         We Would Like To Acknowledge The Following People
For Their Never-Ending Hard Work And Dedication To This Project
=====================================================
   
                --------------------------------------
                 UOAVOS CORE TEAM
                --------------------------------------

                [Project Manager(s)]
                aasr-sva - http://www.uoavocation.net

                [Lead Developer(s)]
                Vorspire - http://www.vita-nex.com

                [Team Developer(s)]
                aasr-sva - http://www.uoavocation.net
                Vorspire - http://www.vita-nex.com
                Zerodowned - https://github.com/zerodowned
    
                [Client Developer(s)]
                Karasho' - https://github.com/andreakarasho

                [UO Isometric Artist(s)]
                Otimpyre - https://github.com/otimpyre

     
         We Would Like To Say Thank You To Those Individuals
 Who Offered Contributions And Coding Assistance To This Project
=====================================================

                Alari, Alien, Deragon, Chocobutter,

                Daat99, Dknight, Felladrin, Freya, Fwiffo, 

                GD13, Hammerhand, HellRazor, Joeku,

                Karasho', Kaybel, koluch, Lord_GreyWolf,

                m309, Morexton, Mortis, PigPen-Divinity,

                Phantom, Praxiiz, Ryan, Soteric, Tab,

                Thagoras, Tindo, Tresdni, and Vorspire...
";

        #endregion

        public CreditsGump() : base(0, 0)
        {
            Client.Game.Audio.PlayMusic(8, false, true);

            LayerOrder = UILayer.Over;
            CanCloseWithRightClick = true;

            GumpPic background = new GumpPic(0, 0, BACKGROUND_IMG, 0);
            Width = background.Width;
            Height = background.Height;

            Add(new AlphaBlendControl(1f) { Width = background.Width, Height = background.Height});

            Add(background);

            Vector2 size = Fonts.Regular.MeasureString(CREDITS);
            _offset.X = (int) (Width / 2f - size.X / 2);
            _offset.Y = Height;
        }

        public override void Update()
        {
            base.Update();

            if (_lastUpdate < Time.Ticks)
            {
                _offset.Y -= 1;
                _lastUpdate = Time.Ticks + 25;
            }
        }

        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            base.Draw(batcher, x, y);

            Vector3 hueVector = ShaderHueTranslator.GetHueVector(0);

            batcher.DrawString(Fonts.Bold, CREDITS, x + _offset.X, y + _offset.Y, hueVector);

            return true;
        }
    }
}