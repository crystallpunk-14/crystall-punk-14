<p align="center"> <img alt="CrystallPunk" width="880" height="440" src="https://github.com/crystallpunk-14/crystall-punk-14/assets/96445749/d1d1907b-aaa4-4491-83da-342de0ac5244" /></p>

Space Station 14 is a remake of SS13 that runs on [Robust Toolbox](https://github.com/space-wizards/RobustToolbox), our homegrown engine written in C#.

This is the primary repo for Space Station 14. To prevent people forking RobustToolbox, a "content" pack is loaded by the client and server. This content pack contains everything needed to play the game on one specific server.

If you want to host or create content for SS14, this is the repo you need. It contains both RobustToolbox and the content pack for development of new content packs.

## Links

[Website](https://spacestation14.io/) | [Discord](https://discord.ss14.io/) | [Forum](https://forum.spacestation14.io/) | [Steam](https://store.steampowered.com/app/1255460/Space_Station_14/) | [Standalone Download](https://spacestation14.io/about/nightlies/)

## Documentation/Wiki

Our [docs site](https://docs.spacestation14.io/) has documentation on SS14s content, engine, game design and more. We also have lots of resources for new contributors to the project.

## Contributing

We are happy to accept contributions from anybody. Get in Discord if you want to help. We've got a [list of issues](https://github.com/space-wizards/space-station-14-content/issues) that need to be done and anybody can pick them up. Don't be afraid to ask for help either!  
Just make sure your changes and pull requests are in accordance with the [contribution guidelines](https://docs.spacestation14.com/en/general-development/codebase-info/pull-request-guidelines.html).

We are not currently accepting translations of the game on our main repository. If you would like to translate the game into another language consider creating a fork or contributing to a fork.

## Building

1. Clone this repo.
2. Run `RUN_THIS.py` to init submodules and download the engine.
3. Compile the solution.

[More detailed instructions on building the project.](https://docs.spacestation14.com/en/general-development/setup.html)

## License
[![license-badge](https://shields.io/badge/license-CC--BY--NC--SA-lightgrey?style=for-the-badge)](https://creativecommons.org/licenses/by-nc-sa/3.0/)

All CrystallPunk14 codebase contributions are licensed under [Creative Commons BY-NC-SA](https://creativecommons.org/licenses/by-nc-sa/3.0/). See LICENSE for more details.

Most visual assets are licensed under [CC-BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/) unless stated otherwise. Assets have their license and the copyright in the metadata file. [Example](https://github.com/crystallpunk-14/crystall-punk-14/blob/master/Resources/Textures/Objects/Tools/crowbar.rsi/meta.json).

**Important:** This means that code from CrystallPunk14 cannot be ported to others codebases. If you wish to port a specific feature, you must get the developer(s) to sublicense it to you under a license like AGPLv3. This also applies in the opposite direction for features ported from other codebases.