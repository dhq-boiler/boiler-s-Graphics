using boilersGraphics.Models.Text;
using R3;

namespace boilersGraphics.ViewModels.Text;

/// <summary>
/// Phase 2.5-a: TextMatrixBlock の VM。
/// Rows / Columns / CellMode / Separator / Sequential / DataGenerator / CustomList の各設定が
/// 変わると同期的に Text を再生成する (Q-5 案 A: 常に同期)。
/// </summary>
public class TextMatrixBlockViewModel : TextElementBaseViewModel
{
    public new TextMatrixBlock Model => (TextMatrixBlock)base.Model;

    public BindableReactiveProperty<int> Rows { get; }
    public BindableReactiveProperty<int> Columns { get; }
    public BindableReactiveProperty<TextMatrixCellMode> CellMode { get; }
    public BindableReactiveProperty<string> Separator { get; }
    public BindableReactiveProperty<int> SequenceStart { get; }
    public BindableReactiveProperty<string> SequenceFormat { get; }
    public BindableReactiveProperty<DataGeneratorType> DataGenType { get; }
    public BindableReactiveProperty<int> DataGenSeed { get; }
    public BindableReactiveProperty<string> CustomItems { get; }

    public TextMatrixBlockViewModel() : this(new TextMatrixBlock())
    {
    }

    public TextMatrixBlockViewModel(TextMatrixBlock model) : base(model)
    {
        Rows = new BindableReactiveProperty<int>(model.Rows);
        Columns = new BindableReactiveProperty<int>(model.Columns);
        CellMode = new BindableReactiveProperty<TextMatrixCellMode>(model.CellMode);
        Separator = new BindableReactiveProperty<string>(model.Separator);
        SequenceStart = new BindableReactiveProperty<int>(model.SequenceStart);
        SequenceFormat = new BindableReactiveProperty<string>(model.SequenceFormat);
        DataGenType = new BindableReactiveProperty<DataGeneratorType>(model.DataGenType);
        DataGenSeed = new BindableReactiveProperty<int>(model.DataGenSeed);
        CustomItems = new BindableReactiveProperty<string>(model.CustomItems);

        Rows.Subscribe(v => model.Rows = v).AddTo(_CompositeDisposable);
        Columns.Subscribe(v => model.Columns = v).AddTo(_CompositeDisposable);
        CellMode.Subscribe(v => model.CellMode = v).AddTo(_CompositeDisposable);
        Separator.Subscribe(v => model.Separator = v).AddTo(_CompositeDisposable);
        SequenceStart.Subscribe(v => model.SequenceStart = v).AddTo(_CompositeDisposable);
        SequenceFormat.Subscribe(v => model.SequenceFormat = v).AddTo(_CompositeDisposable);
        DataGenType.Subscribe(v => model.DataGenType = v).AddTo(_CompositeDisposable);
        DataGenSeed.Subscribe(v => model.DataGenSeed = v).AddTo(_CompositeDisposable);
        CustomItems.Subscribe(v => model.CustomItems = v).AddTo(_CompositeDisposable);

        Rows.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        Columns.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        CellMode.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        Separator.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        SequenceStart.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        SequenceFormat.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        DataGenType.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        DataGenSeed.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        CustomItems.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);

        Regenerate();
    }

    public override bool IsResizable => true;

    public override bool SupportsPropertyDialog => true;

    public override void OpenPropertyDialog()
    {
        // プロパティダイアログ拡充: DetailTextMatrix を Prism Dialog として起動。
        if (System.Windows.Application.Current is not Prism.Unity.PrismApplication app) return;
        if (app.Container is not Prism.Ioc.IContainerExtension container) return;
        var dialogService = new Prism.Services.Dialogs.DialogService(container);
        Prism.Services.Dialogs.IDialogResult result = null;
        dialogService.Show(
            nameof(boilersGraphics.Views.DetailTextMatrix),
            new Prism.Services.Dialogs.DialogParameters { { "ViewModel", this } },
            ret => result = ret);
    }

    public override object Clone()
    {
        var cloneModel = new TextMatrixBlock
        {
            Rows = Model.Rows,
            Columns = Model.Columns,
            CellMode = Model.CellMode,
            Separator = Model.Separator,
            SequenceStart = Model.SequenceStart,
            SequenceFormat = Model.SequenceFormat,
            DataGenType = Model.DataGenType,
            DataGenSeed = Model.DataGenSeed,
            CustomItems = Model.CustomItems,
        };
        var clone = new TextMatrixBlockViewModel(cloneModel);
        CopyCommonPropertiesTo(clone);
        return clone;
    }

    public void Regenerate()
    {
        Text.Value = TextMatrixGenerator.Generate(
            Rows.Value, Columns.Value, CellMode.Value, Separator.Value,
            SequenceStart.Value, SequenceFormat.Value,
            DataGenType.Value, DataGenSeed.Value, CustomItems.Value);
    }
}
