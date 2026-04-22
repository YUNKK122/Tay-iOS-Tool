using iMobileDevice;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IosTools
{
    public partial class MainForm : Form
    {
        //事件代码
        private const int WM_DEVICECHANGE = 0x219; //设备改变
        // 驱动实例
        private Imobiledevice Imobiledevice = null;

        public MainForm()
        {
            InitializeComponent();
            NativeLibraries.Load();
            ScanDevice_s();
            FormClosing += (sender, e) =>
            {
                Imobiledevice?.Destroy();
            };
        }

        // 加载设备
        private void LoadDevice(string udid)
        {
            if (Imobiledevice == null || Imobiledevice.Udid != udid) {
                Imobiledevice?.Destroy();
                Imobiledevice = new Imobiledevice(udid);
            }
            // 设置按钮状态为可用
            SetAllButtonLockStatus(true);
            // 读取加载设备基本信息
            label6.Text = Imobiledevice.ProductType;
            label7.Text = Imobiledevice.ProductVersion;
            label8.Text = Imobiledevice.MobileEquipmentIdentifier;
            label9.Text = Imobiledevice.SerialNumber;
            label10.Text = Imobiledevice.UniqueDeviceID;
        }

        // 扫描接入的usb设备  防抖方法
        System.Timers.Timer scanTimer = null;
        private void ScanDevice_s()
        {
            scanTimer?.Dispose();
            scanTimer = new System.Timers.Timer(100);
            scanTimer.Elapsed += (sender, e) => {
                ScanDeviceRun();
            };
            scanTimer.AutoReset = false;
            scanTimer.Start();
        }
        // 减少嵌套
        private void ScanDeviceRun()
        {
            ReadOnlyCollection<string> uids = Imobiledevice.GetDeviceList();
            if (uids.Count == 0)
            {
                SetAllButtonLockStatus(false);
                ResetLableText();
                return;
            }

            // 加载第一台设备
            LoadDevice(uids[0]);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_DEVICECHANGE) {
                // 电脑usb设备发生变化
                ScanDevice_s();
            }
        }

        private void SetAllButtonLockStatus(bool status)
        {
            button1.Enabled = status;
            button2.Enabled = status;
            button3.Enabled = status;
            button4.Enabled = status;
            button5.Enabled = status;
            button6.Enabled = status;
            button7.Enabled = status;
            button8.Enabled = status;
            button9.Enabled = status;
            button10.Enabled = status;
        }

        private void ResetLableText()
        {
            label6.Text = "";
            label7.Text = "";
            label8.Text = "";
            label9.Text = "";
            label10.Text = "";
        }
    }
}
