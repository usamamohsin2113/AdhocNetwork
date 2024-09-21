using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Net;

namespace Adhoc_Network
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            userNameTextBlock.Text = Environment.UserName;
            
            ShowAdhocNetworkDetails();
            
        }

        private void ShowAdhocNetworkDetails()
        {
            string adhocNetworkDetails = cmdProcessReturn("netsh", "wlan show hostednetwork");
            
            int start = adhocNetworkDetails.IndexOf("\"") + 1;
            int end = adhocNetworkDetails.IndexOf("\"", start);
            
            ssidTextBlock .Text = adhocNetworkDetails.Substring(start, end - start);

            passwordTextBlock.Text = getSecurityKey();

            if (adhocNetworkDetails.Contains("BSSID"))
            {
                networkStatusTextBlock.Text = "Started";
                stopAdhocNetworkButton.Visibility = System.Windows.Visibility.Visible;
                startAdhocNetworkButton.Visibility = System.Windows.Visibility.Hidden;
                changeDetailsButton.Visibility = System.Windows.Visibility.Hidden;
            }
            else 
            {
                networkStatusTextBlock.Text = "Not Started";
                stopAdhocNetworkButton.Visibility = System.Windows.Visibility.Hidden;
                startAdhocNetworkButton.Visibility = System.Windows.Visibility.Visible;
                changeDetailsButton.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private string getSecurityKey()
        {
            string result = cmdProcessReturn("netsh","wlan show hostednetwork setting=security");

            int start = result.IndexOf("User security key") + 17;
            int end = result.IndexOf("User security", start);

            string trimmedResult = result.Substring(start, end - start);

            trimmedResult = trimmedResult.Replace(":","").Replace(" ","");

            return trimmedResult;
        }

        private void createNetwork_Click(object sender, RoutedEventArgs e)
        {
            string ssidNetwork = ssidTextBox.Text;
            string keyNetwork = passwordTextBox.Text;

            if (ssidNetwork=="")
            {
                MessageBox.Show("Network Name (SSID) is necessary for\ncreating the adhoc network ");
            }
            else if (keyNetwork.Length<8)
            {
                MessageBox.Show("Password should be 8 characters long");
            }
            else
            {
                string command = "netsh wlan set hostednetwork mode=allow ssid=" + ssidNetwork + " key=" + keyNetwork;

                if (CmdProcess(command))
                {
                    adhocNetworkDetails.Visibility = System.Windows.Visibility.Visible;
                    createAdhocNetwork.Visibility = System.Windows.Visibility.Hidden;
                    MessageBox.Show("Adhoc Network Created Successfully\n\nNetwork Name (SSID) : " + ssidNetwork + "\n\tPassword\t : " + keyNetwork);
                    ShowAdhocNetworkDetails();
                }                              
            }
        }

        bool CmdProcess(string command)
        {
            try
            {
                ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + command);

                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;

                procStartInfo.CreateNoWindow = true;

                Process proc = new Process();
                proc.StartInfo = procStartInfo;
                proc.Start();

                return true;
            }
            catch (Exception)
            {    
                return false;
                throw;
            }
        }

        private void startAdhocNetworkButton_Click(object sender, RoutedEventArgs e)
        {
            string result = cmdProcessReturn("netsh","wlan start hostednetwork");
            if (result.Contains("network started"))
            {
                MessageBox.Show("Adhoc Network Started Successfully");
                ShowAdhocNetworkDetails();
            }
            else
            {
                MessageBox.Show("Unable to start the Adhoc Network");
            }
        }

        private void stopAdhocNetworkButton_Click(object sender, RoutedEventArgs e)
        {
            string command = "netsh wlan stop hostednetwork";
            if (CmdProcess(command))
            {
                MessageBox.Show("Adhoc Network Stopped Successfully");
                ShowAdhocNetworkDetails();
            }
            else
            {
                MessageBox.Show("Unable to stop the Adhoc Network");
            }
        }

        private static string cmdProcessReturn(string cmd,string parameter)
        {
            Process p = null;
            string output = string.Empty;

            try
            {
                p = Process.Start(new ProcessStartInfo(cmd, parameter)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                });

                output = p.StandardOutput.ReadToEnd();

                p.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to retrieve Adhoc Network Details", ex);
            }
            finally
            {
                if (p != null)
                {
                    p.Close();
                }
            }

            return output;
        }

        private void changeDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            adhocNetworkDetails.Visibility = System.Windows.Visibility.Hidden;
            createAdhocNetwork.Visibility = System.Windows.Visibility.Visible;
            createAdhocNetwork.Header = "Change Details";
            createNetwork.Content = "Change";
            ssidTextBox.Text = string.Empty;
            passwordTextBox.Text = string.Empty;
        }
    }
}
