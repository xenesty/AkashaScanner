# Akasha Scanner

A Genshin Impact tool for extracting game data, including achievements, characters, artifacts, and weapons.


## Table of Contents

- [Features](#features)
- [Getting Started](#getting-started)
- [Usage](#usage)
- [FAQ](#faq)
- [Roadmap](#roadmap)
- [Contributing](#contributing)
    - [Reporting Bugs](#reporting-bugs)
    - [Submitting Pull Requests](#submitting-pull-requests)
- [Acknowledgements](#acknowledgements)
- [License](#license)


## Features

- Scans achievements, characters, artifacts, and weapons by taking and analyzing screenshots of the game automatically
- Exports achievements (and characters) to [Paimon.moe](https://paimon.moe)
- Exports game data in `GOOD`,
a format compatible with tools like
[Genshin Optimizer](https://frzyc.github.io/genshin-optimizer),
[GI DMG Calculator](https://gidmgcalculator.github.io/csb-g7is6)
and [Aspirine's Genshin Impact Calculator](https://genshin.aspirine.su).

> **Note**
>
> This project is currently in beta. It is useable right now, but future changes may invalidate your existing scanned data.


## Getting Started

Download the latest version of the Akasha Scanner [here](https://github.com/xenesty/AkashaScanner/releases/latest) and unzip its files.

You need Genshin Impact installed on your computer.
You will also need to install
[Microsoft Visual C++ Redistributable](https://docs.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#visual-studio-2015-2017-2019-and-2022)
([Direct download link](https://aka.ms/vs/17/release/vc_redist.x64.exe))

To launch the program, open `AkashaScanner.exe`.
It will ask for Administrator permissions,
which is required to take control of the game.

__A few important settings:__
1. Game language must be English
2. The game window should not be wider than 16:9 and not smaller than 1280x720
3. During the scan, the entire game screen should remain visible (no off-screen portion, and nothing covers the game window)
4. Turn off any color filters, such as Reshade, Windows Night Light, F.lux, Nvidia Filters, and Color-blind tools that you may be running


## Usage

Select a scanner from the left menu.
You can follow the instructions on the page to start scanning.

Do not use your mouse or keyboard while scanning.
The scanner uses mouse and keyboard input to automate the scanning.

If you want to interrupt the scan, you may minimize or exit the game using hotkeys like Alt+Tab or Win+D.
If you interrupt the scan, keep in mind that all progress will be lost.


## FAQ

### Why is it called Akasha Scanner?
This project aims to provide a way for players to extract data from the game
and share them with other tools, which resembles an
[in-game device](https://genshin-impact.fandom.com/wiki/Akasha_System) 
that transfers and shares information.

### Will I get banned by the game from using the Akasha Scanner?
Probably not, according to [this](https://genshin.hoyoverse.com/en/news/detail/5763) official article.

In addition, similar software like the [Inventory Kamera](https://github.com/Andrewthe13th/Inventory_Kamera)
and [AdeptiScanner-GI](https://github.com/D1firehail/AdeptiScanner-GI)
have been around for a while, and no one has been banned from using these applications.

However, there is no guarantee that HoYoverse will not take any actions against the scanner.
Use at your own risk.

### How does the Akasha Scanner work?
It uses mouse and keyboard inputs to navigate through your achievements/characters/inventory,
and take screenshots of them.

There are a couple of techniques used to analyze the screenshots.
The first is known as [Optical character recognition (OCR)](https://en.wikipedia.org/wiki/Optical_character_recognition)
using [Tesseract](https://github.com/tesseract-ocr/tesseract).
It is mainly used to identify text from images.
For example, the names and levels of weapons/artifacts/characters are identified this way.

Other things like counting the number of stars of achievements/artifacts
use object recognition provided by [OpenCV](https://opencv.org).

### How reliable are the scan results?
I would say it is currently pretty accurate, but it is not perfect.

For example, identifying the number `1` is quite tricky for the OCR,
particularly in characters' talent levels.
Also, random snowflakes covering the character's name and level can mess up the OCR.

Using a higher resolutions would greatly increase the accuracy.
I personally uses 1920x1080, and it is working quite well for me.

### Can I edit the scan result if it is inaccurate?
I will work on this shortly. Stay tuned for a new release!

For now, you can modify the JSON file under the `ScannedData` folder.
I may consider writing a document about the JSON file schema,
though I think they are pretty self-explanatory.

### I have other questions
You may want to check [this](https://github.com/xenesty/AkashaScanner/discussions) out and search for answers.
If you don't find any, please feel free to start a new discussion. :)


## Roadmap

The following is just a to-do list for me to follow.

* [ ] Interrupt scan with hotkeys
* [ ] Scan lock status of weapons and artifacts
* [ ] Show captured screenshots during the scan
* [ ] Reduce CPU load while scanning weapons and artifacts by replacing OCR with image template matching
* [ ] Validate scanned data
* [ ] View scanned achievements
* [ ] Edit scanned achievements
* [ ] View scanned characters, artifacts, and weapons
* [ ] Edit scanned characters, artifacts, and weapons
* [ ] Save incompleted scan
* [ ] Add support to delete scanned data
* [ ] Add debug mode
* [ ] Add support to scan materials and character materials
* [ ] Add a notice when there is nothing to export
* [ ] Make a better home page than the current one
* [ ] Need a better icon


## Contributing

All contributions are welcome!

### Reporting Bugs

If you run into any weird behavior while using the Akasha Scanner,
feel free to open a new issue in this repository.
Please run a search before opening a new issue,
to make sure that no one has already reported or solved the bug you found.

Please provide as much information as possible,
for example log files (under the `Logs` directory),
your system configurations (game window resolution and your monitor resolution)
and/or screenshots of your game
(__make sure you erase your UID in the bottom right corner!__)

### Submitting Pull Requests
All pull requests are greatly appreciated.
You may want to take a look with
[opened issues](https://github.com/xenesty/AkashaScanner/issues?q=is%3Aopen+is%3Aissue)
if you are looking for somewhere to start.

I am also not very experienced with C# and .NET Core
(in fact, this is my first time writing in C#).
So if I did anything stupid, please kindly point it out.

(Also I need help fixing my English >.<)


## Acknowledgements

Shout out to [Inventory Kamera](https://github.com/Andrewthe13th/Inventory_Kamera) for inspiring the work.


## License
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/xenesty/AkashaScanner/blob/master/LICENSE.txt)
