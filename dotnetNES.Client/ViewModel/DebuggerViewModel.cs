using dotnetNES.Engine.Main;
using dotnetNES.Engine.Models;
using dotnetNES.Engine.Utilities;
using GalaSoft.MvvmLight.Messaging;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;

namespace dotnetNES.Client.ViewModel
{
    public class DebuggerViewModel : DebuggingBaseViewModel
    {
        public ObservableCollection<KeyValuePair<string,Disassembly>> Disassembly
        {
            get
            {
                if (Engine.GetDisassembledMemory() == null)
                {
                    Engine.EnableDisassembly();
                }
                return new ObservableCollection<KeyValuePair<string, Disassembly>>(Engine.GetDisassembledMemory().OrderBy(x => x.Key).ToList());
            }
        }

        protected override void LoadView(NotificationMessage<Engine.Main.Engine> engine)
        { 
            if (engine.Notification != MessageNames.LoadDebugWindow)
            {
                return;
            }

            if (Engine == null)
            {
                Engine = engine.Content;
            }

            if (Engine.GetDisassembledMemory() == null)
            {
                Engine.EnableDisassembly();
            }

            BindingOperations.EnableCollectionSynchronization(Disassembly, Engine.GetDisassemblyLock());   
            RaisePropertyChanged("Disassembly");
        }       

        protected override void Refresh()
        {
            //throw new System.NotImplementedException();
        }
    }
}
