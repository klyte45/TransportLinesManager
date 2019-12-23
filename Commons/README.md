# KlyteCommonsIncludable
This is an evolution of future-dead Klyte Commons mod. This mod have a new organization to store common classes that generally are used in any mod. Is recommended to be added as submodule in any mod that want to use the common classes.
The important parts of this library are below:

## Using as include dependency of a mod

Use git submodule to add this library to a new mod. **It must point to the folder `commons` because there are some constraints with this name.** Is also required a implementation of the class `Klyte.Commons.CommonProperties` in the new mod. The properties required there are the same defined in this [Zone Mixer example](https://raw.githubusercontent.com/klyte45/ZoneMixer/master/CommonProperties.cs).

## I18n resolution

Now each mod can have its own files for translation. Anyway, there are some commons entries under `UI/i18n/` folder. Feel free to translate that files for your language, just commit a new file called as `{lang}.txt`, being lang a 2 letters acronym representing the target language. The supported languages are listed in the `/UI/i18n/KlyteLocaleManager.cs` file, field `locales`. To add a new language support, just add the entry to this array in this file.

For dependent mods, the English language is the only one required, once this will be used as fallback when there's no localization for a entry in the current loaded file.

The translation files can be tested in game by just adding a new file to `[CitiesSkylines AppData Folder]\Klyte45Mods\__translations\[lang]`. The files are all updated with the dll packed versions in every game reload, so avoid using the name of the files which are already there.

In mods, the language files should be included as resource inside the DLL (like the common i18n files must be too) and shall be under `/UI/i18n/`, always following the pattern `{lang}.txt`.

The file format is the same of Klyte Commons i18n:

`IDENTIFIER|KEY>INDEX=Translated value`

Notes:
- Identifier is **mandatory**. Key and Index not.
- All identifiers will have the prefix `K45_` appended at the start of the string. So, `SOME_IDENTIFIER` will became `K45_SOME_IDENTIFIER` in the game. To avoid it (when you really want to override a game locale entry) so you must start your identifier with the `%` character. `%SOME_IDENTIFIER` is registered as `SOME_IDENTIFIER` in the game.
- Commentaries can be added prefixing it with a `#`. `#SOME_IDENTIFIER` will be ignore on file parsing.
- Blank lines will be skipped.

Once there's no central mod anymore, the control over the locale will be delegated to the first mod loaded. The other mods will show a message telling them which mod is controlling the i18n management instead of the language dropdown.

