using boilersGraphics.Properties;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Threading;

namespace boilersGraphics.ViewModels
{
    internal class StatisticsDialogViewModel : BindableBase, IDialogAware, IDisposable
    {
        private bool disposedValue;
        private CompositeDisposable disposables = new CompositeDisposable();
        public MainWindowViewModel MainWindowViewModel { get; set; }
        public DelegateCommand LoadedCommand { get; }

        public ReactivePropertySlim<int> NumberOfBoots { get; } = new ReactivePropertySlim<int>();

        public ReactivePropertySlim<int> NumberOfTimesTheFileWasOpenedBySpecifyingIt { get; } = new ReactivePropertySlim<int>();

        public ReactivePropertySlim<int> NumberOfTimesTheAutoSaveFileIsSpecifiedAndOpened { get; } = new ReactivePropertySlim<int>();

        public ReactivePropertySlim<int> NumberOfClicksWithThePointerTool { get; } = new ReactivePropertySlim<int>();

        public ReactivePropertySlim<int> CumulativeTotalOfItemsSelectedWithTheLassoTool { get; } = new ReactivePropertySlim<int>();

        public ReactivePropertySlim<int> NumberOfDrawsOfTheStraightLineTool { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfDrawsOfTheRectangleTool { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfDrawsOfTheEllipseTool { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfDrawsOfTheImageFileTool { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfDrawsOfTheLetterTool { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfDrawsOfTheVerticalLetterTool { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfDrawsOfPolygonTool { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfDrawsOfBezierCurveTool { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfSnapPointToolInstallations { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> BrushToolDrawCount { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfTimesTheEraserToolHasBeenUsed { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfTimesSaved { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfTimesYouHaveNamedAndSaved { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfExports { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfJpegExports { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfPngExports { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfGifExports { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfBmpExports { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfTiffExports { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfWmpExports { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfTimesGrouped { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfUngrouped { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfMovesToTheFrontend { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfMovesToTheFront { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfMovesToTheBack { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfMovesToTheBackend { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfTopAlignment { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfTimesTheTopAndBottomAreCentered { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfBottomAlignment { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfLeftAlignment { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfTimesLeftAndRightCentered { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfRightAlignment { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfTimesAlignedLeftAndRight { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfTimesAlignedUpAndDown { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfTimesToMatchTheWidth { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfTimesToMatchTheHeight { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfUnions { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfIntersects { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfXors { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfExcludes { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfCuts { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfCopies { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfPasted { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfUndos { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfRedoes { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfTimesAutomaticallySaved { get; } = new ReactivePropertySlim<int>();

        public ReactivePropertySlim<int> NumberOfNewlyCreatedLayers { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfDeletedLayers { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfTimesTheItemWasDrawn { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfTimesTheItemWasDeleted { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfLogLevelChanges { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfTimesTheVersionInformationDialogWasDisplayed { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfTimesTheApplicationLogWasDisplayed { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfTimesSliceToolHasBeenUsed { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfDrawsOfFreeHandTool { get; } = new ReactivePropertySlim<int>();
        public ReactivePropertySlim<int> NumberOfDrawsOfThePieTool { get; } = new ReactivePropertySlim<int>();

        public ReactivePropertySlim<TimeSpan> Uptime { get; } = new ReactivePropertySlim<TimeSpan>();

        public string Title => Resources.Title_Statistics;

        public StatisticsDialogViewModel()
        {
            LoadedCommand = new DelegateCommand(() => Load());
        }

#pragma warning disable CS0067

        public event Action<IDialogResult> RequestClose;

#pragma warning restore CS0067

        public void Load()
        {
            var statistics = (App.Current.MainWindow.DataContext as MainWindowViewModel).Statistics.Value;
            NumberOfBoots.Value = statistics.NumberOfBoots;
            NumberOfTimesTheFileWasOpenedBySpecifyingIt.Value = statistics.NumberOfTimesTheFileWasOpenedBySpecifyingIt;
            NumberOfTimesTheAutoSaveFileIsSpecifiedAndOpened.Value = statistics.NumberOfTimesTheAutoSaveFileIsSpecifiedAndOpened;
            NumberOfClicksWithThePointerTool.Value = statistics.NumberOfClicksWithThePointerTool;
            CumulativeTotalOfItemsSelectedWithTheLassoTool.Value = statistics.CumulativeTotalOfItemsSelectedWithTheLassoTool;
            NumberOfDrawsOfTheStraightLineTool.Value = statistics.NumberOfDrawsOfTheStraightLineTool;
            NumberOfDrawsOfTheRectangleTool.Value = statistics.NumberOfDrawsOfTheRectangleTool;
            NumberOfDrawsOfTheEllipseTool.Value = statistics.NumberOfDrawsOfTheEllipseTool;
            NumberOfDrawsOfTheImageFileTool.Value = statistics.NumberOfDrawsOfTheImageFileTool;
            NumberOfDrawsOfTheLetterTool.Value = statistics.NumberOfDrawsOfTheLetterTool;
            NumberOfDrawsOfTheVerticalLetterTool.Value = statistics.NumberOfDrawsOfTheVerticalLetterTool;
            NumberOfDrawsOfPolygonTool.Value = statistics.NumberOfDrawsOfPolygonTool;
            NumberOfDrawsOfBezierCurveTool.Value = statistics.NumberOfDrawsOfBezierCurveTool;
            NumberOfSnapPointToolInstallations.Value = statistics.NumberOfSnapPointToolInstallations;
            BrushToolDrawCount.Value = statistics.BrushToolDrawCount;
            NumberOfTimesTheEraserToolHasBeenUsed.Value = statistics.NumberOfTimesTheEraserToolHasBeenUsed;
            NumberOfTimesSaved.Value = statistics.NumberOfTimesSaved;
            NumberOfTimesYouHaveNamedAndSaved.Value = statistics.NumberOfTimesYouHaveNamedAndSaved;
            NumberOfExports.Value = statistics.NumberOfExports;
            NumberOfJpegExports.Value = statistics.NumberOfJpegExports;
            NumberOfPngExports.Value = statistics.NumberOfPngExports;
            NumberOfGifExports.Value = statistics.NumberOfGifExports;
            NumberOfBmpExports.Value = statistics.NumberOfBmpExports;
            NumberOfTiffExports.Value = statistics.NumberOfTiffExports;
            NumberOfWmpExports.Value = statistics.NumberOfWmpExports;
            NumberOfTimesGrouped.Value = statistics.NumberOfTimesGrouped;
            NumberOfUngrouped.Value = statistics.NumberOfUngrouped;
            NumberOfMovesToTheFrontend.Value = statistics.NumberOfMovesToTheFrontend;
            NumberOfMovesToTheFront.Value = statistics.NumberOfMovesToTheFront;
            NumberOfMovesToTheBack.Value = statistics.NumberOfMovesToTheBack;
            NumberOfMovesToTheBackend.Value = statistics.NumberOfMovesToTheBackend;
            NumberOfTopAlignment.Value = statistics.NumberOfTopAlignment;
            NumberOfTimesTheTopAndBottomAreCentered.Value = statistics.NumberOfTimesTheTopAndBottomAreCentered;
            NumberOfBottomAlignment.Value = statistics.NumberOfBottomAlignment;
            NumberOfLeftAlignment.Value = statistics.NumberOfLeftAlignment;
            NumberOfTimesLeftAndRightCentered.Value = statistics.NumberOfTimesLeftAndRightCentered;
            NumberOfRightAlignment.Value = statistics.NumberOfRightAlignment;
            NumberOfTimesAlignedLeftAndRight.Value = statistics.NumberOfTimesAlignedLeftAndRight;
            NumberOfTimesAlignedUpAndDown.Value = statistics.NumberOfTimesAlignedUpAndDown;
            NumberOfTimesToMatchTheWidth.Value = statistics.NumberOfTimesToMatchTheWidth;
            NumberOfTimesToMatchTheHeight.Value = statistics.NumberOfTimesToMatchTheHeight;
            NumberOfUnions.Value = statistics.NumberOfUnions;
            NumberOfIntersects.Value = statistics.NumberOfIntersects;
            NumberOfXors.Value = statistics.NumberOfXors;
            NumberOfExcludes.Value = statistics.NumberOfExcludes;
            NumberOfCuts.Value = statistics.NumberOfCuts;
            NumberOfCopies.Value = statistics.NumberOfCopies;
            NumberOfPasted.Value = statistics.NumberOfPasted;
            NumberOfUndos.Value = statistics.NumberOfUndos;
            NumberOfRedoes.Value = statistics.NumberOfRedoes;
            NumberOfTimesAutomaticallySaved.Value = statistics.NumberOfTimesAutomaticallySaved;
            NumberOfNewlyCreatedLayers.Value = statistics.NumberOfNewlyCreatedLayers;
            NumberOfDeletedLayers.Value = statistics.NumberOfDeletedLayers;
            NumberOfTimesTheItemWasDrawn.Value = statistics.NumberOfTimesTheItemWasDrawn;
            NumberOfTimesTheItemWasDeleted.Value = statistics.NumberOfTimesTheItemWasDeleted;
            NumberOfLogLevelChanges.Value = statistics.NumberOfLogLevelChanges;
            NumberOfTimesTheVersionInformationDialogWasDisplayed.Value = statistics.NumberOfTimesTheVersionInformationDialogWasDisplayed;
            NumberOfTimesTheApplicationLogWasDisplayed.Value = statistics.NumberOfTimesTheApplicationLogWasDisplayed;
            NumberOfTimesSliceToolHasBeenUsed.Value = statistics.NumberOfTimesSliceToolHasBeenUsed;
            NumberOfDrawsOfFreeHandTool.Value = statistics.NumberOfDrawsOfFreeHandTool;
            NumberOfDrawsOfThePieTool.Value = statistics.NumberOfDrawsOfThePieTool;

            Observable.Timer(DateTime.Now, TimeSpan.FromSeconds(1))
                      .Subscribe(_ =>
                      {
                          Dispatcher.CurrentDispatcher.Invoke(() =>
                          {
                              var statistics = MainWindowViewModel.Statistics.Value;
                              Uptime.Value = TimeSpan.FromTicks(statistics.UptimeTicks);
                          });
                      })
                      .AddTo(disposables);
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            MainWindowViewModel = parameters.GetValue<MainWindowViewModel>("MainWindowViewModel");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    disposables.Dispose();
                }

                disposables = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
