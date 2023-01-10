<!-- Repository Header Begin -->
<div align="center">
<img src="https://love.puni.sh/resources/avarice.svg" alt="Avarice IconUrl" width="15%">

# Avarice

Positional feedback with pixel perfect rear/flank tracking, range indicators, and more.

</div>

<!-- Repository Header End -->

## Instructions
### ... for users

Avarice is a third party Dalamud plugin - meaning it is not available by default in the plugin installer. Users should add the Puni.sh repository to their Dalamud configuration in order to enable download and installation of the plugin.

Guidance on how to do this can be found [here]().

### ... for developers

Avarice requires two additional helper libraries in order to be complied - [PunishLib](https://github.com/PunishXIV/PunishLib) and [ECommons](https://github.com/NightmareXIV/ECommons) (by NightmareXIV). When building the plugin in `Release` configuration these libraries are automatically pulled from NuGet, however when building in `Debug` configuration the compiler will attempt to use relative paths in order to source them.

`dotnet build --configuration=Release Avarice.csproj`
