using System;

namespace MagicKeyboardUtilities.App.Core
{
    public class ActionDispatcher
    {
        public void Execute(string action)
        {
            Console.WriteLine($"Executing action: {action}");
            // Switch case for built-in actions (Toggle, etc)
        }

        public void SendKey(int vk)
        {
            // P/Invoke SendInput would go here
            Console.WriteLine($"Simulating Key Press: {vk}");
        }
    }
}
