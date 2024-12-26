#!/bin/bash

echo "Running test environment setup script..."

echo "Adding can module to the Linux kernel."
modprobe can

echo "Adding can_raw module to the Linux kernel."
modprobe can_raw

echo "Adding can_bcm module to the Linux kernel."
modprobe can_bcm

echo "Adding can_gw module to the Linux kernel."
modprobe can_gw

echo "Adding can_j1939 module to the Linux kernel."
modprobe can_j1939

echo "Adding can_isotp module to the Linux kernel."
modprobe can_isotp

echo "Adding vcan module to the Linux kernel."
modprobe vcan

echo "Adding vcan0 device and setting link up."
ip link add dev vcan0 type vcan
ip link set dev vcan0 down # Put link down for configuring MTU
ip link set vcan0 mtu 72 # Try setting MTU to CANFD_MTU
ip link set vcan0 mtu 2060 # Try setting MTU to CANXL_MTU
ip link set up vcan0

echo "Adding vcan1 device and setting link up."
ip link add dev vcan1 type vcan
ip link set dev vcan1 down # Put link down for configuring MTU
ip link set vcan1 mtu 72 # Try setting MTU to CANFD_MTU
ip link set vcan1 mtu 2060 # Try setting MTU to CANXL_MTU
ip link set up vcan1

echo "Adding vcan2 device and setting link up."
ip link add dev vcan2 type vcan
ip link set dev vcan2 down # Put link down for configuring MTU
ip link set vcan2 mtu 72 # Try setting MTU to CANFD_MTU
ip link set vcan2 mtu 2060 # Try setting MTU to CANXL_MTU
ip link set up vcan2

echo "Test environment setup script complete!"