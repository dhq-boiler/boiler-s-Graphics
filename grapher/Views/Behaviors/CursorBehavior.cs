using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace grapher.Views.Behaviors
{
    class CursorBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty DefaultCursorProperty = DependencyProperty.Register("DefaultCursor", typeof(Cursor), typeof(CursorBehavior), new FrameworkPropertyMetadata(Cursors.Arrow));
        public static readonly DependencyProperty SpecificCursorProperty = DependencyProperty.Register("SpecificCursor", typeof(Cursor), typeof(CursorBehavior), new FrameworkPropertyMetadata(Cursors.Arrow));

        public Cursor DefaultCursor
        {
            get { return (Cursor)GetValue(DefaultCursorProperty); }
            set { SetValue(DefaultCursorProperty, value); }
        }

        public Cursor SpecificCursor
        {
            get { return (Cursor)GetValue(SpecificCursorProperty); }
            set { SetValue(SpecificCursorProperty, value); }
        }

        protected override void OnAttached()
        {
            this.AssociatedObject.MouseEnter += AssociatedObject_MouseEnter;
            this.AssociatedObject.MouseLeave += AssociatedObject_MouseLeave;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.MouseEnter -= AssociatedObject_MouseEnter;
            this.AssociatedObject.MouseLeave -= AssociatedObject_MouseLeave;
            base.OnDetaching();
        }

        private void AssociatedObject_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (SpecificCursor == null) return;
            (sender as FrameworkElement).Cursor = SpecificCursor;
        }

        private void AssociatedObject_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (DefaultCursor == null) return;
            (sender as FrameworkElement).Cursor = DefaultCursor;
        }
    }
}
