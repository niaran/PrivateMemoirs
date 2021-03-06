﻿using System;
using DialogManagement;
using DialogManagement.Contracts;

namespace PrivateMemoirs
{
    public static class General
    {
        public enum CurrentMemoirField : byte
        {
            NONE = 0,
            MEMOIR_TEXT = 1,
            MEMOIR_DATE_CHANGE = 2,
            MEMOIR_ID = 3,
            MEMOIR_TITLE = 4
        }

        public enum TcpCommands : byte
        {
            ServerOK = 0,
            ServerFailed = 1,
            ServerLoginOK = 2,
            ServerLoginFailed = 3,
            ServerRegistrationFailed = 4,
            ServerRegistrationOK = 5,
            ServerHello = 6,
            ServerBye = 7,
            ServerGetDataResponse = 8,
            ServerGetDataMarkResponse = 9,
            ServerGetDataMarkMemoirResponse = 10,
            ClientHello = 100,
            ClientRegistrationQuery = 150,
            ClientBye = 200,
            ClientLoginQuery = 20,
            ClientGetDataQuery = 30,
            ClientMarkFieldQuery = 40,
            ClientMarkTotalMemoirQuery = 50,
            ClientMarkCurrentMemoirQuery = 55,
            ClientUpdateEndQuery = 60,
            ClientUpdateDataQuery = 70,
            ClientDeleteDataQuery = 80,
            ClientDeleteDataLastQuery = 90
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

            public void CreateQuestioningDialog(IDialogManager dialogManager, Action workerYes, Action workerNo = null)
            {
                var messageDialog = dialogManager.CreateMessageDialog(_message, _caption, DialogMode.YesNo);
                messageDialog.Yes += workerYes;
                if (workerNo != null)
                    messageDialog.No += workerNo;
                _dialogManager = messageDialog;
                messageDialog.Show();
            }
        }
    }
}