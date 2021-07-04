# weiduize

A command line utility to convert IAP/TBG files into weidu driven mods.

## Usage

weiduize filename.tbg
weiduize filename.iap
  

## Download

You can [download](https://github.com/btigi/weiduize/releases/) the latest version of weiduize.


## Technologies

weiduize is written in C# Net Core 5.


## Compiling

To clone and run this application, you'll need [Git](https://git-scm.com) and [.NET](https://dotnet.microsoft.com/) installed on your computer. From your command line:

```
# Clone this repository
$ git clone https://github.com/btigi/weiduize

# Go into the repository
$ cd weiduize

# Build  the app
$ dotnet build
```


## Notes

The Infinity Engine is a video game engine that powers several isometric real-time role-playing games, released between 1998 and 2002 (with enhanced editions released between 2012 and 2014). The engine was released with no modding support but despite this several hundred mods have been created over the years.

The first distribution mechansim for these mods was TBG files, allowing the modidifed spell (SPL) and item (ITM) files and the accompanying text to be distributed as a single file, imported into the game with a dedicated tool. The TBG format was updated to handle other files types, and futher developed allowed embedding in an IAP file to distribute multiple TBG (and non-TBG) files as a single conglomerate file.

TBG and IAP were replaced by [WeiDU](https://weidu.org/) as a preferred mod distribution mechansim around 2002, as WeiDU allows patching of existing game resources rather than solely distributing new files.

The tools to import TBG/IAP files into the engine are closed source and long un-maintained, making the process of using any mod distributed as a TBG or IAP file difficult. weiduize aims to resolve this by converting TBG/IAP style mods into the more modern weidu style.


## License

weiduize is licensed under [CC BY-NC 4.0](https://creativecommons.org/licenses/by-nc/4.0/)