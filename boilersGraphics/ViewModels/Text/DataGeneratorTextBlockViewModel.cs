using boilersGraphics.Models.Text;
using R3;

namespace boilersGraphics.ViewModels.Text;

/// <summary>
/// Phase 2-c: DataGeneratorTextBlock の VM。Type / Seed / Count / Separator / Layout の
/// いずれかが変わると同期的に Text を再生成して反映する (Q-5 案 A: 常に同期)。
/// IsSeedLocked は再生成挙動には影響せず、UI 側で Seed 入力可否のスイッチに使う想定 (Q-3 案 C)。
/// </summary>
public class DataGeneratorTextBlockViewModel : TextElementBaseViewModel
{
    public new DataGeneratorTextBlock Model => (DataGeneratorTextBlock)base.Model;

    public BindableReactiveProperty<DataGeneratorType> Type { get; }
    public BindableReactiveProperty<int> Seed { get; }
    public BindableReactiveProperty<bool> IsSeedLocked { get; }
    public BindableReactiveProperty<int> Count { get; }
    public BindableReactiveProperty<string> Separator { get; }
    public BindableReactiveProperty<DataGeneratorLayout> Layout { get; }

    public DataGeneratorTextBlockViewModel() : this(new DataGeneratorTextBlock())
    {
    }

    public DataGeneratorTextBlockViewModel(DataGeneratorTextBlock model) : base(model)
    {
        Type = new BindableReactiveProperty<DataGeneratorType>(model.Type);
        Seed = new BindableReactiveProperty<int>(model.Seed);
        IsSeedLocked = new BindableReactiveProperty<bool>(model.IsSeedLocked);
        Count = new BindableReactiveProperty<int>(model.Count);
        Separator = new BindableReactiveProperty<string>(model.Separator);
        Layout = new BindableReactiveProperty<DataGeneratorLayout>(model.Layout);

        Type.Subscribe(v => model.Type = v).AddTo(_CompositeDisposable);
        Seed.Subscribe(v => model.Seed = v).AddTo(_CompositeDisposable);
        IsSeedLocked.Subscribe(v => model.IsSeedLocked = v).AddTo(_CompositeDisposable);
        Count.Subscribe(v => model.Count = v).AddTo(_CompositeDisposable);
        Separator.Subscribe(v => model.Separator = v).AddTo(_CompositeDisposable);
        Layout.Subscribe(v => model.Layout = v).AddTo(_CompositeDisposable);

        Type.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        Seed.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        Count.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        Separator.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);
        Layout.Skip(1).Subscribe(_ => Regenerate()).AddTo(_CompositeDisposable);

        Regenerate();
    }

    public override bool IsResizable => true;

    public override bool SupportsPropertyDialog => false;

    public override object Clone()
    {
        var cloneModel = new DataGeneratorTextBlock
        {
            Type = Model.Type,
            Seed = Model.Seed,
            IsSeedLocked = Model.IsSeedLocked,
            Count = Model.Count,
            Separator = Model.Separator,
            Layout = Model.Layout,
        };
        var clone = new DataGeneratorTextBlockViewModel(cloneModel);
        CopyCommonPropertiesTo(clone);
        return clone;
    }

    /// <summary>
    /// 現在の Type / Seed / Count / Separator / Layout から Text を生成して反映する。
    /// </summary>
    public void Regenerate()
    {
        Text.Value = DataGenerator.Generate(Type.Value, Seed.Value, Count.Value, Separator.Value, Layout.Value);
    }
}
