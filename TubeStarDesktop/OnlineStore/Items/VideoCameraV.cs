using System;
using System.Windows.Media;

namespace TubeStar
{
    public class VideoCameraV : StoreItem
    {
        public override string Name
        {
            get { return EnglishStrings.VideoCameraVStoreItem.Translate(); }
        }

        public override string Description
        {
            get { return EnglishStrings.VideoCameraVStoreItemDescription.Translate(); }
        }

        public override string ImageName
        {
            get { return "Camera2"; }
        }

        public override int Cost
        {
            get { return 50000; }
        }

        public override int SkillModifier
        {
            get { return 30; }
        }

        public override SkillModifierType SkillModifierType
        {
            get { return TubeStar.SkillModifierType.Shooting; }
        }
    }
}