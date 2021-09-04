using System;
using System.Collections.Generic;
using System.Text;

namespace TechLink.Core.Interfacing
{
    public interface IPage
    {
        public string Title { get; }
        public void Open();
    }
}
