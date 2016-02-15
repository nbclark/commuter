using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Threading;

namespace Commuter
{
    public class AsyncTask
    {
        public AsyncTask(Dispatcher dispatcher, Action action, Action<Exception> completed)
        {
            _action = action;
            _completed = completed;
            _dispatcher = dispatcher;
        }

        public AsyncTask(Action action, Action<Exception> completed)
        {
            _action = action;
            _completed = completed;
        }

        private Dispatcher _dispatcher;
        private Action _action;
        private Action<Exception> _completed;
        private Exception _exception;
        private bool _cancelled = false;
        private Thread _thread = null;

        public void Execute()
        {
            _thread = new Thread(Start);
            _thread.IsBackground = true;
            _thread.Start();
        }

        public void Cancel()
        {
            _cancelled = true;

            if (null != _thread)
            {
                try
                {
                    _thread.Abort();
                }
                catch
                {
                }
                _thread = null;
            }
        }

        public static void Execute(Dispatcher dispatcher, Action action, Action<Exception> completed)
        {
            new AsyncTask(dispatcher, action, completed).Execute();
        }

        public static void Execute(Action action, Action<Exception> completed)
        {
            new AsyncTask(action, completed).Execute();
        }

        private void Start()
        {
            try
            {
                _action();
            }
            catch (Exception ex)
            {
                _exception = ex;
            }

            if (!_cancelled)
            {
                _dispatcher.BeginInvoke(_completed, _exception);
            }
        }
    }

}