using System;
using System.Windows.Media;

namespace TubeStar
{
    public class VideoCameraIII : StoreItem
    {
        public override string Name
        {
            get { return EnglishStrings.VideoCameraIIIStoreItem.Translate(); }
        }

        public override string Description
        {
            get { return EnglishStrings.VideoCameraIIIStoreItemDescription.Translate(); }
        }

        public override string ImageName
        {
            get { return "Camera2"; }
        }

        public override int Cost
        {
            get { return 8000; }
        }

        public override int SkillModifier
        {
            get { return 16; }
        }

        public override SkillModifierType SkillModifierType
        {
            get { return TubeStar.SkillModifierType.Shooting; }
        }
    }
}