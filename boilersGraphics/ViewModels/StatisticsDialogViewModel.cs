using boilersGraphics.Properties;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Threading;

namespace boilersGraphics.ViewModels;

internal class StatisticsDialogViewModel : BindableBase, IDialogAware, IDisposable
{
    private CompositeDisposable disposables = new();
    private bool disposedValue;

    public StatisticsDialogViewModel()
    {
        LoadedCommand = new DelegateCommand(() => Load());
    }

    public MainWindowViewModel MainWindowViewModel { get; set; }
    public DelegateCommand LoadedCommand { get; }

    public ReactivePropertySlim<int> NumberOfBoots { get; } = new();

    public ReactivePropertySlim<int> NumberOfTimesTheFileWasOpenedBySpecifyingIt { get; } = new();

    public ReactivePropertySlim<int> NumberOfTimesTheAutoSaveFileIsSpecifiedAndOpened { get; } = new();

    public ReactivePropertySlim<int> NumberOfClicksWithThePointerTool { get; } = new();

    public ReactivePropertySlim<int> CumulativeTotalOfItemsSelectedWithTheLassoTool { get; } = new();

    public ReactivePropertySlim<int> NumberOfDrawsOfTheStraightLineTool { get; } = new();
    public ReactivePropertySlim<int> NumberOfDrawsOfTheRectangleTool { get; } = new();
    public ReactivePropertySlim<int> NumberOfDrawsOfTheEllipseTool { get; } = new();
    public ReactivePropertySlim<int> NumberOfDrawsOfTheImageFileTool { get; } = new();
    public ReactivePropertySlim<int> NumberOfDrawsOfTheLetterTool { get; } = new();
    public ReactivePropertySlim<int> NumberOfDrawsOfTheVerticalLetterTool { get; } = new();
    public ReactivePropertySlim<int> NumberOfDrawsOfPolygonTool { get; } = new();
    public ReactivePropertySlim<int> NumberOfDrawsOfBezierCurveTool { get; } = new();
    public ReactivePropertySlim<int> NumberOfSnapPointToolInstallations { get; } = new();
    public ReactivePropertySlim<int> BrushToolDrawCount { get; } = new();
    public ReactivePropertySlim<int> NumberOfTimesTheEraserToolHasBeenUsed { get; } = new();
    public ReactivePropertySlim<int> NumberOfTimesSaved { get; } = new();
    public ReactivePropertySlim<int> NumberOfTimesYouHaveNamedAndSaved { get; } = new();
    public ReactivePropertySlim<int> NumberOfExports { get; } = new();
    public ReactivePropertySlim<int> NumberOfJpegExports { get; } = new();
    public ReactivePropertySlim<int> NumberOfPngExports { get; } = new();
    public ReactivePropertySlim<int> NumberOfGifExports { get; } = new();
    public ReactivePropertySlim<int> NumberOfBmpExports { get; } = new();
    public ReactivePropertySlim<int> NumberOfTiffExports { get; } = new();
    public ReactivePropertySlim<int> NumberOfWmpExports { get; } = new();
    public ReactivePropertySlim<int> NumberOfTimesGrouped { get; } = new();
    public ReactivePropertySlim<int> NumberOfUngrouped { get; } = new();
    public ReactivePropertySlim<int> NumberOfMovesToTheFrontend { get; } = new();
    public ReactivePropertySlim<int> NumberOfMovesToTheFront { get; } = new();
    public ReactivePropertySlim<int> NumberOfMovesToTheBack { get; } = new();
    public ReactivePropertySlim<int> NumberOfMovesToTheBackend { get; } = new();
    public ReactivePropertySlim<int> NumberOfTopAlignment { get; } = new();
    public ReactivePropertySlim<int> NumberOfTimesTheTopAndBottomAreCentered { get; } = new();
    public ReactivePropertySlim<int> NumberOfBottomAlignment { get; } = new();
    public ReactivePropertySlim<int> NumberOfLeftAlignment { get; } = new();
    public ReactivePropertySlim<int> NumberOfTimesLeftAndRightCentered { get; } = new();
    public ReactivePropertySlim<int> NumberOfRightAlignment { get; } = new();
    public ReactivePropertySlim<int> NumberOfTimesAlignedLeftAndRight { get; } = new();
    public ReactivePropertySlim<int> NumberOfTimesAlignedUpAndDown { get; } = new();
    public ReactivePropertySlim<int> NumberOfTimesToMatchTheWidth { get; } = new();
    public ReactivePropertySlim<int> NumberOfTimesToMatchTheHeight { get; } = new();
    public ReactivePropertySlim<int> NumberOfUnions { get; } = new();
    public ReactivePropertySlim<int> NumberOfIntersects { get; } = new();
    public ReactivePropertySlim<int> NumberOfXors { get; } = new();
    public ReactivePropertySlim<int> NumberOfExcludes { get; } = new();
    public ReactivePropertySlim<int> NumberOfCuts { get; } = new();
    public ReactivePropertySlim<int> NumberOfCopies { get; } = new();
    public ReactivePropertySlim<int> NumberOfPasted { get; } = new();
    public ReactivePropertySlim<int> NumberOfUndos { get; } = new();
    public ReactivePropertySlim<int> NumberOfRedoes { get; } = new();
    public ReactivePropertySlim<int> NumberOfTimesAutomaticallySaved { get; } = new();

    public ReactivePropertySlim<int> NumberOfNewlyCreatedLayers { get; } = new();
    public ReactivePropertySlim<int> NumberOfDeletedLayers { get; } = new();
    public ReactivePropertySlim<int> NumberOfTimesTheItemWasDrawn { get; } = new();
    public ReactivePropertySlim<int> NumberOfTimesTheItemWasDeleted { get; } = new();
    public ReactivePropertySlim<int> NumberOfLogLevelChanges { get; } = new();
    public ReactivePropertySlim<int> NumberOfTimesTheVersionInformationDialogWasDisplayed { get; } = new();
    public ReactivePropertySlim<int> NumberOfTimesTheApplicationLogWasDisplayed { get; } = new();
    public ReactivePropertySlim<int> NumberOfTimesSliceToolHasBeenUsed { get; } = new();
    public ReactivePropertySlim<int> NumberOfDrawsOfFreeHandTool { get; } = new();
    public ReactivePropertySlim<int> NumberOfDrawsOfThePieTool { get; } = new();
    public ReactivePropertySlim<int> NumberOfDrawsOfTheMosaicTool { get; } = new();
    public ReactivePropertySlim<int> NumberOfDrawsOfTheGaussianFilterTool { get; } = new();
    public ReactivePropertySlim<int> NumberOfDrawsOfTheColorCorrectTool { get; } = new();

    public ReactivePropertySlim<TimeSpan> Uptime { get; } = new();

    public string Title => Resources.Title_Statistics;

#pragma warning disable CS0067

    public event Action<IDialogResult> RequestClose;

#pragma warning restore CS0067

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

    public void Dispose()
    {
        // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Load()
    {
        var statistics = (Application.Current.MainWindow.DataContext as MainWindowViewModel).Statistics.Value;
        NumberOfBoots.Value = statistics.NumberOfBoots;
        NumberOfTimesTheFileWasOpenedBySpecifyingIt.Value = statistics.NumberOfTimesTheFileWasOpenedBySpecifyingIt;
        NumberOfTimesTheAutoSaveFileIsSpecifiedAndOpened.Value =
            statistics.NumberOfTimesTheAutoSaveFileIsSpecifiedAndOpened;
        NumberOfClicksWithThePointerTool.Value = statistics.NumberOfClicksWithThePointerTool;
        CumulativeTotalOfItemsSelectedWithTheLassoTool.Value =
            statistics.CumulativeTotalOfItemsSelectedWithTheLassoTool;
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
        NumberOfTimesTheVersionInformationDialogWasDisplayed.Value =
            statistics.NumberOfTimesTheVersionInformationDialogWasDisplayed;
        NumberOfTimesTheApplicationLogWasDisplayed.Value = statistics.NumberOfTimesTheApplicationLogWasDisplayed;
        NumberOfTimesSliceToolHasBeenUsed.Value = statistics.NumberOfTimesSliceToolHasBeenUsed;
        NumberOfDrawsOfFreeHandTool.Value = statistics.NumberOfDrawsOfFreeHandTool;
        NumberOfDrawsOfThePieTool.Value = statistics.NumberOfDrawsOfThePieTool;
        NumberOfDrawsOfTheMosaicTool.Value = statistics.NumberOfDrawsOfTheMosaicTool;
        NumberOfDrawsOfTheGaussianFilterTool.Value = statistics.NumberOfDrawsOfTheGaussianFilterTool;
        NumberOfDrawsOfTheColorCorrectTool.Value = statistics.NumberOfDrawsOfTheColorCorrectTool;

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

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing) disposables.Dispose();

            disposables = null;
            disposedValue = true;
        }
    }
}