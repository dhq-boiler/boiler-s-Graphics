﻿using boilersGraphics.ViewModels;

namespace boilersGraphics.Strategies
{
    internal class PolygonalLineFactory : LineFactory
    {
        public override ConnectorBaseViewModel Create(IDiagramViewModel viewModel, ConnectorInfoBase sourceConnectorInfo, ConnectorInfoBase sinkConnectorInfo)
        {
            var ret = new PolygonalConnectorViewModel(sourceConnectorInfo, sinkConnectorInfo)
            {
                Owner = viewModel
            };
            ret.ZIndex.Value = ret.Owner.Items.Count;
            return ret;
        }
    }
}