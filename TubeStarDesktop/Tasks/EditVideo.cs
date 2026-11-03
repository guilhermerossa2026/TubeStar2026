using System;
using System.Windows.Media;

namespace TubeStar
{
    public class EditVideo : Task
    {
        public const int MinimumEditTime = 4;

        public Video Video { get; set; }

        //Horas definidas pela habilidade na criação (null = tarefa de save antigo, usa o mínimo + ExtraHours)
        public int? FixedHours { get; set; }

        public EditVideo()
        { }

        public EditVideo(Video video)
        {
            Video = video;
            FixedHours = HoursForSkill(Player.Current.PostProductionSkill);
        }

        //Editar fica mais rápido com a habilidade: <40 = 6h, 40-59 = 4h, 60-79 = 3h, 80+ = 2h
        public static int HoursForSkill(int skill)
        {
            if (skill >= 80) return 2;
            if (skill >= 60) return 3;
            if (skill >= 40) return 4;
            return 6;
        }

        public override TaskType TaskType
        {
            get { return TaskType.EditVideo; }
        }

        public override string Name
        {
            get { return String.Format(EnglishStrings.EditVideoTask.Translate(), Video.Name); }
        }

        public override Color Color
        {
            get { return Colors.BlueViolet; }
        }

        public override int HoursToComplete
        {
            get { return FixedHours.HasValue ? FixedHours.Value : MinimumEditTime; }
        }
    }
}