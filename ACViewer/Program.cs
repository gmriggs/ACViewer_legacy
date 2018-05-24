using System;
using ACE.Diag.Network;

namespace ACViewer
{
#if WINDOWS || LINUX
    
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        public static bool UseServer = true;

        public static Client Client;
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            if (UseServer)
            {
                // connect to server
                if (!Connect()) return;
            }
            else
            {
                // local simulation

            }
            using (var game = new ACViewer(Client))
                game.Run();
        }

        public static bool Connect()
        {
            var serverIP = "127.0.0.1";
            Client = new Client();
            return Client.Connect(serverIP);
        }
    }
#endif
}
