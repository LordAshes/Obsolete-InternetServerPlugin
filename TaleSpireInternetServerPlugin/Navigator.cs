using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Network
{
    //
    // Noseratio - https://stackoverflow.com/users/1768303/noseratio
    //

    public class Navigator
    {
        private Action<string> _callback = null;

        public Navigator(Action<string> callback)
        {
            _callback = callback;
        }

        public async Task<object> Navigate(object[] args)
        {
            using(WebBrowser _web = new WebBrowser())
            {
                _web.ScriptErrorsSuppressed = true;

                TaskCompletionSource<bool> tcs = null;
                WebBrowserDocumentCompletedEventHandler documentCompletedHandler = (s, e) =>
                    tcs.TrySetResult(true);

                // navigate to each URL in the list
                foreach (var url in args)
                {
                    tcs = new TaskCompletionSource<bool>();
                    _web.DocumentCompleted += documentCompletedHandler;
                    try
                    {
                        _web.Navigate(url.ToString());
                        // await for DocumentCompleted
                        await tcs.Task;
                    }
                    finally
                    {
                        _web.DocumentCompleted -= documentCompletedHandler;
                    }
                    // the DOM is ready
                    if (_callback != null) { _callback(_web.Document.Body.InnerHtml); }
                }
            }

            return null;
        }
    }
}
