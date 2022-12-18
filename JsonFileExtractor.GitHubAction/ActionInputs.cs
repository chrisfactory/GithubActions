namespace JsonFileExtractor.GitHubAction;

public class ActionInputs
{
    [Option('p', "path", Required = true, HelpText = "The path of the json file.")]
    public string JSonFilePath { get; set; } = null!;

    [Option('v', "properties", Required = true, HelpText = "The JSON properties to extract. (';' separator)")]
    public string Properties { get; set; } = null!;

    [Option('a', "propertiesAlias", Required = false, HelpText = "Property Alias to changed output property name. exmaple: myproperty.subproperty:myalias;autherpro:autheralias")]
    public string PropertiesAlias { get; set; } = null!;

}
