using iMobileDevice;
using iMobileDevice.iDevice;
using iMobileDevice.Lockdown;
using iMobileDevice.Plist;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IosTools
{
    internal class Imobiledevice
    {
        public string Udid;
        // 设备型号
        public string ProductType;
        // 系统版本
        public string ProductVersion;
        // 序列号
        public string SerialNumber;
        // 第一Imei
        public string MobileEquipmentIdentifier;
        // Ecid
        public string UniqueDeviceID;


        private readonly IiDeviceApi Idevice = LibiMobileDevice.Instance.iDevice;
        private readonly ILockdownApi Lockdown = LibiMobileDevice.Instance.Lockdown;

        private iDeviceHandle DeviceHandle;

        // 初始化，获取设备信息，并加载
        public Imobiledevice(string udid)
        {
            Idevice.idevice_new(out DeviceHandle, udid).ThrowOnError();
            this.Udid = udid;
            ProductType = GetProductType();
            ProductVersion = GetProductVersion();
            SerialNumber = GetSerialNumber();
            MobileEquipmentIdentifier = GetMainMobileEquipmentIdentifier();
            UniqueDeviceID = GetUniqueDeviceID();
        }

        public PlistHandle ReadDeviceInfo(string key, string domain = null)
        {
            LockdownClientHandle lockdownHandle;
            PlistHandle plistHandle;
            Lockdown.lockdownd_client_new_with_handshake(DeviceHandle, out lockdownHandle, "Quamotion").ThrowOnError();
            Lockdown.lockdownd_get_value(lockdownHandle, domain, key, out plistHandle);
            lockdownHandle.Dispose();
            return plistHandle;
        }

        // 获取设备型号
        public string GetProductType()
        {
            return PlistParserToString(ReadDeviceInfo("ProductType"));
        }
        // 获取系统版本
        public string GetProductVersion()
        {
            return PlistParserToString(ReadDeviceInfo("ProductVersion"));
        }
        // 获取设备序列号
        public string GetSerialNumber()
        {
            return PlistParserToString(ReadDeviceInfo("SerialNumber"));
        }
        // 获取运营商捆绑信息
        public List<PlistHandle> GetCarrierBundleInfo()
        {
            return PlistParserToList(ReadDeviceInfo("CarrierBundleInfoArray"));
        }
        // 获取第一卡槽IMEI信息
        public string GetMainMobileEquipmentIdentifier()
        {
            List<PlistHandle> carrierBundleList = GetCarrierBundleInfo();
            string r = null;

            if (carrierBundleList.Count() != 0) {
                r = PlistParserToDict(carrierBundleList[0], "MobileEquipmentIdentifier");
            }

            if (r == null || r.Length == 0) {
                r = PlistParserToString(ReadDeviceInfo("InternationalMobileEquipmentIdentity"));
            }
            return r;
        }
        // 获取Ecid
        public string GetUniqueDeviceID()
        {
            return PlistParserToString(ReadDeviceInfo("UniqueDeviceID"));
        }


        // 销毁实例
        public void Destroy()
        {
            DeviceHandle.Dispose();
        }

        // 获取设备列表
        public static ReadOnlyCollection<string> GetDeviceList()
        {
            ReadOnlyCollection<string> udids;
            int count = 0;
            var ret = LibiMobileDevice.Instance.iDevice.idevice_get_device_list(out udids, ref count);
            return udids;
        }

        // 字符串类型数据解析
        public static string PlistParserToString(PlistHandle plistHandle)
        {
            string result;
            plistHandle.Api.Plist.plist_get_string_val(plistHandle, out result);
            return result;
        }
        // 数组类型数据解析
        public static List<PlistHandle> PlistParserToList(PlistHandle plistHandle)
        {
            List<PlistHandle> result = new List<PlistHandle>();

            uint len = plistHandle.Api.Plist.plist_array_get_size(plistHandle);
            for (uint i = 0; i < len; i++) {
                PlistHandle item = plistHandle.Api.Plist.plist_array_get_item(plistHandle, i);
                result.Add(item);
            }
            return result;
        }

        // Dict类型数据取item
        public static string PlistParserToDict(PlistHandle plistHandle, string key) {
            return PlistParserToString(plistHandle.Api.Plist.plist_dict_get_item(plistHandle, key));
        }
    }
}
