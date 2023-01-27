﻿using System.Windows.Controls;
using Prism.Regions;

namespace boilersGraphics.Views;

/// <summary>
///     DetailPie.xaml の相互作用ロジック
/// </summary>
public partial class DetailPie : UserControl
{
    public DetailPie(IRegionManager regionManager)
    {
        InitializeComponent();
        RegionManager.SetRegionManager(_DetailRegion, regionManager);
    }
}