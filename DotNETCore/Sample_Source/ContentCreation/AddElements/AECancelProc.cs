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
