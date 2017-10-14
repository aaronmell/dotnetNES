using dotnetNES.Engine.Main;
using dotnetNES.Engine.Models;
using GalaSoft.MvvmLight.Messaging;
using System.Collections;
using System.Collections.Generic;

namespace dotnetNES.Client.ViewModel
{
    public class DebuggerViewModel : DebuggingBaseViewModel
    {        
        public Dictionary<string, Disassembly> Disassembly { get; set; }

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

            Disassembly = Engine.GetDisassembledMemory();
            RaisePropertyChanged("Disassembly");
        }

        protected override void Refresh()
        {
            //throw new System.NotImplementedException();
        }
    }
}
