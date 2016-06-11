using System;
using DialogManagement;
using DialogManagement.Contracts;

namespace PrivateMemoirEnum
{
    public static class PrivateMemoirEnum
    {
        public enum CurrentMemoirField
        {
            NONE = 0,
            MEMOIR_TEXT = 1,
            MEMOIR_TITLE = 2,
            MEMOIR_DATE_CHANGE = 3
        }

        public enum TcpCommands
        {
            ServerFailed = 1,
            ServerOK = 0,
            ServerLoginFailed = 3,
            ServerLoginOK = 2,
            ServerRegistrationFailed = 5,
            ServerRegistrationOK = 6,
            ServerHello = 7,
            ServerBye = 8,
            ServerGetDataResponse = 10,
            ClientHello = 100,
            ClientRegistrationQuery = 150,
            ClientBye = 200,
            ClientLoginQuery = 20,
            ClientGetDataQuery = 30,
            ClientMarkFieldQuery = 40,
            ClientMarkMemoirQuery = 50,
            ClientAddDataQuery = 60,
            ClientUpdateDataQuery = 70,
            ClientDeleteDataQuery = 80
        };

        public class Message
        {
            private IMessageDialog _dialogManager;
            private string _caption;
            private string _message;


            public Message(string caption, string message)
            {
                _caption = caption;
                _message = message;
            }

            public void Close()
            {
                _dialogManager.Close();
            }

            public void CreateWaitDialog(IDialogManager dialogManager, Action worker = null, Action workerReady = null)
            {
                var waitDialog = dialogManager.CreateWaitDialog(_message, DialogMode.None);
                waitDialog.Caption = _caption;
                if (workerReady != null)
                    waitDialog.WorkerReady += workerReady;
                waitDialog.CloseWhenWorkerFinished = false;
                _dialogManager = waitDialog;
                if (worker != null)
                    waitDialog.Show(worker);
                else
                    waitDialog.Show();
            }

            public void CreateMessageDialog(IDialogManager dialogManager, Action workerReady = null)
            {
                var messageDialog = dialogManager.CreateMessageDialog(_message, _caption, DialogMode.Ok);
                if (workerReady != null)
                    messageDialog.Ok += workerReady;
                _dialogManager = messageDialog;
                messageDialog.Show();
            }
        }
    }
}