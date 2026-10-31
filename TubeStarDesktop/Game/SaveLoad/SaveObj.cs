using System;
using System.Collections.Generic;
using System.Linq;

namespace TubeStar
{
    public class SaveObj
    {
        public Player Player { get; set; }
        public List<SerializableShareData> ShareViews { get; set; }
        public List<SerializableShareData> BoughtViews { get; set; }
        public Studies Studies { get; set; }
        public StoreItems StoreItems { get; set; }
        public List<Rival> Rivals { get; set; }

        //Agenda do dia atual: "Job" ou o índice da tarefa em Player.TasksInProgress.
        //Nulo em saves antigos (aí o Continuar começa um dia novo, como antes).
        public List<string> DailyAgenda { get; set; }
        public double? MoneyAtStartOfDay { get; set; }
    }
}