using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KGySoft.ComponentModel;

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
