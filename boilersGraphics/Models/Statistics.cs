using Homura.ORM.Mapping;

namespace boilersGraphics.Models
{
    public class Statistics : PkIdEntity
    {
        private int _number_of_boots;
        private long _uptime;
        private int _NumberOfTimesTheFileWasOpenedBySpecifyingIt;
        private int _NumberOfTimesTheAutoSaveFileIsSpecifiedAndOpened;
        private int _NumberOfClicksWithThePointerTool;
        private int _CumulativeTotalOfItemsSelectedWithTheLassoTool;

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

        /// <summary>
        /// ファイルを指定して開いた回数
        /// </summary>
        [Column("NumberOfTimesTheFileWasOpenedBySpecifyingIt", "INTEGER", 3)]
        public int NumberOfTimesTheFileWasOpenedBySpecifyingIt
        {
            get { return _NumberOfTimesTheFileWasOpenedBySpecifyingIt; }
            set { SetProperty(ref _NumberOfTimesTheFileWasOpenedBySpecifyingIt, value); }
        }

        /// <summary>
        /// 自動保存ファイルを指定して開いた回数
        /// </summary>
        [Column("NumberOfTimesTheAutoSaveFileIsSpecifiedAndOpened", "INTEGER", 4)]
        public int NumberOfTimesTheAutoSaveFileIsSpecifiedAndOpened
        {
            get { return _NumberOfTimesTheAutoSaveFileIsSpecifiedAndOpened; }
            set { SetProperty(ref _NumberOfTimesTheAutoSaveFileIsSpecifiedAndOpened, value); }
        }

        [Column("NumberOfClicksWithThePointerTool", "INTEGER", 5)]
        public int NumberOfClicksWithThePointerTool
        {
            get { return _NumberOfClicksWithThePointerTool; }
            set { SetProperty(ref _NumberOfClicksWithThePointerTool, value); }
        }

        [Column("CumulativeTotalOfItemsSelectedWithTheLassoTool", "INTEGER", 6)]
        public int CumulativeTotalOfItemsSelectedWithTheLassoTool 
        {
            get => _CumulativeTotalOfItemsSelectedWithTheLassoTool;
            set => SetProperty(ref _CumulativeTotalOfItemsSelectedWithTheLassoTool, value);
        }
    }
}
