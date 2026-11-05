using System;
using System.Windows.Media;

namespace TubeStar
{
    public class VideoCameraIV : StoreItem
    {
        public override string Name
        {
            get { return EnglishStrings.VideoCameraIVStoreItem.Translate(); }
        }

        public override string Description
        {
            get { return EnglishStrings.VideoCameraIVStoreItemDescription.Translate(); }
        }

        public override string ImageName
        {
            get { return "Camera2"; }
        }

        public override int Cost
        {
            get { return 20000; }
        }

        public override int SkillModifier
        {
            get { return 22; }
        }

        public override SkillModifierType SkillModifierType
        {
            get { return TubeStar.SkillModifierType.Shooting; }
        }
    }
}