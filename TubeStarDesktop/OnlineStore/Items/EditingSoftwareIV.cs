using System;
using System.Windows.Media;

namespace TubeStar
{
    public class EditingSoftwareIV : StoreItem
    {
        public override string Name
        {
            get { return EnglishStrings.EditingSoftwareIVStoreItem.Translate(); }
        }

        public override string Description
        {
            get { return EnglishStrings.EditingSoftwareIVStoreItemDescription.Translate(); }
        }

        public override string ImageName
        {
            get { return "Software2"; }
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
            get { return TubeStar.SkillModifierType.PostProduction; }
        }
    }
}