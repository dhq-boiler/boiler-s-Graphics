using Homura.ORM.Mapping;

namespace boilersGraphics.Models
{
    public class Statistics : PkIdEntity
    {
        private int _number_of_boots;
        private long _uptime;
        private int _numberOfTimesTheFileWasOpenedBySpecifyingIt;
        private int _numberOfTimesTheAutoSaveFileIsSpecifiedAndOpened;
        private int _numberOfClicksWithThePointerTool;
        private int _cumulativeTotalOfItemsSelectedWithTheLassoTool;
        private int _numberOfDrawsOfTheStraightLineTool;
        private int _numberOfDrawsOfTheRectangleTool;
        private int _numberOfDrawsOfTheEllipseTool;
        private int _numberOfDrawsOfTheImageFileTool;
        private int _numberOfDrawsOfTheLetterTool;
        private int _numberOfDrawsOfTheVerticalLetterTool;

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
            get { return _numberOfTimesTheFileWasOpenedBySpecifyingIt; }
            set { SetProperty(ref _numberOfTimesTheFileWasOpenedBySpecifyingIt, value); }
        }

        /// <summary>
        /// 自動保存ファイルを指定して開いた回数
        /// </summary>
        [Column("NumberOfTimesTheAutoSaveFileIsSpecifiedAndOpened", "INTEGER", 4)]
        public int NumberOfTimesTheAutoSaveFileIsSpecifiedAndOpened
        {
            get { return _numberOfTimesTheAutoSaveFileIsSpecifiedAndOpened; }
            set { SetProperty(ref _numberOfTimesTheAutoSaveFileIsSpecifiedAndOpened, value); }
        }

        [Column("NumberOfClicksWithThePointerTool", "INTEGER", 5)]
        public int NumberOfClicksWithThePointerTool
        {
            get { return _numberOfClicksWithThePointerTool; }
            set { SetProperty(ref _numberOfClicksWithThePointerTool, value); }
        }

        [Column("CumulativeTotalOfItemsSelectedWithTheLassoTool", "INTEGER", 6)]
        public int CumulativeTotalOfItemsSelectedWithTheLassoTool 
        {
            get => _cumulativeTotalOfItemsSelectedWithTheLassoTool;
            set => SetProperty(ref _cumulativeTotalOfItemsSelectedWithTheLassoTool, value);
        }

        [Column("NumberOfDrawsOfTheStraightLineTool", "INTEGER", 7)]
        public int NumberOfDrawsOfTheStraightLineTool
        {
            get => _numberOfDrawsOfTheStraightLineTool;
            set => SetProperty(ref _numberOfDrawsOfTheStraightLineTool, value);
        }

        [Column("NumberOfDrawsOfTheRectangleTool", "INTEGER", 7)]
        public int NumberOfDrawsOfTheRectangleTool
        {
            get => _numberOfDrawsOfTheRectangleTool;
            set => SetProperty(ref _numberOfDrawsOfTheRectangleTool, value);
        }

        [Column("NumberOfDrawsOfTheEllipseTool", "INTEGER", 7)]
        public int NumberOfDrawsOfTheEllipseTool
        {
            get => _numberOfDrawsOfTheEllipseTool;
            set => SetProperty(ref _numberOfDrawsOfTheEllipseTool, value);
        }

        [Column("NumberOfDrawsOfTheImageFileTool", "INTEGER", 7)]
        public int NumberOfDrawsOfTheImageFileTool
        {
            get => _numberOfDrawsOfTheImageFileTool;
            set => SetProperty(ref _numberOfDrawsOfTheImageFileTool, value);
        }

        [Column("NumberOfDrawsOfTheLetterTool", "INTEGER", 7)]
        public int NumberOfDrawsOfTheLetterTool
        {
            get => _numberOfDrawsOfTheLetterTool;
            set => SetProperty(ref _numberOfDrawsOfTheLetterTool, value);
        }

        [Column("NumberOfDrawsOfTheVerticalLetterTool", "INTEGER", 7)]
        public int NumberOfDrawsOfTheVerticalLetterTool
        {
            get => _numberOfDrawsOfTheVerticalLetterTool;
            set => SetProperty(ref _numberOfDrawsOfTheVerticalLetterTool, value);
        }
    }
}
