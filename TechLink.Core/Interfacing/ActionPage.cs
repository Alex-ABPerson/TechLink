using System;
using System.Collections.Generic;
using System.Text;

namespace TechLink.Core.Interfacing
{
    public class ActionPage : IPage
    {
        readonly string _title;
        readonly Action _action;

        public string Title => _title;
        public void Open()
        {
            Interface.Clear();
            _action();
        }

        public ActionPage(string title, Action action) => (_title, _action) = (title, action);
    }
}
