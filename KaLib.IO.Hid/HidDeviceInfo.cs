﻿using System;
using System.Runtime.InteropServices;
using KaLib.IO.Hid.Native;

namespace KaLib.IO.Hid
{
    public class HidDeviceInfo
    {
        /* device path */
        public string Path { get; private set; }
        /* vendor ID */
        public ushort VendorId { get; private set; }
        /* product id */
        public ushort ProductId { get; private set; }
        /* usb product string */
        public string Product { get; private set; }
        /* usb manufacturer string */
        public string Manufacturer { get; private set; }
        /* usb serial number string */
        public string SerialNumber { get; private set; }
        
        internal NativeHidDeviceInfo Handle { get; private set; }

        internal HidDeviceInfo(NativeHidDeviceInfo handle)
        {
            unsafe
            {
                var path = Marshal.PtrToStringUTF8((IntPtr)handle.path);
                var manufacturer = new string(handle.manufacturer_string);
                var product = new string(handle.product_string);
                var vendorId = handle.vendor_id;
                var productId = handle.product_id;
                var serial = new string(handle.serial_number);

                Handle = handle;
                Product = product;
                SerialNumber = serial;
                Manufacturer = manufacturer;
                Path = path;
                VendorId = vendorId;
                ProductId = productId;
            }
        }
    }
}