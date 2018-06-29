using grapher.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace grapher.ViewModels
{
    public class GroupItemViewModel : DesignerItemViewModelBase, IObservable<GroupTransformNotification>
    {
        private double _leftOld;
        private double _topOld;

        public GroupItemViewModel()
            : base()
        {
            Left.Subscribe(l =>
            {
                var notification = new GroupTransformNotification()
                {
                    LeftChange = l - _leftOld
                };
                GroupTransformObserversOnNext(notification);
                _leftOld = l;
            });
            Top.Subscribe(t =>
            {
                var notification = new GroupTransformNotification()
                {
                    TopChange = t - _topOld
                };
                GroupTransformObserversOnNext(notification);
                _topOld = t;
            });
        }

        private void GroupTransformObserversOnNext(GroupTransformNotification notification)
        {
            observers.ForEach(x => x.OnNext(notification));
        }

        public void AddGroup(SelectableDesignerItemViewModelBase viewModel)
        {
            Subscribe(viewModel);
        }

        #region IObservable<GroupTransformNotification>

        public List<IObserver<GroupTransformNotification>> observers = new List<IObserver<GroupTransformNotification>>();

        public IDisposable Subscribe(IObserver<GroupTransformNotification> observer)
        {
            observers.Add(observer);
            return new GroupItemViewModelDisposable(this, observer);
        }

        private class GroupItemViewModelDisposable : IDisposable
        {
            private GroupItemViewModel _groupItemViewModel;
            private IObserver<GroupTransformNotification> _observer;

            public GroupItemViewModelDisposable(GroupItemViewModel groupItemViewModel, IObserver<GroupTransformNotification> observer)
            {
                _groupItemViewModel = groupItemViewModel;
                _observer = observer;
            }

            public void Dispose()
            {
                _groupItemViewModel.observers.Remove(_observer);
            }
        }

        #endregion IObservable<GroupTransformNotification>
    }
}
