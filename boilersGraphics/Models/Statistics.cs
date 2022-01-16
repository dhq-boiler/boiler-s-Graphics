using boilersGraphics.Dao.Migration.Version;
using Homura.ORM.Mapping;

namespace boilersGraphics.Models
{
	[DefaultVersion(typeof(Version3))]
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
        private int _numberOfDrawsOfPolygonTool;
        private int _numberOfDrawsOfBezierCurveTool;
        private int _numberOfSnapPointToolInstallations;
        private int _brushToolDrawCount;
        private int _numberOfTimesTheEraserToolHasBeenUsed;
        private int _numberOfTimesSaved;
        private int _numberOfTimesYouHaveNamedAndSaved;
        private int _numberOfExports;
        private int _numberOfJpegExports;
        private int _numberOfPngExports;
        private int _numberOfGifExports;
        private int _numberOfBmpExports;
        private int _numberOfTiffExports;
        private int _numberOfWmpExports;
        private int _numberOfTimesGrouped;
        private int _numberOfUngrouped;
        private int _numberOfMovesToTheFrontend;
        private int _numberOfMovesToTheFront;
        private int _numberOfMovesToTheBack;
        private int _numberOfMovesToTheBackend;
        private int _numberOfTopAlignment;
        private int _numberOfTimesTheTopAndBottomAreCentered;
        private int _numberOfBottomAlignment;
        private int _numberOfLeftAlignment;
        private int _numberOfTimesLeftAndRightCentered;
        private int _numberOfRightAlignment;
        private int _numberOfTimesAlignedLeftAndRight;
        private int _numberOfTimesAlignedUpAndDown;
        private int _numberOfTimesToMatchTheWidth;
        private int _numberOfTimesToMatchTheHeight;
        private int _numberOfUnions;
        private int _numberOfIntersects;
        private int _numberOfXors;
        private int _numberOfExcludes;
        private int _numberOfCuts;
        private int _numberOfCopies;
        private int _numberOfPasted;
        private int _numberOfUndos;
        private int _numberOfRedoes;
        private int _numberOfTimesAutomaticallySaved;
        private int _numberOfNewlyCreatedLayers;
        private int _numberOfDeletedLayers;
        private int _numberOfTimesTheItemWasDrawn;
        private int _numberOfTimesTheItemWasDeleted;
        private int _numberOfLogLevelChanges;
        private int _numberOfTimesTheVersionInformationDialogWasDisplayed;
        private int _numberOfTimesTheApplicationLogWasDisplayed;
        private int _numberOfTimesSliceToolHasBeenUsed;
        private int _numberOfDrawsOfFreeHandTool;
		private int _numberOfDrawsOfThePieTool;


		/// <summary>
		/// boiler's Graphics 起動回数
		/// </summary>
		[Column("NumberOfBoots", "INTEGER", 1), NotNull]
        public int NumberOfBoots
        {
            get { return _number_of_boots; }
            set { SetProperty(ref _number_of_boots, value); }
        }

        /// <summary>
        /// boiler's Graphics 稼働時間
        /// </summary>
        [Column("UptimeTicks", "BIGINT", 2), NotNull]
        public long UptimeTicks
        {
            get { return _uptime; }
            set { SetProperty(ref _uptime, value); }
        }

        /// <summary>
        /// ファイルを指定して開いた回数
        /// </summary>
        [Column("NumberOfTimesTheFileWasOpenedBySpecifyingIt", "INTEGER", 3), NotNull]
        public int NumberOfTimesTheFileWasOpenedBySpecifyingIt
        {
            get { return _numberOfTimesTheFileWasOpenedBySpecifyingIt; }
            set { SetProperty(ref _numberOfTimesTheFileWasOpenedBySpecifyingIt, value); }
        }

        /// <summary>
        /// 自動保存ファイルを指定して開いた回数
        /// </summary>
        [Column("NumberOfTimesTheAutoSaveFileIsSpecifiedAndOpened", "INTEGER", 4), NotNull]
        public int NumberOfTimesTheAutoSaveFileIsSpecifiedAndOpened
        {
            get { return _numberOfTimesTheAutoSaveFileIsSpecifiedAndOpened; }
            set { SetProperty(ref _numberOfTimesTheAutoSaveFileIsSpecifiedAndOpened, value); }
        }

		/// <summary>
		/// ポインターツールでクリックした回数
		/// </summary>
		[Column("NumberOfClicksWithThePointerTool", "INTEGER", 5), NotNull]
        public int NumberOfClicksWithThePointerTool
        {
            get { return _numberOfClicksWithThePointerTool; }
            set { SetProperty(ref _numberOfClicksWithThePointerTool, value); }
        }

		/// <summary>
		/// なげなわツールで選択したアイテムの累計
		/// </summary>
		[Column("CumulativeTotalOfItemsSelectedWithTheLassoTool", "INTEGER", 6), NotNull]
        public int CumulativeTotalOfItemsSelectedWithTheLassoTool 
        {
            get => _cumulativeTotalOfItemsSelectedWithTheLassoTool;
            set => SetProperty(ref _cumulativeTotalOfItemsSelectedWithTheLassoTool, value);
        }

		/// <summary>
		/// 直線ツールの描画回数
		/// </summary>
		[Column("NumberOfDrawsOfTheStraightLineTool", "INTEGER", 7), NotNull]
        public int NumberOfDrawsOfTheStraightLineTool
        {
            get => _numberOfDrawsOfTheStraightLineTool;
            set => SetProperty(ref _numberOfDrawsOfTheStraightLineTool, value);
        }

		/// <summary>
		/// 四角形ツールの描画回数
		/// </summary>
		[Column("NumberOfDrawsOfTheRectangleTool", "INTEGER", 8), NotNull]
        public int NumberOfDrawsOfTheRectangleTool
        {
            get => _numberOfDrawsOfTheRectangleTool;
            set => SetProperty(ref _numberOfDrawsOfTheRectangleTool, value);
        }

		/// <summary>
		/// 円ツールの描画回数
		/// </summary>
		[Column("NumberOfDrawsOfTheEllipseTool", "INTEGER", 9), NotNull]
        public int NumberOfDrawsOfTheEllipseTool
        {
            get => _numberOfDrawsOfTheEllipseTool;
            set => SetProperty(ref _numberOfDrawsOfTheEllipseTool, value);
        }

		/// <summary>
		/// 画像ファイルツールの描画回数
		/// </summary>
		[Column("NumberOfDrawsOfTheImageFileTool", "INTEGER", 10), NotNull]
        public int NumberOfDrawsOfTheImageFileTool
        {
            get => _numberOfDrawsOfTheImageFileTool;
            set => SetProperty(ref _numberOfDrawsOfTheImageFileTool, value);
        }

		/// <summary>
		/// 文字ツールの描画回数
		/// </summary>
		[Column("NumberOfDrawsOfTheLetterTool", "INTEGER", 11), NotNull]
        public int NumberOfDrawsOfTheLetterTool
        {
            get => _numberOfDrawsOfTheLetterTool;
            set => SetProperty(ref _numberOfDrawsOfTheLetterTool, value);
        }

		/// <summary>
		/// 縦書きツールの描画回数
		/// </summary>
		[Column("NumberOfDrawsOfTheVerticalLetterTool", "INTEGER", 12), NotNull]
        public int NumberOfDrawsOfTheVerticalLetterTool
        {
            get => _numberOfDrawsOfTheVerticalLetterTool;
            set => SetProperty(ref _numberOfDrawsOfTheVerticalLetterTool, value);
		}

        /// <summary>
		/// 多角形ツールの描画回数
		/// </summary>
		[Column("NumberOfDrawsOfPolygonTool", "INTEGER", 13), NotNull]
		public int NumberOfDrawsOfPolygonTool
		{
			get => _numberOfDrawsOfPolygonTool;
			set => SetProperty(ref _numberOfDrawsOfPolygonTool, value);
		}

		///<summary>
		/// ベジエ曲線ツールの描画回数
		/// </summary>
		[Column("NumberOfDrawsOfBezierCurveTool", "INTEGER", 14), NotNull]
		public int NumberOfDrawsOfBezierCurveTool
		{
			get => _numberOfDrawsOfBezierCurveTool;
			set => SetProperty(ref _numberOfDrawsOfBezierCurveTool, value);
		}

		///<summary>
		/// スナップポイントツールの設置回数
		/// </summary>
		[Column("NumberOfSnapPointToolInstallations", "INTEGER", 15), NotNull]
		public int NumberOfSnapPointToolInstallations
		{
			get => _numberOfSnapPointToolInstallations;
			set => SetProperty(ref _numberOfSnapPointToolInstallations, value);
		}

		///<summary>
		/// ブラシツールの描画回数
		/// </summary>
		[Column("BrushToolDrawCount", "INTEGER", 16), NotNull]
		public int BrushToolDrawCount
		{
			get => _brushToolDrawCount;
			set => SetProperty(ref _brushToolDrawCount, value);
		}

		///<summary>
		/// 消しゴムツールの使用回数
		/// </summary>
		[Column("NumberOfTimesTheEraserToolHasBeenUsed", "INTEGER", 17), NotNull]
		public int NumberOfTimesTheEraserToolHasBeenUsed
		{
			get => _numberOfTimesTheEraserToolHasBeenUsed;
			set => SetProperty(ref _numberOfTimesTheEraserToolHasBeenUsed, value);
		}

		///<summary>
		/// 上書き保存した回数
		/// </summary>
		[Column("NumberOfTimesSaved", "INTEGER", 18), NotNull]
		public int NumberOfTimesSaved
		{
			get => _numberOfTimesSaved;
			set => SetProperty(ref _numberOfTimesSaved, value);
		}

		///<summary>
		/// 名前を付けて保存した回数
		/// </summary>
		[Column("NumberOfTimesYouHaveNamedAndSaved", "INTEGER", 19), NotNull]
		public int NumberOfTimesYouHaveNamedAndSaved
		{
			get => _numberOfTimesYouHaveNamedAndSaved;
			set => SetProperty(ref _numberOfTimesYouHaveNamedAndSaved, value);
		}

		///<summary>
		/// エクスポートした回数
		/// </summary>
		[Column("NumberOfExports", "INTEGER", 20), NotNull]
		public int NumberOfExports
		{
			get => _numberOfExports;
			set => SetProperty(ref _numberOfExports, value);
		}

		///<summary>
		/// Jpegエクスポートした回数
		/// </summary>
		[Column("NumberOfJpegExports", "INTEGER", 21), NotNull]
		public int NumberOfJpegExports
		{
			get => _numberOfJpegExports;
			set => SetProperty(ref _numberOfJpegExports, value);
		}

		///<summary>
		/// PNGエクスポートした回数
		/// </summary>
		[Column("NumberOfPngExports", "INTEGER", 22), NotNull]
		public int NumberOfPngExports
		{
			get => _numberOfPngExports;
			set => SetProperty(ref _numberOfPngExports, value);
		}

		///<summary>
		/// GIFエクスポートした回数
		/// </summary>
		[Column("NumberOfGifExports", "INTEGER", 23), NotNull]
		public int NumberOfGifExports
		{
			get => _numberOfGifExports;
			set => SetProperty(ref _numberOfGifExports, value);
		}

		///<summary>
		/// BMPエクスポートした回数
		/// </summary>
		[Column("NumberOfBmpExports", "INTEGER", 24), NotNull]
		public int NumberOfBmpExports
		{
			get => _numberOfBmpExports;
			set => SetProperty(ref _numberOfBmpExports, value);
		}

		///<summary>
		/// TIFFエクスポートした回数
		/// </summary>
		[Column("NumberOfTiffExports", "INTEGER", 25), NotNull]
		public int NumberOfTiffExports
		{
			get => _numberOfTiffExports;
			set => SetProperty(ref _numberOfTiffExports, value);
		}

		///<summary>
		/// WMPエクスポートした回数
		/// </summary>
		[Column("NumberOfWmpExports", "INTEGER", 26), NotNull]
		public int NumberOfWmpExports
		{
			get => _numberOfWmpExports;
			set => SetProperty(ref _numberOfWmpExports, value);
		}

		///<summary>
		/// グループ化した回数
		/// </summary>
		[Column("NumberOfTimesGrouped", "INTEGER", 27), NotNull]
		public int NumberOfTimesGrouped
		{
			get => _numberOfTimesGrouped;
			set => SetProperty(ref _numberOfTimesGrouped, value);
		}

		///<summary>
		/// グループ化解除した回数
		/// </summary>
		[Column("NumberOfUngrouped", "INTEGER", 28), NotNull]
		public int NumberOfUngrouped
		{
			get => _numberOfUngrouped;
			set => SetProperty(ref _numberOfUngrouped, value);
		}

		///<summary>
		/// 最前面へ移動した回数
		/// </summary>
		[Column("NumberOfMovesToTheFrontend", "INTEGER", 29), NotNull]
		public int NumberOfMovesToTheFrontend
		{
			get => _numberOfMovesToTheFrontend;
			set => SetProperty(ref _numberOfMovesToTheFrontend, value);
		}

		///<summary>
		/// 前面へ移動した回数
		/// </summary>
		[Column("NumberOfMovesToTheFront", "INTEGER", 30), NotNull]
		public int NumberOfMovesToTheFront
		{
			get => _numberOfMovesToTheFront;
			set => SetProperty(ref _numberOfMovesToTheFront, value);
		}

		///<summary>
		/// 背面へ移動した回数
		/// </summary>
		[Column("NumberOfMovesToTheBack", "INTEGER", 31), NotNull]
		public int NumberOfMovesToTheBack
		{
			get => _numberOfMovesToTheBack;
			set => SetProperty(ref _numberOfMovesToTheBack, value);
		}

		///<summary>
		/// 最背面へ移動した回数
		/// </summary>
		[Column("NumberOfMovesToTheBackend", "INTEGER", 32), NotNull]
		public int NumberOfMovesToTheBackend
		{
			get => _numberOfMovesToTheBackend;
			set => SetProperty(ref _numberOfMovesToTheBackend, value);
		}

		///<summary>
		/// 上揃えした回数
		/// </summary>
		[Column("NumberOfTopAlignment", "INTEGER", 33), NotNull]
		public int NumberOfTopAlignment
		{
			get => _numberOfTopAlignment;
			set => SetProperty(ref _numberOfTopAlignment, value);
		}

		///<summary>
		/// 上下中央揃えした回数
		/// </summary>
		[Column("NumberOfTimesTheTopAndBottomAreCentered", "INTEGER", 34), NotNull]
		public int NumberOfTimesTheTopAndBottomAreCentered
		{
			get => _numberOfTimesTheTopAndBottomAreCentered;
			set => SetProperty(ref _numberOfTimesTheTopAndBottomAreCentered, value);
		}

		///<summary>
		/// 下揃えした回数
		/// </summary>
		[Column("NumberOfBottomAlignment", "INTEGER", 35), NotNull]
		public int NumberOfBottomAlignment
		{
			get => _numberOfBottomAlignment;
			set => SetProperty(ref _numberOfBottomAlignment, value);
		}
		///<summary>
		/// 左揃えした回数
		/// </summary>
		[Column("NumberOfLeftAlignment", "INTEGER", 36), NotNull]
		public int NumberOfLeftAlignment
		{
			get => _numberOfLeftAlignment;
			set => SetProperty(ref _numberOfLeftAlignment, value);
		}

		///<summary>
		/// 左右中央揃えした回数
		/// </summary>
		[Column("NumberOfTimesLeftAndRightCentered", "INTEGER", 37), NotNull]
		public int NumberOfTimesLeftAndRightCentered
		{
			get => _numberOfTimesLeftAndRightCentered;
			set => SetProperty(ref _numberOfTimesLeftAndRightCentered, value);
		}

		///<summary>
		/// 右揃えした回数
		/// </summary>
		[Column("NumberOfRightAlignment", "INTEGER", 38), NotNull]
		public int NumberOfRightAlignment
		{
			get => _numberOfRightAlignment;
			set => SetProperty(ref _numberOfRightAlignment, value);
		}

		///<summary>
		/// 左右に整列した回数
		/// </summary>
		[Column("NumberOfTimesAlignedLeftAndRight", "INTEGER", 39), NotNull]
		public int NumberOfTimesAlignedLeftAndRight
		{
			get => _numberOfTimesAlignedLeftAndRight;
			set => SetProperty(ref _numberOfTimesAlignedLeftAndRight, value);
		}

		///<summary>
		/// 上下に整列した回数
		/// </summary>
		[Column("NumberOfTimesAlignedUpAndDown", "INTEGER", 40), NotNull]
		public int NumberOfTimesAlignedUpAndDown
		{
			get => _numberOfTimesAlignedUpAndDown;
			set => SetProperty(ref _numberOfTimesAlignedUpAndDown, value);
		}

		///<summary>
		/// 幅を合わせた回数
		/// </summary>
		[Column("NumberOfTimesToMatchTheWidth", "INTEGER", 41), NotNull]
		public int NumberOfTimesToMatchTheWidth
		{
			get => _numberOfTimesToMatchTheWidth;
			set => SetProperty(ref _numberOfTimesToMatchTheWidth, value);
		}

		///<summary>
		/// 高さを合わせた回数
		/// </summary>
		[Column("NumberOfTimesToMatchTheHeight", "INTEGER", 42), NotNull]
		public int NumberOfTimesToMatchTheHeight
		{
			get => _numberOfTimesToMatchTheHeight;
			set => SetProperty(ref _numberOfTimesToMatchTheHeight, value);
		}

		///<summary>
		/// Unionした回数
		/// </summary>
		[Column("NumberOfUnions", "INTEGER", 43), NotNull]
		public int NumberOfUnions
		{
			get => _numberOfUnions;
			set => SetProperty(ref _numberOfUnions, value);
		}

		///<summary>
		/// Intersectした回数
		/// </summary>
		[Column("NumberOfIntersects", "INTEGER", 44), NotNull]
		public int NumberOfIntersects
		{
			get => _numberOfIntersects;
			set => SetProperty(ref _numberOfIntersects, value);
		}

		///<summary>
		/// Xorした回数
		/// </summary>
		[Column("NumberOfXors", "INTEGER", 45), NotNull]
		public int NumberOfXors
		{
			get => _numberOfXors;
			set => SetProperty(ref _numberOfXors, value);
		}

		///<summary>
		/// Excludeした回数
		/// </summary>
		[Column("NumberOfExcludes", "INTEGER", 46), NotNull]
		public int NumberOfExcludes
		{
			get => _numberOfExcludes;
			set => SetProperty(ref _numberOfExcludes, value);
		}

		///<summary>
		/// 切り取りした回数
		/// </summary>
		[Column("NumberOfCuts", "INTEGER", 47), NotNull]
		public int NumberOfCuts
		{
			get => _numberOfCuts;
			set => SetProperty(ref _numberOfCuts, value);
		}

		///<summary>
		/// コピーした回数
		/// </summary>
		[Column("NumberOfCopies", "INTEGER", 48), NotNull]
		public int NumberOfCopies
		{
			get => _numberOfCopies;
			set => SetProperty(ref _numberOfCopies, value);
		}

		///<summary>
		/// 貼り付けした回数
		/// </summary>
		[Column("NumberOfPasted", "INTEGER", 49), NotNull]
		public int NumberOfPasted
		{
			get => _numberOfPasted;
			set => SetProperty(ref _numberOfPasted, value);
		}

		///<summary>
		/// 元に戻した回数
		/// </summary>
		[Column("NumberOfUndos", "INTEGER", 50), NotNull]
		public int NumberOfUndos
		{
			get => _numberOfUndos;
			set => SetProperty(ref _numberOfUndos, value);
		}

		///<summary>
		/// やり直しした回数
		/// </summary>
		[Column("NumberOfRedoes", "INTEGER", 51), NotNull]
		public int NumberOfRedoes
		{
			get => _numberOfRedoes;
			set => SetProperty(ref _numberOfRedoes, value);
		}

		///<summary>
		/// 自動保存した回数
		/// </summary>
		[Column("NumberOfTimesAutomaticallySaved", "INTEGER", 52), NotNull]
		public int NumberOfTimesAutomaticallySaved
		{
			get => _numberOfTimesAutomaticallySaved;
			set => SetProperty(ref _numberOfTimesAutomaticallySaved, value);
		}

		///<summary>
		/// 新規作成したレイヤー数
		/// </summary>
		[Column("NumberOfNewlyCreatedLayers", "INTEGER", 53), NotNull]
		public int NumberOfNewlyCreatedLayers
		{
			get => _numberOfNewlyCreatedLayers;
			set => SetProperty(ref _numberOfNewlyCreatedLayers, value);
		}

		///<summary>
		/// 削除したレイヤー数
		/// </summary>
		[Column("NumberOfDeletedLayers", "INTEGER", 54), NotNull]
		public int NumberOfDeletedLayers
		{
			get => _numberOfDeletedLayers;
			set => SetProperty(ref _numberOfDeletedLayers, value);
		}

		///<summary>
		/// アイテムを描画した回数
		/// </summary>
		[Column("NumberOfTimesTheItemWasDrawn", "INTEGER", 55), NotNull]
		public int NumberOfTimesTheItemWasDrawn
		{
			get => _numberOfTimesTheItemWasDrawn;
			set => SetProperty(ref _numberOfTimesTheItemWasDrawn, value);
		}

		///<summary>
		/// アイテムを削除した回数
		/// </summary>
		[Column("NumberOfTimesTheItemWasDeleted", "INTEGER", 56), NotNull]
		public int NumberOfTimesTheItemWasDeleted
		{
			get => _numberOfTimesTheItemWasDeleted;
			set => SetProperty(ref _numberOfTimesTheItemWasDeleted, value);
		}

		///<summary>
		/// ログレベルを変更した回数
		/// </summary>
		[Column("NumberOfLogLevelChanges", "INTEGER", 57), NotNull]
		public int NumberOfLogLevelChanges
		{
			get => _numberOfLogLevelChanges;
			set => SetProperty(ref _numberOfLogLevelChanges, value);
		}

		///<summary>
		/// バージョン情報ダイアログを表示した回数
		/// </summary>
		[Column("NumberOfTimesTheVersionInformationDialogWasDisplayed", "INTEGER", 58), NotNull]
		public int NumberOfTimesTheVersionInformationDialogWasDisplayed
		{
			get => _numberOfTimesTheVersionInformationDialogWasDisplayed;
			set => SetProperty(ref _numberOfTimesTheVersionInformationDialogWasDisplayed, value);
		}

		///<summary>
		/// アプリケーションログを表示した回数
		/// </summary>
		[Column("NumberOfTimesTheApplicationLogWasDisplayed", "INTEGER", 59), NotNull]
		public int NumberOfTimesTheApplicationLogWasDisplayed
		{
			get => _numberOfTimesTheApplicationLogWasDisplayed;
			set => SetProperty(ref _numberOfTimesTheApplicationLogWasDisplayed, value);
		}

		/// <summary>
		/// スライスツールの使用回数
		/// </summary>
		[Since(typeof(Version1))]
		[Column("NumberOfTimesSliceToolHasBeenUsed", "INTEGER", 60), NotNull]
		public int NumberOfTimesSliceToolHasBeenUsed
		{
			get => _numberOfTimesSliceToolHasBeenUsed;
			set => SetProperty(ref _numberOfTimesSliceToolHasBeenUsed, value);
		}

		///<summary>
		/// フリーハンドツールの描画回数
		/// </summary>
		[Since(typeof(Version2))]
		[Column("NumberOfDrawsOfFreeHandTool", "INTEGER", 61)]
		public int NumberOfDrawsOfFreeHandTool
		{
			get => _numberOfDrawsOfFreeHandTool;
			set => SetProperty(ref _numberOfDrawsOfFreeHandTool, value);
		}

		/// <summary>
		/// パイツールの描画回数
		/// </summary>
		[Since(typeof(Version3))]
		[Column("NumberOfDrawsOfThePieTool", "INTEGER", 61), NotNull]
		public int NumberOfDrawsOfThePieTool
        {
			get => _numberOfDrawsOfThePieTool;
			set => SetProperty(ref _numberOfDrawsOfThePieTool, value);
        }
    }
}
