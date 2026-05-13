using boilersGraphics.Models.Text;
using R3;

namespace boilersGraphics.ViewModels.Text;

/// <summary>
/// Phase 2-d: NumberSequenceBlock の VM。Start/End/Step/Format/Separator/Direction/GridRows/GridColumns
/// のいずれかが変わると同期再生成 (Q-5 案 A: 常に同期)。
/// </summary>
public class NumberSequenceBlockViewModel : TextElementBaseViewModel
{
    public new NumberSequenceBlock Model => (NumberSequenceBlock)base.Model;

    public BindableReactiveProperty<double> Start { get; }
    public BindableReactiveProperty<double> End { get; }
    public BindableReactiveProperty<double> Step { get; }
    public BindableReactiveProperty<string> Format { get; }
    public BindableReactiveProperty<string> Separator { get; }
    public BindableReactiveProperty<NumberSequenceDirection> Direction { get; }
    public BindableReactiveProperty<int> GridRows { get; }
    public BindableReactiveProperty<int> GridColumns { get; }

    public NumberSequenceBlockViewModel() : this(new NumberSequenceBlock())
    {
    }

    public NumberSequenceBlockViewModel(NumberSequenceBlock model) : base(model)
    {
        Start = new BindableReactiveProperty<double>(model.Start);
        End = new BindableReactiveProperty<double>(model.End);
        Step = new BindableReactiveProperty<double>(model.Step);
        Format = new BindableReactiveProperty<string>(model.Format);
        Separator = new BindableReactiveProperty<string>(model.Separator);
        Direction = new BindableReactiveProperty<NumberSequenceDirection>(model.Direction);
        GridRows = new BindableReactiveProperty<int>(model.GridRows);
        GridColumns = new BindableReactiveProperty<int>(model.GridColumns);

        Start.Subscribe(v => model.Start = v).AddTo(_CompositeDisposable);
        End.Subscribe(v => model.End = v).AddTo(_CompositeDisposable);
        Step.Subscribe(v => model.Step = v).AddTo(_CompositeDisposable);
        Format.Subscribe(v => model.Format = v).AddTo(_CompositeDisposable);
        Separator.Subscribe(v => model.Separator = v).AddTo(_CompositeDisposable);
        Direction.Subscribe(v => model.Direction = v).AddTo(_CompositeDisposable);
        GridRows.Subscribe(v => model.GridRows = v).AddTo(_CompositeDisposable);
        GridColumns.Subscribe(v => model.GridColumns = v).AddTo(_CompositeDisposable);

        Start.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        End.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        Step.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        Format.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        Separator.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        Direction.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        GridRows.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        GridColumns.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);

        Regenerate();
    }

    public override bool IsResizable => true;

    public override bool SupportsPropertyDialog => false;

    public override object Clone()
    {
        var cloneModel = new NumberSequenceBlock
        {
            Start = Model.Start,
            End = Model.End,
            Step = Model.Step,
            Format = Model.Format,
            Separator = Model.Separator,
            Direction = Model.Direction,
            GridRows = Model.GridRows,
            GridColumns = Model.GridColumns,
        };
        var clone = new NumberSequenceBlockViewModel(cloneModel);
        CopyCommonPropertiesTo(clone);
        return clone;
    }

    public void Regenerate()
    {
        Text.Value = NumberSequenceGenerator.Generate(
            Start.Value, End.Value, Step.Value, Format.Value, Separator.Value,
            Direction.Value, GridRows.Value, GridColumns.Value);
    }
}
