using System;
using System.Windows.Media;

namespace TubeStar
{
    public class EditingSoftwareIII : StoreItem
    {
        public override string Name
        {
            get { return EnglishStrings.EditingSoftwareIIIStoreItem.Translate(); }
        }

        public override string Description
        {
            get { return EnglishStrings.EditingSoftwareIIIStoreItemDescription.Translate(); }
        }

        public override string ImageName
        {
            get { return "Software2"; }
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
            get { return TubeStar.SkillModifierType.PostProduction; }
        }
    }
}