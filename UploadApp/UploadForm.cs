using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UploadApp
{
    public partial class UploadForm : Form
    {
        public UploadForm()
        {
            InitializeComponent();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
        }

        struct FtpSetting
        {
            public string Server { get; set; }

            public string UserName { get; set; }

            public string Password { get; set; }

            public string FileName { get; set; }

            public string FullName { get; set; }
        }

        FtpSetting _inputParameter;

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var fileName = ((FtpSetting)e.Argument).FileName;
                var fullName = ((FtpSetting)e.Argument).FullName;
                var userName = ((FtpSetting)e.Argument).UserName;
                var password = ((FtpSetting)e.Argument).Password;
                var server = ((FtpSetting)e.Argument).Server;

                var request = (FtpWebRequest)WebRequest.Create(new Uri($"{server}/{fileName}"));
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.Credentials = new NetworkCredential(userName, password);

                var ftpStream = request.GetRequestStream();
                var fileStream = File.OpenRead(fullName);
                var buffer = new byte[1024];
                var total = (double)fileStream.Length;
                int byteRead = 0;
                double read = 0;

                do
                {
                    if (!backgroundWorker1.CancellationPending)
                    {
                        byteRead = fileStream.Read(buffer, 0, 1024);
                        ftpStream.Write(buffer, 0, byteRead);
                        read += byteRead;
                        var percentage = read / total * 100;
                        backgroundWorker1.ReportProgress((int)percentage);
                    }
                }
                while (byteRead != 0);
                fileStream.Close();
                ftpStream.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Mensagem do Sistema", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lbStatus.Text = $"Progresso: {e.ProgressPercentage}%";
            progressBar.Value = e.ProgressPercentage;
            progressBar.Update();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lbStatus.Text = "Upload completo!";
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            try
            {
                ValidateFields();

                var fileInfo = new FileInfo(txtFileName.Text);
                _inputParameter.UserName = txtUserName.Text;
                _inputParameter.Password = txtPassword.Text;
                _inputParameter.Server = txtServer.Text;
                _inputParameter.FileName = fileInfo.Name;
                _inputParameter.FullName = fileInfo.FullName;

                backgroundWorker1.RunWorkerAsync(_inputParameter);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Mensagem do Sistema", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            try
            {
                using (var fileDialog = new OpenFileDialog { Multiselect = false, ValidateNames = true, Filter = "Todos os Arquivos|*.*" })
                {
                    if (fileDialog.ShowDialog() == DialogResult.OK)
                        txtFileName.Text = fileDialog.FileName;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Mensagem do Sistema", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ValidateFields()
        {
            if (string.IsNullOrEmpty(txtServer.Text))
                throw new Exception("Informe o servidor");

            if (string.IsNullOrEmpty(txtUserName.Text))
                throw new Exception("Informe o usuário");

            if (string.IsNullOrEmpty(txtPassword.Text))
                throw new Exception("Informe a senha");

            if (string.IsNullOrEmpty(txtFileName.Text))
                throw new Exception("Selecione um arquivo");
        }
    }
}
