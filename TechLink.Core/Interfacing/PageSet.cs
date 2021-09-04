using System;
using System.Collections.Generic;
using System.Text;

namespace TechLink.Core.Interfacing
{
    public abstract class PageSet : IPage
    {
        public abstract IPage[] SubPages { get; }

        public abstract string Title { get; }
        public abstract void Open();

        public void ShowMenu()
        {
            var subs = SubPages;

            Interface.WriteLine();

            for (int i = 0; i < subs.Length; i++)
            {
                Interface.SetColor(InterfaceColor.Unimportant);
                Interface.Write("  ");
                Interface.Write((i + 1).ToString());
                Interface.Write(". ");
                Interface.SetColor(InterfaceColor.Default);

                Interface.WriteLine(subs[i].Title);
            }

            Interface.WriteLine();
            Interface.Write("> ");

            string input = Interface.ReadLine();

            if (int.TryParse(input, out int res))
                Interface.VisitPage(subs[res - 1]);
            else
                Interface.ShowErrorScreen("Invalid input!");
        }
    }
}
