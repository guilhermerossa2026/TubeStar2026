using System;
using System.Windows.Media;

namespace TubeStar
{
    public class EditingSoftwareV : StoreItem
    {
        public override string Name
        {
            get { return EnglishStrings.EditingSoftwareVStoreItem.Translate(); }
        }

        public override string Description
        {
            get { return EnglishStrings.EditingSoftwareVStoreItemDescription.Translate(); }
        }

        public override string ImageName
        {
            get { return "Software2"; }
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
            get { return TubeStar.SkillModifierType.PostProduction; }
        }
    }
}