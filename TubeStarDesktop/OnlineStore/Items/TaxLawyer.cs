using System;
using System.Windows.Media;

namespace TubeStar
{
    public class TaxLawyer : StoreItem
    {
        public override string Name
        {
            get { return EnglishStrings.TaxLawyerStoreItem.Translate(); }
        }

        public override string Description
        {
            get { return String.Format(EnglishStrings.TaxLawyerStoreItemDescription.Translate(), (RateReduction * 100).ToString("0")); }
        }

        public override string ImageName
        {
            get { return "Lawyer"; }
        }

        public override int Cost
        {
            get { return 50000; }
        }

        //Redução da alíquota em cada faixa do imposto semanal (5 pontos percentuais)
        public double RateReduction
        {
            get { return 0.05; }
        }

        public override int SkillModifier
        {
            get { return 0; }
        }

        public override SkillModifierType SkillModifierType
        {
            get { return TubeStar.SkillModifierType.None; }
        }
    }
}
