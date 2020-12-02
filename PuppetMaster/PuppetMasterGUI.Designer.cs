using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;

namespace PuppetMaster
{
    partial class PuppetMasterGUI
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private Button InitBtn;
        private PuppetMaster puppetMaster;
        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.InitBtn = new System.Windows.Forms.Button();
            this.InitBtn.Click += this.InitBtn_Click;

            this.ServerBtn = new System.Windows.Forms.Button();
            this.ServerBtn.Click += this.ServerBtn_click;


            this.server_server_id = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.PartitionBtn = new System.Windows.Forms.Button();
            this.PartitionBtn.Click += this.PartitionBtn_click;

            this.partition_name = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.partition_server1 = new System.Windows.Forms.RadioButton();
            this.partition_server2 = new System.Windows.Forms.RadioButton();
            this.partition_server3 = new System.Windows.Forms.RadioButton();
            this.ClientBtn = new System.Windows.Forms.Button();
            this.ClientBtn.Click += this.ClientBtn_click;

            this.label6 = new System.Windows.Forms.Label();
            this.client_username = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.client_url = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.client_script = new System.Windows.Forms.TextBox();

            this.StatusBtn = new System.Windows.Forms.Button();
            this.StatusBtn.Click += this.StatusBtn_click;

            this.crash_server1 = new System.Windows.Forms.Button();
            this.crash_server1.Click += this.crash_server;

            this.freeze_server1 = new System.Windows.Forms.Button();
            this.freeze_server1.Click += this.freeze_server;

            this.unfreeze_server1 = new System.Windows.Forms.Button();
            this.unfreeze_server1.Click += this.unfreeze_server;

            this.unfreeze_server2 = new System.Windows.Forms.Button();
            this.unfreeze_server2.Click += this.unfreeze_server;

            this.freeze_server2 = new System.Windows.Forms.Button();
            this.freeze_server2.Click += this.freeze_server;

            this.crash_server2 = new System.Windows.Forms.Button();
            this.crash_server2.Click += this.crash_server;

            this.unfreeze_server3 = new System.Windows.Forms.Button();
            this.unfreeze_server3.Click += this.unfreeze_server;

            this.freeze_server3 = new System.Windows.Forms.Button();
            this.freeze_server3.Click += this.freeze_server;

            this.crash_server3 = new System.Windows.Forms.Button();
            this.crash_server3.Click += this.crash_server;
            
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.textConsole = new System.Windows.Forms.TextBox();
            this.server_url = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.server_min = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.server_max = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.partition_nr = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // InitBtn
            // 
            this.InitBtn.BackColor = System.Drawing.Color.LimeGreen;
            this.InitBtn.Location = new System.Drawing.Point(12, 11);
            this.InitBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.InitBtn.Name = "InitBtn";
            this.InitBtn.Size = new System.Drawing.Size(117, 35);
            this.InitBtn.TabIndex = 0;
            this.InitBtn.Text = "Init";
            this.InitBtn.UseVisualStyleBackColor = false;
            // 
            // ServerBtn
            // 
            this.ServerBtn.Location = new System.Drawing.Point(12, 69);
            this.ServerBtn.Name = "ServerBtn";
            this.ServerBtn.Size = new System.Drawing.Size(117, 33);
            this.ServerBtn.TabIndex = 1;
            this.ServerBtn.Text = "Server";
            this.ServerBtn.UseVisualStyleBackColor = true;
            // 
            // server_server_id
            // 
            this.server_server_id.Location = new System.Drawing.Point(170, 75);
            this.server_server_id.Name = "server_server_id";
            this.server_server_id.Size = new System.Drawing.Size(100, 23);
            this.server_server_id.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(170, 57);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "Server id";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // PartitionBtn
            // 
            this.PartitionBtn.Location = new System.Drawing.Point(12, 139);
            this.PartitionBtn.Name = "PartitionBtn";
            this.PartitionBtn.Size = new System.Drawing.Size(117, 33);
            this.PartitionBtn.TabIndex = 10;
            this.PartitionBtn.Text = "Partition";
            this.PartitionBtn.UseVisualStyleBackColor = true;
            // 
            // partition_name
            // 
            this.partition_name.Location = new System.Drawing.Point(276, 145);
            this.partition_name.Name = "partition_name";
            this.partition_name.Size = new System.Drawing.Size(100, 23);
            this.partition_name.TabIndex = 11;
            // 
            // Partition
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(276, 127);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 15);
            this.label5.TabIndex = 12;
            this.label5.Text = "Partition Name";
            // 
            // partition_server1
            // 
            this.partition_server1.AutoSize = true;
            this.partition_server1.Location = new System.Drawing.Point(382, 149);
            this.partition_server1.Name = "partition_server1";
            this.partition_server1.Size = new System.Drawing.Size(66, 19);
            this.partition_server1.TabIndex = 13;
            this.partition_server1.TabStop = true;
            this.partition_server1.Text = "Server 1";
            this.partition_server1.UseVisualStyleBackColor = true;
            // 
            // partition_server2
            // 
            this.partition_server2.AutoSize = true;
            this.partition_server2.Location = new System.Drawing.Point(454, 149);
            this.partition_server2.Name = "partition_server2";
            this.partition_server2.Size = new System.Drawing.Size(66, 19);
            this.partition_server2.TabIndex = 14;
            this.partition_server2.TabStop = true;
            this.partition_server2.Text = "Server 2";
            this.partition_server2.UseVisualStyleBackColor = true;
            // 
            // partition_server3
            // 
            this.partition_server3.AutoSize = true;
            this.partition_server3.Location = new System.Drawing.Point(526, 149);
            this.partition_server3.Name = "partition_server3";
            this.partition_server3.Size = new System.Drawing.Size(66, 19);
            this.partition_server3.TabIndex = 15;
            this.partition_server3.TabStop = true;
            this.partition_server3.Text = "Server 3";
            this.partition_server3.UseVisualStyleBackColor = true;
            // 
            // ClientBtn
            // 
            this.ClientBtn.Location = new System.Drawing.Point(12, 212);
            this.ClientBtn.Name = "ClientBtn";
            this.ClientBtn.Size = new System.Drawing.Size(117, 33);
            this.ClientBtn.TabIndex = 16;
            this.ClientBtn.Text = "Client";
            this.ClientBtn.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(170, 204);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(60, 15);
            this.label6.TabIndex = 18;
            this.label6.Text = "Username";
            // 
            // client_username
            // 
            this.client_username.Location = new System.Drawing.Point(170, 222);
            this.client_username.Name = "client_username";
            this.client_username.Size = new System.Drawing.Size(100, 23);
            this.client_username.TabIndex = 17;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(276, 204);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(28, 15);
            this.label7.TabIndex = 20;
            this.label7.Text = "URL";
            this.label7.Click += new System.EventHandler(this.label7_Click);
            // 
            // client_url
            // 
            this.client_url.Location = new System.Drawing.Point(276, 222);
            this.client_url.Name = "client_url";
            this.client_url.Size = new System.Drawing.Size(100, 23);
            this.client_url.TabIndex = 19;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(382, 204);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(37, 15);
            this.label8.TabIndex = 22;
            this.label8.Text = "Script";
            // 
            // client_script
            // 
            this.client_script.Location = new System.Drawing.Point(382, 222);
            this.client_script.Name = "client_script";
            this.client_script.Size = new System.Drawing.Size(100, 23);
            this.client_script.TabIndex = 21;
            // 
            // StatusBtn
            // 
            this.StatusBtn.BackColor = System.Drawing.Color.SkyBlue;
            this.StatusBtn.Location = new System.Drawing.Point(12, 270);
            this.StatusBtn.Name = "StatusBtn";
            this.StatusBtn.Size = new System.Drawing.Size(117, 33);
            this.StatusBtn.TabIndex = 23;
            this.StatusBtn.Text = "Status";
            this.StatusBtn.UseVisualStyleBackColor = false;
            // 
            // crash_server1
            // 
            this.crash_server1.BackColor = System.Drawing.Color.Red;
            this.crash_server1.Location = new System.Drawing.Point(102, 383);
            this.crash_server1.Name = "crash_server1";
            this.crash_server1.Size = new System.Drawing.Size(62, 25);
            this.crash_server1.TabIndex = 24;
            this.crash_server1.Text = "Crash";
            this.crash_server1.UseVisualStyleBackColor = false;
            // 
            // freeze_server1
            // 
            this.freeze_server1.BackColor = System.Drawing.Color.AliceBlue;
            this.freeze_server1.Location = new System.Drawing.Point(170, 383);
            this.freeze_server1.Name = "freeze_server1";
            this.freeze_server1.Size = new System.Drawing.Size(62, 25);
            this.freeze_server1.TabIndex = 25;
            this.freeze_server1.Text = "Freeze";
            this.freeze_server1.UseVisualStyleBackColor = false;
            // 
            // unfreeze_server1
            // 
            this.unfreeze_server1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.unfreeze_server1.Location = new System.Drawing.Point(241, 383);
            this.unfreeze_server1.Name = "unfreeze_server1";
            this.unfreeze_server1.Size = new System.Drawing.Size(62, 25);
            this.unfreeze_server1.TabIndex = 26;
            this.unfreeze_server1.Text = "Unfreeze";
            this.unfreeze_server1.UseVisualStyleBackColor = false;
            this.unfreeze_server1.Click += new System.EventHandler(this.button7_Click);
            // 
            // unfreeze_server2
            // 
            this.unfreeze_server2.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.unfreeze_server2.Location = new System.Drawing.Point(241, 424);
            this.unfreeze_server2.Name = "unfreeze_server2";
            this.unfreeze_server2.Size = new System.Drawing.Size(62, 25);
            this.unfreeze_server2.TabIndex = 29;
            this.unfreeze_server2.Text = "Unfreeze";
            this.unfreeze_server2.UseVisualStyleBackColor = false;
            // 
            // freeze_server2
            // 
            this.freeze_server2.BackColor = System.Drawing.Color.AliceBlue;
            this.freeze_server2.Location = new System.Drawing.Point(170, 424);
            this.freeze_server2.Name = "freeze_server2";
            this.freeze_server2.Size = new System.Drawing.Size(62, 25);
            this.freeze_server2.TabIndex = 28;
            this.freeze_server2.Text = "Freeze";
            this.freeze_server2.UseVisualStyleBackColor = false;
            // 
            // crash_server2
            // 
            this.crash_server2.BackColor = System.Drawing.Color.Red;
            this.crash_server2.Location = new System.Drawing.Point(102, 424);
            this.crash_server2.Name = "crash_server2";
            this.crash_server2.Size = new System.Drawing.Size(62, 25);
            this.crash_server2.TabIndex = 27;
            this.crash_server2.Text = "Crash";
            this.crash_server2.UseVisualStyleBackColor = false;
            // 
            // unfreeze_server3
            // 
            this.unfreeze_server3.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.unfreeze_server3.Location = new System.Drawing.Point(241, 466);
            this.unfreeze_server3.Name = "unfreeze_server3";
            this.unfreeze_server3.Size = new System.Drawing.Size(62, 25);
            this.unfreeze_server3.TabIndex = 32;
            this.unfreeze_server3.Text = "Unfreeze";
            this.unfreeze_server3.UseVisualStyleBackColor = false;
            // 
            // freeze_server3
            // 
            this.freeze_server3.BackColor = System.Drawing.Color.AliceBlue;
            this.freeze_server3.Location = new System.Drawing.Point(170, 466);
            this.freeze_server3.Name = "freeze_server3";
            this.freeze_server3.Size = new System.Drawing.Size(62, 25);
            this.freeze_server3.TabIndex = 31;
            this.freeze_server3.Text = "Freeze";
            this.freeze_server3.UseVisualStyleBackColor = false;
            // 
            // crash_server3
            // 
            this.crash_server3.BackColor = System.Drawing.Color.Red;
            this.crash_server3.Location = new System.Drawing.Point(102, 466);
            this.crash_server3.Name = "crash_server3";
            this.crash_server3.Size = new System.Drawing.Size(62, 25);
            this.crash_server3.TabIndex = 30;
            this.crash_server3.Text = "Crash";
            this.crash_server3.UseVisualStyleBackColor = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(28, 388);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(48, 15);
            this.label9.TabIndex = 33;
            this.label9.Text = "Server 1";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(28, 434);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(48, 15);
            this.label10.TabIndex = 34;
            this.label10.Text = "Server 2";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(28, 471);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(48, 15);
            this.label11.TabIndex = 35;
            this.label11.Text = "Server 3";
            // 
            // textConsole
            // 
            this.textConsole.BackColor = System.Drawing.SystemColors.Window;
            this.textConsole.Location = new System.Drawing.Point(309, 270);
            this.textConsole.Multiline = true;
            this.textConsole.Name = "textConsole";
            this.textConsole.ReadOnly = true;
            this.textConsole.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textConsole.Size = new System.Drawing.Size(417, 390);
            this.textConsole.TabIndex = 36;
            // 
            // server_url
            // 
            this.server_url.Location = new System.Drawing.Point(276, 75);
            this.server_url.Name = "server_url";
            this.server_url.Size = new System.Drawing.Size(100, 23);
            this.server_url.TabIndex = 2;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(276, 57);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(28, 15);
            this.label12.TabIndex = 3;
            this.label12.Text = "URL";
            this.label12.Click += new System.EventHandler(this.label1_Click);
            // 
            // server_min
            // 
            this.server_min.Location = new System.Drawing.Point(382, 75);
            this.server_min.Name = "server_min";
            this.server_min.Size = new System.Drawing.Size(100, 23);
            this.server_min.TabIndex = 4;
            this.server_min.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(382, 57);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(87, 15);
            this.label13.TabIndex = 5;
            this.label13.Text = "Min Delay (ms)";
            this.label13.Click += new System.EventHandler(this.label2_Click);
            // 
            // server_max
            // 
            this.server_max.Location = new System.Drawing.Point(488, 75);
            this.server_max.Name = "server_max";
            this.server_max.Size = new System.Drawing.Size(100, 23);
            this.server_max.TabIndex = 6;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(488, 57);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(89, 15);
            this.label14.TabIndex = 7;
            this.label14.Text = "Max Delay (ms)";
            // 
            // partition_nr
            // 
            this.partition_nr.Location = new System.Drawing.Point(170, 145);
            this.partition_nr.Name = "partition_nr";
            this.partition_nr.Size = new System.Drawing.Size(100, 23);
            this.partition_nr.TabIndex = 37;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(170, 127);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 15);
            this.label2.TabIndex = 38;
            this.label2.Text = "Nr of Replicas";
            // 
            // PuppetMasterGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(728, 663);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.partition_nr);
            this.Controls.Add(this.textConsole);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.unfreeze_server3);
            this.Controls.Add(this.freeze_server3);
            this.Controls.Add(this.crash_server3);
            this.Controls.Add(this.unfreeze_server2);
            this.Controls.Add(this.freeze_server2);
            this.Controls.Add(this.crash_server2);
            this.Controls.Add(this.unfreeze_server1);
            this.Controls.Add(this.freeze_server1);
            this.Controls.Add(this.crash_server1);
            this.Controls.Add(this.StatusBtn);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.client_script);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.client_url);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.client_username);
            this.Controls.Add(this.ClientBtn);
            this.Controls.Add(this.partition_server3);
            this.Controls.Add(this.partition_server2);
            this.Controls.Add(this.partition_server1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.partition_name);
            this.Controls.Add(this.PartitionBtn);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.server_max);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.server_min);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.server_url);
            this.Controls.Add(this.server_server_id);
            this.Controls.Add(this.ServerBtn);
            this.Controls.Add(this.InitBtn);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "PuppetMasterGUI";
            this.Text = "Puppet Master";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void InitBtn_Click(object sender, EventArgs e)
        {
            if (puppetMaster == null)
            {
                this.puppetMaster = new PuppetMaster(this);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Only one instance of PuppetMaster is authorized");

            }



        }
        private void ServerBtn_click(object sender, EventArgs e)
        {
            if (checkIfInit())
            {
                try
                {
                    puppetMaster.Server(Int32.Parse(this.server_server_id.Text), this.server_url.Text, Int32.Parse(this.server_min.Text), Int32.Parse(this.server_max.Text));
                }catch(Exception exc)
                {
                    System.Windows.Forms.MessageBox.Show("Wrong syntax, please correct");

                }
            }
        }

        private void PartitionBtn_click(object sender, EventArgs e)
        {
            if (checkIfInit())
            {
                try
                {
                    List<int> servers = new List<int>();
                    if (partition_server1.Checked)
                        servers.Add(1);
                    if (partition_server2.Checked)
                        servers.Add(2);
                    if (partition_server3.Checked)
                        servers.Add(3);
                    if (servers.Count == 0)
                    {
                        System.Windows.Forms.MessageBox.Show("Please select at least one server");
                        return;
                    }
                    else
                    {
                        puppetMaster.Partition(Int32.Parse(this.partition_nr.Text), Int32.Parse(this.partition_name.Text), servers.ToArray());
                    }
                }
                catch (Exception exc)
                {
                    System.Windows.Forms.MessageBox.Show("Wrong syntax, please correct");

                }
            }
        }

        private void ClientBtn_click(object sender, EventArgs e)
        {
            if (checkIfInit())
            {
                try
                {
                    puppetMaster.Client(Int32.Parse(this.client_username.Text), this.client_url.Text, client_url.Text);
                }
                catch (Exception exc)
                {
                    System.Windows.Forms.MessageBox.Show("Wrong syntax, please correct");

                }
            }

        }

        private void StatusBtn_click(object sender, EventArgs e)
        {
            if (checkIfInit())
            {
                try
                {
                    puppetMaster.Status();
                }
                catch (Exception exc)
                {
                    System.Windows.Forms.MessageBox.Show("Wrong syntax, please correct");

                }
            }
        }

        private void unfreeze_server(object sender, EventArgs e)
        {
            if (checkIfInit())
            {
                try
                {
                    Button btn = (Button)sender;
                    puppetMaster.Crash(Int32.Parse(new string(btn.Name.Replace("unfreeze_server", ""))));
                }
                catch (Exception exc)
                {
                    System.Windows.Forms.MessageBox.Show("Something went wrong");

                }
            }
        }

        private void freeze_server(object sender, EventArgs e)
        {
            if (checkIfInit())
            {
                try
                {
                    Button btn = (Button)sender;
                    puppetMaster.Freeze(Int32.Parse(new string(btn.Name.Replace("freeze_server", ""))));
                }
                catch (Exception exc)
                {
                    System.Windows.Forms.MessageBox.Show("Something went wrong");

                }
            }
        }

        private void crash_server(object sender, EventArgs ed)
        {
            if (checkIfInit())
            {
                try
                {

                    Button btn = (Button)sender;
                    puppetMaster.Crash(Int32.Parse(new string(btn.Name.Replace("crash_server", ""))));
                }
                catch (Exception exc)
                {
                    System.Windows.Forms.MessageBox.Show("Something went wrong");

                }
            }

        }
        private delegate void SafeCallDelegate(string text);

        public void WriteLine(string s)
        {
            if (textConsole.InvokeRequired)
            {
                var d = new SafeCallDelegate(WriteLine);
                textConsole.Invoke(d, new object[] { s });
            }
            else
            {
                Debug.WriteLine("Reached console: " + s);
                textConsole.AppendText(s);
                textConsole.AppendText(Environment.NewLine);
            }
        }
        private bool checkIfInit() {
            if (puppetMaster == null)
            {
                System.Windows.Forms.MessageBox.Show("Puppet Master is not initialize. Click Init");
                return false;
            }
            else return true;
        }

        #endregion

        private Button ServerBtn;
        private TextBox server_server_id;
        private Label label1;
        private Button PartitionBtn;
        private TextBox partition_name;
        private Label label5;
        private RadioButton partition_server1;
        private RadioButton partition_server2;
        private RadioButton partition_server3;
        private Button ClientBtn;
        private Label label6;
        private TextBox client_username;
        private Label label7;
        private TextBox client_url;
        private Label label8;
        private TextBox client_script;
        private Button StatusBtn;
        private Button crash_server1;
        private Button freeze_server1;
        private Button unfreeze_server1;
        private Button unfreeze_server2;
        private Button freeze_server2;
        private Button crash_server2;
        private Button unfreeze_server3;
        private Button freeze_server3;
        private Button crash_server3;
        private Label label9;
        private Label label10;
        private Label label11;
        public TextBox textConsole;
        private TextBox server_url;
        private Label label12;
        private TextBox server_min;
        private Label label13;
        private TextBox server_max;
        private Label label14;
        private TextBox partition_nr;
        private Label label2;
    }
}

