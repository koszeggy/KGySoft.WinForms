using System.Collections.Generic;
using KGySoft.ComponentModel;
using KGySoft.Controls.Classes;

namespace KGySoft.Controls
{
    public class WinformsCommandBindingsCollection : CommandBindingsCollection
    {
        public override ICommandBinding Add(ICommand command, IDictionary<string, object> initialState = null, bool disposeCommand = false)
            => base.Add(command, initialState, disposeCommand)
                .AddStateUpdater(WinFormsPropertyCommandStateUpdater.Updater)
                .AddStateUpdater(PropertyCommandStateUpdater.Updater);
    }
}
