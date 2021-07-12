using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace boilersGraphics.Controls
{
    //WPFでTextBoxにdoubleをBindingする
    //https://qiita.com/naminodarie/items/131a534bfe1a5bca1e5f
    //author : @naminodarie
    public class DoubleTextBox : TextBox
    {
        public string DoubleText
        {
            get => (string)GetValue(DoubleTextProperty);
            set => SetValue(DoubleTextProperty, value);
        }
        public static readonly DependencyProperty DoubleTextProperty =
              DependencyProperty.Register(
                  nameof(DoubleText),
                  typeof(string),
                  typeof(DoubleTextBox),
                  new FrameworkPropertyMetadata(
                      string.Empty,
                      FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                      new PropertyChangedCallback(OnDoubleTextChanged),
                      null,
                      true,
                      UpdateSourceTrigger.LostFocus));

        private static void OnDoubleTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                var currentText = textBox.Text;
                var newText = (string)e.NewValue;
                if (currentText == newText)
                    return;
                if (
                    double.TryParse(currentText, out var currentDouble) &&
                    double.TryParse(newText, out var newDouble) &&
                    currentDouble == newDouble
                    )
                    return;

                textBox.Text = newText;
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (!IsFocused)
            {
                FocusManager.SetFocusedElement(FocusManager.GetFocusScope(this), this);
                SelectAll();
                e.Handled = true;
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            SelectAll();
            e.Handled = true;
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            this.DoubleText = this.Text;
        }
    }
}
