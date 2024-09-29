# Comparison between Iot.Device.SocketCan and SocketCAN#

## What is Iot.Device.SocketCan?

Iot.Device.SocketCan is a part of [Iot.Device.Bindings](https://www.nuget.org/packages/Iot.Device.Bindings/) which are community-maintained device bindings for IoT components. Iot.Device.Bindings is owned by the .NET Foundation.

## What is SocketCAN#?

SocketCAN# is an independently developed and maintained project. It aims to make all SocketCAN features available to .NET developers in a timely manner with a focus on quality and stability. 

## Supported Features

As of SocketCAN# 0.11.0 and Iot.Device.Bindings 3.2.0:

| Feature                                | SocketCAN#             | Iot.Device.SocketCan | 
|:---------------------------------------|:-----------------------|:---------------------| 
| Raw CAN                                | :heavy_check_mark:     | :heavy_check_mark: | 
| Standard Frame Format (11-bit CAN ID)  | :heavy_check_mark:     | :heavy_check_mark: | 
| Extended Frame Format (29-bit CAN ID)  | :heavy_check_mark:     | :heavy_check_mark: | 
| Remote Transimission Request (RTR)     | :heavy_check_mark:     | :heavy_check_mark: |
| Error Message Frames                   | :heavy_check_mark:     | :heavy_check_mark: |
| CAN ID Filtering                       | :heavy_check_mark:     | Limited - :heavy_exclamation_mark: |
| Error Message Frame Filtering          | :heavy_check_mark:     | :x: | 
| Disabling Local Loopback               | :heavy_check_mark:     | :x: | 
| Enabling Receiving Own Messages        | :heavy_check_mark:     | :x: | 
| Joining of Filters                     | :heavy_check_mark:     | :x: |
| Classical CAN Frames                   | :heavy_check_mark:     | :heavy_check_mark: | 
| CAN FD Frames                          | :heavy_check_mark:     | :x: | 
| CAN XL Frames                          | Coming Soon - :clock9: | :x: | 
| CAN XL VCID Configuration              | Coming Soon - :clock9: | :x: |
| Broadcast Manager                      | :heavy_check_mark:     | :x: | 
| ISO-TP (ISO 15765-2)                   | :heavy_check_mark:     | :x: | 
| SAE J1939                              | :heavy_check_mark:     | :x: | 
| Query Available CAN Network Interfaces | :heavy_check_mark:     | :x: | 
| Read CAN Network Interface Attributes  | :heavy_check_mark:     | :x: | 
| CAN-to-CAN Gateway                     | :heavy_check_mark:     | :x: | 
