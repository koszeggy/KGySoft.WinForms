using System.Collections.Generic;
using KGySoft.ComponentModel;
using KGySoft.Controls.Classes;

namespace KGySoft.Controls
{
    internal class WinformsCollandBindingsCollection : CommandBindingsCollection
    {
        public override ICommandBinding Add(ICommand command, IDictionary<string, object> configuration, bool disposeCommand = false) 
            => base.Add(command, configuration, disposeCommand).AddStateUpdater(WinFormsStateUpdater.Updater);
    }
}
