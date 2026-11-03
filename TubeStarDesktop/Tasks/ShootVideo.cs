using System;
using System.Windows.Media;

namespace TubeStar
{
    public class ShootVideo : Task
    {
        public const int MinimumShootTime = 2;

        public Video Video { get; set; }

        //Horas definidas pela habilidade na criação (null = tarefa de save antigo, usa o mínimo + ExtraHours)
        public int? FixedHours { get; set; }

        public ShootVideo()
        { }

        public ShootVideo(Video video)
        {
            Video = video;
            FixedHours = HoursForSkill(Player.Current.ShootingSkill);
        }

        //Gravar fica mais rápido com a habilidade: <40 = 4h, 40-59 = 3h, 60-79 = 2h, 80+ = 1h
        public static int HoursForSkill(int skill)
        {
            if (skill >= 80) return 1;
            if (skill >= 60) return 2;
            if (skill >= 40) return 3;
            return 4;
        }

        public override TaskType TaskType
        {
            get { return TaskType.ShootVideo; }
        }

        public override string Name
        {
            get { return String.Format(EnglishStrings.ShootVideoTask.Translate(), Video.Name); }
        }

        public override Color Color
        {
            get { return Colors.Green; }
        }

        public override int HoursToComplete
        {
            get { return FixedHours.HasValue ? FixedHours.Value : MinimumShootTime; }
        }
    }
}