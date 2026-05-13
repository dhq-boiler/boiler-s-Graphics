using boilersGraphics.Models.Parts;
using boilersGraphics.ViewModels;
using boilersGraphics.ViewModels.Parts;
using Moq;
using NUnit.Framework;
using Prism.Services.Dialogs;
using R3;
using System.Threading;

namespace boilersGraphics.Test.ViewModels.Parts;

[TestFixture]
public class PartEditorViewModelTest
{
    [Test, RequiresThread(ApartmentState.STA)]
    public void Title_初期値はパーツ編集()
    {
        var vm = new PartEditorViewModel();
        Assert.That(vm.Title, Is.EqualTo("パーツ編集"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void Definition_初期値はnull()
    {
        var vm = new PartEditorViewModel();
        Assert.That(vm.Definition, Is.Null);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OnDialogOpened_PartDefinition付き_DefinitionとTitleが反映される()
    {
        var vm = new PartEditorViewModel();
        var model = new PartDefinition { Name = "リング" };
        var defVm = new PartDefinitionViewModel(model);

        var parameters = new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, defVm }
        };
        vm.OnDialogOpened(parameters);

        Assert.That(vm.Definition, Is.SameAs(defVm));
        Assert.That(vm.Title, Is.EqualTo("パーツ編集: リング"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OnDialogOpened_DefinitionのNameが変わるとTitleが連動する()
    {
        var vm = new PartEditorViewModel();
        var model = new PartDefinition { Name = "リング" };
        var defVm = new PartDefinitionViewModel(model);

        var parameters = new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, defVm }
        };
        vm.OnDialogOpened(parameters);

        defVm.Name.Value = "目盛り";

        Assert.That(vm.Title, Is.EqualTo("パーツ編集: 目盛り"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OnDialogOpened_NameがnullまたはWhitespaceならパーツ編集()
    {
        var vm = new PartEditorViewModel();
        var model = new PartDefinition { Name = "" };
        var defVm = new PartDefinitionViewModel(model);

        var parameters = new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, defVm }
        };
        vm.OnDialogOpened(parameters);

        Assert.That(vm.Title, Is.EqualTo("パーツ編集"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OnDialogOpened_パラメータがnull_例外なし()
    {
        var vm = new PartEditorViewModel();
        Assert.DoesNotThrow(() => vm.OnDialogOpened(null));
        Assert.That(vm.Definition, Is.Null);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OnDialogOpened_PartDefinitionKeyなし_例外なし()
    {
        var vm = new PartEditorViewModel();
        var parameters = new DialogParameters
        {
            { "OtherKey", "value" }
        };
        Assert.DoesNotThrow(() => vm.OnDialogOpened(parameters));
        Assert.That(vm.Definition, Is.Null);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void CloseCommand_RequestCloseでButtonResultOKを返す()
    {
        var vm = new PartEditorViewModel();
        IDialogResult result = null;
        vm.RequestClose += r => result = r;

        vm.CloseCommand.Execute();

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Result, Is.EqualTo(ButtonResult.OK));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void CanCloseDialog_常にtrue()
    {
        var vm = new PartEditorViewModel();
        Assert.That(vm.CanCloseDialog(), Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AddRectangleCommand_Definitionなし_例外なし()
    {
        var vm = new PartEditorViewModel();
        Assert.DoesNotThrow(() => vm.AddRectangleCommand.Execute(Unit.Default));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AddRectangleCommand_Execute_DefinitionのItemsにNRectangleが追加される()
    {
        var vm = new PartEditorViewModel();
        var defVm = new PartDefinitionViewModel(new PartDefinition { Name = "リング" });
        vm.OnDialogOpened(new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, defVm }
        });

        vm.AddRectangleCommand.Execute(Unit.Default);

        Assert.That(defVm.Items.Count, Is.EqualTo(1));
        Assert.That(defVm.Items[0], Is.TypeOf<NRectangleViewModel>());
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AddRectangleCommand_複数回Execute_その分だけItemsが増える()
    {
        var vm = new PartEditorViewModel();
        var defVm = new PartDefinitionViewModel(new PartDefinition { Name = "メーター" });
        vm.OnDialogOpened(new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, defVm }
        });

        vm.AddRectangleCommand.Execute(Unit.Default);
        vm.AddRectangleCommand.Execute(Unit.Default);
        vm.AddRectangleCommand.Execute(Unit.Default);

        Assert.That(defVm.Items.Count, Is.EqualTo(3));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void SelectedItem_初期値はnull()
    {
        var vm = new PartEditorViewModel();
        Assert.That(vm.SelectedItem.Value, Is.Null);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void SelectItem_対象のIsSelectedがtrueになりSelectedItemに反映される()
    {
        var vm = new PartEditorViewModel();
        var defVm = new PartDefinitionViewModel(new PartDefinition { Name = "リング" });
        vm.OnDialogOpened(new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, defVm }
        });

        vm.AddRectangleCommand.Execute(Unit.Default);
        var rect = defVm.Items[0];

        vm.SelectItem(rect);

        Assert.That(vm.SelectedItem.Value, Is.SameAs(rect));
        Assert.That(rect.IsSelected.Value, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void SelectItem_別アイテムを選択すると前の選択が解除される()
    {
        var vm = new PartEditorViewModel();
        var defVm = new PartDefinitionViewModel(new PartDefinition { Name = "リング" });
        vm.OnDialogOpened(new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, defVm }
        });

        vm.AddRectangleCommand.Execute(Unit.Default);
        vm.AddRectangleCommand.Execute(Unit.Default);
        var first = defVm.Items[0];
        var second = defVm.Items[1];

        vm.SelectItem(first);
        vm.SelectItem(second);

        Assert.That(first.IsSelected.Value, Is.False);
        Assert.That(second.IsSelected.Value, Is.True);
        Assert.That(vm.SelectedItem.Value, Is.SameAs(second));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void DeleteSelectedCommand_SelectedItemがItemsから消えてSelectedItemはnullになる()
    {
        var vm = new PartEditorViewModel();
        var defVm = new PartDefinitionViewModel(new PartDefinition { Name = "リング" });
        vm.OnDialogOpened(new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, defVm }
        });

        vm.AddRectangleCommand.Execute(Unit.Default);
        var rect = defVm.Items[0];
        vm.SelectItem(rect);

        vm.DeleteSelectedCommand.Execute(Unit.Default);

        Assert.That(defVm.Items, Has.Count.EqualTo(0));
        Assert.That(vm.SelectedItem.Value, Is.Null);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void DeleteSelectedCommand_未選択なら何も起きない()
    {
        var vm = new PartEditorViewModel();
        var defVm = new PartDefinitionViewModel(new PartDefinition { Name = "リング" });
        vm.OnDialogOpened(new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, defVm }
        });

        vm.AddRectangleCommand.Execute(Unit.Default);

        vm.DeleteSelectedCommand.Execute(Unit.Default);

        Assert.That(defVm.Items, Has.Count.EqualTo(1));
        Assert.That(vm.SelectedItem.Value, Is.Null);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void DeleteSelectedCommand_Definitionなし_例外なし()
    {
        var vm = new PartEditorViewModel();
        Assert.DoesNotThrow(() => vm.DeleteSelectedCommand.Execute(Unit.Default));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void SelectEdgeColorCommand_DialogServiceなし_SelectedItemなし_例外なし()
    {
        var vm = new PartEditorViewModel();
        Assert.DoesNotThrow(() => vm.SelectEdgeColorCommand.Execute(Unit.Default));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void SelectFillColorCommand_DialogServiceなし_SelectedItemなし_例外なし()
    {
        var vm = new PartEditorViewModel();
        Assert.DoesNotThrow(() => vm.SelectFillColorCommand.Execute(Unit.Default));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void SelectEdgeColorCommand_DialogServiceなし_SelectedItemあり_例外なし()
    {
        var vm = new PartEditorViewModel();
        var defVm = new PartDefinitionViewModel(new PartDefinition { Name = "リング" });
        vm.OnDialogOpened(new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, defVm }
        });
        vm.AddRectangleCommand.Execute(Unit.Default);
        vm.SelectItem(defVm.Items[0]);

        Assert.DoesNotThrow(() => vm.SelectEdgeColorCommand.Execute(Unit.Default));
    }

    // Phase 1-c-6-d-6: 公開パラメータ編集 UI

    [Test, RequiresThread(ApartmentState.STA)]
    public void AddExposedPropertyDirect_Diagramなし_DefinitionのExposedPropertiesに追加される()
    {
        var vm = new PartEditorViewModel();
        var defVm = new PartDefinitionViewModel(new PartDefinition { Name = "リング" });
        vm.OnDialogOpened(new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, defVm }
        });
        var ep = new ExposedPropertyViewModel(new ExposedProperty
        {
            Name = "半径",
            Type = ExposedPropertyType.Double,
            DefaultValue = 10d,
        });

        vm.AddExposedPropertyDirect(ep);

        Assert.That(defVm.ExposedProperties, Has.Count.EqualTo(1));
        Assert.That(defVm.ExposedProperties[0], Is.SameAs(ep));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AddExposedPropertyDirect_Definitionなし_例外なし()
    {
        var vm = new PartEditorViewModel();
        var ep = new ExposedPropertyViewModel(new ExposedProperty { Name = "X" });
        Assert.DoesNotThrow(() => vm.AddExposedPropertyDirect(ep));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AddExposedPropertyDirect_nullEp_例外なし()
    {
        var vm = new PartEditorViewModel();
        var defVm = new PartDefinitionViewModel(new PartDefinition { Name = "リング" });
        vm.OnDialogOpened(new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, defVm }
        });
        Assert.DoesNotThrow(() => vm.AddExposedPropertyDirect(null));
        Assert.That(defVm.ExposedProperties, Has.Count.EqualTo(0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void RemoveExposedPropertyCommand_Diagramなし_DefinitionのExposedPropertiesから消える()
    {
        var vm = new PartEditorViewModel();
        var defVm = new PartDefinitionViewModel(new PartDefinition { Name = "リング" });
        vm.OnDialogOpened(new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, defVm }
        });
        var ep = new ExposedPropertyViewModel(new ExposedProperty { Name = "半径" });
        vm.AddExposedPropertyDirect(ep);

        vm.RemoveExposedPropertyCommand.Execute(ep);

        Assert.That(defVm.ExposedProperties, Has.Count.EqualTo(0));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void RemoveExposedPropertyCommand_存在しないep_例外なし()
    {
        var vm = new PartEditorViewModel();
        var defVm = new PartDefinitionViewModel(new PartDefinition { Name = "リング" });
        vm.OnDialogOpened(new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, defVm }
        });
        var ep = new ExposedPropertyViewModel(new ExposedProperty { Name = "未登録" });

        Assert.DoesNotThrow(() => vm.RemoveExposedPropertyCommand.Execute(ep));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void AddExposedPropertyCommand_DialogServiceなし_例外なし_何も追加されない()
    {
        var vm = new PartEditorViewModel();
        var defVm = new PartDefinitionViewModel(new PartDefinition { Name = "リング" });
        vm.OnDialogOpened(new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, defVm }
        });

        Assert.DoesNotThrow(() => vm.AddExposedPropertyCommand.Execute(Unit.Default));
        Assert.That(defVm.ExposedProperties, Has.Count.EqualTo(0),
            "DialogService が無いと追加経路が無いので、何も追加されない");
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OnDialogOpened_DiagramKey付き_Diagramプロパティに反映される()
    {
        boilersGraphics.App.IsTest = true;
        var dlgService = new Mock<IDialogService>();
        var mw = new MainWindowViewModel(dlgService.Object);
        var diagram = new DiagramViewModel(mw);

        var vm = new PartEditorViewModel();
        var defVm = new PartDefinitionViewModel(new PartDefinition { Name = "リング" });
        vm.OnDialogOpened(new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, defVm },
            { PartEditorViewModel.DiagramKey, diagram },
        });

        Assert.That(vm.Diagram, Is.SameAs(diagram));
    }

    // Phase 1 フォローアップ A: ピンアイコントグル方式 + Binding 自動配線

    private static (PartEditorViewModel vm, PartDefinitionViewModel def, DesignerItemViewModelBase rect)
        SetupWithRectangle()
    {
        boilersGraphics.App.IsTest = true;
        var vm = new PartEditorViewModel();
        var defVm = new PartDefinitionViewModel(new PartDefinition { Name = "P" });
        vm.OnDialogOpened(new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, defVm }
        });
        vm.AddRectangleCommand.Execute(Unit.Default);
        var rect = (DesignerItemViewModelBase)defVm.Items[0];
        vm.SelectItem(rect);
        return (vm, defVm, rect);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void TogglePropertyExposure_未公開_新規ExposedPropertyが追加される()
    {
        var (vm, def, _) = SetupWithRectangle();

        vm.TogglePropertyExposureCommand.Execute("Left");

        Assert.That(def.ExposedProperties, Has.Count.EqualTo(1));
        Assert.That(def.ExposedProperties[0].Name.Value, Is.EqualTo("Left"));
        Assert.That(def.ExposedProperties[0].Type.Value, Is.EqualTo(ExposedPropertyType.Double));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void TogglePropertyExposure_未公開_BindingがSelectedItemに紐づく()
    {
        var (vm, def, rect) = SetupWithRectangle();

        vm.TogglePropertyExposureCommand.Execute("Width");

        var ep = def.ExposedProperties[0];
        Assert.That(ep.Bindings, Has.Count.EqualTo(1));
        Assert.That(ep.Bindings[0].TargetItemId.Value, Is.EqualTo(rect.ID));
        Assert.That(ep.Bindings[0].TargetProperty.Value, Is.EqualTo("Width"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void TogglePropertyExposure_未公開_DefaultValueは現在の図形の値()
    {
        var (vm, def, rect) = SetupWithRectangle();
        rect.Width.Value = 123d;

        vm.TogglePropertyExposureCommand.Execute("Width");

        Assert.That(def.ExposedProperties[0].DefaultValue.Value, Is.EqualTo(123d));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void TogglePropertyExposure_公開済み_再トグルで削除される()
    {
        var (vm, def, _) = SetupWithRectangle();
        vm.TogglePropertyExposureCommand.Execute("Left");

        vm.TogglePropertyExposureCommand.Execute("Left");

        Assert.That(def.ExposedProperties, Is.Empty);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void TogglePropertyExposure_別図形の同名プロパティは独立に公開できる()
    {
        var (vm, def, _) = SetupWithRectangle();
        vm.TogglePropertyExposureCommand.Execute("Left");
        vm.AddRectangleCommand.Execute(Unit.Default);
        var second = (DesignerItemViewModelBase)def.Items[1];
        vm.SelectItem(second);

        vm.TogglePropertyExposureCommand.Execute("Left");

        Assert.That(def.ExposedProperties, Has.Count.EqualTo(2));
        // 2 件目はユニーク化された名前になる
        Assert.That(def.ExposedProperties[1].Name.Value, Is.EqualTo("Left2"));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void TogglePropertyExposure_EdgeBrushはBrush型として公開される()
    {
        var (vm, def, _) = SetupWithRectangle();

        vm.TogglePropertyExposureCommand.Execute("EdgeBrush");

        Assert.That(def.ExposedProperties[0].Type.Value, Is.EqualTo(ExposedPropertyType.Brush));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void TogglePropertyExposure_FillBrushはBrush型として公開される()
    {
        var (vm, def, _) = SetupWithRectangle();

        vm.TogglePropertyExposureCommand.Execute("FillBrush");

        Assert.That(def.ExposedProperties[0].Type.Value, Is.EqualTo(ExposedPropertyType.Brush));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void TogglePropertyExposure_未サポートのプロパティ名_例外なくスキップされる()
    {
        var (vm, def, _) = SetupWithRectangle();

        Assert.DoesNotThrow(() => vm.TogglePropertyExposureCommand.Execute("Unknown"));
        Assert.That(def.ExposedProperties, Is.Empty);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void TogglePropertyExposure_SelectedItemなし_例外なくスキップされる()
    {
        boilersGraphics.App.IsTest = true;
        var vm = new PartEditorViewModel();
        var def = new PartDefinitionViewModel(new PartDefinition { Name = "P" });
        vm.OnDialogOpened(new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, def }
        });

        Assert.DoesNotThrow(() => vm.TogglePropertyExposureCommand.Execute("Left"));
        Assert.That(def.ExposedProperties, Is.Empty);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void IsLeftExposed_初期値はfalse_TogglePropertyExposureでtrueになる()
    {
        var (vm, _, _) = SetupWithRectangle();
        Assert.That(vm.IsLeftExposed.Value, Is.False);

        vm.TogglePropertyExposureCommand.Execute("Left");

        Assert.That(vm.IsLeftExposed.Value, Is.True);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void IsExposedFlags_別図形を選択するとフラグが再計算される()
    {
        var (vm, def, first) = SetupWithRectangle();
        vm.TogglePropertyExposureCommand.Execute("Left");
        Assert.That(vm.IsLeftExposed.Value, Is.True);

        vm.AddRectangleCommand.Execute(Unit.Default);
        var second = (DesignerItemViewModelBase)def.Items[1];
        vm.SelectItem(second);

        Assert.That(vm.IsLeftExposed.Value, Is.False, "別図形は Left を公開していない");
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void IsExposedFlags_ExposedPropertyを削除するとfalseに戻る()
    {
        var (vm, def, _) = SetupWithRectangle();
        vm.TogglePropertyExposureCommand.Execute("Left");
        Assert.That(vm.IsLeftExposed.Value, Is.True);

        vm.RemoveExposedPropertyCommand.Execute(def.ExposedProperties[0]);

        Assert.That(vm.IsLeftExposed.Value, Is.False);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void FindExposedPropertyFor_一致するBindingを持つExposedPropertyを返す()
    {
        var (vm, def, rect) = SetupWithRectangle();
        vm.TogglePropertyExposureCommand.Execute("Height");

        var found = vm.FindExposedPropertyFor(rect, "Height");

        Assert.That(found, Is.Not.Null);
        Assert.That(found, Is.SameAs(def.ExposedProperties[0]));
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void FindExposedPropertyFor_該当なしの場合_nullを返す()
    {
        var (vm, _, rect) = SetupWithRectangle();

        var found = vm.FindExposedPropertyFor(rect, "Top");

        Assert.That(found, Is.Null);
    }

    [Test, RequiresThread(ApartmentState.STA)]
    public void OnDialogClosed_例外なくDisposeできる()
    {
        var vm = new PartEditorViewModel();
        var model = new PartDefinition { Name = "リング" };
        var defVm = new PartDefinitionViewModel(model);
        var parameters = new DialogParameters
        {
            { PartEditorViewModel.PartDefinitionKey, defVm }
        };
        vm.OnDialogOpened(parameters);

        Assert.DoesNotThrow(() => vm.OnDialogClosed());
        Assert.DoesNotThrow(() => vm.OnDialogClosed()); // 二重 Dispose も問題なし
    }
}
