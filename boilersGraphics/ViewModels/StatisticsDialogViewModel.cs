using boilersGraphics.Properties;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using R3;
using System;
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

    public BindableReactiveProperty<int> NumberOfBoots { get; } = new();

    public BindableReactiveProperty<int> NumberOfTimesTheFileWasOpenedBySpecifyingIt { get; } = new();

    public BindableReactiveProperty<int> NumberOfTimesTheAutoSaveFileIsSpecifiedAndOpened { get; } = new();

    public BindableReactiveProperty<int> NumberOfClicksWithThePointerTool { get; } = new();

    public BindableReactiveProperty<int> CumulativeTotalOfItemsSelectedWithTheLassoTool { get; } = new();

    public BindableReactiveProperty<int> NumberOfDrawsOfTheStraightLineTool { get; } = new();
    public BindableReactiveProperty<int> NumberOfDrawsOfTheRectangleTool { get; } = new();
    public BindableReactiveProperty<int> NumberOfDrawsOfTheEllipseTool { get; } = new();
    public BindableReactiveProperty<int> NumberOfDrawsOfTheImageFileTool { get; } = new();
    public BindableReactiveProperty<int> NumberOfDrawsOfTheLetterTool { get; } = new();
    public BindableReactiveProperty<int> NumberOfDrawsOfTheVerticalLetterTool { get; } = new();
    public BindableReactiveProperty<int> NumberOfDrawsOfPolygonTool { get; } = new();
    public BindableReactiveProperty<int> NumberOfDrawsOfBezierCurveTool { get; } = new();
    public BindableReactiveProperty<int> NumberOfSnapPointToolInstallations { get; } = new();
    public BindableReactiveProperty<int> BrushToolDrawCount { get; } = new();
    public BindableReactiveProperty<int> NumberOfTimesTheEraserToolHasBeenUsed { get; } = new();
    public BindableReactiveProperty<int> NumberOfTimesSaved { get; } = new();
    public BindableReactiveProperty<int> NumberOfTimesYouHaveNamedAndSaved { get; } = new();
    public BindableReactiveProperty<int> NumberOfExports { get; } = new();
    public BindableReactiveProperty<int> NumberOfJpegExports { get; } = new();
    public BindableReactiveProperty<int> NumberOfPngExports { get; } = new();
    public BindableReactiveProperty<int> NumberOfGifExports { get; } = new();
    public BindableReactiveProperty<int> NumberOfBmpExports { get; } = new();
    public BindableReactiveProperty<int> NumberOfTiffExports { get; } = new();
    public BindableReactiveProperty<int> NumberOfWmpExports { get; } = new();
    public BindableReactiveProperty<int> NumberOfTimesGrouped { get; } = new();
    public BindableReactiveProperty<int> NumberOfUngrouped { get; } = new();
    public BindableReactiveProperty<int> NumberOfMovesToTheFrontend { get; } = new();
    public BindableReactiveProperty<int> NumberOfMovesToTheFront { get; } = new();
    public BindableReactiveProperty<int> NumberOfMovesToTheBack { get; } = new();
    public BindableReactiveProperty<int> NumberOfMovesToTheBackend { get; } = new();
    public BindableReactiveProperty<int> NumberOfTopAlignment { get; } = new();
    public BindableReactiveProperty<int> NumberOfTimesTheTopAndBottomAreCentered { get; } = new();
    public BindableReactiveProperty<int> NumberOfBottomAlignment { get; } = new();
    public BindableReactiveProperty<int> NumberOfLeftAlignment { get; } = new();
    public BindableReactiveProperty<int> NumberOfTimesLeftAndRightCentered { get; } = new();
    public BindableReactiveProperty<int> NumberOfRightAlignment { get; } = new();
    public BindableReactiveProperty<int> NumberOfTimesAlignedLeftAndRight { get; } = new();
    public BindableReactiveProperty<int> NumberOfTimesAlignedUpAndDown { get; } = new();
    public BindableReactiveProperty<int> NumberOfTimesToMatchTheWidth { get; } = new();
    public BindableReactiveProperty<int> NumberOfTimesToMatchTheHeight { get; } = new();
    public BindableReactiveProperty<int> NumberOfUnions { get; } = new();
    public BindableReactiveProperty<int> NumberOfIntersects { get; } = new();
    public BindableReactiveProperty<int> NumberOfXors { get; } = new();
    public BindableReactiveProperty<int> NumberOfExcludes { get; } = new();
    public BindableReactiveProperty<int> NumberOfCuts { get; } = new();
    public BindableReactiveProperty<int> NumberOfCopies { get; } = new();
    public BindableReactiveProperty<int> NumberOfPasted { get; } = new();
    public BindableReactiveProperty<int> NumberOfUndos { get; } = new();
    public BindableReactiveProperty<int> NumberOfRedoes { get; } = new();
    public BindableReactiveProperty<int> NumberOfTimesAutomaticallySaved { get; } = new();

    public BindableReactiveProperty<int> NumberOfNewlyCreatedLayers { get; } = new();
    public BindableReactiveProperty<int> NumberOfDeletedLayers { get; } = new();
    public BindableReactiveProperty<int> NumberOfTimesTheItemWasDrawn { get; } = new();
    public BindableReactiveProperty<int> NumberOfTimesTheItemWasDeleted { get; } = new();
    public BindableReactiveProperty<int> NumberOfLogLevelChanges { get; } = new();
    public BindableReactiveProperty<int> NumberOfTimesTheVersionInformationDialogWasDisplayed { get; } = new();
    public BindableReactiveProperty<int> NumberOfTimesTheApplicationLogWasDisplayed { get; } = new();
    public BindableReactiveProperty<int> NumberOfTimesSliceToolHasBeenUsed { get; } = new();
    public BindableReactiveProperty<int> NumberOfDrawsOfFreeHandTool { get; } = new();
    public BindableReactiveProperty<int> NumberOfDrawsOfThePieTool { get; } = new();
    public BindableReactiveProperty<int> NumberOfDrawsOfTheMosaicTool { get; } = new();
    public BindableReactiveProperty<int> NumberOfDrawsOfTheGaussianFilterTool { get; } = new();
    public BindableReactiveProperty<int> NumberOfDrawsOfTheColorCorrectTool { get; } = new();

    public BindableReactiveProperty<TimeSpan> Uptime { get; } = new();

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