﻿using NLog;
using NLog.Config;
using SmartQuant;
using SmartQuant.Providers;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace QuantBox.OQ.Esunny
{
    public partial class EsunnyProvider : IProvider, IDisposable
    {
        private ProviderStatus status;
        private bool isConnected;

        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event ProviderErrorEventHandler Error;

        private bool disposed;

        private static readonly Logger ehlog = LogManager.GetLogger("Esunny.H");

        public EsunnyProvider()
        {
            try
            {
                LogManager.Configuration = new XmlLoggingConfiguration(@"Bin/QuantBox.nlog");
            }
            catch(Exception ex)
            {
                ehlog.Warn(ex.Message);
            }

            timerDisconnect.Elapsed += timerDisconnect_Elapsed;

            InitCallbacks();
            InitSettings();

            status = ProviderStatus.Unknown;
            SmartQuant.Providers.ProviderManager.Add(this);
        }

        //Implement IDisposable.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.
                Shutdown();
                disposed = true;
            }
            //base.Dispose(disposing);
        }

        // Use C# destructor syntax for finalization code.
        ~EsunnyProvider()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }

        #region IProvider
        [Category(CATEGORY_INFO)]
        public byte Id
        {
            get { return 57; }//不能与已经安装的插件ID重复
        }

        [Category(CATEGORY_INFO)]
        public string Name
        {
            get { return "Esunny"; }//不能与已经安装的插件Name重复
        }

        [Category(CATEGORY_INFO)]
        public string Title
        {
            get { return "QuantBox Esunny Provider"; }
        }

        [Category(CATEGORY_INFO)]
        public string URL
        {
            get { return "www.quantbox.cn"; }
        }

        [Category(CATEGORY_INFO)]
        [Description("插件版本信息")]
        public string Version
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        }

        [Category(CATEGORY_STATUS)]
        public bool IsConnected
        {
            get { return isConnected; }
        }

        [Category(CATEGORY_STATUS)]
        public ProviderStatus Status
        {
            get { return status; }
        }

        public void Connect(int timeout)
        {
            Connect();
            ProviderManager.WaitConnected(this, timeout);
        }

        public void Connect()
        {
            _Connect();
        }

        public void Disconnect()
        {
            _Disconnect();
        }

        public void Shutdown()
        {
            Disconnect();

            SettingsChanged();

            timerDisconnect.Elapsed -= timerDisconnect_Elapsed;
        }
        
        public event EventHandler StatusChanged;

        private void ChangeStatus(ProviderStatus status)
        {
            this.status = status;
            EmitStatusChangedEvent();
        }

        private void EmitStatusChangedEvent()
        {
            if (StatusChanged != null)
            {
                StatusChanged(this, EventArgs.Empty);
            }
        }

        private void EmitConnectedEvent()
        {
            if (Connected != null)
            {
                Connected(this, EventArgs.Empty);
            }
        }

        private void EmitDisconnectedEvent()
        {
            if (Disconnected != null)
            {
                Disconnected(this, EventArgs.Empty);
            }
        }

        private void EmitError(int id, int code, string message)
        {
            if (Error != null)
                Error(new ProviderErrorEventArgs(new ProviderError(Clock.Now, this, id, code, message)));
        }
        #endregion
    }
}
