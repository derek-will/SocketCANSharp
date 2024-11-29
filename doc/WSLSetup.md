# Setting Up Windows Subsystem for Linux (WSL) for SocketCAN#

## Set Up

If you already have an active install of WSL running Ubuntu on your Windows 10 or 11 machine, then you may skip this step. 

1. In Windows, open Command Prompt in administrator mode.
2. Run `wsl.exe --install` command.
3. Restart the machine.

This will result in the ability to run WSL and will install Ubuntu as the Linux distribution. The WSL version will be set to WSL 2 by default. Please note that the distribution installed and WSL version may change in the future.

4. Setup your username and password for the Ubuntu installation in WSL. 
5. In your Ubuntu install, run `apt-get update && apt-get dist-upgrade` to update the system.

## Getting WSL 2 Linux Kernel Source for your kernel version.

1. In the Ubuntu WSL instance, clone the WSL2-Linux-Kernel repository via `git clone https://github.com/microsoft/WSL2-Linux-Kernel`.
2. Navigate to the *WSL2-Linux-Kernel* directory.
3. Checkout the branch tagged with your kernel version.

You can find out your kernel version by running `uname -r`. For me, my kernel version was 5.15.153.1 so I checked out `linux-msft-wsl-5.15.153.1` via `git checkout`.

## Prepare for building the Kernel

1. While still in the *WSL2-Linux-Kernel* directory, retrieve the kernel configuration information and decompress it to a file named **.config** by running `cat /proc/config.gz | gunzip > .config`.
2. Verify **.config** is there by running `cat .config`.
3. Install the necessary tools for kernel build prep. 
    - Install ***Make*** via `apt install make`.
    - Install ***GNU Compiler Collection (GCC)*** via `apt install build-essential`.
    - Install ***Flex*** via `apt install flex`.
    - Install ***Bison*** via `apt install bison`.
    - Install ***Basic Calculator*** via `apt install bc`.
    - Install ***SSL library*** via `apt install libssl-dev`.
    - Install ***ELF development libraries and header files*** via `apt install libelf-dev`.
4. Check for any new configuration options in the Linux kernel codebase that are currently not specified in the **.config** file and default them via `make olddefconfig`.
5. Instruct the build system to prepare the kernel source tree for building the kernel itself and to prepare it for building external modules by running `make prepare modules_prepare`.

We are now ready to configure our kernel build.

## Configuring the kernel for CAN

1. Install ***ncurses*** via `apt install libncurses-dev`.
2. Open the menu configuration interface via `make menuconfig`.
3. In the menu configuration interface, go to *Networking Support* and press Enter.
4. Under *Networking Support*, go to *CAN bus subsystem support* and press M.
5. Go into the *CAN bus subsystem support* submenu by pressing Enter.
6. Select all options in the *CAN bus subsystem support* submenu. 

In my case, I had to select both SAE J1939 and ISO 15765-2 by selecting and pressing M on both of them.

7. Go into the *CAN Device Drivers* submenu by pressing Enter.
8. Go to *Virtual Local CAN Interface* and press M.
9. Save these changes by overwriting **.config** and then exit the configuration menu.

We are now ready to build our customized kernel.

## Build the customized kernel

1. Install ***DWARF utilities*** via `apt install dwarves`.
2. Build the compiled binaries by running `make modules`.
3. Install the compiled binaries by running `make modules_install`.
4. Copy the custom kernel to an accessible location via `cp vmlinux /mnt/c/Users/<UserName>/`. Where `<UserName>` is your Windows User.
5. Provide the custom kernel path to WSL 2 by creating a **.wslconfig** file. 

    ```
    cat >> /mnt/c/Users/<UserName>/.wslconfig << "ENDL"
    [wsl2]
    kernel=C:\\Users\\<UserName>\\vmlinux
    ENDL
    ```
    Again, `<UserName>` is your Windows User.

6. Exit out of WSL via `exit` in the Ubuntu WSL terminal.
7. Restart WSL via `wsl --shutdown` using the Windows Command Prompt.
8. Start up WSL by typing `wsl` in the Windows Command Prompt.
9. Verify you are running the new kernel by running `uname -r` in your Ubuntu WSL terminal.

In my case, the output showed `5.15.153.1-microsoft-standard-WSL+`. Note the plus (+) sign now.

10. Load ***vcan*** via `modprobe vcan` to verify that the new CAN modules are now installed.

## Running SocketCAN# on your Customized Ubuntu WSL instance

1. Clone the SocketCAN# repository via `git clone https://github.com/derek-will/SocketCANSharp.git`.
2. Install ***dotnet*** via `apt install dotnet-sdk-8.0`.
3. Navigate to the *SocketCANSharp* directory.
4. Run the test environment setup script included in the SocketCAN# repo via `./test/SocketCANSharpTest/test_env_setup.sh`.
5. Navigate to the *examples/ObjectOrientedDiagAppSimulator* directory.
6. Run the ***ObjectOrientedDiagAppSimulator*** app via `dotnet run` and view the output which should look something like this:

```
Tester :: Sent Request for ECU Serial Number (DID 0xF18C)
ECU :: Received Request
ECU :: Received ReadDataByIdentifier Request
ECU :: DID Requested 0xF18C
ECU :: Sent Response for DID 0xF18C
Tester :: Received Response
Tester :: Received ReadDataByIdentifier Positive Response
Tester :: DID Received 0xF18C
Tester :: Received ECU Serial Number (0xF18C): ESN-123456
```

Congratulations! You are now running SocketCAN# on Windows Subsystem for Linux (WSL). 