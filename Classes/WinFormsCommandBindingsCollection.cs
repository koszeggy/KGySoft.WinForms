using System.Collections.Generic;
using KGySoft.ComponentModel;
using KGySoft.Controls.Classes;

namespace KGySoft.Controls
{
    internal class WinformsCollandBindingsCollection : CommandBindingsCollection
    {
        public override ICommandBinding Add(ICommand command, object source, string eventName, IDictionary<string, object> configuration, params object[] targets)
        {
            return base.Add(command, source, eventName, configuration, targets).AddStateUpdater(WinFormsStateUpdater.Updater);
        }
    }
}
