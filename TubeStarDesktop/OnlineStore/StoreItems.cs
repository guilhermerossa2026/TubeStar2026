using System;
using System.Collections.Generic;
using System.Linq;

namespace TubeStar
{
    public class StoreItems
    {
        private static StoreItems _current;
        public static StoreItems Current
        {
            get
            {
                if (_current == null)
                    _current = new StoreItems();
                return _current;
            }
            set { _current = value; }
        }

        public VideoCameraI VideoCameraI { get; set; }
        public VideoCameraII VideoCameraII { get; set; }
        public VideoCameraIII VideoCameraIII { get; set; }
        public VideoCameraIV VideoCameraIV { get; set; }
        public VideoCameraV VideoCameraV { get; set; }

        public EditingSoftwareI EditingSoftwareI { get; set; }
        public EditingSoftwareII EditingSoftwareII { get; set; }
        public EditingSoftwareIII EditingSoftwareIII { get; set; }
        public EditingSoftwareIV EditingSoftwareIV { get; set; }
        public EditingSoftwareV EditingSoftwareV { get; set; }

        public Lawyer Lawyer { get; set; }
        public Consultant Consultant { get; set; }

        public Loan Loan { get; set; }

        public List<StoreItem> All
        {
            get
            {
                return new List<StoreItem>()
                {
                    Loan,

                    Consultant,
                    Lawyer,

                    EditingSoftwareV,
                    EditingSoftwareIV,
                    EditingSoftwareIII,
                    EditingSoftwareII,
                    EditingSoftwareI,

                    VideoCameraV,
                    VideoCameraIV,
                    VideoCameraIII,
                    VideoCameraII,
                    VideoCameraI,                    
                };
            }
        }

        /// <summary>
        /// Nível anterior da mesma linha (câmera ou edição) que precisa estar comprado. Null se não há requisito.
        /// </summary>
        public StoreItem GetRequiredItem(StoreItem item)
        {
            if (item.SkillModifierType != SkillModifierType.Shooting && item.SkillModifierType != SkillModifierType.PostProduction)
                return null;

            var line = All.Where(i => i.SkillModifierType == item.SkillModifierType).OrderBy(i => i.SkillModifier).ToList();
            int index = line.IndexOf(item);
            return index > 0 ? line[index - 1] : null;
        }

        public StoreItems()
        {
            VideoCameraI = new VideoCameraI();
            VideoCameraII = new VideoCameraII();
            VideoCameraIII = new VideoCameraIII();
            VideoCameraIV = new VideoCameraIV();
            VideoCameraV = new VideoCameraV();

            EditingSoftwareI = new EditingSoftwareI();
            EditingSoftwareII = new EditingSoftwareII();
            EditingSoftwareIII = new EditingSoftwareIII();
            EditingSoftwareIV = new EditingSoftwareIV();
            EditingSoftwareV = new EditingSoftwareV();

            Lawyer = new Lawyer();
            Consultant = new Consultant();

            Loan = new Loan();
        }
    }
}