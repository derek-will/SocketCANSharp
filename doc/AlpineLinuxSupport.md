# Support for Alpine Linux

## Testing Notes
- Tested on Alpine Linux v3.20, Kernel v6.6.49
- Running NET SDK v6.0.135 and .NET Runtime v6.0.35
    - Unit Test Results: 613 Passed, 2 Skipped (CAN XL VCID Options)
- Running NET SDK v8.0.111 and .NET Runtime v8.0.11
    - Unit Test Results: 620 Passed, 2 Skipped (CAN XL VCID Options)
- All example applications confirmed to work

## System Setup
- Enable community repository: [Alpine Wiki Link](https://wiki.alpinelinux.org/w/index.php?title=Enable_Community_Repository&redirect=no)
- Install .NET runtime: `apk add dotnet6-runtime` for running example applications.
- (Optional) Install .NET SDK: `apk add dotnet6-sdk` for running unit tests.
- When setting up a physical CAN interface (i.e., `ip link set can0 up type can bitrate 500000`) you may receive `ip: either "dev" is duplicate, or "type" is garbage` error message. If that is the case, then install the Iproute2 network configuration utilities collection via `apk add iproute2`.