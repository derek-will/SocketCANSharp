#!/bin/bash

echo "Running CAN device setup script..."

echo "Adding can_dev module to the Linux kernel."
modprobe can_dev

echo "Adding can module to the Linux kernel."
modprobe can

echo "Putting can0 offline."
ip link set can0 down # Put link down for configuring bitrate

echo "Configuring can0 to 500Kbit/s bitrate."
ip link set can0 type can bitrate 500000

echo "Putting can0 online."
ip link set can0 up

echo "CAN device setup script complete!"