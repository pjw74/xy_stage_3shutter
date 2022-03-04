using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using System.IO.Ports;

using System.Text.RegularExpressions;

using System.IO;
using System.Diagnostics;

using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.KCube.SolenoidCLI;



namespace xz_3shutter_v1
{
    public partial class Form1 : Form
    {

        delegate int on_off(int sigh);

        KCubeSolenoid shutter_device1;
        KCubeSolenoid shutter_device2;
        KCubeSolenoid shutter_device3;

        string shutter_serialNo1;
        string shutter_serialNo2;
        string shutter_serialNo3;

        CycleSettings settings = new CycleSettings();

        uint signal1, signal2, signal3;

        Socket sock;

        byte dummy = 0xff;
        byte stx = 0x02;
        byte etx = 0x03;
        byte ACK = 0x06;
        byte nak = 0x15;
        byte rst = 0x12;

        string x_position, y_position, z_position, w_position;
        string error_result_st;

        string spd_str = "0000";

        string ltn_stra1 = "00000.0000";
        string ltn_stra2 = "00000.0000";

        string status = "";

        string ltn_stra5 = "    0.000 ";//일의 자리 수일 때         

        char[] x_pos = new char[10];
        char[] y_pos = new char[10];
        char[] z_pos = new char[10];
        char[] w_pos = new char[10];

        double x_start, y_start, z_start;


        CycleSettings settings1 = new CycleSettings();
        CycleSettings settings2 = new CycleSettings();
        CycleSettings settings3 = new CycleSettings();






        double x_stage_point, y_stage_point, z_stage_point, w_stage_point;



        decimal x_complete, y_complete, z_complete, w_complete;




        public Form1()
        {
            InitializeComponent();
        }








        private void Form1_Load(object sender, EventArgs e)
        {
            scanning_radio.Checked = true;
            spot_radio.Checked = true;
            spot_radio.Checked = false;

            //scanning_radio.



        }


        public byte[] speed(string channel, string spd)
        {
            byte[] command = Encoding.UTF8.GetBytes("CB");
            byte[] channel_ba = Encoding.UTF8.GetBytes(channel);
            byte[] speed_set = Encoding.UTF8.GetBytes(spd);
            byte[] make_msg = Combine(command, channel_ba);
            make_msg = Combine(make_msg, speed_set);

            byte lrc = lrc_cal(make_msg);

            byte[] etx_ba = new byte[] { etx };
            byte[] lrc_ba = new byte[] { lrc };

            make_msg = Combine(make_msg, etx_ba);
            make_msg = Combine(make_msg, lrc_ba);

            return make_msg;
        }



        private void z_position_compare(byte[] msg) //좌표 수신
        {
            byte[] bytes = new byte[50];

            byte[] ack = new byte[] { ACK };
            byte[] header = new byte[] { stx, dummy };

            msg = Combine(header, msg);

            if (sock.Available > 0) // here we clean up the current queue
            {
                sock.Receive(bytes);
                //sock1.Receive(bytes1);
            }

            sock.Send(msg);

            while (sock.Available == 0) // wait for the controller response
            {
                Thread.Sleep(10);
            }

            sock.Receive(bytes); // after receiving the data, we should check the LRC if possible

            if (bytes.Contains<byte>(nak) || bytes.Contains<byte>(rst) == true)
            {
                sock.Send(msg);
            }
            else
            {
                sock.Send(ack);
            }

            status = Encoding.UTF8.GetString(bytes);


            //status.CopyTo(3, x_pos, 0, 10);
            //status.CopyTo(13, y_pos, 0, 10);
            status.CopyTo(23, z_pos, 0, 7);
            status.CopyTo(33, w_pos, 0, 7);

            //int val = Convert.ToInt32(x_pos[8]);
            //x_pos[8] = Convert.ToChar(48);//Convert.ToChar(val - 1);

            //x_position = new string(x_pos);//비교를 위해 초기값 저장
            //x_position = Math.Abs(x_position);

            //x_position = x_position.Trim();

            z_position = new string(z_pos);//비교를 위해 초기값 저장
            z_position = z_position.Trim();

            w_position = new string(w_pos);//비교를 위해 초기값 저장
            w_position = w_position.Trim();

            msg.Initialize();
            bytes.Initialize();
        }






        public byte[] move_axis_all_z(string channel, double x_axis_point, double z_axis_point)
        {
            byte[] command = Encoding.UTF8.GetBytes("BC");
            byte[] channel_ba = Encoding.UTF8.GetBytes(channel);
            byte[] motion_type = Encoding.UTF8.GetBytes("0");
            byte[] xy_type = Encoding.UTF8.GetBytes("1");

            decimal null_byte = 0;
            string null_st_fn = null_byte.ToString(ltn_stra2);

            decimal x_st = Convert.ToDecimal(x_axis_point);
            string x_st_fn = x_st.ToString(ltn_stra2);
            byte[] location_x = Encoding.UTF8.GetBytes(x_st_fn);

            //decimal y_st = Convert.ToDecimal(y_axis_point);
            string y_st_fn = ltn_stra2.ToString();
            byte[] location_y = Encoding.UTF8.GetBytes(y_st_fn);

            decimal z_st = Convert.ToDecimal(z_axis_point);
            string z_st_fn = z_st.ToString(ltn_stra2);
            byte[] location_z1 = Encoding.UTF8.GetBytes(z_st_fn);

            decimal w_st = Convert.ToDecimal(z_axis_point);
            string w_st_fn = z_st.ToString(ltn_stra2);
            byte[] location_w1 = Encoding.UTF8.GetBytes(z_st_fn);

            //byte[] location_null_z1 = Encoding.UTF8.GetBytes(null_st_fn);

            //byte[] location_z2 = Encoding.UTF8.GetBytes(z_st_fn);
            //byte[] location_null_z2 = Encoding.UTF8.GetBytes(null_st_fn);

            //byte[] xy_location_final = Combine(xy_location1, xy_location2);
            //byte[] xy_location_final1 = Combine(xy_location2, xy_location3);
            //byte[] xy_location_final2 = Combine(xy_location3, xy_location4);

            //byte[] xy_location_final = Combine(xy_location1, xy_location_null);
            //xy_location_final = Combine(xy_location_final, xy_location2);
            //xy_location_final = Combine(xy_location_final, xy_location_null2);

            byte[] make_msg = Combine(command, channel_ba);
            make_msg = Combine(make_msg, motion_type);
            make_msg = Combine(make_msg, xy_type);

            make_msg = Combine(make_msg, location_x);
            make_msg = Combine(make_msg, location_y);
            make_msg = Combine(make_msg, location_z1);
            make_msg = Combine(make_msg, location_z1);

            byte lrc = lrc_cal(make_msg);

            byte[] etx_ba = new byte[] { etx };
            byte[] lrc_ba = new byte[] { lrc };

            make_msg = Combine(make_msg, etx_ba);
            make_msg = Combine(make_msg, lrc_ba);

            return make_msg;
        }

        public byte[] move_axis_x(string channel, double x_axis_point, double z_axis_point)
        {
            byte[] command = Encoding.UTF8.GetBytes("BC");
            byte[] channel_ba = Encoding.UTF8.GetBytes(channel);
            byte[] motion_type = Encoding.UTF8.GetBytes("0");
            byte[] xy_type = Encoding.UTF8.GetBytes("1");

            decimal null_byte = 0;
            string null_st_fn = null_byte.ToString(ltn_stra2);

            decimal x_st = Convert.ToDecimal(x_axis_point);
            string x_st_fn = x_st.ToString(ltn_stra2);
            byte[] location_x = Encoding.UTF8.GetBytes(x_st_fn);

            //decimal y_st = Convert.ToDecimal(y_axis_point);
            string y_st_fn = ltn_stra2.ToString();
            byte[] location_y = Encoding.UTF8.GetBytes(y_st_fn);

            decimal z_st = Convert.ToDecimal(z_axis_point);
            string z_st_fn = z_st.ToString(ltn_stra2);
            byte[] location_z1 = Encoding.UTF8.GetBytes(z_st_fn);

            decimal w_st = Convert.ToDecimal(z_axis_point);
            string w_st_fn = z_st.ToString(ltn_stra2);
            byte[] location_w1 = Encoding.UTF8.GetBytes(z_st_fn);


            //byte[] location_null_z1 = Encoding.UTF8.GetBytes(null_st_fn);

            //byte[] location_z2 = Encoding.UTF8.GetBytes(z_st_fn);
            //byte[] location_null_z2 = Encoding.UTF8.GetBytes(null_st_fn);

            //byte[] xy_location_final = Combine(xy_location1, xy_location2);
            //byte[] xy_location_final1 = Combine(xy_location2, xy_location3);
            //byte[] xy_location_final2 = Combine(xy_location3, xy_location4);

            //byte[] xy_location_final = Combine(xy_location1, xy_location_null);
            //xy_location_final = Combine(xy_location_final, xy_location2);
            //xy_location_final = Combine(xy_location_final, xy_location_null2);

            byte[] make_msg = Combine(command, channel_ba);
            make_msg = Combine(make_msg, motion_type);
            make_msg = Combine(make_msg, xy_type);

            make_msg = Combine(make_msg, location_x);
            make_msg = Combine(make_msg, location_y);
            make_msg = Combine(make_msg, location_z1);
            make_msg = Combine(make_msg, location_z1);

            byte lrc = lrc_cal(make_msg);

            byte[] etx_ba = new byte[] { etx };
            byte[] lrc_ba = new byte[] { lrc };

            make_msg = Combine(make_msg, etx_ba);
            make_msg = Combine(make_msg, lrc_ba);

            return make_msg;
        }






        private void send_position(byte[] msg) //좌표 수신
        {

            byte[] bytes = new byte[50];

            byte[] ack = new byte[] { ACK };
            byte[] header = new byte[] { stx, dummy };

            msg = Combine(header, msg);

            if (sock.Available > 0) // here we clean up the current queue
            {
                sock.Receive(bytes);
                //sock1.Receive(bytes1);
            }

            sock.Send(msg);

            while (sock.Available == 0) // wait for the controller response
            {
                Thread.Sleep(100);
            }

            sock.Receive(bytes); // after receiving the data, we should check the LRC if possible

            if (bytes.Contains<byte>(nak) || bytes.Contains<byte>(rst) == true)
            {
                sock.Send(msg);
            }
            else
            {
                sock.Send(ack);
            }

            status = Encoding.UTF8.GetString(bytes);

            status.CopyTo(3, x_pos, 0, 8);
            //status.CopyTo(13, y_pos, 0, 7);
            status.CopyTo(23, z_pos, 0, 8);
            status.CopyTo(33, w_pos, 0, 8);

            //int val = Convert.ToInt32(x_pos[8]);
            //x_pos[8] = Convert.ToChar(48);//Convert.ToChar(val - 1);
            //int x_pos_abs = Convert.ToInt16(x_pos);

            x_position = new string(x_pos);//비교를 위해 초기값 저장
            x_position = x_position.Trim();

            decimal x_pos_abs = Convert.ToDecimal(x_position);
            x_pos_abs = Math.Abs(x_pos_abs);
            x_position = Convert.ToString(x_pos_abs);

            y_position = new string(y_pos);//비교를 위해 초기값 저장
            y_position = y_position.Trim();

            //decimal y_pos_abs = Convert.ToDecimal(y_position);
            //y_pos_abs = Math.Abs(y_pos_abs);
            //y_position = Convert.ToString(y_pos_abs);

            z_position = new string(z_pos);
            z_position = z_position.Trim();

            decimal z_pos_abs = Convert.ToDecimal(z_position);
            z_pos_abs = Math.Abs(z_pos_abs);
            z_position = Convert.ToString(z_pos_abs);

            msg.Initialize();
            bytes.Initialize();
        }


        private void posi_chk_Click(object sender, EventArgs e)
        {
            var comm = posi_check_robot("0");
            send_position(comm);
            // Compare_string(x_stage_point, y_stage_point);
            //Task.Run(() => SetText());
            this.Invoke(new Action(SetText));
        }

        public byte[] posi_check_robot(string channel) //robot_position chk
        {
            byte[] command = Encoding.UTF8.GetBytes("AC");
            byte[] channel_ba = Encoding.UTF8.GetBytes(channel);
            byte[] data_type = Encoding.UTF8.GetBytes("2");
            byte[] make_msg = Combine(command, channel_ba);
            make_msg = Combine(make_msg, data_type);

            byte lrc = lrc_cal(make_msg);

            byte[] etx_ba = new byte[] { etx };
            byte[] lrc_ba = new byte[] { lrc };

            make_msg = Combine(make_msg, etx_ba);
            make_msg = Combine(make_msg, lrc_ba);

            return make_msg;
        }

        private void error_check_Click(object sender, EventArgs e)
        {

        }

        public void SetText()
        {
            lb_x_position.Text = x_position + "mm";
            //lb_y_position.Text = y_position;
            lb_z_position.Text = z_position + "mm";
        }
        private void sv_off_Click(object sender, EventArgs e)
        {

            var comm = servo_off("0");
            send_function(comm);
        }



        public byte[] servo_off(string channel)
        {
            byte[] command = Encoding.UTF8.GetBytes("DB");
            byte[] channel_ba = Encoding.UTF8.GetBytes(channel);
            byte[] data_type = Encoding.UTF8.GetBytes("0");
            byte[] make_msg = Combine(command, channel_ba);
            make_msg = Combine(make_msg, data_type);

            byte lrc = lrc_cal(make_msg);

            byte[] etx_ba = new byte[] { etx };
            byte[] lrc_ba = new byte[] { lrc };

            make_msg = Combine(make_msg, etx_ba);
            make_msg = Combine(make_msg, lrc_ba);

            return make_msg;
        }








        public static byte[] Combine(byte[] first, byte[] second) //byte 결합하는 함수에 관한 부분
        {
            return first.Concat(second).ToArray();
        }





        public byte lrc_cal(byte[] data)  //명령어 LRC 계산하는 부분
        {

            //byte XOR 연산
            byte lrc = dummy;

            for (int n = 0; n < data.Length; n++)
            {
                lrc = (byte)(lrc ^ data[n]);
            }

            if (lrc == 0)
            {
                lrc = etx;
            }

            return lrc;
        }







        private void robo_con_Click(object sender, EventArgs e)
        {
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ip = IPAddress.Parse("192.168.1.203");//인자값 : 서버측 IP         
            IPEndPoint endPoint = new IPEndPoint(ip, 20000);//인자값 : IPAddress,포트번호

            while (sock.Connected == false)
            {
                sock.Connect(endPoint);
            }

            if (sock.Connected == true)
            {
                textBox1.Text = ("Stage Connected");
                robo_con.Enabled = false;
            }
        }



        private void send_function(byte[] msg)
        {
            byte[] bytes = new byte[50];

            byte[] ack = new byte[] { ACK };
            byte[] header = new byte[] { stx, dummy };

            msg = Combine(header, msg);

            if (sock.Available > 0) // here we clean up the current queue
            {
                sock.Receive(bytes);
            }

            sock.Send(msg);

            while (sock.Available == 0) // wait for the controller response
            {
                Thread.Sleep(100);
            }

            sock.Receive(bytes); // after receiving the data, we should check the LRC if possible
                                 //string status = Encoding.UTF8.GetString(bytes);

            if (bytes.Contains<byte>(nak) || bytes.Contains<byte>(rst) == true)
            {
                sock.Send(msg);
            }
            else
            {
                sock.Send(ack);
            }

            status = Encoding.UTF8.GetString(bytes);

            //msg.Initialize();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void origin_Click(object sender, EventArgs e)
        {
            var comm = move_zero("0");
            send_function(comm);
        }



        public byte[] move_zero(string channel) // 원점 이동 부분
        {
            byte[] command = Encoding.UTF8.GetBytes("BA");
            byte[] channel_ba = Encoding.UTF8.GetBytes(channel);
            byte[] make_msg = Combine(command, channel_ba);

            byte lrc = lrc_cal(make_msg);

            byte[] etx_ba = new byte[] { etx };
            byte[] lrc_ba = new byte[] { lrc };

            make_msg = Combine(make_msg, etx_ba);
            make_msg = Combine(make_msg, lrc_ba);

            return make_msg;
        }


        private void sv_on_Click(object sender, EventArgs e)
        {
            var comm = servo_on("0");
            send_function(comm);
        }

        public byte[] servo_on(string channel)
        {
            byte[] command = Encoding.UTF8.GetBytes("DB");
            byte[] channel_ba = Encoding.UTF8.GetBytes(channel);
            byte[] data_type = Encoding.UTF8.GetBytes("1");
            byte[] make_msg = Combine(command, channel_ba);
            make_msg = Combine(make_msg, data_type);

            byte lrc = lrc_cal(make_msg);

            byte[] etx_ba = new byte[] { etx };
            byte[] lrc_ba = new byte[] { lrc };

            make_msg = Combine(make_msg, etx_ba);
            make_msg = Combine(make_msg, lrc_ba);

            return make_msg;
        }










        private void shut_con_Click(object sender, EventArgs e)
        {
            serial_number_load();



            try

            {
                // Tell the device manager to get the list of all devices connected to the computer
                DeviceManagerCLI.BuildDeviceList();
            }
            catch (Exception ex)
            {
                // An error occurred - see ex for details
                Console.WriteLine("Exception raised by BuildDeviceList {0}", ex);
                //Console.ReadKey();
                return;
            }

            // Get available KCube Solenoid and check our serial number is correct - by using the device prefix
            // (i.e. for serial number 68000123, the device prefix is 68)
            List<string> serialNumbers = DeviceManagerCLI.GetDeviceList(KCubeSolenoid.DevicePrefix);

            if (!serialNumbers.Contains(shutter_serialNo1))
            {
                // The requested serial number is not a KSC or is not connected
                Console.WriteLine("{0} is not a valid serial number", shutter_serialNo1);
                //Console.ReadKey();
                return;
            }

            // Create the device
            shutter_device1 = KCubeSolenoid.CreateKCubeSolenoid(shutter_serialNo1);
            shutter_device2 = KCubeSolenoid.CreateKCubeSolenoid(shutter_serialNo2);
            shutter_device3 = KCubeSolenoid.CreateKCubeSolenoid(shutter_serialNo3);

            if (shutter_device1 == null)
            {
                // An error occured
                Console.WriteLine("{0} is not a KCubeSolenoid", shutter_serialNo1);

                //Console.ReadKey();
                return;
            }

            // Open a connection to the device.
            try
            {
                Console.WriteLine("Opening device {0}", shutter_serialNo1);

                shutter_device1.Connect(shutter_serialNo1);
                shutter_device2.Connect(shutter_serialNo2);
                shutter_device3.Connect(shutter_serialNo3);

            }
            catch (Exception)
            {
                // Connection failed
                Console.WriteLine("Failed to open device {0}", shutter_serialNo1);
                // Console.ReadKey();
                return;
            }

            // Wait for the device settings to initialize - timeout 5000ms
            if (!shutter_device1.IsSettingsInitialized())
            {
                try
                {
                    shutter_device1.WaitForSettingsInitialized(5000);
                }
                catch (Exception)
                {
                    Console.WriteLine("Settings failed to initialize");
                }
            }

            if (!shutter_device2.IsSettingsInitialized())
            {
                try
                {
                    shutter_device2.WaitForSettingsInitialized(5000);
                }
                catch (Exception)
                {
                    Console.WriteLine("Settings failed to initialize");
                }
            }

            if (!shutter_device3.IsSettingsInitialized())
            {
                try
                {
                    shutter_device3.WaitForSettingsInitialized(5000);
                }
                catch (Exception)
                {
                    Console.WriteLine("Settings failed to initialize");
                }
            }
            // Display info about device
            DeviceInfo deviceInfo1 = shutter_device1.GetDeviceInfo();
            DeviceInfo deviceInfo2 = shutter_device2.GetDeviceInfo();
            DeviceInfo deviceInfo3 = shutter_device3.GetDeviceInfo();

            Console.WriteLine("Device {0} = {1}", deviceInfo1.SerialNumber, deviceInfo1.Name);
            Console.WriteLine("Device {0} = {1}", deviceInfo2.SerialNumber, deviceInfo2.Name);
            Console.WriteLine("Device {0} = {1}", deviceInfo3.SerialNumber, deviceInfo3.Name);

            // Start the device polling
            // The polling loop requests regular status requests to the motor to ensure the program keeps track of the device. 
            shutter_device1.StartPolling(250);
            shutter_device2.StartPolling(250);
            shutter_device3.StartPolling(250);

            // Needs a delay so that the current enabled state can be obtained
            Thread.Sleep(500);
            // Enable the channel otherwise any move is ignored 
            shutter_device1.EnableDevice();
            shutter_device2.EnableDevice();
            shutter_device3.EnableDevice();
            // Needs a delay to give time for the device to be enabled
            Thread.Sleep(500);

            // get Device Configuration
            SolenoidConfiguration solenoidConfiguration1 = shutter_device1.GetSolenoidConfiguration(shutter_serialNo1);
            ThorlabsKCubeSolenoidSettings currentDeviceSettings1 = ThorlabsKCubeSolenoidSettings.GetSettings(solenoidConfiguration1);

            SolenoidConfiguration solenoidConfiguration2 = shutter_device2.GetSolenoidConfiguration(shutter_serialNo2);
            ThorlabsKCubeSolenoidSettings currentDeviceSettings2 = ThorlabsKCubeSolenoidSettings.GetSettings(solenoidConfiguration2);

            SolenoidConfiguration solenoidConfiguration3 = shutter_device3.GetSolenoidConfiguration(shutter_serialNo3);
            ThorlabsKCubeSolenoidSettings currentDeviceSettings3 = ThorlabsKCubeSolenoidSettings.GetSettings(solenoidConfiguration3);




            bool shutter1_connect = DeviceManagerCLI.IsDeviceConnected(shutter_serialNo1);
            bool shutter2_connect = DeviceManagerCLI.IsDeviceConnected(shutter_serialNo2);
            bool shutter3_connect = DeviceManagerCLI.IsDeviceConnected(shutter_serialNo3);

            if (shutter1_connect && shutter2_connect && shutter3_connect == true)
            {

                textBox1.AppendText("\r\n");
                textBox1.AppendText("Shutter Connected");

                // shut_con.Enabled = false;
                shut_con.Enabled = false;

                if (expo_time_r.Value == 0 && expo_time_g.Value == 0 && expo_time_b.Value == 0)
                {
                    MessageBox.Show("노출 시간을 설정하세요!");

                }
                //shutter_con1.Enabled = false;
            }
        }

        private void serial_number_load()
        {
            string savePath = @"./Serial Number.txt";
            //all_serial = System.IO.File.ReadAllLines(savePath);

            string a = System.IO.File.ReadAllText(savePath);


            //string shut_serial1;// = "68250283";
            //string shut_serial2;// = "68250366";
            //string shut_serial3;// = "68250378";

            //string[] serial = new string[20];
            //all_serial.CopyTo(serial, 0);



            string[] all_serial1 = a.Split(new char[] { ',' });

            //int c = all_serial1.Count < "6825" > ();

            //string[] shutter_serial_num = new string[all_serial1.Length];

            for (int i = 0; i < all_serial1.Length; i++)
            {
                if (all_serial1[i].Contains(" "))
                {
                    all_serial1[i] = all_serial1[i].Trim();

                }
                if (all_serial1[i].Contains("6825"))
                {

                    //shutter_serial_num[i] = all_serial1[i].Trim();
                    serial_count(i, all_serial1[i]);


                    //shutter_serialNo2 = all_serial1[1].Trim();
                    //shutter_serialNo3 = all_serial1[2].Trim();
                }
                else
                {
                    //if(all_serial1[i].)
                    //serialNo_lts = all_serial1[i];
                    //serialNo_lts.Trim();

                    //stage_serialNo = all_serial1[i];
                    //stage_serialNo.Trim();
                }

            }


        }
        private void serial_count(int i, string shut_count)
        {
            shut_count.Trim();

            switch (i)
            {
                case 0:
                    shutter_serialNo1 = shut_count;
                    break;
                case 1:
                    shutter_serialNo2 = shut_count;
                    break;
                case 2:
                    shutter_serialNo3 = shut_count;
                    break;


            }
            //return;
        }







        private async void jog_move_Click(object sender, EventArgs e)
        {
            int open_delay = Convert.ToInt32(shut_delay.Value) * 1000;


            x_stage_point = Convert.ToDouble(x_srt.Value);
            z_stage_point = Convert.ToDouble(z_srt.Value);
            w_stage_point = Convert.ToDouble(z_srt.Value);

            double x_set = Convert.ToDouble(x_position);
            double z_set = Convert.ToDouble(z_position);
            double w_set = Convert.ToDouble(w_position);

            decimal speed_1 = spd_value.Value;
            string spd = speed_1.ToString(spd_str);
            var comm = speed("0", spd);
            send_function(comm);




            if (scanning_radio.Checked)
            {
                //if(x_stage_point + )
                if (x_stage_point + Convert.ToDouble(scanning_pitch.Value) >= 800.000)
                {
                    MessageBox.Show("Stage Pitch Limit");
                    return;
                }


                shutter_device1.SetOperatingMode(SolenoidStatus.OperatingModes.Manual);
                shutter_device2.SetOperatingMode(SolenoidStatus.OperatingModes.Manual);
                shutter_device3.SetOperatingMode(SolenoidStatus.OperatingModes.Manual);

                Thread.Sleep(300);




                if (textBox3.Text == "setup")
                {

                    
                    
                    //for (int i = 0; i < y_count_1; i++) //y가 1씩 증가할 때
                    //{
                    // }

                    //z_stage_point = z_up_move(z_stage_point);
                    //var comm_z_up = move_axis_all_z("0", x_stage_point, y_stage_point, z_stage_point);
                    //send_function(comm_z_up);

                    //z_up_db = CustomRound(RoundType.Truncate, z_stage_point, 2);

                    //Thread.Sleep(500);


                    //on_off on = (sigh) => 
                    //var shutter_check = Task.Factory.StartNew(three_shutter_open_manual(0));
                    var shutter_open = Task.Factory.StartNew(three_shutter_open_manual);
                    //shutter_check.Wait();


                    //x scanning pitch까지 이동
                    //이동체크 후 셔터 메뉴얼 클로즈
                    var task_scanning = Task.Run(() =>
                    {
                        x_start = x_stage_point;
                        z_start = z_stage_point;

                        x_stage_point += Convert.ToDouble(scanning_pitch.Value);


                        double x_setup_db = CustomRound(RoundType.Truncate, x_stage_point, 3);
                        double z_setup_db = CustomRound(RoundType.Truncate, z_stage_point, 3);



                        var comm_move_xz = move_axis_all_z("0", x_stage_point, z_stage_point);
                        send_function(comm_move_xz);

                        Thread.Sleep(1000);


                        while (true)
                        {
                            var comm_posi = posi_check_robot("0");
                            send_position(comm_posi);

                            x_complete = Convert.ToDecimal(x_position);
                            z_complete = Convert.ToDecimal(z_position);

                            if (x_complete == Convert.ToDecimal(x_setup_db) && z_complete == Convert.ToDecimal(z_setup_db))
                            // y축 좌표 비교 이동 완료
                            {

                                var shutter_close = Task.Factory.StartNew(three_shutter_close_manual);
                                break;
                            }
                            else
                            {
                                //    var comm_1 = move_axis_all("0", x_stage_point, y_stage_point, 0);
                                //     send_function(comm_1);

                                var comm_move_xz_re = move_axis_all_z("0", x_stage_point, z_stage_point);
                                send_function(comm_move_xz_re);
                            }
                        }

                    });
                    //setup_point.Enabled = false;
                    await task_scanning;//.Wait();
                }

                setup_point.Enabled = true;

                //});
                //await task_all_end;
               
            }





            if (spot_radio.Checked && textBox3.Text == "setup")                
            {
                double x_spot_pitch_1 = Convert.ToDouble(spot_pitch.Value);
                int a = Convert.ToInt32(spot_count.Value) - 1;

                
                
                if (x_stage_point + (x_spot_pitch_1 * a) > 800.000)
                {
                    MessageBox.Show("Stage Pitch Limit");
                    return;
                }
                

                shutter_device1.SetOperatingMode(SolenoidStatus.OperatingModes.SingleToggle);
                shutter_device2.SetOperatingMode(SolenoidStatus.OperatingModes.SingleToggle);
                shutter_device3.SetOperatingMode(SolenoidStatus.OperatingModes.SingleToggle);



                //open, close, move, move_chk, open, close 
                x_start = x_stage_point;
                z_start = z_stage_point;

                var shutter_open_single = Task.Factory.StartNew(three_shutter_open_single);
                await shutter_open_single;


                for (int i = 1; i < spot_count.Value; i++)
                {

                    var task_spot = Task.Run(() =>
                    {                       

                        x_stage_point += Convert.ToDouble(spot_pitch.Value);

                        double x_setup_db = CustomRound(RoundType.Truncate, x_stage_point, 3);
                        double z_setup_db = CustomRound(RoundType.Truncate, z_stage_point, 3);

                        var comm_move_xz = move_axis_all_z("0", x_stage_point, z_stage_point);
                        send_function(comm_move_xz);

                        Thread.Sleep(1000);


                        while (true)
                        {
                            var comm_posi = posi_check_robot("0");
                            send_position(comm_posi);

                            x_complete = Convert.ToDecimal(x_position);
                            z_complete = Convert.ToDecimal(z_position);

                            if (x_complete == Convert.ToDecimal(x_setup_db) && z_complete == Convert.ToDecimal(z_setup_db))
                            // y축 좌표 비교 이동 완료
                            {
                                Thread.Sleep(open_delay);

                                var shutter_open_single2 = Task.Factory.StartNew(three_shutter_open_single);
                                shutter_open_single2.Wait();

                                break;
                            }
                            else
                            {
                            //    var comm_1 = move_axis_all("0", x_stage_point, y_stage_point, 0);
                            //     send_function(comm_1);

                                var comm_move_xz_re = move_axis_all_z("0", x_stage_point, z_stage_point);
                                send_function(comm_move_xz_re);
                            }
                        }

                    });
                    //setup_point.Enabled = false;
                    await task_spot;//.Wait();
                }
            }
                


            else if(!scanning_radio.Checked && !spot_radio.Checked)
            {
                MessageBox.Show("복제 방식을 선택하세요");
                return;

            }

            x_start = 0;
            z_start = 0;

            x_complete = 0;
            z_complete = 0;

            decimal speed_2 = spd_value_start.Value;
            string spd1 = speed_2.ToString(spd_str);
            var comm1 = speed("0", spd1);
            send_function(comm1);

            Thread.Sleep(500);

            var comm_all_end = move_axis_all_z("0", x_start, z_start);
            send_function(comm_all_end);

            Thread.Sleep(500);

            



            var task_setup = Task.Run(() =>
            {
                while (true)
                {
                    var comm_posi = posi_check_robot("0");
                    send_position(comm_posi);

                    x_complete = Convert.ToDecimal(x_position);
                    z_complete = Convert.ToDecimal(z_position);

                    if (x_complete == Convert.ToDecimal(x_start) && z_complete == Convert.ToDecimal(z_start)) // y축 좌표 비교 이동 완료
                    {
                        //this.Invoke(new Action(finish_print));
                        MessageBox.Show("Finish Job");


                        this.Invoke(new Action(delegate()
                        {
                            textBox3.Clear();
                        }));




                        break;
                    }
                    else
                    {
                        send_function(comm_all_end);
                    }
                }
            });
            await task_setup;

        }





        public void three_shutter_open_manual()
        {




            shutter_device1.SetOperatingState(SolenoidStatus.OperatingStates.Active);
            shutter_device2.SetOperatingState(SolenoidStatus.OperatingStates.Active);
            shutter_device3.SetOperatingState(SolenoidStatus.OperatingStates.Active);


        }

        public void three_shutter_close_manual()
        {

            shutter_device1.SetOperatingState(SolenoidStatus.OperatingStates.Inactive);
            shutter_device2.SetOperatingState(SolenoidStatus.OperatingStates.Inactive);
            shutter_device3.SetOperatingState(SolenoidStatus.OperatingStates.Inactive);

        }















        private void spot_pitch_ValueChanged(object sender, EventArgs e)
        {
            double x_limt_spot = x_start + (Convert.ToDouble(spot_pitch.Value) *
                    ((Convert.ToInt32(spot_count.Value)) - 1));

            double z_limt_spot = Convert.ToDouble(z_srt.Value);



            if (z_limt_spot > 310.00)//y 센서 거리 초과
            {
                //var comm = move_axis("0", x_stage_point, y_stage_point);
                //send2controller(comm);
                MessageBox.Show("OVER Z-AXIS LIMIT");
            }

            else if (x_limt_spot > 800.00)//x 센서 거리 초과
            {
                //var comm = move_axis("0", x_stage_point, y_stage_point);
                //send2controller(comm);
                MessageBox.Show("OVER X-AXIS LIMIT");


            }
        }



        private void three_shutter_open_single()
        {
            //if (_taskComplete_cancel == false)
            // {
            //MessageBox.Show("Stop Job");
            //textBox2.Text = "Please Origin & Start Job!";
            //progressBar1.Value = 0;
            //shutter_device1.SetOperatingState(SolenoidStatus.OperatingStates.Inactive);
            //shutter_device2.SetOperatingState(SolenoidStatus.OperatingStates.Inactive);
            //shutter_device3.SetOperatingState(SolenoidStatus.OperatingStates.Inactive);

            //  return;
            //break;
            //  }

            shutter_device1.SetOperatingState(SolenoidStatus.OperatingStates.Active);
            shutter_device2.SetOperatingState(SolenoidStatus.OperatingStates.Active);
            shutter_device3.SetOperatingState(SolenoidStatus.OperatingStates.Active);



            int a = settings1.OpenTime;
            int b = settings2.OpenTime;
            int c = settings3.OpenTime;



            int high_time, low_time;

            if (a > b)
            {
                high_time = a;

            }
            else
            {
                high_time = b;
            }

            if (high_time < c)
            {
                high_time = c;
            }
            else
            {

            }

            Thread.Sleep((high_time / 2) + 500);

            while (true)
            {
                signal1 = shutter_device1.GetStatusBits();
                signal2 = shutter_device2.GetStatusBits();
                signal3 = shutter_device3.GetStatusBits();

                if (signal1 == 8192 && signal2 == 8192 && signal3 == 8192)
                    break;
            }
        }



        private async void setup_point_Click(object sender, EventArgs e)
        {

            this.Invoke(new Action(delegate ()
            {
                textBox3.Clear();
            }));


            decimal speed_2 = spd_value_start.Value;
            string spd = speed_2.ToString(spd_str);
            var comm = speed("0", spd);
            send_function(comm);


            //spd_value_start


            x_stage_point = Convert.ToDouble(x_srt.Value);
            // y_stage_point = Convert.ToDouble(str_y.Value);
            z_stage_point = Convert.ToDouble(z_srt.Value);
            w_stage_point = Convert.ToDouble(z_srt.Value);

            //var comm_posi_1 = posi_check_robot("0");
            //send_position(comm_posi_1);

            double x_set = Convert.ToDouble(x_position);
            //double y_set = Convert.ToDouble(y_position);
            double z_set = Convert.ToDouble(z_position);
            double w_set = Convert.ToDouble(w_position);

            Thread.Sleep(300);

            x_start = x_stage_point;
            z_start = z_stage_point;

            double x_setup_db = CustomRound(RoundType.Truncate, x_stage_point, 3);
            //double y_setup_db = CustomRound(RoundType.Truncate, y_stage_point, 2);
            double z_setup_db = CustomRound(RoundType.Truncate, z_stage_point, 3);

            var task_setup = Task.Run(() =>
            {
                var comm_move_xz = move_axis_all_z("0", x_stage_point, z_stage_point);
                send_function(comm_move_xz);

                Thread.Sleep(1000);


                while (true)
                {
                    var comm_posi = posi_check_robot("0");
                    send_position(comm_posi);

                    x_complete = Convert.ToDecimal(x_position);
                    //y_complete = Convert.ToDecimal(y_position);
                    z_complete = Convert.ToDecimal(z_position);

                    if (x_complete == Convert.ToDecimal(x_setup_db) && z_complete == Convert.ToDecimal(z_setup_db))
                    // y축 좌표 비교 이동 완료
                    {

                        //   var comm_down_z = move_axis_all_z("0", x_stage_point, y_stage_point, z_stage_point);
                        //   send_function(comm_down_z);

                        this.Invoke(new Action(setup_ok_print));

                        break;
                    }
                    else
                    {
                        //    var comm_1 = move_axis_all("0", x_stage_point, y_stage_point, 0);
                        //     send_function(comm_1);

                        var comm_move_xz_re = move_axis_all_z("0", x_stage_point, z_stage_point);
                        send_function(comm_move_xz_re);
                    }
                }
            });
            await task_setup;//.Wait();




        }



        public void setup_ok_print()
        {
            textBox3.ResetText();
            textBox3.Text = "setup";

            MessageBox.Show("Start Point OK");

        }

        private enum RoundType
        {
            Ceiling,
            Round,
            Truncate
        }

       

        static private double CustomRound(RoundType roundType, double value, int digit = 1)
        {
            double dReturn = 0;

            // 지정 자릿수의 올림,반올림, 버림을 계산하기 위한 중간 계산
            double digitCal = Math.Pow(10, digit) / 10;

            switch (roundType)
            {
                case RoundType.Ceiling:
                    dReturn = Math.Ceiling(value * digitCal) / digitCal;
                    break;
                case RoundType.Round:
                    dReturn = Math.Round(value * digitCal) / digitCal;
                    break;
                case RoundType.Truncate:
                    dReturn = Math.Truncate(value * digitCal) / digitCal;
                    break;
            }
            return dReturn;
        }

        private void x_left_Click(object sender, EventArgs e)
        {

        }

        private void open_1_Click(object sender, EventArgs e)
        {

            shutter_device1.SetOperatingMode(SolenoidStatus.OperatingModes.Manual);
            shutter_device1.SetOperatingState(SolenoidStatus.OperatingStates.Active);
        }

        private void close_1_Click(object sender, EventArgs e)
        {
            shutter_device1.SetOperatingMode(SolenoidStatus.OperatingModes.Manual);
            shutter_device1.SetOperatingState(SolenoidStatus.OperatingStates.Inactive);
        }

        private void open_2_Click(object sender, EventArgs e)
        {
            shutter_device2.SetOperatingMode(SolenoidStatus.OperatingModes.Manual);
            shutter_device2.SetOperatingState(SolenoidStatus.OperatingStates.Active);
        }

        private void close_2_Click(object sender, EventArgs e)
        {
            shutter_device2.SetOperatingMode(SolenoidStatus.OperatingModes.Manual);
            shutter_device2.SetOperatingState(SolenoidStatus.OperatingStates.Inactive);
        }

        private void open_3_Click(object sender, EventArgs e)
        {
            shutter_device3.SetOperatingMode(SolenoidStatus.OperatingModes.Manual);
            shutter_device3.SetOperatingState(SolenoidStatus.OperatingStates.Active);
        }

        private void close_3_Click(object sender, EventArgs e)
        {
            shutter_device3.SetOperatingMode(SolenoidStatus.OperatingModes.Manual);
            shutter_device3.SetOperatingState(SolenoidStatus.OperatingStates.Inactive);
        }

        private void all_open_Click(object sender, EventArgs e)
        {
            shutter_device1.SetOperatingMode(SolenoidStatus.OperatingModes.Manual);
            shutter_device2.SetOperatingMode(SolenoidStatus.OperatingModes.Manual);
            shutter_device3.SetOperatingMode(SolenoidStatus.OperatingModes.Manual);

            shutter_device1.SetOperatingState(SolenoidStatus.OperatingStates.Active);
            shutter_device2.SetOperatingState(SolenoidStatus.OperatingStates.Active);
            shutter_device3.SetOperatingState(SolenoidStatus.OperatingStates.Active);
        }

        private void all_close_Click(object sender, EventArgs e)
        {
            shutter_device1.SetOperatingMode(SolenoidStatus.OperatingModes.Manual);
            shutter_device2.SetOperatingMode(SolenoidStatus.OperatingModes.Manual);
            shutter_device3.SetOperatingMode(SolenoidStatus.OperatingModes.Manual);

            shutter_device1.SetOperatingState(SolenoidStatus.OperatingStates.Inactive);
            shutter_device2.SetOperatingState(SolenoidStatus.OperatingStates.Inactive);
            shutter_device3.SetOperatingState(SolenoidStatus.OperatingStates.Inactive);
        }

        private async void one_shot_Click(object sender, EventArgs e)
        {
            settings1.OpenTime = (Convert.ToInt16(expo_time_r.Value) * 1000);
            settings1.ClosedTime = 1000;
            settings1.NumberOfCycles = 0;

            settings2.OpenTime = (Convert.ToInt16(expo_time_g.Value) * 1000);
            settings2.ClosedTime = 1000;
            settings2.NumberOfCycles = 0;

            settings3.OpenTime = (Convert.ToInt16(expo_time_b.Value) * 1000);
            settings3.ClosedTime = 1000;
            settings3.NumberOfCycles = 0;

            shutter_device1.SetCycleParams(settings1);
            shutter_device2.SetCycleParams(settings2);
            shutter_device3.SetCycleParams(settings3);

            shutter_device1.SetOperatingMode(SolenoidStatus.OperatingModes.SingleToggle);
            shutter_device2.SetOperatingMode(SolenoidStatus.OperatingModes.SingleToggle);
            shutter_device3.SetOperatingMode(SolenoidStatus.OperatingModes.SingleToggle);





            //    List<int> list = new List<int>();



            if (expo_time_r.Value == 0)
            {
                shutter_device1.DisableDevice();
            }

            if (expo_time_g.Value == 0)
            {
                shutter_device2.DisableDevice();
            }

            if (expo_time_b.Value == 0)
            {
                shutter_device3.DisableDevice();
            }


            if (settings1.OpenTime > 0 && settings2.OpenTime > 0 && settings3.OpenTime > 0)
            {
                var shutter1 = Task.Factory.StartNew(shutter1_oneshot);
                var shutter2 = Task.Factory.StartNew(shutter2_oneshot);
                var shutter3 = Task.Factory.StartNew(shutter3_oneshot);

                await shutter1;
                await shutter2;
                await shutter3;
            }

            else if (settings1.OpenTime > 0 && settings2.OpenTime == 0 && settings3.OpenTime == 0)
            {
                var shutter1 = Task.Factory.StartNew(shutter1_oneshot);
                await shutter1;
            }

            else if (settings1.OpenTime == 0 && settings2.OpenTime > 0 && settings3.OpenTime == 0)
            {
                var shutter2 = Task.Factory.StartNew(shutter2_oneshot);
                await shutter2;
            }

            else if (settings1.OpenTime == 0 && settings2.OpenTime == 0 && settings3.OpenTime > 0)
            {
                var shutter3 = Task.Factory.StartNew(shutter3_oneshot);
                await shutter3;
            }






            else if (settings1.OpenTime > 0 && settings2.OpenTime > 0 && settings3.OpenTime == 0)
            {
                var shutter1 = Task.Factory.StartNew(shutter1_oneshot);
                var shutter2 = Task.Factory.StartNew(shutter2_oneshot);

                await shutter1;
                await shutter2;


            }

            else if (settings1.OpenTime == 0 && settings2.OpenTime > 0 && settings3.OpenTime > 0)
            {
                var shutter2 = Task.Factory.StartNew(shutter2_oneshot);
                var shutter3 = Task.Factory.StartNew(shutter3_oneshot);

                await shutter2;
                await shutter3;

            }

            else if (settings1.OpenTime > 0 && settings2.OpenTime == 0 && settings3.OpenTime > 0)
            {
                var shutter1 = Task.Factory.StartNew(shutter1_oneshot);
                var shutter3 = Task.Factory.StartNew(shutter3_oneshot);

                await shutter1;
                await shutter3;
            }

            else if (settings1.OpenTime == 0 && settings2.OpenTime == 0 && settings3.OpenTime == 0)
            {
                MessageBox.Show("시간을 설정하세요!");
            }
        }



        private void shutter1_oneshot()
        {
            shutter_device1.SetOperatingState(SolenoidStatus.OperatingStates.Active);

            //Thread.Sleep(settings.OpenTime / 2);
            //Thread.Sleep(200);

            while (true)
            {
                signal1 = shutter_device1.GetStatusBits();

                if (signal1 == 8192)
                    break;
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        private void shutter2_oneshot()
        {
            shutter_device2.SetOperatingState(SolenoidStatus.OperatingStates.Active);

            //Thread.Sleep(settings.OpenTime / 2);
            //Thread.Sleep(200);

            while (true)
            {
                signal2 = shutter_device2.GetStatusBits();

                if (signal2 == 8192)
                    break;
                else
                {
                    Thread.Sleep(100);
                }
            }
        }



        private void shutter3_oneshot()
        {
            shutter_device3.SetOperatingState(SolenoidStatus.OperatingStates.Active);
            //Thread.Sleep(200);

            //Thread.Sleep(settings.OpenTime / 2);

            while (true)
            {
                signal3 = shutter_device3.GetStatusBits();

                if (signal3 == 8192)
                    break;
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        private void scanning_pitch_ValueChanged(object sender, EventArgs e)
        {

            double x_limt = x_start + (Convert.ToDouble(scanning_pitch.Value));
            double z_limt = Convert.ToDouble(z_srt.Value);

            if (z_limt > 310.00)//y 센서 거리 초과
            {
                //var comm = move_axis("0", x_stage_point, y_stage_point);
                //send2controller(comm);
                //textBox3.Text = ("OVER Y-AXIS LIMIT");
                MessageBox.Show("OVER Z-AXIS LIMIT");
                return;
            }

            else if (x_limt >= 800.00)//x 센서 거리 초과
            {
                //var comm = move_axis("0", x_stage_point, y_stage_point);
                //send2controller(comm);
                //textBox3.Text = ("OVER X-AXIS LIMIT");
                MessageBox.Show("OVER X-AXIS LIMIT");
                return;
            }
        }


    }
}
