using System.Collections.Generic;

namespace KGySoft.ComponentModel
{
    public class WinformsCommandBindingsCollection : CommandBindingsCollection
    {
        public override ICommandBinding Add(ICommand command, IDictionary<string, object> initialState = null, bool disposeCommand = false)
            => base.Add(command, initialState, disposeCommand)
                .AddStateUpdater(WinFormsPropertyCommandStateUpdater.Updater)
                .AddStateUpdater(PropertyCommandStateUpdater.Updater);
    }
}
