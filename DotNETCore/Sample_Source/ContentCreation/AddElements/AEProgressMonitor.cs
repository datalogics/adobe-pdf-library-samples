using System;
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

        public override void SetDuration(int newDuration)
        {
            this.duration = newDuration;
        }

        public override int GetCurrentValue()
        {
            return currentValue;
        }

        public override void SetCurrentValue(int newValue)
        {
            this.currentValue = newValue;
            Console.Write(newValue * 100 / duration);
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
