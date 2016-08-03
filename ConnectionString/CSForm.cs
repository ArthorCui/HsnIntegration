using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utilities.Entriq;

namespace ConnectionString
{
    public partial class CSForm : Form
    {
        public string message = string.Empty;

        public CSForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.textBox_output.Text = string.Empty;

            var connectionStrings =GetConnectionStrings(this.textBox_input.Text);

            var result = EncryptOrDecrypt(connectionStrings);

            result.Values.ToList().ForEach(x =>
            {
                this.textBox_output.Text += string.Format("{0} \r\n", x);
            });

            if (!string.IsNullOrEmpty(message))
            {
                MessageBox.Show(message);
            }
        }

        private List<string> GetConnectionStrings(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
            {
                MessageBox.Show("Please input Encrypt/Decrypt connection strings...");
            }
            var array = inputString.Split(new string[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);

            return array.ToList();
        }

        private Dictionary<string, string> EncryptOrDecrypt(List<string> connectionStrings)
        {
            var outputDic = new Dictionary<string, string>();
            Encryption instance = new Encryption();

            if (connectionStrings.Count > 0)
            {
                connectionStrings.ForEach(x =>
                {
                    if (x.StartsWith("v3basic"))
                    {
                        if (instance.Decrypt(x))
                        {
                            outputDic.Add(x, instance.Result);
                        }
                    }
                    else
                    {
                        if (instance.Encrypt(x))
                        {
                            //if key is exsit
                            if (outputDic.ContainsKey(x))
                            {
                                message += string.Format("input string contains duplicate item:\r\n{0}", x);
                            }
                            outputDic.Add(x, instance.Result);
                        }
                    }
                });
            }
            return outputDic;
        }
    }
}
