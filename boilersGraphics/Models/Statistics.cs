using Homura.ORM.Mapping;

namespace boilersGraphics.Models
{
    public class Statistics : PkIdEntity
    {
        private int _number_of_boots;
        private long _uptime;

        /// <summary>
        /// boiler's Graphics 起動回数
        /// </summary>
        [Column("NumberOfBoots", "INTEGER", 1)]
        public int NumberOfBoots
        {
            get { return _number_of_boots; }
            set { SetProperty(ref _number_of_boots, value); }
        }

        /// <summary>
        /// boiler's Graphics 稼働時間
        /// </summary>
        [Column("UptimeTicks", "BIGINT", 2)]
        public long UptimeTicks
        {
            get { return _uptime; }
            set { SetProperty(ref _uptime, value); }
        }
    }
}
