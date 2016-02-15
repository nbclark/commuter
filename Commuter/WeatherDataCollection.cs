using System.Collections.Generic;
using System.Collections.Specialized;
using Microsoft.Phone.Controls;
using System.Windows.Media;

namespace Commuter
{
    public class CommuteCollection : List<MobileSrc.Commuter.Shared.CommuteDefinition>, INotifyCollectionChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Refresh()
        {
            if (null != CollectionChanged)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public new void Add(MobileSrc.Commuter.Shared.CommuteDefinition item)
        {
            base.Add(item);

            Refresh();
        }
        public new void Remove(MobileSrc.Commuter.Shared.CommuteDefinition item)
        {
            base.Remove(item);

            Refresh();
        }
        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);

            Refresh();
        }
        public new void Insert(int index, MobileSrc.Commuter.Shared.CommuteDefinition item)
        {
            base.Insert(index, item);

            Refresh();
        }
    }

    public class CommuteDataCollection : PivotItemCollectionAdaptor<MobileSrc.Commuter.Shared.CommuteDefinition>
    {
        public CommuteDataCollection()
        {
        }

        public CommuteDataCollection(IList<MobileSrc.Commuter.Shared.CommuteDefinition> list)
            : base(list)
        {
        }

        protected override MobileSrc.Commuter.Shared.CommuteDefinition OnGetItem(PivotItem pivot)
        {
            ICommutePivotControl ctrl = (ICommutePivotControl)pivot.Content;
            return null;
        }

        protected override void OnSetItem(PivotItem pivot, MobileSrc.Commuter.Shared.CommuteDefinition item)
        {
            CommuteOverviewControl ctrl = pivot.Content as CommuteOverviewControl;

            if (null != ctrl)
            {
                ctrl.Source = item;
            }
        }

        protected override void OnCreateItem(PivotItem pivot, MobileSrc.Commuter.Shared.CommuteDefinition item)
        {
            //pivot.Title = item.Description.ToUpper();
            pivot.Header = item.Name.ToLower();

        }
    }
}
