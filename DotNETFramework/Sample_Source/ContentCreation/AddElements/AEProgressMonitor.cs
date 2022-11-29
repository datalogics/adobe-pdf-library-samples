using System;
using System.Collections.Generic;
using System.Text;
using Datalogics.PDFL;

namespace AddElements
{
    class AEProgressMonitor : ProgressMonitor
    {
        public int duration;
        public int currentValue;

        public override void EndOperation()
        {
            base.EndOperation();
            if (currentValue != duration)
                Console.Write("100%");
            Console.WriteLine();
            Console.Out.Flush();
        }

        public override int GetDuration()
        {
            return duration;
        }

        public override void SetDuration(int duration)
        {
            this.duration = duration;
        }

        public override int GetCurrentValue()
        {
            return currentValue;
        }

        public override void SetCurrentValue(int currentValue)
        {
            this.currentValue = currentValue;
            Console.Write(currentValue * 100 / duration);
            Console.Write("%...");
            Console.Out.Flush();
        }

        public override void SetText(string text)
        {
            base.SetText(text);
            Console.Write(text);
            Console.Write("...");
            Console.Out.Flush();
        }
    }
}
