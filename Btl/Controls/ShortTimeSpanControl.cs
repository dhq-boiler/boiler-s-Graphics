// Copyright 2012 lapthorn.net.
//
// This software is provided "as is" without a warranty of any kind. All
// express or implied conditions, representations and warranties, including
// any implied warranty of merchantability, fitness for a particular purpose
// or non-infringement, are hereby excluded. lapthorn.net and its licensors
// shall not be liable for any damages suffered by licensee as a result of
// using the software. In no event will lapthorn.net be liable for any
// lost revenue, profit or data, or for direct, indirect, special,
// consequential, incidental or punitive damages, however caused and regardless
// of the theory of liability, arising out of the use of or inability to use
// software, even if lapthorn.net has been advised of the possibility of
// such damages.

using System;
using System.Windows;
using System.Windows.Controls;

namespace Btl.Controls
{
    public class ShortTimeSpanControl : Control
    {
        public ShortTimeSpanControl()
        {
        }

        static ShortTimeSpanControl()
        {
            
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ShortTimeSpanControl), new FrameworkPropertyMetadata(typeof(ShortTimeSpanControl)));
        }

        // Properties...

        public static readonly DependencyProperty IsEnableProperty = DependencyProperty.Register("IsEnable", typeof(bool), typeof(ShortTimeSpanControl));

        public bool IsEnable
        {
            get { return (bool)GetValue(IsEnableProperty); }
            set { SetValue(IsEnableProperty, value); }
        }

        #region Hours, minutes, and seconds

        #region Hours

        public int Hours
        {
            get { return (int)GetValue(HoursProperty); }
            set { SetValue(HoursProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Hours.  This enables animation, styling, binding, etc...
        protected static readonly DependencyProperty HoursProperty =
            DependencyProperty.Register("Hours", typeof(int), typeof(ShortTimeSpanControl), new UIPropertyMetadata(0, OnHoursChanged));

        private static void OnHoursChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var control = d as ShortTimeSpanControl;
            if (d == null)
                return;

            var oldValue = (int)args.OldValue;
            var newValue = (int)args.NewValue;

            //  make sure we don't get into a loop.
            if (oldValue != newValue)
            {
                control.Value = new TimeSpan(newValue, control.Value.Minutes, control.Value.Seconds);
            }
            var e = new RoutedPropertyChangedEventArgs<int>(oldValue, newValue, HoursChangedEvent);
            control.OnHoursChanged(e);
        }

        /// <summary>
        /// Raise the ValueChangedEvent
        /// </summary>
        /// <param name="e"></param>
        virtual protected void OnHoursChanged(RoutedPropertyChangedEventArgs<int> e)
        {
            RaiseEvent(e);
        }  
        #endregion


        #region Minutes

        public int Minutes
        {
            get { return (int)GetValue(MinutesProperty); }
            set { SetValue(MinutesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Minutes.  This enables animation, styling, binding, etc...
        protected static readonly DependencyProperty MinutesProperty =
            DependencyProperty.Register("Minutes", typeof(int), typeof(ShortTimeSpanControl), new UIPropertyMetadata(0, OnMinutesChanged));

        private static void OnMinutesChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var control = d as ShortTimeSpanControl;
            if (d == null)
                return;

            var oldValue = (int)args.OldValue;
            var newValue = (int)args.NewValue;

            //  make sure we don't get into a loop.
            if (oldValue != newValue)
            {
                control.Value = new TimeSpan(control.Value.Hours, newValue, control.Value.Seconds);
            }

            var e = new RoutedPropertyChangedEventArgs<int>(oldValue, newValue, MinutesChangedEvent);
            control.OnMinutesChanged(e);
        }

        /// <summary>
        /// Raise the ValueChangedEvent
        /// </summary>
        /// <param name="e"></param>
        virtual protected void OnMinutesChanged(RoutedPropertyChangedEventArgs<int> e)
        {
            RaiseEvent(e);
        }         
        #endregion


        #region Seconds

        public int Seconds
        {
            get { return (int)GetValue(SecondsProperty); }
            set { SetValue(SecondsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Seconds.  This enables animation, styling, binding, etc...
        protected static readonly DependencyProperty SecondsProperty =
            DependencyProperty.Register("Seconds", typeof(int), typeof(ShortTimeSpanControl), new UIPropertyMetadata(0, OnSecondsChanged));

        private static void OnSecondsChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var control = d as ShortTimeSpanControl;
            if (d == null)
                return;

            var oldValue = (int)args.OldValue;
            var newValue = (int)args.NewValue;

            //  make sure we don't get into a loop.
            if (oldValue != newValue)
            {
                control.Value = new TimeSpan(control.Value.Hours, control.Value.Minutes, newValue);
            }

            var e = new RoutedPropertyChangedEventArgs<int>(oldValue, newValue, SecondsChangedEvent);
            control.OnSecondsChanged(e);
        }

        /// <summary>
        /// Raise the ValueChangedEvent
        /// </summary>
        /// <param name="e"></param>
        virtual protected void OnSecondsChanged(RoutedPropertyChangedEventArgs<int> e)
        {
            RaiseEvent(e);
        }        
        #endregion

        #endregion

        #region Value TimeSpan Property
        public TimeSpan Value
        {
            get { return (TimeSpan)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        protected static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(TimeSpan), typeof(ShortTimeSpanControl), 
            new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));


        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            var control = d as ShortTimeSpanControl;
            if (d == null)
                return;

            var oldValue = (TimeSpan)args.OldValue;
            var newValue = (TimeSpan)args.NewValue;

            //  ensure we don't get into a loop with the 4 properties changing
            //  by only changing the value if it has changed. 

            if (oldValue != newValue)
            {
                control.Hours = newValue.Hours;
                control.Minutes = newValue.Minutes;
                control.Seconds = newValue.Seconds;
            }

            var e = new RoutedPropertyChangedEventArgs<TimeSpan>(oldValue, newValue, ValueChangedEvent);

            control.OnValueChanged(e);
        }

        /// <summary>
        /// Raise the ValueChangedEvent
        /// </summary>
        /// <param name="e"></param>
        virtual protected void OnValueChanged(RoutedPropertyChangedEventArgs<TimeSpan> e)
        {
            RaiseEvent(e);
        }

        #endregion


        #region Events
        #region ValueChanged Event

        /// <summary>
        /// The ValueChangedEvent, raised if  the value changes.
        /// </summary>
        private static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<TimeSpan>), typeof(ShortTimeSpanControl));

        /// <summary>
        /// Occurs when the Value property changes.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<TimeSpan> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }
        #endregion

        #region HoursChanged Event

        /// <summary>
        /// The ValueChangedEvent, raised if  the value changes.
        /// </summary>
        private static readonly RoutedEvent HoursChangedEvent =
            EventManager.RegisterRoutedEvent("HoursChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<int>), typeof(ShortTimeSpanControl));

        /// <summary>
        /// Occurs when the Value property changes.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<int> HoursChanged
        {
            add { AddHandler(HoursChangedEvent, value); }
            remove { RemoveHandler(HoursChangedEvent, value); }
        }

        #endregion

        #region MinutesChanged Event
        /// <summary>
        /// The ValueChangedEvent, raised if  the value changes.
        /// </summary>
        private static readonly RoutedEvent MinutesChangedEvent =
            EventManager.RegisterRoutedEvent("MinutesChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<int>), typeof(ShortTimeSpanControl));

        /// <summary>
        /// Occurs when the Value property changes.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<int> MinutesChanged
        {
            add { AddHandler(MinutesChangedEvent, value); }
            remove { RemoveHandler(MinutesChangedEvent, value); }
        }
        #endregion

        #region SecondsChanged Event
        /// <summary>
        /// The ValueChangedEvent, raised if  the value changes.
        /// </summary>
        private static readonly RoutedEvent SecondsChangedEvent =
            EventManager.RegisterRoutedEvent("SecondsChanged", RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<int>), typeof(ShortTimeSpanControl));

        /// <summary>
        /// Occurs when the Value property changes.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<int> SecondsChanged
        {
            add { AddHandler(SecondsChangedEvent, value); }
            remove { RemoveHandler(SecondsChangedEvent, value); }
        }
        #endregion
        #endregion
    }
}