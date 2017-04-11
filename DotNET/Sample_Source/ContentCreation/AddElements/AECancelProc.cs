using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;

namespace AddElements
{
    class AECancelProc : CancelProc
    {
        public override bool Call()
        {
            return false;
        }
    }
}
