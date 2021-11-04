using Homura.ORM.Mapping;

namespace boilersGraphics.Models
{
    class Statistics : PkIdEntity
    {
        private int _number_of_boots;

        /// <summary>
        /// boiler's Graphics 起動回数
        /// </summary>
        [Column("NumberOfBoots", "INTEGER", 1)]
        public int NumberOfBoots
        {
            get { return _number_of_boots; }
            set { SetProperty(ref _number_of_boots, value); }
        }
    }
}
