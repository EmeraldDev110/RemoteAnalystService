using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using ConfigurationManager.Infrastructure;
using Ionic.Zip;
using RemoteAnalyst.BusinessLogic.BLL;
using RemoteAnalyst.BusinessLogic.Email;
using RemoteAnalyst.BusinessLogic.Enums;
using RemoteAnalyst.BusinessLogic.Infrastructure;
using RemoteAnalyst.BusinessLogic.RemoteAnalystServices;
using RemoteAnalyst.BusinessLogic.RemoteAnalystSPAMServices;
using RemoteAnalyst.BusinessLogic.UWSLoader;
using ConfigurationManager.Service;
using Rebex.Net;
using Microsoft.Web.Administration;
using RemoteAnalyst.BusinessLogic.Util;

namespace ConfigurationManager {
    public partial class Form1 : Form {
        private enum PMCServices { WEBSITE = 0, RELAY = 1, LOADER = 2, SCHEDULER = 3, STATICREPORT = 4, DYNAMICREPORT = 5 }
        private Dictionary<string, int> timeZoneNameIndexPair = new Dictionary<string, int>();
        private string _websiteFolderName = "Website";
        private string _relayFolderName = "Relay";
        private string _loaderFolderName = "Loader";
        private string _schedulerFolderName = "Scheduler";
        private string _staticReportFolderName = "Static Report Generator";
        private string _dynamicReportFolderName = "Dynamic Report Generator";
        private string _laConfigDirectory = @"C:\LA-Config";
        private string _laConfigXMLValueFile = @"C:\LA-Config\XMLValue.xml";
        private string _laConfigProgramDirectory = @"C:\Program Files (x86)\Hewlett-Packard Enterprise\Local Analyst\LA-Config";
		private string _laConfigXMLValueProgramFile = @"C:\Program Files (x86)\Hewlett-Packard Enterprise\Local Analyst\LA-Config\XMLValue.xml";
        private string _laConfigXMLValueUWSRelay = @"C:\LA-Config\XMLValue-MySQL.xml";

        private string _pmcConfigDirectory = @"C:\PMC-Config";
        private string _pmcConfigXMLValueFile = @"C:\PMC-Config\XMLValue.xml";
        private string _pmcConfigXMLValueUWSRelay = @"C:\PMC-Config\XMLValue-MySQL.xml";

        private string _pmcConfigProgramParentDirectory = @"C:\Program Files (x86)\Hewlett-Packard Enterprise\PMC";
        private string _pmcConfigProgramDirectory = @"C:\Program Files (x86)\Hewlett-Packard Enterprise\PMC\PMC-Config";
        private string _pmcConfigXMLValueProgramFile = @"C:\Program Files (x86)\Hewlett-Packard Enterprise\PMC\PMC-Config\XMLValue.xml";

        private bool   _copiedPreviousConfig = false;
        private string _connectionString;
        private string _connectionStringTrend;
        private string _connectionStringSPAM;
        private string _connectionStringComparative;

        private string _serverPath;
        private string _systemLocation;
        private string _zipLocation;
        private string _networkStorageLocation;

        private string _webSite;
        private string _webSiteAddress;

        private string _supportEmail;
        private string _advisorEmail;
        private string _mailTo;
        private string _emailServer;
        private string _emailPort;
        private string _emailUser;
        private string _emailPassword;
        private bool _emailSSL = true;
        private bool _emailAuthentication = true;
        private bool _isMultiServer = false;
        private string _tempDatabaseConnectionString;
        private string _masterDatabaseConnectionString;

        private int _totalPercent = 10;
        private int _currentPercent = 0;

        private bool _dbConnectionCheck = false;
        private bool _mailServerCheck = false;

        private Error _errorForm;
        private Dictionary<string, Systems> SystemList = new Dictionary<string, Systems>();
        private ToolTip tt;

		private bool isValidPassword = false;
        private Dictionary<int, bool> _servicesAvailable = new Dictionary<int, bool>();
        public Form1(string sericeName) {
            HandlePreviousVersionConfiguration();
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeComponent();
            this.Shown += new System.EventHandler(this.Form1_Shown);
        }

        #region Functions
        private void HandlePreviousVersionConfiguration()
        {
            /* Code to handle reading from L01 or L01AAA Configuration if present */
            try { 
                if (Directory.Exists(_pmcConfigDirectory)) {
                    if (!Directory.Exists(_laConfigDirectory)) { 
                        Directory.CreateDirectory(_laConfigDirectory);
                    }
                    if (File.Exists(_pmcConfigXMLValueUWSRelay)) { 
                        File.Copy(_pmcConfigXMLValueUWSRelay, _laConfigXMLValueUWSRelay);
                        File.Delete(_pmcConfigXMLValueUWSRelay);
                    }
                    if (File.Exists(_pmcConfigXMLValueFile))
                    {
                        File.Copy(_pmcConfigXMLValueFile, _laConfigXMLValueFile);
                        File.Delete(_pmcConfigXMLValueFile);
                    }
                    Directory.Delete(_pmcConfigDirectory);
                }

                if (Directory.Exists(_pmcConfigProgramDirectory))
                {
                    if (!Directory.Exists(_laConfigProgramDirectory))
                    {
                        Directory.CreateDirectory(_laConfigProgramDirectory);
                    }
                    if (File.Exists(_pmcConfigXMLValueProgramFile))
                    {
                        File.Copy(_pmcConfigXMLValueProgramFile, _laConfigXMLValueProgramFile);
                        File.Delete(_pmcConfigXMLValueProgramFile);
                    }
                    Directory.Delete(_pmcConfigProgramDirectory);
                    _copiedPreviousConfig = true;
                }

                if (Directory.Exists(_pmcConfigProgramParentDirectory))
                    Directory.Delete(_pmcConfigProgramParentDirectory);
            }
            catch (Exception e)
            {

            }
        }

        private void StartProcessing() {
            if (chbSkipEmailValidation.Checked) {
                setDefaultEmailInformation();
            }
            this.Invoke(new Action(() => progressBar1.Value = 10));
            if (ValidateForm()) {
                this.Invoke(new Action(() => progressBar1.Value = 30));
                var installLocationMessage = ValidateInstallLocation();
                if (installLocationMessage.Length == 0) {
                    var network = CheckNetworkStorage();
                    this.Invoke(new Action(() => progressBar1.Value = 70));
                    if (network) {
                        var folder = CreateFolder();
                        this.Invoke(new Action(() => progressBar1.Value = 90));
                        if (folder) {
                            _webSite = txtWebsiteName.Text.Trim();
                            _webSiteAddress = txtWebsiteName.Text.Trim();

                            try
                            {
                                _laConfigProgramDirectory = $"{txtPMCLocation.Text.Trim()}\\LA-Config";
                                _laConfigXMLValueProgramFile = $"{_laConfigProgramDirectory}\\XMLValue.xml";
                                if (!Directory.Exists(_laConfigProgramDirectory))
                                {
                                    Directory.CreateDirectory(_laConfigProgramDirectory);
                                }

                                if (_servicesAvailable.ContainsKey((int)PMCServices.DYNAMICREPORT) ||
                                    _servicesAvailable.ContainsKey((int)PMCServices.STATICREPORT) ||
                                    _servicesAvailable.ContainsKey((int)PMCServices.LOADER) ||
                                    _servicesAvailable.ContainsKey((int)PMCServices.SCHEDULER) ||
                                    _servicesAvailable.ContainsKey((int)PMCServices.WEBSITE))
                                {
                                    CreateXML();
                                }

                                else if (_servicesAvailable.ContainsKey((int)PMCServices.RELAY))
                                {
                                    CreateUWSRelayXML();
                                }

                                AnsibleVaultDecoder ansibleVaultDecoder = new AnsibleVaultDecoder();
                                ansibleVaultDecoder.ConvertXMLValueToAnsibleVault(_laConfigXMLValueProgramFile, _laConfigProgramDirectory, "vaultPassword");
                                this.Invoke(new Action(() => MessageBox.Show(this, "XML file created", "", MessageBoxButtons.OK, MessageBoxIcon.Information)));
                                this.Invoke(new Action(() => this.Close()));
                            }
                            catch (Exception ex)
                            {
                                this.Invoke(new Action(() => MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Information)));
                                this.Invoke(new Action(() => this.progressBar1.Visible = false));
                                this.Invoke(new Action(() => this.btnSubmit.Enabled = true));
                            }
                            return;
                        } else {
                            this.Invoke(new Action(() => MessageBox.Show(this, "Unable to access FTP Location: " + txtFTPLocation.Text.Trim(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Information)));
                        }
                    } else {
                        this.Invoke(new Action(() => MessageBox.Show(this, "Unable to access Network Location: " + txtNetworkStorageLocation.Text.Trim(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Information)));
                    }
                } else {
                    this.Invoke(new Action(() => MessageBox.Show(this, installLocationMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Information)));
                }

            }

            this.Invoke(new Action(() => this.progressBar1.Visible = false));
            this.Invoke(new Action(() => this.btnSubmit.Enabled = true));
    
        }
        
        private string ValidateInstallLocation() {
            string message = "";

            if (!Directory.Exists(txtPMCLocation.Text.Trim())) {
                message += $"Install directory {txtPMCLocation.Text.Trim()} does not exist.\n\nPlease ensure the install location matches the path where you initially installed Local Analyst. \n";
            } else {
                if (_servicesAvailable.ContainsKey((int)PMCServices.WEBSITE) && !Directory.Exists(txtPMCLocation.Text.Trim() + "\\" + _websiteFolderName)) {
                    message += $"{txtPMCLocation.Text.Trim()}\\{_websiteFolderName} was not found.\n\n";
                }

                if (_servicesAvailable.ContainsKey((int)PMCServices.RELAY) && !Directory.Exists(txtPMCLocation.Text.Trim() + "\\" + _relayFolderName)) {
                    message += $"{txtPMCLocation.Text.Trim()}\\{_relayFolderName} was not found.\n\n";
                }

                if (_servicesAvailable.ContainsKey((int)PMCServices.LOADER) && !Directory.Exists(txtPMCLocation.Text.Trim() + "\\" + _loaderFolderName)) {
                    message += $"{txtPMCLocation.Text.Trim()}\\{_loaderFolderName} was not found.\n\n";
                }

                if (_servicesAvailable.ContainsKey((int)PMCServices.SCHEDULER) && !Directory.Exists(txtPMCLocation.Text.Trim() + "\\" + _schedulerFolderName)) {
                    message += $"{txtPMCLocation.Text.Trim()}\\{_schedulerFolderName} was not found.\n\n";
                }

                if (_servicesAvailable.ContainsKey((int)PMCServices.STATICREPORT) && !Directory.Exists(txtPMCLocation.Text.Trim() + "\\" + _staticReportFolderName)) {
                    message += $"{txtPMCLocation.Text.Trim()}\\{_staticReportFolderName} was not found.\n\n";
                }

                if (_servicesAvailable.ContainsKey((int)PMCServices.DYNAMICREPORT) && !Directory.Exists(txtPMCLocation.Text.Trim() + "\\" + _dynamicReportFolderName)) {
                    message += $"{txtPMCLocation.Text.Trim()}\\{_dynamicReportFolderName} was not found.\n\n";
                }

                if (message.Length > 0) {
                    message += "One or more components could not be found. Please ensure Local Analyst was installed correctly.\n\n";
                }
            }

            return message;
        }

        private bool ValidateForm() {
            if (txtDatabaseDomainName.Text.Length == 0) {
                MessageBox.Show(this, "Please enter Database IP/Domain Name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtDatabaseDomainName.Focus();
                return false;
            }
            if (txtDatabasePort.Text.Length == 0) {
                MessageBox.Show(this, "Please enter Database Port", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtDatabasePort.Focus();
                return false;
            }
            if (txtDatabaseUserName.Text.Length == 0) {
                MessageBox.Show(this, "Please enter Database User Name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtDatabaseUserName.Focus();
                return false;
            }
            if (txtDatabasePassword.Text.Length == 0) {
                MessageBox.Show(this, "Please enter Database Password", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtDatabasePassword.Focus();
                return false;
            }

            if (!chbSkipEmailValidation.Checked) { 
                if (txtMailFromAccount.Text.Length == 0) {
                    this.Invoke(new Action(() => MessageBox.Show(this, "Please enter Email Address", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop)));
                    this.Invoke(new Action(() => txtMailFromAccount.Focus()));
                    return false;
                }
                if (txtEmailServer.Text.Length == 0)
                {
                    this.Invoke(new Action(() => MessageBox.Show(this, "Please enter Email Server", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop)));
                    this.Invoke(new Action(() => txtEmailServer.Focus()));
                    return false;
                }
                if (txtEmailPort.Text.Length == 0) {
                    this.Invoke(new Action(() => MessageBox.Show(this, "Please enter Email Port", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop)));
                    this.Invoke(new Action(() => txtEmailPort.Focus()));
                    return false;
                
                }
            }
            //if (txtTestEmailUser.Text.Length == 0) {
            //    MessageBox.Show(this, "Please enter a Test Email", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            //    txtTestEmailUser.Focus();
            //    return false;
            //}
            // Remove requirement for the email password because some email servers don't require password
            //if (txtEmailPassword.Text.Length == 0) {
            //    MessageBox.Show(this, "Please enter Email User Password", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            //    txtEmailPassword.Focus();
            //    return false;
            //}


            if (txtPMCLocation.Text.Length == 0) {
                this.Invoke(new Action(() => MessageBox.Show(this, "Please select Local Analyst Folder Location", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop)));
                this.Invoke(new Action(() => txtPMCLocation.Focus()));
                return false;
            }
            if (txtWebsiteName.Text.Length == 0) {
                this.Invoke(new Action(() => MessageBox.Show(this, "Please enter Website IP/Domain Name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop)));
                this.Invoke(new Action(() => txtWebsiteName.Focus()));
                return false;
            }

            if (txtWebsiteName.Text.ToLower() == "localhost") {
                this.Invoke(new Action(() => MessageBox.Show(this, "localhost is not supported. Please enter a valid IP address or web domain name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop)));
                this.Invoke(new Action(() => txtWebsiteName.Focus()));
                return false;
            }

            if ((!_servicesAvailable.ContainsKey((int)PMCServices.SCHEDULER) && _servicesAvailable.Count == 1) || _servicesAvailable.Count > 1) {
                if (txtNetworkStorageLocation.Text.Length == 0) {
                    if (_isMultiServer) {
                        this.Invoke(new Action(() => MessageBox.Show(this, "Please enter a valid network storage location.\n\nEx:  C:\\Local Analyst Network Storage", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop)));
                        this.Invoke(new Action(() => txtNetworkStorageLocation.Focus()));
                    } else {
                        this.Invoke(new Action(() => MessageBox.Show(this, "Please enter a valid network storage location.\n\nEx:  \\\\10.0.0.1\\Local Analyst Network Storage", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop)));
                        this.Invoke(new Action(() => txtNetworkStorageLocation.Focus()));
                    }
                    this.Invoke(new Action(() =>txtNetworkStorageLocation.BackColor = System.Drawing.Color.Red));
                    return false;
                } else {
                    var ValidSSFilePath = @"^([a-zA-Z]\:)(\\[A-Za-z0-9-_ ()]+){1,}(\\?)$";
                    var ValidSS2FilePath = @"^([a-zA-Z]\:\\)$";
                    var ValidMSFilePath = @"^(\\\\[A-Za-z0-9-._]+)(\\[A-Za-z0-9-._ ()]+){1,}(\\?)$";

                    Regex SSRegex = new Regex(ValidSSFilePath);
                    Regex SS2Regex = new Regex(ValidSS2FilePath);
                    Regex MSRegex = new Regex(ValidMSFilePath);

                    Match SSMatch = SSRegex.Match(txtNetworkStorageLocation.Text);
                    Match SS2Match = SS2Regex.Match(txtNetworkStorageLocation.Text);
                    Match MSMatch = MSRegex.Match(txtNetworkStorageLocation.Text);

                    if (!_isMultiServer && !SSMatch.Success && !SS2Match.Success && !MSMatch.Success) {
                        this.Invoke(new Action(() => MessageBox.Show(this, "Please enter a valid network storage location.\n\nEx:  C:\\Local Analyst Network Storage", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop)));
                        this.Invoke(new Action(() => txtNetworkStorageLocation.Focus()));
                        this.Invoke(new Action(() => txtNetworkStorageLocation.BackColor = System.Drawing.Color.Red));
                        return false;
                    } else if (_isMultiServer && !MSMatch.Success) {
                        this.Invoke(new Action(() => MessageBox.Show(this, "Please enter a valid network storage location.\n\nEx:  \\\\10.0.0.1\\Local Analyst Network Storage", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop)));
                        this.Invoke(new Action(() => txtNetworkStorageLocation.Focus()));
                        this.Invoke(new Action(() => txtNetworkStorageLocation.BackColor = System.Drawing.Color.Red));
                        return false;
                    }
                }
            }



            if (_servicesAvailable.ContainsKey((int)PMCServices.RELAY)) {
                if (txtFTPLocation.Text.Length == 0) {
                    this.Invoke(new Action(() => MessageBox.Show(this, "Please enter FTP Location.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop)));
                    this.Invoke(new Action(() => btnFTPLocation.Focus()));
                    return false;
                }
            }

            return true;
        }

        private bool ValidateDatabase() {
            if (txtDatabaseDomainName.Text.Length == 0) {
                MessageBox.Show(this, "Please enter Database IP/Domain Name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtDatabaseDomainName.Focus();
                return false;
            }
            if (txtDatabasePort.Text.Length == 0) {
                MessageBox.Show(this, "Please enter Database Port", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtDatabasePort.Focus();
                return false;
            }
            if (txtDatabaseUserName.Text.Length == 0) {
                MessageBox.Show(this, "Please enter Database User Name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtDatabaseUserName.Focus();
                return false;
            }
            if (txtDatabasePassword.Text.Length == 0) {
                MessageBox.Show(this, "Please enter Database Password", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtDatabasePassword.Focus();
                return false;
            }

            return true;
        }

        private bool ValidateMail() {
            if (txtMailFromAccount.Text.Length == 0) {
                MessageBox.Show(this, "Please enter Email Address", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtMailFromAccount.Focus();
                return false;
            }
            if (txtEmailServer.Text.Length == 0) {
                MessageBox.Show(this, "Please enter Email Server", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtEmailServer.Focus();
                return false;
            }
            if (txtEmailPort.Text.Length == 0) {
                MessageBox.Show(this, "Please enter Email Port", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtEmailPort.Focus();
                return false;
            }
            if (txtTestEmailUser.Text.Length == 0) {
                MessageBox.Show(this, "Please enter a Test Email", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtTestEmailUser.Focus();
                return false;
            }

            return true;
        }

        private bool CheckMySQLConnection() {
            var pass = true;
            var server = txtDatabaseDomainName.Text.Trim();
            var port = txtDatabasePort.Text.Trim();
            var uId = txtDatabaseUserName.Text.Trim();
            var password = txtDatabasePassword.Text.Trim();
            progressBar1.Value = 40;

            var connectionString = "SERVER=" + server + ";PORT=" + port + ";DATABASE=pmc;UID=" + uId + ";PASSWORD=" + password + ";";
            var databaseMapping = new DatabaseMappingService(connectionString);
            progressBar1.Value = 60;
            try {
                var isConnect = databaseMapping.CheckConnectionFor(connectionString);
                progressBar1.Value = 90;

                if (isConnect.Length != 0) {
                    MessageBox.Show(this, "Failed to connect to MySQL at " + server + " with user " + uId + Environment.NewLine + isConnect, "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    txtDatabaseDomainName.Focus();
                    pass = false;
                    progressBar1.Value = 0;
                }
                progressBar1.Value = 100;
            }
            catch (Exception ex) {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtDatabaseDomainName.Focus();
                pass = false;
                progressBar1.Value = 0;
            }

            _connectionString = "SERVER=" + server + ";PORT=" + port + ";DATABASE=pmc;UID=" + uId + ";PASSWORD=" + password + ";";
            _connectionStringTrend = "SERVER=" + server + ";PORT=" + port + ";DATABASE=pmc;UID=" + uId + ";PASSWORD=" + password + ";";
            _connectionStringSPAM = "SERVER=" + server + ";PORT=" + port + ";DATABASE=pmc;UID=" + uId + ";PASSWORD=" + password + ";";
            _connectionStringComparative = "SERVER=" + server + ";PORT=" + port + ";DATABASE=pmccomparision;UID=" + uId + ";PASSWORD=" + password + ";";



            _tempDatabaseConnectionString = "SERVER=" + server + ";PORT=" + port + ";DATABASE=pmcSERIALNUMBER;UID=" + uId + ";PASSWORD=" + password + ";";
            _masterDatabaseConnectionString = "SERVER=" + server + ";PORT=" + port + ";DATABASE=sys;UID=" + uId + ";PASSWORD=" + password + ";";

            return pass;
        }

        private bool CheckEmailServer() {
            var isNumeric = !string.IsNullOrEmpty(txtEmailPort.Text.Trim()) && txtEmailPort.Text.Trim().All(Char.IsDigit);
            if (!isNumeric) {
                MessageBox.Show(this, "Email Port Invalid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtEmailPort.Focus();
                return false;
            }

            var pass = true;
            var advisorEmail = txtMailFromAccount.Text.Trim();
            var supportEmail = txtMailFromAccount.Text.Trim();

            var emailServer = txtEmailServer.Text.Trim();
            var emailPort = Convert.ToInt32(txtEmailPort.Text.Trim());
            var testEmail = txtTestEmailUser.Text.Trim();

            //var isSSL = cbSSL.Text == "SSL";
            var isSSL = chbIsSSL.Checked;
            var isAuth = true;
            this.Invoke((MethodInvoker)delegate () {
                isSSL = chbIsSSL.Checked;
                isAuth = chbEmailAuth.Checked;
            });

            progressBar1.Value = 30;

            //var emailUser = txtEmailUser.Text.Trim();
            var emailUser = txtMailFromAccount.Text.Trim();
            var emailPassword = txtEmailPassword.Text;
            var isWelcomeMessage = true;
#if (!DEBUG)
            string serverPath = AppContext.BaseDirectory; // e.g. C:\\Program Files (x86)\\Local Analyst\\Local Analyst Services\\Configuration\\
#else
            string serverPath = "C:\\Users\\Idel3\\source\\repos\\RemoteAnalyst Service Git\\";
#endif
            serverPath = serverPath.Substring(0, serverPath.LastIndexOf("\\"));
            serverPath = serverPath.Substring(0, serverPath.LastIndexOf("\\"));
            try {

                progressBar1.Value = 70;
                // Mailgun parameters are null since, LA will not use it
                var emailToSupport = new EmailToSupport(advisorEmail, supportEmail, "", 
                                                        emailServer, emailPort, emailUser, emailPassword, isAuth,
                                                        "", serverPath, isSSL, true, null, null);
                emailToSupport.SendPmcTestEmail(testEmail);
            }
            catch (Exception ex) {
                try {
                    progressBar1.Value = 80;
                    isWelcomeMessage = false;
                    // Mailgun parameters are null since, LA will not use it
                    var emailToSupportSecondTry = new EmailToSupport(advisorEmail, supportEmail, "", 
                                                        emailServer, emailPort, emailUser, emailPassword, isAuth,
                                                        "", serverPath, isSSL, true, null, null);
                    emailToSupportSecondTry.SendPmcTestEmail(testEmail);
                }
                catch (Exception exep) {
                    MessageBox.Show(this, "Cannot connect to the Mail Server", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    txtMailFromAccount.Focus();
                    pass = false;
                    progressBar1.Value = 0;
                }
            }

            progressBar1.Value = 100;
            _supportEmail = supportEmail;
            _advisorEmail = advisorEmail;
            _mailTo = "mailto:" + supportEmail;
            _emailServer = emailServer;
            _emailPort = emailPort.ToString();
            _emailUser = supportEmail;
            _emailPassword = emailPassword;
            _emailAuthentication = isAuth;
            _emailSSL = isSSL;
            return pass;
        }

        private bool CreateFolder() {
            var pass = true;
            try {
                //Create default Dir.
                if (!Directory.Exists(txtPMCLocation.Text.Trim()))
                    Directory.CreateDirectory(txtPMCLocation.Text.Trim());

                //Create Log Dir.
                var logDir = txtPMCLocation.Text + @"\Logs\";
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                //Create temp Dir.
                var tempDir = txtPMCLocation.Text + @"\TempImg\";
                if (!Directory.Exists(tempDir))
                    Directory.CreateDirectory(tempDir);

                //Create System Dir.
                var systemDir = txtPMCLocation.Text + @"\Systems\";
                if (!Directory.Exists(systemDir))
                    Directory.CreateDirectory(systemDir);
                foreach (var item in SystemList) {
                    var systemSubDir = txtPMCLocation.Text + @"\Systems\" + item.Value.SystemSerial + @"\";
                    if (!Directory.Exists(systemSubDir))
                        Directory.CreateDirectory(systemSubDir);
                }


                //Create ZIP Dir.
                var uwsDir = txtPMCLocation.Text + @"\UWS\";
                if (!Directory.Exists(uwsDir))
                    Directory.CreateDirectory(uwsDir);


                if (_servicesAvailable.ContainsKey((int)PMCServices.RELAY)) {
                    //Create FTP Dir.
                    var ftpLocation = txtFTPLocation.Text;
                    if (!Directory.Exists(ftpLocation))
                        Directory.CreateDirectory(ftpLocation);

                    //Create System Folder.
                    var ftpSystemLocation = txtFTPLocation.Text + @"\Systems\";
                    if (!Directory.Exists(ftpSystemLocation))
                        Directory.CreateDirectory(ftpSystemLocation);
                    //Create Log Folder.
                    var ftpLogLocation = txtFTPLocation.Text + @"\Logs\";
                    if (!Directory.Exists(ftpLogLocation))
                        Directory.CreateDirectory(ftpLogLocation);

                    //Create ZIP Dir.
                    var ftpZipLocation = txtFTPLocation.Text + @"\UWS\";
                    if (!Directory.Exists(ftpZipLocation))
                        Directory.CreateDirectory(ftpZipLocation);

                    //Create sub System Folder.
                    foreach (var item in SystemList) {
                        var systemSubDir = ftpSystemLocation + item.Value.SystemSerial + @"\";
                        if (!Directory.Exists(systemSubDir))
                            Directory.CreateDirectory(systemSubDir);
                    }
                }

                _serverPath = txtPMCLocation.Text.Trim() + @"\";
                _systemLocation = txtPMCLocation.Text + @"\Systems\";
                _zipLocation = txtPMCLocation.Text + @"\UWS\";;
            }
            catch (Exception ex) {
                pass = false;
            }

            return pass;
        }

        private bool CheckNetworkStorage() {
            if (_servicesAvailable.ContainsKey((int)PMCServices.SCHEDULER) && _servicesAvailable.Count == 1) return true;

            var pass = true;
            var fileInfo = new FileInfo(System.Windows.Forms.Application.ExecutablePath);
            //Create dummy file and move the file to the folder.
            var writer = new StreamWriter(fileInfo.DirectoryName + "\\test.txt");
            writer.WriteLine("test");
            
            writer.Close();

            var networkLocation = txtNetworkStorageLocation.Text.Trim();

            try {
                
                if (!Directory.Exists(networkLocation))
                    Directory.CreateDirectory(networkLocation);

                File.Move(fileInfo.DirectoryName + "\\test.txt", networkLocation + "\\test.txt");
                File.Delete(networkLocation + "\\test.txt");
            }
            catch (Exception ex) {
                pass = false;
            }

            if (networkLocation.EndsWith("\\"))
                networkLocation = networkLocation.Remove(networkLocation.Length - 1, 1);

            _networkStorageLocation = networkLocation + "\\";

            return pass;
        }

        private void CreateXML() {
            if (!Directory.Exists(_laConfigDirectory))
                Directory.CreateDirectory(_laConfigDirectory);

            if (File.Exists(_laConfigXMLValueFile))
                File.Delete(_laConfigXMLValueFile);

            if (!Directory.Exists(_laConfigProgramDirectory))
                Directory.CreateDirectory(_laConfigProgramDirectory);

            if (File.Exists(_laConfigXMLValueProgramFile))
                File.Delete(_laConfigXMLValueProgramFile);

            var encrypt = new Decrypt();

            XmlTextWriter writer = new XmlTextWriter(_laConfigXMLValueFile, System.Text.Encoding.UTF8);
            #region Core Value
            writer.WriteStartDocument(true);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 2;
            writer.WriteStartElement("PMC", "urn:PMC-schema");

            #region Database

            //ConnectionStringDB
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("IsWebsiteUpdate");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            if (_copiedPreviousConfig)
            {
                writer.WriteString("false");
            }
            else
            {
                writer.WriteString("true");
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
            

            //ConnectionStringDB
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("ConnectionStringDB");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_connectionString));
            writer.WriteEndElement();
            writer.WriteEndElement();
            

            //ConnectionStringTrend
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("ConnectionStringTrend");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_connectionStringTrend));
            writer.WriteEndElement();
            writer.WriteEndElement();
            


            //ConnectionStringSPAM
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("ConnectionStringSPAM");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_connectionStringSPAM));
            writer.WriteEndElement();
            writer.WriteEndElement();
            


            //ConnectionStringComparative
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("ConnectionStringComparative");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_connectionStringComparative));
            writer.WriteEndElement();
            writer.WriteEndElement();
            

            #endregion

            //ServerPath
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("ServerPath");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_serverPath));
            writer.WriteEndElement();
            writer.WriteEndElement();

            //SystemLocation
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("SystemLocation");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_systemLocation));
            writer.WriteEndElement();
            writer.WriteEndElement();

            //FTPLocation
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("FTPLocation");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(txtFTPLocation.Text));
            writer.WriteEndElement();
            writer.WriteEndElement();

            //FTPSystemLocation
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("FTPSystemLocation");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            var ftpSystemLocation = txtFTPLocation.Text + @"\Systems\";
            writer.WriteString(encrypt.strDESEncrypt(ftpSystemLocation));
            writer.WriteEndElement();
            writer.WriteEndElement();

            //ZIPLocation
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("ZIPLocation");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_zipLocation));
            writer.WriteEndElement();
            writer.WriteEndElement();
            

            //WebSite
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("WebSite");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_webSite));
            writer.WriteEndElement();
            writer.WriteEndElement();
            

            //WebsiteAddress.
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("WebSiteAddress");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_webSiteAddress));
            writer.WriteEndElement();
            writer.WriteEndElement();
            

            #region email


            //SupportEmail
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("SupportEmail");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_supportEmail));
            writer.WriteEndElement();
            writer.WriteEndElement();
            
            //AdvisorEmail
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("AdvisorEmail");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_advisorEmail));
            writer.WriteEndElement();
            writer.WriteEndElement();
            
            //MailTo
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("MailTo");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_mailTo));
            writer.WriteEndElement();
            writer.WriteEndElement();
            
            //EmailServer
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("EmailServer");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_emailServer));
            writer.WriteEndElement();
            writer.WriteEndElement();
            
            //EmailPort
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("EmailPort");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_emailPort));
            writer.WriteEndElement();
            writer.WriteEndElement();
            
            //EmailUser
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("EmailUser");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_emailUser));
            writer.WriteEndElement();
            writer.WriteEndElement();
            
            //EmailPassword
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("EmailPassword");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_emailPassword));
            writer.WriteEndElement();
            writer.WriteEndElement();
            
            //EmailAuthentication
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("EmailAuthentication");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(_emailAuthentication ? encrypt.strDESEncrypt("true") : encrypt.strDESEncrypt("false"));
            writer.WriteEndElement();
            writer.WriteEndElement();
            
            //IsSSL
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("EmailIsSSL");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(_emailSSL ? encrypt.strDESEncrypt("true") : encrypt.strDESEncrypt("false"));
            writer.WriteEndElement();
            writer.WriteEndElement();
            

            #endregion

            //AWS
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("AWS");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt("false"));
            writer.WriteEndElement();
            writer.WriteEndElement();
            

            //TempDatabaseConnectionString
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("TempDatabaseConnectionString");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_tempDatabaseConnectionString));
            writer.WriteEndElement();
            writer.WriteEndElement();
            
            //MasterDatabaseConnectionString
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("MasterDatabaseConnectionString");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_masterDatabaseConnectionString));
            writer.WriteEndElement();
            writer.WriteEndElement();
            

            // Do not include if the only service installed is scheduler.
            if (_servicesAvailable.Count == 1 && !_servicesAvailable.ContainsKey((int)PMCServices.SCHEDULER) || _servicesAvailable.Count > 1) {
                //NetworkStorageLocation
                writer.WriteStartElement("Element");
                writer.WriteStartElement("ID");
                writer.WriteString("NetworkStorageLocation");
                writer.WriteEndElement();
                writer.WriteStartElement("Value");
                writer.WriteString(encrypt.strDESEncrypt(_networkStorageLocation));
                writer.WriteEndElement();
                writer.WriteEndElement();
                
            }

            //DatabasePrefix
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("DatabasePrefix");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt("pmc"));
            writer.WriteEndElement();
            writer.WriteEndElement();
            
            
            //IsLocalAnalyst
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("IsLocalAnalyst");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt("true"));
            writer.WriteEndElement();
            writer.WriteEndElement();
            

            #endregion


            if (_servicesAvailable.ContainsKey((int)PMCServices.WEBSITE)) {
                #region System Info.

                writer.WriteStartElement("Element");
                writer.WriteStartElement("ID");
                writer.WriteString("SystemCount");
                writer.WriteEndElement();
                writer.WriteStartElement("Value");
                writer.WriteString(encrypt.strDESEncrypt(SystemList.Count.ToString()));
                writer.WriteEndElement();
                writer.WriteEndElement();
                

                var systemCounter = 0;
                foreach (var sys in SystemList) {
                    writer.WriteStartElement("Element");
                    writer.WriteStartElement("ID");
                    writer.WriteString("SystemName_" + systemCounter);
                    writer.WriteEndElement();
                    writer.WriteStartElement("Value");
                    writer.WriteString(encrypt.strDESEncrypt(sys.Value.SystemName));
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    

                    writer.WriteStartElement("Element");
                    writer.WriteStartElement("ID");
                    writer.WriteString("SystemSerial_" + systemCounter);
                    writer.WriteEndElement();
                    writer.WriteStartElement("Value");
                    writer.WriteString(encrypt.strDESEncrypt(sys.Value.SystemSerial));
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    

                    writer.WriteStartElement("Element");
                    writer.WriteStartElement("ID");
                    writer.WriteString("SystemTimeZone_" + systemCounter);
                    writer.WriteEndElement();
                    writer.WriteStartElement("Value");
                    writer.WriteString(encrypt.strDESEncrypt(sys.Value.TimeZone));
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    

                    writer.WriteStartElement("Element");
                    writer.WriteStartElement("ID");
                    writer.WriteString("SystemMeasFH_" + systemCounter);
                    writer.WriteEndElement();
                    writer.WriteStartElement("Value");
                    writer.WriteString(encrypt.strDESEncrypt(sys.Value.MeashFH));
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    

                    writer.WriteStartElement("Element");
                    writer.WriteStartElement("ID");
                    writer.WriteString("SystemLocation_" + systemCounter);
                    writer.WriteEndElement();
                    writer.WriteStartElement("Value");
                    writer.WriteString(encrypt.strDESEncrypt(sys.Value.Location));
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    

                    ///////////////
                    writer.WriteStartElement("Element");
                    writer.WriteStartElement("ID");
                    writer.WriteString("NonStopIP_" + systemCounter);
                    writer.WriteEndElement();
                    writer.WriteStartElement("Value");
                    writer.WriteString(encrypt.strDESEncrypt(sys.Value.NonStopIP));
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    


                    writer.WriteStartElement("Element");
                    writer.WriteStartElement("ID");
                    writer.WriteString("MonitorPort_" + systemCounter);
                    writer.WriteEndElement();
                    writer.WriteStartElement("Value");
                    writer.WriteString(encrypt.strDESEncrypt(sys.Value.MonitorPort));
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    

                    writer.WriteStartElement("Element");
                    writer.WriteStartElement("ID");
                    writer.WriteString("StoreVolume_" + systemCounter);
                    writer.WriteEndElement();
                    writer.WriteStartElement("Value");
                    writer.WriteString(encrypt.strDESEncrypt(sys.Value.StoreVolume));
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    

                    writer.WriteStartElement("Element");
                    writer.WriteStartElement("ID");
                    writer.WriteString("FTPUserName_" + systemCounter);
                    writer.WriteEndElement();
                    writer.WriteStartElement("Value");
                    writer.WriteString(encrypt.strDESEncrypt(sys.Value.FtpUserName));
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    

                    writer.WriteStartElement("Element");
                    writer.WriteStartElement("ID");
                    writer.WriteString("FTPPassword_" + systemCounter);
                    writer.WriteEndElement();
                    writer.WriteStartElement("Value");
                    writer.WriteString(encrypt.strDESEncrypt(sys.Value.FtpPassword));
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    

                    writer.WriteStartElement("Element");
                    writer.WriteStartElement("ID");
                    writer.WriteString("FTPPort_" + systemCounter);
                    writer.WriteEndElement();
                    writer.WriteStartElement("Value");
                    writer.WriteString(encrypt.strDESEncrypt(sys.Value.FtpPort));
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    

                    writer.WriteStartElement("Element");
                    writer.WriteStartElement("ID");
                    writer.WriteString("FTPMeasFhVolume_" + systemCounter);
                    writer.WriteEndElement();
                    writer.WriteStartElement("Value");
                    writer.WriteString(encrypt.strDESEncrypt(sys.Value.MeasFhVolume));
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    

                    writer.WriteStartElement("Element");
                    writer.WriteStartElement("ID");
                    writer.WriteString("MeasFhSubVolume_" + systemCounter);
                    writer.WriteEndElement();
                    writer.WriteStartElement("Value");
                    writer.WriteString(encrypt.strDESEncrypt(sys.Value.MeasFhSubVolume));
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    
                    systemCounter++;
                }

                #endregion

                #region User Info.
                writer.WriteStartElement("Element");
                writer.WriteStartElement("ID");
                writer.WriteString("UserFirstName");
                writer.WriteEndElement();
                writer.WriteStartElement("Value");
                writer.WriteString(encrypt.strDESEncrypt(txtUserFirstName.Text.Trim()));
                writer.WriteEndElement();
                writer.WriteEndElement();
                

                writer.WriteStartElement("Element");
                writer.WriteStartElement("ID");
                writer.WriteString("UserLastName");
                writer.WriteEndElement();
                writer.WriteStartElement("Value");
                writer.WriteString(encrypt.strDESEncrypt(txtUserLastName.Text.Trim()));
                writer.WriteEndElement();
                writer.WriteEndElement();
                

                writer.WriteStartElement("Element");
                writer.WriteStartElement("ID");
                writer.WriteString("UserEmailAddress");
                writer.WriteEndElement();
                writer.WriteStartElement("Value");
                writer.WriteString(encrypt.strDESEncrypt(txtUserEmail.Text.Trim()));
                writer.WriteEndElement();
                writer.WriteEndElement();
                

                writer.WriteStartElement("Element");
                writer.WriteStartElement("ID");
                writer.WriteString("UserPassword");
                writer.WriteEndElement();
                writer.WriteStartElement("Value");
                writer.WriteString(encrypt.strDESEncrypt(txtUserPassword.Text.Trim()));
                writer.WriteEndElement();
                writer.WriteEndElement();
                
                #endregion

                #region Company Info.

                writer.WriteStartElement("Element");
                writer.WriteStartElement("ID");
                writer.WriteString("CompanyName");
                writer.WriteEndElement();
                writer.WriteStartElement("Value");
                writer.WriteString(encrypt.strDESEncrypt(txtCompanyName.Text.Trim()));
                writer.WriteEndElement();
                writer.WriteEndElement();
                

                writer.WriteStartElement("Element");
                writer.WriteStartElement("ID");
                writer.WriteString("CompanyAddress1");
                writer.WriteEndElement();
                writer.WriteStartElement("Value");
                writer.WriteString(encrypt.strDESEncrypt(txtCompanyAddress1.Text.Trim()));
                writer.WriteEndElement();
                writer.WriteEndElement();
                

                writer.WriteStartElement("Element");
                writer.WriteStartElement("ID");
                writer.WriteString("CompanyAddress2");
                writer.WriteEndElement();
                writer.WriteStartElement("Value");
                writer.WriteString(encrypt.strDESEncrypt(txtCompanyAddress2.Text.Trim()));
                writer.WriteEndElement();
                writer.WriteEndElement();
                

                writer.WriteStartElement("Element");
                writer.WriteStartElement("ID");
                writer.WriteString("CompanyCity");
                writer.WriteEndElement();
                writer.WriteStartElement("Value");
                writer.WriteString(encrypt.strDESEncrypt(txtCompanyCity.Text.Trim()));
                writer.WriteEndElement();
                writer.WriteEndElement();
                

                writer.WriteStartElement("Element");
                writer.WriteStartElement("ID");
                writer.WriteString("CompanyState");
                writer.WriteEndElement();
                writer.WriteStartElement("Value");
                writer.WriteString(encrypt.strDESEncrypt(txtCompanyState.Text.Trim()));
                writer.WriteEndElement();
                writer.WriteEndElement();
                

                writer.WriteStartElement("Element");
                writer.WriteStartElement("ID");
                writer.WriteString("CompanyPostalCode");
                writer.WriteEndElement();
                writer.WriteStartElement("Value");
                writer.WriteString(encrypt.strDESEncrypt(txtCompanyPostalCode.Text.Trim()));
                writer.WriteEndElement();
                writer.WriteEndElement();
                

                writer.WriteStartElement("Element");
                writer.WriteStartElement("ID");
                writer.WriteString("CompanyCountry");
                writer.WriteEndElement();
                writer.WriteStartElement("Value");
                var countrySelectedValue = "";
                this.Invoke((MethodInvoker)delegate () {
                    countrySelectedValue = cbCompanyCountry.SelectedValue.ToString();
                });

                writer.WriteString(encrypt.strDESEncrypt(countrySelectedValue));
                writer.WriteEndElement();
                writer.WriteEndElement();
                

                writer.WriteStartElement("Element");
                writer.WriteStartElement("ID");
                writer.WriteString("CompanyContactNumber");
                writer.WriteEndElement();
                writer.WriteStartElement("Value");
                writer.WriteString(encrypt.strDESEncrypt(txtContactNumber.Text.Trim()));
                writer.WriteEndElement();
                writer.WriteEndElement();
                
                #endregion

            }



            writer.WriteEndDocument();
            writer.Close();

            //Copy the writer to new location.
            var fileInfo = new FileInfo(_laConfigXMLValueFile);
            fileInfo.CopyTo(_laConfigXMLValueProgramFile);
        }

        private void CreateUWSRelayXML()
        {
            if (!Directory.Exists(_laConfigDirectory))
                Directory.CreateDirectory(_laConfigDirectory);

            if (File.Exists(_laConfigXMLValueFile))
                File.Delete(_laConfigXMLValueFile);

            if (!Directory.Exists(_laConfigProgramDirectory))
                Directory.CreateDirectory(_laConfigProgramDirectory);

            if (File.Exists(_laConfigXMLValueProgramFile))
                File.Delete(_laConfigXMLValueProgramFile);

            var encrypt = new Decrypt();
            XmlTextWriter writer = new XmlTextWriter(_laConfigXMLValueFile, System.Text.Encoding.UTF8);
            writer.WriteStartDocument(true);
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 2;
            writer.WriteStartElement("PMC", "urn:PMC-schema");

            //ConnectionStringDB
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("ConnectionStringDB");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_connectionString));
            writer.WriteEndElement();
            writer.WriteEndElement();


            //ServerPath
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("ServerPath");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_serverPath));
            writer.WriteEndElement();
            writer.WriteEndElement();


            //SystemLocation
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("SystemLocation");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_systemLocation));
            writer.WriteEndElement();
            writer.WriteEndElement();


            //FTPLocation
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("FTPLocation");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(txtFTPLocation.Text));
            writer.WriteEndElement();
            writer.WriteEndElement();


            //FTPSystemLocation
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("FTPSystemLocation");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            var ftpSystemLocation = txtFTPLocation.Text + @"\Systems\";
            writer.WriteString(encrypt.strDESEncrypt(ftpSystemLocation));
            writer.WriteEndElement();
            writer.WriteEndElement();


            //ZIPLocation
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("ZIPLocation");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(txtFTPLocation.Text + @"\UWS\"));
            writer.WriteEndElement();
            writer.WriteEndElement();
            

            //WebSite
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("WebSite");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_webSite));
            writer.WriteEndElement();
            writer.WriteEndElement();
            

            //WebsiteAddress.
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("WebSiteAddress");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_webSiteAddress));
            writer.WriteEndElement();
            writer.WriteEndElement();
            


            //SupportEmail
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("SupportEmail");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_supportEmail));
            writer.WriteEndElement();
            writer.WriteEndElement();
            

            /*//AdvisorEmail
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("AdvisorEmail");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(_advisorEmail);
            writer.WriteEndElement();
            writer.WriteEndElement();
            */


            //MailTo
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("MailTo");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_mailTo));
            writer.WriteEndElement();
            writer.WriteEndElement();

            //EmailServer
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("EmailServer");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_emailServer));
            writer.WriteEndElement();
            writer.WriteEndElement();
            
            //EmailPort
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("EmailPort");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_emailPort));
            writer.WriteEndElement();
            writer.WriteEndElement();
            
            //EmailUser
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("EmailUser");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_emailUser));
            writer.WriteEndElement();
            writer.WriteEndElement();
            
            //EmailPassword
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("EmailPassword");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_emailPassword));
            writer.WriteEndElement();
            writer.WriteEndElement();
            
            //EmailAuthentication
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("EmailAuthentication");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(_emailAuthentication ? encrypt.strDESEncrypt("true") : encrypt.strDESEncrypt("false"));
            writer.WriteEndElement();
            writer.WriteEndElement();
            
            //IsSSL
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("EmailIsSSL");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(_emailSSL ? encrypt.strDESEncrypt("true") : encrypt.strDESEncrypt("false"));
            writer.WriteEndElement();
            writer.WriteEndElement();
            
            //AWS
            //writer.WriteStartElement("Element");
            //writer.WriteStartElement("ID");
            //writer.WriteString("AWS");
            //writer.WriteEndElement();
            //writer.WriteStartElement("Value");
            //writer.WriteString("false");
            //writer.WriteEndElement();
            //writer.WriteEndElement();
            //


            //NetworkStorageLocation
            writer.WriteStartElement("Element");
            writer.WriteStartElement("ID");
            writer.WriteString("NetworkStorageLocation");
            writer.WriteEndElement();
            writer.WriteStartElement("Value");
            writer.WriteString(encrypt.strDESEncrypt(_networkStorageLocation));
            writer.WriteEndElement();
            writer.WriteEndElement();
            

            //DatabasePrefix
            //writer.WriteStartElement("Element");
            //writer.WriteStartElement("ID");
            //writer.WriteString("DatabasePrefix");
            //writer.WriteEndElement();
            //writer.WriteStartElement("Value");
            //writer.WriteString("pmc");
            //writer.WriteEndElement();
            //writer.WriteEndElement();
            //

            writer.WriteEndDocument();
            writer.Close();

            //Copy the writer to new location.
            var fileInfo = new FileInfo(_laConfigXMLValueFile);
            fileInfo.CopyTo(_laConfigXMLValueProgramFile);

            this.Invoke(new Action(() => progressBar1.Value = 100));
        }

        private void GetTimeZone() {
            var curTimeZone = TimeZone.CurrentTimeZone;
            var timezoneInfo = TimeZoneInformation.EnumZones();
            var loop = 0;
            var index = 0;
            foreach (TimeZoneInformation a in timezoneInfo) {
                timeZoneNameIndexPair.Add(a.DisplayName, a.Index);
                cbTimeZone.Items.Add(a.DisplayName);
                if (a.StandardName == curTimeZone.StandardName)
                    index = loop;
                else loop++;
            }

            cbTimeZone.SelectedIndex = index;
        }

        private void CheckSubmitButton() {
            if (_servicesAvailable.ContainsKey((int)PMCServices.WEBSITE) &&
                _dbConnectionCheck &&
                _mailServerCheck &&
                txtPMCLocation.Text.Length > 0 &&
                txtWebsiteName.Text.Length > 0 &&
                txtNetworkStorageLocation.Text.Length > 0 &&
                SystemList.Count > 0 && 
                txtUserFirstName.Text.Length > 0 &&
                txtUserLastName.Text.Length > 0 &&
                txtUserEmail.Text.Length > 0 &&
                IsValidEmail(txtUserEmail.Text.Trim()) &&
                isValidPassword &&
                txtCompanyName.Text.Length > 0 &&
                txtCompanyAddress1.Text.Length > 0 &&
                txtCompanyCity.Text.Length > 0 &&
                txtCompanyState.Text.Length > 0 &&
                txtCompanyPostalCode.Text.Length > 0 &&
                txtUserPassword.Text.Length > 0 &&
                cbCompanyCountry.SelectedItem != null &&
                cbCompanyCountry.SelectedItem.ToString().Length > 0 &&
                txtContactNumber.Text.Length > 0 &&
                (_servicesAvailable.ContainsKey((int)PMCServices.RELAY) && txtFTPLocation.Text.Length > 0 || !_servicesAvailable.ContainsKey((int)PMCServices.RELAY))) {               
                    btnSubmit.Enabled = true;                             
            } else if (!_servicesAvailable.ContainsKey((int)PMCServices.WEBSITE) &&
                        (_servicesAvailable.ContainsKey((int)PMCServices.SCHEDULER) ||
                        _servicesAvailable.ContainsKey((int)PMCServices.RELAY) ||
                        _servicesAvailable.ContainsKey((int)PMCServices.LOADER) ||
                        _servicesAvailable.ContainsKey((int)PMCServices.STATICREPORT) ||
                        _servicesAvailable.ContainsKey((int)PMCServices.DYNAMICREPORT)) &&
                       _dbConnectionCheck &&
                       _mailServerCheck
                       ) {
                btnSubmit.Enabled = true;
            }
			else {
				btnSubmit.Enabled = false;
			}
        }

        private bool HasSpecialChar(string input) {
            var specialCharacter = "/[ !\"#$%&\'()*+,-./:;<=>?@\\^_`{|}~]";
            foreach (var item in specialCharacter) {
                if (input.Contains(item)) return true;
            }

            return false;

        }

        private void GetCountryList() {
            var countryList = new Dictionary<string, string>();
            countryList.Add("Afghanistan", "AF");
            countryList.Add("Aland Islands", "AX");
            countryList.Add("Albania", "AL");
            countryList.Add("Algeria", "DZ");
            countryList.Add("American Samoa", "AS");
            countryList.Add("Andorra", "AD");
            countryList.Add("Angola", "AO");
            countryList.Add("Anguilla", "AI");
            countryList.Add("Antarctica", "AQ");
            countryList.Add("Antigua and Barbuda", "AG");
            countryList.Add("Argentina", "AR");
            countryList.Add("Armenia", "AM");
            countryList.Add("Aruba", "AW");
            countryList.Add("Australia", "AU");
            countryList.Add("Austria", "AT");
            countryList.Add("Azerbaijan", "AZ");
            countryList.Add("Bahamas", "BS");
            countryList.Add("Bahrain", "BH");
            countryList.Add("Bangladesh", "BD");
            countryList.Add("Barbados", "BB");
            countryList.Add("Belarus", "BY");
            countryList.Add("Belgium", "BE");
            countryList.Add("Belize", "BZ");
            countryList.Add("Benin", "BJ");
            countryList.Add("Bermuda", "BM");
            countryList.Add("Bhutan", "BT");
            countryList.Add("Bolivia", "BO");
            countryList.Add("Bosnia and Herzegovina", "BA");
            countryList.Add("Botswana", "BW");
            countryList.Add("Bouvet Island", "BV");
            countryList.Add("Brazil", "BR");
            countryList.Add("British Indian Ocean Territory", "IO");
            countryList.Add("Brunei Darussalam", "BN");
            countryList.Add("Bulgaria", "BG");
            countryList.Add("Burkina Faso", "BF");
            countryList.Add("Burundi", "BI");
            countryList.Add("Cambodia", "KH");
            countryList.Add("Cameroon", "CM");
            countryList.Add("Canada", "CA");
            countryList.Add("Cape Verde", "CV");
            countryList.Add("Cayman Islands", "KY");
            countryList.Add("Central African Republic", "CF");
            countryList.Add("Chad", "TD");
            countryList.Add("Chile", "CL");
            countryList.Add("China", "CN");
            countryList.Add("Christmas Island", "CX");
            countryList.Add("Cocos (Keeling) Islands", "CC");
            countryList.Add("Colombia", "CO");
            countryList.Add("Comoros", "KM");
            countryList.Add("Congo", "CG");
            countryList.Add("Congo, The Democratic Republic of the", "CD");
            countryList.Add("Cook Islands", "CK");
            countryList.Add("Costa Rica", "CR");
            countryList.Add("Cote D'Ivoire", "CI");
            countryList.Add("Croatia", "HR");
            countryList.Add("Cuba", "CU");
            countryList.Add("Cyprus", "CY");
            countryList.Add("Czech Republic", "CZ");
            countryList.Add("Denmark", "DK");
            countryList.Add("Djibouti", "DJ");
            countryList.Add("Dominica", "DM");
            countryList.Add("Dominican Republic", "DO");
            countryList.Add("Ecuador", "EC");
            countryList.Add("Egypt", "EG");
            countryList.Add("El Salvador", "SV");
            countryList.Add("Equatorial Guinea", "GQ");
            countryList.Add("Eritrea", "ER");
            countryList.Add("Estonia", "EE");
            countryList.Add("Ethiopia", "ET");
            countryList.Add("Falkland Islands (Malvinas)", "FK");
            countryList.Add("Faroe Islands", "FO");
            countryList.Add("Fiji", "FJ");
            countryList.Add("Finland", "FI");
            countryList.Add("France", "FR");
            countryList.Add("French Guiana", "GF");
            countryList.Add("French Polynesia", "PF");
            countryList.Add("French Southern Territories", "TF");
            countryList.Add("Gabon", "GA");
            countryList.Add("Gambia", "GM");
            countryList.Add("Georgia", "GE");
            countryList.Add("Germany", "DE");
            countryList.Add("Ghana", "GH");
            countryList.Add("Gibraltar", "GI");
            countryList.Add("Greece", "GR");
            countryList.Add("Greenland", "GL");
            countryList.Add("Grenada", "GD");
            countryList.Add("Guadeloupe", "GP");
            countryList.Add("Guam", "GU");
            countryList.Add("Guatemala", "GT");
            countryList.Add("Guernsey", "GG");
            countryList.Add("Guinea", "GN");
            countryList.Add("Guinea-Bissau", "GW");
            countryList.Add("Guyana", "GY");
            countryList.Add("Haiti", "HT");
            countryList.Add("Heard Island and Mcdonald Islands", "HM");
            countryList.Add("Holy See (Vatican City State)", "VA");
            countryList.Add("Honduras", "HN");
            countryList.Add("Hong Kong", "HK");
            countryList.Add("Hungary", "HU");
            countryList.Add("Iceland", "IS");
            countryList.Add("India", "IN");
            countryList.Add("Indonesia", "ID");
            countryList.Add("Iran, Islamic Republic Of", "IR");
            countryList.Add("Iraq", "IQ");
            countryList.Add("Ireland", "IE");
            countryList.Add("Isle of Man", "IM");
            countryList.Add("Israel", "IL");
            countryList.Add("Italy", "IT");
            countryList.Add("Jamaica", "JM");
            countryList.Add("Japan", "JP");
            countryList.Add("Jersey", "JE");
            countryList.Add("Jordan", "JO");
            countryList.Add("Kazakhstan", "KZ");
            countryList.Add("Kenya", "KE");
            countryList.Add("Kiribati", "KI");
            countryList.Add("Korea, Democratic People's Republic of", "KP");
            countryList.Add("Korea, Republic of", "KR");
            countryList.Add("Kuwait", "KW");
            countryList.Add("Kyrgyzstan", "KG");
            countryList.Add("Lao People's Democratic Republic", "LA");
            countryList.Add("Latvia", "LV");
            countryList.Add("Lebanon", "LB");
            countryList.Add("Lesotho", "LS");
            countryList.Add("Liberia", "LR");
            countryList.Add("Libyan Arab Jamahiriya", "LY");
            countryList.Add("Liechtenstein", "LI");
            countryList.Add("Lithuania", "LT");
            countryList.Add("Luxembourg", "LU");
            countryList.Add("Macao", "MO");
            countryList.Add("Macedonia, The Former Yugoslav Republic of", "MK");
            countryList.Add("Madagascar", "MG");
            countryList.Add("Malawi", "MW");
            countryList.Add("Malaysia", "MY");
            countryList.Add("Maldives", "MV");
            countryList.Add("Mali", "ML");
            countryList.Add("Malta", "MT");
            countryList.Add("Marshall Islands", "MH");
            countryList.Add("Martinique", "MQ");
            countryList.Add("Mauritania", "MR");
            countryList.Add("Mauritius", "MU");
            countryList.Add("Mayotte", "YT");
            countryList.Add("Mexico", "MX");
            countryList.Add("Micronesia, Federated States of", "FM");
            countryList.Add("Moldova, Republic of", "MD");
            countryList.Add("Monaco", "MC");
            countryList.Add("Mongolia", "MN");
            countryList.Add("Montserrat", "MS");
            countryList.Add("Morocco", "MA");
            countryList.Add("Mozambique", "MZ");
            countryList.Add("Myanmar", "MM");
            countryList.Add("Namibia", "NA");
            countryList.Add("Nauru", "NR");
            countryList.Add("Nepal", "NP");
            countryList.Add("Netherlands", "NL");
            countryList.Add("Netherlands Antilles", "AN");
            countryList.Add("New Caledonia", "NC");
            countryList.Add("New Zealand", "NZ");
            countryList.Add("Nicaragua", "NI");
            countryList.Add("Niger", "NE");
            countryList.Add("Nigeria", "NG");
            countryList.Add("Niue", "NU");
            countryList.Add("Norfolk Island", "NF");
            countryList.Add("Northern Mariana Islands", "MP");
            countryList.Add("Norway", "NO");
            countryList.Add("Oman", "OM");
            countryList.Add("Pakistan", "PK");
            countryList.Add("Palau", "PW");
            countryList.Add("Palestinian Territory, Occupied", "PS");
            countryList.Add("Panama", "PA");
            countryList.Add("Papua New Guinea", "PG");
            countryList.Add("Paraguay", "PY");
            countryList.Add("Peru", "PE");
            countryList.Add("Philippines", "PH");
            countryList.Add("Pitcairn", "PN");
            countryList.Add("Poland", "PL");
            countryList.Add("Portugal", "PT");
            countryList.Add("Puerto Rico", "PR");
            countryList.Add("Qatar", "QA");
            countryList.Add("Reunion", "RE");
            countryList.Add("Romania", "RO");
            countryList.Add("Russian Federation", "RU");
            countryList.Add("Rwanda", "RW");
            countryList.Add("Saint Helena", "SH");
            countryList.Add("Saint Kitts and Nevis", "KN");
            countryList.Add("Saint Lucia", "LC");
            countryList.Add("Saint Pierre and Miquelon", "PM");
            countryList.Add("Saint Vincent and the Grenadines", "VC");
            countryList.Add("Samoa", "WS");
            countryList.Add("San Marino", "SM");
            countryList.Add("Sao Tome and Principe", "ST");
            countryList.Add("Saudi Arabia", "SA");
            countryList.Add("Senegal", "SN");
            countryList.Add("Serbia and Montenegro", "CS");
            countryList.Add("Seychelles", "SC");
            countryList.Add("Sierra Leone", "SL");
            countryList.Add("Singapore", "SG");
            countryList.Add("Slovakia", "SK");
            countryList.Add("Slovenia", "SI");
            countryList.Add("Solomon Islands", "SB");
            countryList.Add("Somalia", "SO");
            countryList.Add("South Africa", "ZA");
            countryList.Add("South Georgia and the South Sandwich Islands", "GS");
            countryList.Add("Spain", "ES");
            countryList.Add("Sri Lanka", "LK");
            countryList.Add("Sudan", "SD");
            countryList.Add("Suriname", "SR");
            countryList.Add("Svalbard and Jan Mayen", "SJ");
            countryList.Add("Swaziland", "SZ");
            countryList.Add("Sweden", "SE");
            countryList.Add("Switzerland", "CH");
            countryList.Add("Syrian Arab Republic", "SY");
            countryList.Add("Taiwan, Province of China", "TW");
            countryList.Add("Tajikistan", "TJ");
            countryList.Add("Tanzania, United Republic of", "TZ");
            countryList.Add("Thailand", "TH");
            countryList.Add("Timor-Leste", "TL");
            countryList.Add("Togo", "TG");
            countryList.Add("Tokelau", "TK");
            countryList.Add("Tonga", "TO");
            countryList.Add("Trinidad and Tobago", "TT");
            countryList.Add("Tunisia", "TN");
            countryList.Add("Turkey", "TR");
            countryList.Add("Turkmenistan", "TM");
            countryList.Add("Turks and Caicos Islands", "TC");
            countryList.Add("Tuvalu", "TV");
            countryList.Add("Uganda", "UG");
            countryList.Add("Ukraine", "UA");
            countryList.Add("United Arab Emirates", "AE");
            countryList.Add("United Kingdom", "GB");
            countryList.Add("United States", "US");
            countryList.Add("United States Minor Outlying Islands", "UM");
            countryList.Add("Uruguay", "UY");
            countryList.Add("Uzbekistan", "UZ");
            countryList.Add("Vanuatu", "VU");
            countryList.Add("Venezuela", "VE");
            countryList.Add("Vietnam", "VN");
            countryList.Add("Virgin Islands, British", "VG");
            countryList.Add("Virgin Islands, U.S.", "VI");
            countryList.Add("Wallis and Futuna", "WF");
            countryList.Add("Western Sahara", "EH");
            countryList.Add("Yemen", "YE");
            countryList.Add("Zambia", "ZM");
            countryList.Add("Zimbabwe", "ZW");

            var bs = new BindingSource(countryList, null);

            cbCompanyCountry.DataSource = bs;
            cbCompanyCountry.DisplayMember = "Key";
            cbCompanyCountry.ValueMember = "Value";


            string name = System.Globalization.RegionInfo.CurrentRegion.DisplayName;
            if (countryList.ContainsKey(name)) {
                var loop = 0;
                foreach (var country in countryList) {
                    if (country.Key == name)
                        cbCompanyCountry.SelectedIndex = loop;
                    else loop++;
                }
            }

        }

        bool IsValidEmail(string email) {
            try {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch {
                return false;
            }
        }

        private void CheckConfigFile() {
            var fileFullName =_laConfigXMLValueFile;

            try {
                if (File.Exists(fileFullName)) {
                    string id = string.Empty;
                    string values = string.Empty;


                    var databaseConnectionString = "";
                    var databaseDomainName = "";
                    var databasePort = "";
                    var databaseUserName = "";
                    var databasePassword = "";

                    var emailAddress = "";
                    var emailServer = "";
                    var emailPort = "";
                    var emailUserName = "";
                    var emailPassword = "";
                    var emailAuthentication = true;
                    var emailUseBefore220Welcome = false;
                    var emailIsSSL = true;
					isValidPassword = true;

                    var pmcLocation = "";
                    var ftpLocation = "";
					var networkSaveLocation = "";
                    var websiteIp = "";
                    #region SystemInfo

                    var systemCount = 0;
                    var systemInfo = new List<Systems>();
                    #endregion
                    #region User Info

                    var userFirstName = "";
                    var userLastName = "";
                    var userEmailAddress = "";
                    var userPassword = "";

                    #endregion

                    #region Company Info

                    var companyName = "";
                    var companyAddress1 = "";
                    var companyAddress2 = "";
                    var companyCity = "";
                    var companyState = "";
                    var companyPostalCode = "";
                    var companyCountry = "";
                    var companyContactNumber = "";

                    #endregion
                    var doc = new XmlDocument();
                    doc.Load(fileFullName);

                    XmlNodeList nodeList;
                    XmlElement root = doc.DocumentElement;

                    // Create an XmlNamespaceManager to resolve the default namespace.
                    var nsmgr = new XmlNamespaceManager(doc.NameTable);
                    var encrypt = new Decrypt();
                    nsmgr.AddNamespace("ra", "urn:PMC-schema");
                    nodeList = root.SelectNodes("/ra:PMC/ra:Element", nsmgr);
                    for (int x = 0; x < nodeList.Count; x++) {
                        id = nodeList.Item(x).ChildNodes[0].InnerText;
                        values = nodeList.Item(x).ChildNodes[1].InnerText;

                        switch (id) {
                            case "ConnectionStringDB":
                                databaseConnectionString = encrypt.strDESDecrypt(values);
                                break;
                            case "ServerPath":
                                pmcLocation = encrypt.strDESDecrypt(values);
                                txtPMCLocation.Text = pmcLocation.TrimEnd(Path.DirectorySeparatorChar);
                                break;
                            case "FTPLocation":
                                ftpLocation = encrypt.strDESDecrypt(values);
                                txtFTPLocation.Text = ftpLocation.TrimEnd(Path.DirectorySeparatorChar);
                                break;
                            case "EmailServer":
                                emailServer = encrypt.strDESDecrypt(values);
                                break;
                            case "EmailPort":
                                emailPort = encrypt.strDESDecrypt(values);
                                break;
                            case "EmailUser":
                                emailUserName = encrypt.strDESDecrypt(values);
                                break;
                            case "EmailAuthentication":
                                emailAuthentication = Convert.ToBoolean(encrypt.strDESDecrypt(values));
                                break;
                            case "EmailIsSSL":
                                emailIsSSL = Convert.ToBoolean(encrypt.strDESDecrypt(values));
                                break;
                            case "NetworkStorageLocation":
                                networkSaveLocation = encrypt.strDESDecrypt(values);
                                break;
                            case "EmailPassword":
                                emailPassword = encrypt.strDESDecrypt(values);
                                break;
                            case "SupportEmail":
                                emailAddress = encrypt.strDESDecrypt(values);
                                break;
                            case "UserFirstName":
                                userFirstName = encrypt.strDESDecrypt(values);
                                break;
                            case "UserLastName":
                                userLastName = encrypt.strDESDecrypt(values);
                                break;
                            case "UserEmailAddress":
                                userEmailAddress = encrypt.strDESDecrypt(values);
                                break;
                            case "UserPassword":
                                userPassword = encrypt.strDESDecrypt(values);
                                break;
                            case "CompanyName":
                                companyName = encrypt.strDESDecrypt(values);
                                break;
                            case "CompanyAddress1":
                                companyAddress1 = encrypt.strDESDecrypt(values);
                                break;
                            case "CompanyAddress2":
                                companyAddress2 = encrypt.strDESDecrypt(values);
                                break;
                            case "CompanyCity":
                                companyCity = encrypt.strDESDecrypt(values);
                                break;
                            case "CompanyState":
                                companyState = encrypt.strDESDecrypt(values);
                                break;
                            case "CompanyPostalCode":
                                companyPostalCode = encrypt.strDESDecrypt(values);
                                break;
                            case "CompanyCountry":
                                companyCountry = encrypt.strDESDecrypt(values);
                                break;
                            case "CompanyContactNumber":
                                companyContactNumber = encrypt.strDESDecrypt(values);
                                break;
                            case "SystemCount":
                                systemCount = Convert.ToInt32(encrypt.strDESDecrypt(values));
                                break;
                            case "WebSite":
                                websiteIp = encrypt.strDESDecrypt(values);
                                break;
                        }
                    }

                    if (systemCount > 0) {
                        //Get System Info.
                        for (var i = 0; i < systemCount; i++) {
                            var sysInfo = new Systems();

                            for (int x = 0; x < nodeList.Count; x++) {
                                id = nodeList.Item(x).ChildNodes[0].InnerText;
                                values = nodeList.Item(x).ChildNodes[1].InnerText;

                                if (id == "SystemName_" + i) {
                                    var systemName = encrypt.strDESDecrypt(values);
                                    if (!systemName.StartsWith("\\"))
                                        systemName = "\\" + systemName;

                                    sysInfo.SystemName = systemName;
                                }
                                else if (id == "SystemSerial_" + i) {
                                    sysInfo.SystemSerial = encrypt.strDESDecrypt(values);
                                }
                                else if (id == "SystemTimeZone_" + i) {
                                    sysInfo.TimeZone = encrypt.strDESDecrypt(values);
                                }

                                else if (id == "SystemMeasFH_" + i) sysInfo.MeashFH = encrypt.strDESDecrypt(values);
                                else if (id == "SystemLocation_" + i) sysInfo.Location = encrypt.strDESDecrypt(values);
                                else if (id == "NonStopIP_" + i) sysInfo.NonStopIP = encrypt.strDESDecrypt(values);
                                else if (id == "MonitorPort_" + i) sysInfo.MonitorPort = encrypt.strDESDecrypt(values);
                                else if (id == "StoreVolume_" + i) sysInfo.StoreVolume = encrypt.strDESDecrypt(values);
                                else if (id == "FTPUserName_" + i) sysInfo.FtpUserName = encrypt.strDESDecrypt(values);
                                else if (id == "FTPPassword_" + i) sysInfo.FtpPassword = encrypt.strDESDecrypt(values);
                                else if (id == "FTPPort_" + i) sysInfo.FtpPort = encrypt.strDESDecrypt(values);
                                else if (id == "FTPMeasFhVolume_" + i) sysInfo.MeasFhVolume = encrypt.strDESDecrypt(values);
                                else if (id == "MeasFhSubVolume_" + i) sysInfo.MeasFhSubVolume = encrypt.strDESDecrypt(values);
                            }

                            clbSystemNumber.Items.Add(sysInfo.SystemName);
                            SystemList.Add(sysInfo.SystemName, sysInfo);
                        }
                    }
                    try {
                        //populate Database.
                        var databaseTemp = databaseConnectionString.Split(';');
                        foreach (var s in databaseTemp) {
                            if (s.Split('=')[0].ToUpper() == "SERVER") {
                                txtDatabaseDomainName.Text = s.Split('=')[1];
                            }
                            else if (s.Split('=')[0].ToUpper() == "PORT") {
                                txtDatabasePort.Text = s.Split('=')[1];
                            }
                            else if (s.Split('=')[0].ToUpper() == "UID") {
                                txtDatabaseUserName.Text = s.Split('=')[1];
                            }
                            else if (s.Split('=')[0].ToUpper() == "PASSWORD") {
                                txtDatabasePassword.Text = s.Split('=')[1];
                            }
                        }
                        var server = txtDatabaseDomainName.Text;
                        var port = txtDatabasePort.Text;
                        var uId = txtDatabaseUserName.Text;
                        var password = txtDatabasePassword.Text;

                        _connectionString = "SERVER=" + server + ";PORT=" + port + ";DATABASE=pmc;UID=" + uId + ";PASSWORD=" + password + ";";
                        _connectionStringTrend = "SERVER=" + server + ";PORT=" + port + ";DATABASE=pmc;UID=" + uId + ";PASSWORD=" + password + ";";
                        _connectionStringSPAM = "SERVER=" + server + ";PORT=" + port + ";DATABASE=pmc;UID=" + uId + ";PASSWORD=" + password + ";";
                        _connectionStringComparative = "SERVER=" + server + ";PORT=" + port + ";DATABASE=pmccomparision;UID=" + uId + ";PASSWORD=" + password + ";";
                        _tempDatabaseConnectionString = "SERVER=" + server + ";PORT=" + port + ";DATABASE=pmcSERIALNUMBER;UID=" + uId + ";PASSWORD=" + password + ";";
                        _masterDatabaseConnectionString = "SERVER=" + server + ";PORT=" + port + ";DATABASE=sys;UID=" + uId + ";PASSWORD=" + password + ";";

                        //populate email.
                        txtMailFromAccount.Text = emailAddress;
                        txtEmailServer.Text = emailServer;
                        txtEmailPort.Text = emailPort;
                        txtTestEmailUser.Text = "";
                        txtEmailPassword.Text = emailPassword;
                        chbEmailAuth.Checked = emailAuthentication;
                        chbIsSSL.Checked = emailIsSSL;


                        _supportEmail = emailAddress;
                        _advisorEmail = emailAddress;
                        _mailTo = "mailto:" + emailAddress;
                        _emailServer = emailServer;
                        _emailPort = emailPort;
                        _emailUser = emailAddress;
                        _emailPassword = emailPassword;
                        _emailAuthentication = emailAuthentication;
                        _emailSSL = emailIsSSL;
                        
                        //populate PMC Location and others.
                        txtWebsiteName.Text = websiteIp;
                        txtNetworkStorageLocation.Text = networkSaveLocation;

                        //populate Company.
                        txtCompanyName.Text = companyName;
                        txtContactNumber.Text = companyContactNumber;
                        txtCompanyAddress1.Text = companyAddress1;
                        txtCompanyAddress2.Text = companyAddress2;
                        txtCompanyCity.Text = companyCity;
                        txtCompanyState.Text = companyState;
                        txtCompanyPostalCode.Text = companyPostalCode;
                        var loop = 0;
                        foreach (var country in cbCompanyCountry.Items) {
                            if (country.ToString() == companyCountry)
                                cbCompanyCountry.SelectedIndex = loop;
                            else loop++;
                        }


                        //populate User.
                        txtUserFirstName.Text = userFirstName;
                        txtUserLastName.Text = userLastName;
                        txtUserEmail.Text = userEmailAddress;
                        txtUserPassword.Text = userPassword;

                        //Disable the textbox for System, User and Company.
                        gbSystem.Enabled = false;
                        gbUser.Enabled = false;
                        gbCompany.Enabled = false;

                        _dbConnectionCheck = true;
                        _mailServerCheck = true;
                    }
                    catch (Exception ex) {
                    }
                }
            }
            catch (Exception ex) {
            }

            CheckSubmitButton();
        }
        #endregion

        #region Events
        private void Form1_Load(object sender, EventArgs e) {
            // code to adjust form fields based on what PMC services are installed

            ServiceController[] scServices;
            scServices = ServiceController.GetServices();
            foreach (var scService in scServices) {
                if (scService.ServiceName.ToUpper() == "LOCAL_ANALYST_RELAY") {
                    _servicesAvailable[(int)PMCServices.RELAY] = true;
                }
                if (scService.ServiceName.ToUpper() == "LOCAL_ANALYST_LOADER") {
                    _servicesAvailable[(int)PMCServices.LOADER] = true;
                }
                if (scService.ServiceName.ToUpper() == "LOCAL_ANALYST_SCHEDULER") {
                    _servicesAvailable[(int)PMCServices.SCHEDULER] = true;
                }
                if (scService.ServiceName.ToUpper() == "LOCAL_ANALYST_DYNAMIC_REPORT_GENERATOR") {
                    _servicesAvailable[(int)PMCServices.DYNAMICREPORT] = true;
                }
                if (scService.ServiceName.ToUpper() == "LOCAL_ANALYST_STATIC_REPORT_GENERATOR") {
                    _servicesAvailable[(int)PMCServices.STATICREPORT] = true;
                }
            }

            // Put the iisManager in a try catch as it will crash if the user does not run the .exe as administrator
            try {
                var iisManager = new ServerManager();
                SiteCollection sites = iisManager.Sites;


                foreach (var site in sites) {
                    if (site.Name.ToLower() == "localanalyst") {
                        _servicesAvailable[(int)PMCServices.WEBSITE] = true;
                    }
                }
            } catch {
                
            }

#if (DEBUG) 
            _servicesAvailable.Clear();
            _servicesAvailable[(int)PMCServices.WEBSITE] = true;
            _servicesAvailable[(int)PMCServices.SCHEDULER] = true;
            _servicesAvailable[(int)PMCServices.LOADER] = true;
            _servicesAvailable[(int)PMCServices.RELAY] = true;
            _servicesAvailable[(int)PMCServices.STATICREPORT] = true;
            _servicesAvailable[(int)PMCServices.DYNAMICREPORT] = true;
#endif
            if (_servicesAvailable.Count > 0) {
                ModifyForm();
                GetTimeZone();
                if (_servicesAvailable.ContainsKey((int)PMCServices.WEBSITE)) {
                    GetCountryList();
                }

                CheckConfigFile();
            }
        }

        private void ModifyForm() {
            // Criteria Below:
            // 1. If the service doesn't have the website, don't render the user, company, or system fields
            // 2. If the service has the website
            //      a. If the only service installed is scheduler, remove the network storage path
            //      b. If the relay isn't present, remove the ftp location

            if (!_servicesAvailable.ContainsKey((int)PMCServices.WEBSITE)) {
                MaximumSize = new Size(630, 380);
                btnSubmit.Location = new Point(530, 300);
                progressBar1.Location = new Point(15, 300);
                progressBar1.Width = 500;

                RemoveControl(gbUser);
                RemoveControl(gbCompany);
                RemoveControl(gbSystem);
            }

            if (_servicesAvailable.ContainsKey((int)PMCServices.SCHEDULER) && _servicesAvailable.Count == 1) {
                RemoveNetworkStorageFields();
            }

            if (!_servicesAvailable.ContainsKey((int)PMCServices.RELAY)) {
                RemoveFTPFields();
            }

            if (!_servicesAvailable.ContainsKey((int)PMCServices.RELAY) || !_servicesAvailable.ContainsKey((int)PMCServices.LOADER) ||
                !_servicesAvailable.ContainsKey((int)PMCServices.WEBSITE) || !_servicesAvailable.ContainsKey((int)PMCServices.SCHEDULER) ||
                !_servicesAvailable.ContainsKey((int)PMCServices.STATICREPORT) || !_servicesAvailable.ContainsKey((int)PMCServices.DYNAMICREPORT)) {
                _isMultiServer = true;
                RemoveControl(btnNetworkStorage);
            }
        }
        
        private void RemoveControl(Control control) {
            Controls.Remove(control);
            control.Dispose();
        }

        private void RemoveNetworkStorageFields() {
            RemoveControl(label13);
            RemoveControl(txtNetworkStorageLocation);
            RemoveControl(btnNetworkStorage);
        }

        private void RemoveFTPFields() {
            RemoveControl(label8);
            RemoveControl(txtFTPLocation);
            RemoveControl(btnFTPLocation);
        }

        private void RemoveUserFields() {
            RemoveControl(txtUserFirstName);
            RemoveControl(txtUserLastName);
            RemoveControl(txtUserEmail);
            RemoveControl(txtUserPassword);
            RemoveControl(gbUser);
        }

        private void RemoveCompanyFields() {
            RemoveControl(txtCompanyName);
            RemoveControl(txtCompanyAddress1);
            RemoveControl(txtCompanyAddress2);
            RemoveControl(txtCompanyCity);
            RemoveControl(txtCompanyState);
            RemoveControl(txtCompanyPostalCode);
            RemoveControl(cbCompanyCountry);
            RemoveControl(txtContactNumber);
            RemoveControl(gbCompany);
        }

        private void RemoveSystemFields() {
            RemoveControl(gbSystem);
            RemoveControl(txtSystemName);
            RemoveControl(txtSystemNumber);
            RemoveControl(txtNonStopIP);
            RemoveControl(txtMonitorPort);
            RemoveControl(txtStoreVolume);
            RemoveControl(cbTimeZone);
            RemoveControl(txtMeasFh);
            RemoveControl(txtSystemLocation);
            RemoveControl(txtFTPUserName);
            RemoveControl(txtFTPPassword);
            RemoveControl(txtFTPPort);
            RemoveControl(clbSystemNumber);
            RemoveControl(btnRemove);
            RemoveControl(btnSystemAdd);
            RemoveControl(txtMeasFhSubVolume);
            RemoveControl(txtMeasFhVolume);

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e) {
            for (_currentPercent = 1; _currentPercent <= _totalPercent; _currentPercent++) {
                //for (var x = 1; x  <= 100; x++) {
                // Wait 100 milliseconds.
                Thread.Sleep(100);
                // Report progress.
                //backgroundWorker1.ReportProgress(x);
                backgroundWorker1.ReportProgress(_currentPercent);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            // Change the value of the ProgressBar to the BackgroundWorker progress.
            progressBar1.Value = e.ProgressPercentage;
            // Set the text.
            this.Text = e.ProgressPercentage.ToString();
        }

        private void btnSubmit_Click(object sender, EventArgs e) {
            progressBar1.Visible = true;
            btnSubmit.Enabled = false;

            var t = new Thread(StartProcessing) { IsBackground = true }; //change the functio name.
            t.Start();
        }

        private void btnPMCLocation_Click(object sender, EventArgs e) {
            var folderBrowserDlg = new FolderBrowserDialog { ShowNewFolderButton = true };
            var dlgResult = folderBrowserDlg.ShowDialog();

            if (dlgResult.Equals(DialogResult.OK)) {
                txtPMCLocation.Text = folderBrowserDlg.SelectedPath;
                CheckSubmitButton();
            }
        }

        private void btnNetworkStorage_Click(object sender, EventArgs e) {
            var folderBrowserDlg = new FolderBrowserDialog { ShowNewFolderButton = true };
            var dlgResult = folderBrowserDlg.ShowDialog();

            if (dlgResult.Equals(DialogResult.OK)) {
                txtNetworkStorageLocation.Text = folderBrowserDlg.SelectedPath;
                CheckSubmitButton();
            }
        }

        private void btnCheckConnection_Click(object sender, EventArgs e) {
            progressBar1.Visible = true;

            if (ValidateDatabase()) {
                progressBar1.Value = 10;
                var dbCheck = CheckMySQLConnection();
                if (dbCheck) {
                    _dbConnectionCheck = true;

                    CheckSubmitButton();
                    MessageBox.Show(this, "Database connection successful", "Database", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            progressBar1.Visible = false;
        }

        private void btnMailTest_Click(object sender, EventArgs e) {
            progressBar1.Visible = true;

            if (ValidateMail()) {
                progressBar1.Value = 10;
                var mailCheck = CheckEmailServer();
                if (mailCheck) {
                    _mailServerCheck = true;

                    CheckSubmitButton();
                    MessageBox.Show(this, "Mail Server connection successful", "Email", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            progressBar1.Visible = false;
        }

        private void btnSystemAdd_Click(object sender, EventArgs e) {
            var systemSerial = txtSystemNumber.Text.Trim();
            var systemName = txtSystemName.Text.Trim();
            var meshFh = txtMeasFh.Text.Trim();
            var systemLocation = txtSystemLocation.Text.Trim();
            var timezone = timeZoneNameIndexPair[cbTimeZone.SelectedItem.ToString()].ToString();
            var nonStopIp = txtNonStopIP.Text.Trim();
            var monitorPort = txtMonitorPort.Text;
            var storeVolume = txtStoreVolume.Text.Trim();
            var ftpUserName = txtFTPUserName.Text.Trim();
            var ftpPassword = txtFTPPassword.Text;
            var ftpPort = txtFTPPort.Text.Trim();
            var measFhVolume = txtMeasFhVolume.Text.Trim();
            var measFhSubVolume = txtMeasFhSubVolume.Text.Trim();

            if (systemName.Length == 0) {
                MessageBox.Show(this, "Please Enter System Name", "System Name", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtSystemName.Focus();
                return;
            }
            if (systemSerial.Length == 0) {
                MessageBox.Show(this, "Please Enter System Number", "System Number", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtSystemNumber.Focus();
                return;
            }
            var isNumeric = !string.IsNullOrEmpty(txtSystemNumber.Text.Trim()) && txtSystemNumber.Text.Trim().All(Char.IsDigit);
            if (!isNumeric) {
                MessageBox.Show(this, "System Serial can only contain numeric vaues", "Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtSystemNumber.Focus();
                return;
            }
            if (systemSerial.Length != 6) {
                MessageBox.Show(this, "System Serial must have exactly 6 digits", "System Number", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtSystemNumber.Focus();
                return;
            }

            if (timezone.Length == 0) {
                MessageBox.Show(this, "Please Select Time Zone", "Time Zone", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                cbTimeZone.Focus();
                return;
            }
            if (meshFh.Length == 0) {
                MessageBox.Show(this, "Please Enter MeasFH", "MeasFH", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtMeasFh.Focus();
                return;
            }
            if (systemLocation.Length == 0) {
                MessageBox.Show(this, "Please Enter System Location", "System Location", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtSystemLocation.Focus();
                return;
            }
            if (systemLocation.Length == 0) {
                MessageBox.Show(this, "Please Enter System Location", "System Location", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtSystemLocation.Focus();
                return;
            }
            if (nonStopIp.Length == 0) {
                MessageBox.Show(this, "Please Enter NonStop IP", "NonStop IP", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtNonStopIP.Focus();
                return;
            }
            if (monitorPort.Length == 0) {
                MessageBox.Show(this, "Please Enter Monitor Port", "Monitor Port", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtMonitorPort.Focus();
                return;
            }
            if (storeVolume.Length == 0) {
                MessageBox.Show(this, "Please Enter Store Volume", "Store Volume", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtStoreVolume.Focus();
                return;
            }
            if (ftpUserName.Length == 0) {
                MessageBox.Show(this, "Please Enter FTP User Name", "FTP User Name", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtFTPUserName.Focus();
                return;
            }
            if (ftpPassword.Length == 0) {
                MessageBox.Show(this, "Please Enter FTP Password", "FTP Password", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtFTPPassword.Focus();
                return;
            }
            if (ftpPort.Length == 0) {
                MessageBox.Show(this, "Please Enter FTP Port", "FTP Port", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtFTPPassword.Focus();
                return;
            }
            if (measFhVolume.Length == 0) {
                MessageBox.Show(this, "Please Enter a Volume for MEASFH", "FTP Port", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtMeasFhVolume.Focus();
                return;
            }
            if (measFhSubVolume.Length == 0) {
                MessageBox.Show(this, "Please Enter a SubVolume for MEASFH", "FTP Port", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtMeasFhSubVolume.Focus();
                return;
            }

            foreach (var item in clbSystemNumber.Items) {
                if (item.ToString() == systemSerial) {
                    MessageBox.Show(this, "Duplicated System Number", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    txtSystemNumber.Focus();
                    return;
                }
            }



            var returnMessage = "";

            // Skip connection check
            if (!chbSkipNSValidation.Checked)
            {
                progressBar1.Visible = true;
                progressBar1.Value = 10;
                if (txtFTPPort.Text.Trim() == "21") {
                    var ftp = new CustomFTP();
                    var connect = ftp.Connect(txtNonStopIP.Text.Trim());
                    if (connect) {
                        progressBar1.Value = 40;
                        var login = ftp.Login(txtFTPUserName.Text.Trim(), txtFTPPassword.Text);

                        if (!login)
                            returnMessage = "Unable to login with user " + txtFTPUserName.Text.Trim();
                    }
                    else {
                        returnMessage = "Unable to connect to the server.";
                    }
                }
                else {
                    using (var ftp = new Sftp()) {
                        try {
                            ftp.Connect(txtNonStopIP.Text.Trim());
                        }
                        catch (Exception ex) {
                            var retry = 0;
                            var retryOkay = false;
                            while (retry < 3) {
                                try {
                                    retry++;
                                    ftp.Connect(txtNonStopIP.Text.Trim());
                                    returnMessage = "";
                                    retry = 3;
                                    retryOkay = true;
                                }
                                catch (Exception exSub) {
                                    returnMessage = "Unable to connect to the server. " + exSub.Message;
                                }
                            }
                            if(!retryOkay)
                                returnMessage = "Unable to connect to the server. " + ex.Message;
                        }

                        if (returnMessage.Length == 0) {
                            try {
                                progressBar1.Value = 40;
                                ftp.Login(txtFTPUserName.Text.Trim(), txtFTPPassword.Text);
                                ftp.Disconnect();
                            }
                            catch {
                                returnMessage = "Unable to login with user " + txtFTPUserName.Text.Trim();
                            }
                        }
                    }
                }

                if (returnMessage.Length == 0) {
                    this.Invoke(new Action(() => progressBar1.Value = 80));
                    var uploadService = new UploadService(_connectionString);
                    var isHttp = uploadService.CheckHttpCallToNonStop(txtNonStopIP.Text.Trim(), Convert.ToInt32(txtFTPPort.Text));

                    if (isHttp) {
                        progressBar1.Value = 100;
                    }
                    else {
                        returnMessage = "Unable to communicate with the Host using HTTP.";
                    }
                }

                progressBar1.Visible = false;
            }
            if (returnMessage.Length == 0) {
                var systemInfo = new Systems {
                    SystemName = systemName,
                    SystemSerial = systemSerial,
                    MeashFH = meshFh,
                    Location = systemLocation,
                    TimeZone = timezone,
                    NonStopIP = nonStopIp,
                    MonitorPort = monitorPort,
                    StoreVolume = storeVolume,
                    FtpUserName = ftpUserName,
                    FtpPassword = ftpPassword,
                    FtpPort = ftpPort,
                    MeasFhVolume = measFhVolume,
                    MeasFhSubVolume = measFhSubVolume
                };
                SystemList.Add(systemName, systemInfo);

                clbSystemNumber.Items.Add(systemName);
                txtSystemNumber.Text = "";
                txtSystemName.Text = "";
                txtMeasFh.Text = "";
                txtSystemLocation.Text = "";
                txtNonStopIP.Text = "";
                txtMonitorPort.Text = "";
                txtStoreVolume.Text = "";
                txtFTPUserName.Text = "";
                txtFTPPassword.Text = "";
                txtFTPPort.Text = "22";
                txtMeasFhVolume.Text = "";
                txtMeasFhSubVolume.Text = "";

                CheckSubmitButton();
            }
            else {
                MessageBox.Show(this, returnMessage, "NonStop", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void btnRemove_Click(object sender, EventArgs e) {
            var deleteItemList = new List<string>();
            foreach (var item in clbSystemNumber.CheckedItems) {
                deleteItemList.Add(item.ToString());
            }

            foreach (var item in deleteItemList) {
                clbSystemNumber.Items.Remove(item);
                SystemList.Remove(item);
            }
            CheckSubmitButton();
        }

        private void txtWebsiteName_KeyUp(object sender, KeyEventArgs e) {
            CheckSubmitButton();
        }

        private void btnFTPLocation_Click(object sender, EventArgs e) {
            var folderBrowserDlg = new FolderBrowserDialog { ShowNewFolderButton = true };
            var dlgResult = folderBrowserDlg.ShowDialog();

            if (dlgResult.Equals(DialogResult.OK)) {
                txtFTPLocation.Text = folderBrowserDlg.SelectedPath;
                CheckSubmitButton();
            }
        }

        private void txtUserFirstName_KeyUp(object sender, KeyEventArgs e) {
            CheckSubmitButton();
        }

        private void txtUserLastName_KeyUp(object sender, KeyEventArgs e) {
            CheckSubmitButton();
        }

        private void txtUserEmail_KeyUp(object sender, KeyEventArgs e) {
            CheckSubmitButton();

            if (!IsValidEmail(txtUserEmail.Text.Trim())) {
                tt = new ToolTip();
                tt.InitialDelay = 0;
                tt.ShowAlways = true;
                tt.IsBalloon = true;
                tt.Show(string.Empty, txtUserEmail);
                var passwordMessage = new StringBuilder();
                var visibleTime = 1000;  //in milliseconds
                passwordMessage.Append("Please enter valid email address");
                tt.Show(passwordMessage.ToString(), txtUserEmail, visibleTime);
            }
        }

        private void txtUserPassword_KeyUp(object sender, KeyEventArgs e) {
            CheckSubmitButton();

            tt = new ToolTip();
            tt.InitialDelay = 0;
            tt.ShowAlways = true;
            tt.IsBalloon = true;
            tt.Show(string.Empty, txtUserPassword);
            var passwordMessage = new StringBuilder();
            passwordMessage.Append("Password must contain the following:" + Environment.NewLine);
			bool lower = false, upper = false, digit = false, length = false, special = false;
			if (txtUserPassword.Text.Any(char.IsLower)) {
				passwordMessage.Append("    ✔ A lowercase letter" + Environment.NewLine);
				lower = true;
			}else {
				passwordMessage.Append("    ✖ A lowercase letter" + Environment.NewLine);
				lower = false;
			}
			if (txtUserPassword.Text.Any(char.IsUpper)) {
				passwordMessage.Append("    ✔ A capital(uppercase) letter" + Environment.NewLine);
				upper = true;
			}else {
				passwordMessage.Append("    ✖ A capital(uppercase) letter" + Environment.NewLine);
				upper = false;
			}
			if (txtUserPassword.Text.Any(char.IsDigit)) {
				passwordMessage.Append("    ✔ A number" + Environment.NewLine);
				digit = true;
			}else {
				passwordMessage.Append("    ✖ A number" + Environment.NewLine);
				digit = false;
			}
			if (txtUserPassword.Text.Length >= 8) {
				passwordMessage.Append("    ✔ Minimum 8 characters" + Environment.NewLine);
				length = true;
			}else {
				passwordMessage.Append("    ✖ Minimum 8 characters" + Environment.NewLine);
				length = false;
			}
			if (HasSpecialChar(txtUserPassword.Text)) {
				passwordMessage.Append("    ✔ A Special Character" + Environment.NewLine);
				special = true;
			}else {
				passwordMessage.Append("    ✖ A Special Character" + Environment.NewLine);
				special = false;
			}
			if(lower && upper && digit && length && special) {
				isValidPassword = true;
			}else {
				isValidPassword = false;
			}
			CheckSubmitButton();
			var visibleTime = 3000;  //in milliseconds
            tt.Show(passwordMessage.ToString(), txtUserPassword, visibleTime);
        }

        private void txtUserPassword_Enter(object sender, EventArgs e) {
            if (txtUserPassword.Text.Length == 0) {
                tt = new ToolTip();
                tt.InitialDelay = 0;
                tt.ShowAlways = true;
                tt.IsBalloon = true;
                tt.Show(string.Empty, txtUserPassword);
                var passwordMessage = new StringBuilder();
                passwordMessage.Append("Password must contain the following:" + Environment.NewLine);
                passwordMessage.Append("    ✖ A lowercase letter" + Environment.NewLine);
                passwordMessage.Append("    ✖ A capital(uppercase) letter" + Environment.NewLine);
                passwordMessage.Append("    ✖ A number" + Environment.NewLine);
                passwordMessage.Append("    ✖ Minimum 8 characters" + Environment.NewLine);
                passwordMessage.Append("    ✖ A Special Character" + Environment.NewLine);
                var visibleTime = 3000;  //in milliseconds
                tt.Show(passwordMessage.ToString(), txtUserPassword, visibleTime);
            }
        }
        private void txtCompanyName_KeyUp(object sender, KeyEventArgs e) {
            CheckSubmitButton();
        }

        private void txtCompanyAddress1_KeyUp(object sender, KeyEventArgs e) {
            CheckSubmitButton();
        }

        private void txtCompanyAddress2_KeyUp(object sender, KeyEventArgs e) {
            CheckSubmitButton();
        }

        private void txtCompanyCity_KeyUp(object sender, KeyEventArgs e) {
            CheckSubmitButton();
        }

        private void txtCompanyState_KeyUp(object sender, KeyEventArgs e) {
            CheckSubmitButton();
        }

        private void txtCompanyPostalCode_KeyUp(object sender, KeyEventArgs e) {
            CheckSubmitButton();
        }

        private void txtContactNumber_KeyUp(object sender, KeyEventArgs e) {
            CheckSubmitButton();
        }
        private void txtMonitorPort_KeyPress(object sender, KeyPressEventArgs e) {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.')) {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1)) {
                e.Handled = true;
            }
        }

        private void txtFTPPort_KeyPress(object sender, KeyPressEventArgs e) {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                (e.KeyChar != '.')) {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1)) {
                e.Handled = true;
            }
        }
        #endregion

        private void cbSSL_CheckedChanged(object sender, EventArgs e) {
            //cbSSL.Checked = !cbSSL.Checked;
            btnSubmit.Enabled = false;


        }

        private void chbEmailAuth_CheckedChanged(object sender, EventArgs e) {
            //chbEmailAuth.Checked = !chbEmailAuth.Checked;
            btnSubmit.Enabled = false;

        }

        private void txtEmailAddress_TextChanged(object sender, EventArgs e) {
            btnSubmit.Enabled = false;
        }

        private void txtEmailServer_TextChanged(object sender, EventArgs e) {
            btnSubmit.Enabled = false;
        }

        private void txtEmailPort_TextChanged(object sender, EventArgs e) {
            btnSubmit.Enabled = false;
        }

        private void txtEmailUser_TextChanged(object sender, EventArgs e) {
            //btnSubmit.Enabled = false;
        }

        private void txtEmailPassword_TextChanged(object sender, EventArgs e) {
            btnSubmit.Enabled = false;
        }

        private void txtDatabaseDomainName_TextChanged(object sender, EventArgs e) {
            tt = new ToolTip();
            tt.InitialDelay = 0;
            tt.ShowAlways = true;
            tt.IsBalloon = true;
            tt.ForeColor = Color.Red;
            var message = new StringBuilder();
            var visibleTime = 10000;
            if (txtDatabaseDomainName.Text.ToLower() == "localhost") {
                message.Append("'localhost' is not supported");
                tt.Show(message.ToString(), txtDatabaseDomainName, visibleTime);
                btnCheckConnection.Enabled = false;
            } else {
                btnCheckConnection.Enabled = true;
            }
            btnSubmit.Enabled = false;
        }

        private void txtDatabasePort_TextChanged(object sender, EventArgs e) {
            btnSubmit.Enabled = false;
        }

        private void txtDatabaseUserName_TextChanged(object sender, EventArgs e) {
            btnSubmit.Enabled = false;
        }

        private void txtDatabasePassword_TextChanged(object sender, EventArgs e) {
            btnSubmit.Enabled = false;
        }

        private void Form1_Shown(object sender, EventArgs e) {
            //MessageBox.Show("Form shown");
            if (_servicesAvailable.Count == 0) {
                var errorForm = new Error(this);
                errorForm.StartPosition = FormStartPosition.CenterScreen;
                errorForm.Show();

                this.Enabled = false;
            }
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e) {

        }

        private void txtEmailServer_MouseHover(object sender, EventArgs e) {
            if (tt != null && tt.Active) {
                tt.Dispose();
            }
            tt = new ToolTip();
            tt.InitialDelay = 0;
            tt.ShowAlways = true;
            tt.Show(string.Empty, txtEmailServer);
            var message = new StringBuilder();
            var visibleTime = 5000;  //in milliseconds
            message.Append("The IP address or fully qualified hostname of the SMTP server.");
            tt.Show(message.ToString(), txtEmailServer, visibleTime);
            

        }

        private void txtEmailPort_MouseHover(object sender, EventArgs e) {
            if (tt != null && tt.Active) {
                tt.Dispose();
            }
            tt = new ToolTip();
            tt.InitialDelay = 0;
            tt.ShowAlways = true;
            tt.Show(string.Empty, txtEmailPort);
            var message = new StringBuilder();
            var visibleTime = 5000;  //in milliseconds
            message.Append("Specify the mail server port number.");
            tt.Show(message.ToString(), txtEmailPort, visibleTime);
            
        }

        private void txtMailFromAccount_MouseHover(object sender, EventArgs e) {
            if (tt != null && tt.Active) {
                tt.Dispose();
            }
            tt = new ToolTip();
            tt.InitialDelay = 0;
            tt.ShowAlways = true;
            tt.Show(string.Empty, txtMailFromAccount);
            var message = new StringBuilder();
            var visibleTime = 5000;  //in milliseconds
            message.Append("Specify the email address from which the email notifications will be sent out.");
            tt.Show(message.ToString(), txtMailFromAccount, visibleTime);
            
        }

        private void txtEmailPassword_MouseHover(object sender, EventArgs e) {
            if (tt != null && tt.Active) {
                tt.Dispose();
            }
            tt = new ToolTip();
            tt.InitialDelay = 0;
            tt.ShowAlways = true;
            tt.Show(string.Empty, txtEmailPassword);
            var message = new StringBuilder();
            var visibleTime = 5000;  //in milliseconds
            message.Append("Specify the password associated to the username in case the server requires authentication.");
            tt.Show(message.ToString(), txtEmailPassword, visibleTime);
            
        }

        private void txtTestEmailUser_MouseHover(object sender, EventArgs e) {
            if (tt != null && tt.Active) {
                tt.Dispose();
            }
            tt = new ToolTip();
            tt.InitialDelay = 0;
            tt.ShowAlways = true;
            tt.Show(string.Empty, txtTestEmailUser);
            var message = new StringBuilder();
            var visibleTime = 5000;  //in milliseconds
            message.Append("Specify the recipient to verify that emails are successfully sent and received.");
            tt.Show(message.ToString(), txtTestEmailUser, visibleTime);
            
        }

        private void txtNetworkStorageLocation_MouseHover(object sender, EventArgs e) {
            if (tt != null && tt.Active) {
                tt.Dispose();
            }
            tt = new ToolTip();
            tt.InitialDelay = 0;
            tt.ShowAlways = true;
            tt.Show(string.Empty, txtNetworkStorageLocation);
            //tt.IsBalloon = true;
            var message = new StringBuilder();
            var visibleTime = 30000;  //in milliseconds
            message.Append("Location on your Windows server where processed measure files and generated reports get stored.\n");
            message.Append("This must be a centralized location where all the Local Analyst components can have access & connect to.\n");
            message.Append("If this location is on a different server than the one you are running the configuration manager on, please\n");
            message.Append("use the below format to point to the correct location.\\\\NNN.NNN.NNN.NNN\\< Location of the folder >.\n\n");
            message.Append("Ex: \\\\10.0.0.1\\Local Analyst Storage Location");
            tt.Show(message.ToString(), txtNetworkStorageLocation, visibleTime);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            btnSubmit.Enabled = false;
        }

        private void txtUserEmail_TextChanged(object sender, EventArgs e) {

        }

        private void txtDatabaseDomainName_MouseHover(object sender, EventArgs e) {
            if (tt != null && tt.Active) {
                tt.Dispose();
            }
            tt = new ToolTip();
            tt.InitialDelay = 0;
            tt.ShowAlways = true;
            tt.Show(string.Empty, txtDatabaseDomainName);
            var message = new StringBuilder();
            var visibleTime = 10000;  //in milliseconds
            message.Append("Enter the IP address of the database server you are connecting to.\n");
            message.Append("\n");
            message.Append("Note: This field allows you to enter IP address only.");
            tt.Show(message.ToString(), txtDatabaseDomainName, visibleTime);
        }

        private void txtWebsiteName_TextChanged(object sender, EventArgs e) {

        }

        private void txtNetworkStorageLocation_TextChanged(object sender, EventArgs e) {
            this.Invoke(new Action(() => txtNetworkStorageLocation.BackColor = System.Drawing.Color.White));
        }

        private void chbSkipEmailValidation_CheckedChanged(object sender, EventArgs e)
        {
            if (chbSkipEmailValidation.Checked) { 
                _mailServerCheck = true;
                btnMailTest.Enabled = false;
            }
            else
            {
                _mailServerCheck = false;
                btnMailTest.Enabled = true;
            }
            CheckSubmitButton();
        }

        private void setDefaultEmailInformation()
        {
            _supportEmail = txtMailFromAccount.Text.Trim();
            _advisorEmail = _supportEmail;
            _mailTo = "mailto:" + _supportEmail;
            _emailServer = txtEmailServer.Text.Trim();
            _emailPort = txtEmailPort.Text.Trim();
            _emailUser = _supportEmail;
            _emailPassword = txtEmailPassword.Text;
            _emailAuthentication = true;
            _emailSSL = true;
        }
    }
}
