using System;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace boiler_sGraphics.Views.Behaviors
{
    public sealed class DropAcceptDescription
    {
        #region Events
        /// <summary>
        /// ドラッグオーバーイベント
        /// </summary>
        public event Action<DragEventArgs> DragOver;

        /// <summary>
        /// ドロップイベント
        /// </summary>
        public event Action<DragEventArgs> DragDrop;
        #endregion Events
        #region Methods
        /// <summary>
        /// ドラッグオーバー処理呼び出し
        /// </summary>
        /// <param name="dragEventArgs"></param>
        public void OnDragOver(DragEventArgs dragEventArgs)
        {
            var handler = this.DragOver;
            if (handler != null)
            {
                handler(dragEventArgs);
            }
        }

        /// <summary>
        /// ドロップ処理呼び出し
        /// </summary>
        /// <param name="dragEventArgs"></param>
        public void OnDrop(DragEventArgs dragEventArgs)
        {
            var handler = this.DragDrop;
            if (handler != null)
            {
                handler(dragEventArgs);
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// ドロップ対象オブジェクト用ビヘイビア
    /// <see cref="http://b.starwing.net/?p=131"/>
    /// </summary>
    public class DragAcceptBehavior : Behavior<FrameworkElement>
    {
        #region Fields
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(DropAcceptDescription),
            typeof(DragAcceptBehavior), new PropertyMetadata(null));
        #endregion Fields
        #region Properties
        public DropAcceptDescription Description
        {
            get { return (DropAcceptDescription)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }
        #endregion Properties
        #region Methods
        /// <summary>
        /// 初期化
        /// </summary>
        protected override void OnAttached()
        {
            this.AssociatedObject.PreviewDragOver += DragOverHandler;
            this.AssociatedObject.PreviewDrop += DropHandler;
            base.OnAttached();
        }

        /// <summary>
        /// 後始末
        /// </summary>
        protected override void OnDetaching()
        {
            this.AssociatedObject.PreviewDragOver -= DragOverHandler;
            this.AssociatedObject.PreviewDrop -= DropHandler;
            base.OnDetaching();
        }

        /// <summary>
        /// ドラッグオーバー処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DragOverHandler(object sender, DragEventArgs e)
        {
            var desc = this.Description;
            if (desc == null)
            {
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
                return;
            }
            desc.OnDragOver(e);
            e.Handled = true;
        }

        /// <summary>
        /// ドロップ処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DropHandler(object sender, DragEventArgs e)
        {
            var desc = this.Description;
            if (desc == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            desc.OnDrop(e);
            e.Handled = true;
        }
        #endregion Method
    }
}
